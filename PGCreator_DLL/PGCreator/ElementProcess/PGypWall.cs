using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PGypWall
    {
        private enum WallHightMode
        {
            Partial,
            Full
        }
        private enum ReportRule
        {
            Gyp,
            Finish,
            Both
        }
        private enum FinishType
        {
            None = 1,
            OneWallpaper = 3,
            TwoWallpaper,
            OneCeramic,
            TwoCeramic,
            OneWood,
            TwoWood
        }
        private struct RichWall
        {
            public Wall wall;
            public FinishType finishType;
            public RichWall(Wall wall, FinishType finishType)
            {
                this.wall = wall;
                this.finishType = finishType;
            }
        }
        
        private static class GypWallRecognizer
        {
            private static Wall _wall;
            private static FinishType _finishType;
            private static int _floor_top, _floor_bottom;
            private static Direction _direction;
            private static double _length;

            private static Level _level_bottom, _level_top;
            private static double _offset_bottom, _offset_top;
            private static double _noConsHeight;

            private static double _area;
            private static Dictionary<string, int> _dictionary = new Dictionary<string, int>(17);   //total 16

            private static Direction GetWallDirection(Wall wall)
            {
                if (ErrorCTRL_WallDirection < System.Math.Abs(wall.Orientation.X)) return Direction.Y;
                else if (ErrorCTRL_WallDirection < System.Math.Abs(wall.Orientation.Y)) return Direction.X;
                else return Direction.Undefined;
            }
            private static void WriteIntoPG(string FGCode, string PGName, string PinYinSuffix, PGComponents PGComp, double areaBase)
            {
                int index;
                if (_dictionary.TryGetValue(FGCode + _direction.ToString(), out index))
                {
                    _PGItems.ElementAt(index).Num[_floor_bottom] += _area / areaBase;
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = PGName;
                    pgItem.PinYinSuffix = PinYinSuffix;
                    pgItem.Code = FGCode;
                    pgItem.direction = _direction;
                    pgItem.Num[_floor_bottom] += ((byte)_finishType % 2 == 1 ? 1:2) * _area / areaBase;
                    pgItem.Price = _addiInfo.prices[(byte)PGComp];
                    if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                    else pgItem.IfDefinePrice = true;
                    _PGItems.Add(pgItem);
                    _dictionary.Add(FGCode + _direction.ToString(), _PGItems.Count - 1);
                }
            }
            private static void Update(WallHightMode hiMode)
            {
                string FGCode = "";
                
                if (_reportRule == ReportRule.Gyp || _reportRule == ReportRule.Both)
                {
                    switch (hiMode)
                    {
                        case WallHightMode.Full:
                            FGCode = "C1011.001a";
                            break;
                        case WallHightMode.Partial:
                            FGCode = "C1011.001b";
                            break;
                    }
                    WriteIntoPG(FGCode, "石膏板隔墙", "ShiGaoBanGeQiang", PGComponents.GypWall, 1300.0);
                }
                if (_reportRule == ReportRule.Finish || _reportRule == ReportRule.Both)
                {
                    FGCode = "C3011.";
                    if (_finishType == FinishType.OneWallpaper || _finishType == FinishType.TwoWallpaper) FGCode += "001";
                    else if (_finishType == FinishType.OneCeramic || _finishType == FinishType.TwoCeramic) FGCode += "002";
                    else if (_finishType == FinishType.OneWood || _finishType == FinishType.TwoWood) FGCode += "003";
                    switch (hiMode)
                    {
                        case WallHightMode.Full:
                            FGCode += "a";
                            break;
                        case WallHightMode.Partial:
                            FGCode += "b";
                            break;
                    }
                    WriteIntoPG(FGCode, "表面装饰", "BiaoMianZhuangShi", PGComponents.WallFinish, 900.0);
                }
            }

            public static bool Recognization(RichWall richWall)
            {
                _wall = richWall.wall;
                _finishType = richWall.finishType;
                _length = _wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                _level_bottom =
                    _doc.GetElement(_wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsElementId()) as Level;
                _level_top =
                    _doc.GetElement(_wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId()) as Level;
                _offset_bottom = _wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                _offset_top = _wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsDouble();
                _noConsHeight = _wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();

                _direction = GetWallDirection(_wall);
                if (_direction == Direction.Undefined)
                {
                    _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.SkewWall);
                    return false;
                }

                return true;
                
            }
            public static void UpdateToPGs()
            {
                 bool isFound;
                _floor_bottom =
                    _myLevel.GetFloor(out isFound, _level_bottom, _offset_bottom);
                _floor_top =
                    _myLevel.GetWallTopFloor(out isFound, _level_bottom, _offset_bottom, _noConsHeight);
                if (_floor_top <= _floor_bottom)
                {
                    _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.WallBottomError);
                    return;
                }
                if (!isFound)
                {
                    --_floor_top;
                    if (_floor_top == MyLevel.GetLevelNum())
                    {
                        _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.WallBeyondRoof);
                    }
                    else                                                //partial height
                    {
                        _area = _length * (_level_top.Elevation + _offset_top - _myLevel.GetElevation(_floor_top));
                        Update(WallHightMode.Partial);
                    }
                }
                while (_floor_bottom < _floor_top)                      //full height
                {
                    _area = _length * (_myLevel.GetElevation(_floor_top) - _myLevel.GetElevation(_floor_bottom));
                    Update(WallHightMode.Full);
                    ++_floor_bottom;
                }
            }
        }

        private static readonly double ErrorCTRL_WallDirection = System.Math.Cos(ConstSet.AngleTol);

        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems; 
        private static List<RichWall> _GypWalls; 
        private static List<RichWall> _GeneticWalls;
        private static ReportRule _reportRule;

        private static void ExtractObjects()
        {
            List<ElementId> materials = null;
            Material material = null;
            ElementFilter WallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ElementFilter NonStruWallFilter = new StructuralWallUsageFilter(StructuralWallUsage.NonBearing);
            ElementFilter WallClassFilter = new ElementClassFilter(typeof(Wall));
            FilteredElementCollector GypWalls = new FilteredElementCollector(_doc);
            GypWalls.WherePasses(WallFilter).WherePasses(NonStruWallFilter);
            int[] count1 = new int[2];              //0:Gyp, 1:wallpaper, 2:ceramic
            foreach (Wall wall in GypWalls)
            {
                materials = wall.GetMaterialIds(false).ToList();
                foreach (ElementId eleId in materials)
                {
                    material = _doc.GetElement(eleId) as Material;
                    if (material.MaterialCategory == _addiInfo.materialTypes[(byte)PGMaterialType.Gypsum]) ++count1[0];
                    else if (material.MaterialCategory == _addiInfo.materialTypes[(byte)PGMaterialType.WallPaper]) ++count1[1];
                    else if (material.MaterialCategory == _addiInfo.materialTypes[(byte)PGMaterialType.Ceramic]) ++count1[2];
                }
                //assert: count1[i] is non-negative
                if (count1[0] == 0) continue;
                if (count1[1] == 0 && count1[2] == 0) _GypWalls.Add(new RichWall(wall,FinishType.None));
                else if(count1[2] == 0)             //assert: count1[1] != 0
                {
                    if (count1[1] == 1) _GypWalls.Add(new RichWall(wall,FinishType.OneWallpaper));
                    else if (count1[1] == 2) _GypWalls.Add(new RichWall(wall,FinishType.TwoWallpaper));
                }
                else if(count1[1] == 0)             //assert: count1[2] != 0
                {
                    if (count1[2] == 1) _GypWalls.Add(new RichWall(wall,FinishType.OneCeramic));
                    else if (count1[2] == 2) _GypWalls.Add(new RichWall(wall,FinishType.TwoCeramic));
                }
                else _abandonWriter.WriteAbandonment(wall, AbandonmentTable.TooManyFinishes);
            }

            if (_addiInfo.requiredComp[(byte)PGComponents.WallFinish])
            {
                int count2 = 0;
                FilteredElementCollector GeneticWalls = new FilteredElementCollector(_doc);
                GeneticWalls.WherePasses(WallFilter).WherePasses(WallClassFilter);
                foreach (Wall wall in GeneticWalls)
                {
                    materials = wall.GetMaterialIds(false).ToList();
                    foreach (ElementId eleId in materials)
                    {
                        material = _doc.GetElement(eleId) as Material;
                        if (material.MaterialCategory == _addiInfo.materialTypes[(byte)PGMaterialType.Wood]
                        || material.MaterialCategory == _addiInfo.materialTypes[(byte)PGMaterialType.Marble]) ++count2;
                    }
                    if (count2 == 1) _GeneticWalls.Add(new RichWall(wall,FinishType.OneWood));
                    else if (count2 == 2) _GeneticWalls.Add(new RichWall(wall,FinishType.TwoWood));
                    else if (2 <= count2) _abandonWriter.WriteAbandonment(wall, AbandonmentTable.TooManyFinishes);
                }
            }
        }
        private static void Process()
        {
            foreach (RichWall richWall in _GypWalls)
            {
                if (GypWallRecognizer.Recognization(richWall)) GypWallRecognizer.UpdateToPGs();
            }
            foreach (RichWall richWall in _GeneticWalls)
            {
                if (GypWallRecognizer.Recognization(richWall)) GypWallRecognizer.UpdateToPGs();
            }
        }
        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _PGItems = new List<PGItem>(4);

            _GypWalls = new List<RichWall>(10);

            if (_addiInfo.requiredComp[(byte)PGComponents.GypWall] && _addiInfo.requiredComp[(byte)PGComponents.WallFinish])
                _reportRule = ReportRule.Both;
            else if (_addiInfo.requiredComp[(byte)PGComponents.GypWall]) _reportRule = ReportRule.Gyp;
            else _reportRule = ReportRule.Finish;

            if (_addiInfo.requiredComp[(byte)PGComponents.WallFinish])
            {
                _GeneticWalls = new List<RichWall>(6);
            }

            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}
