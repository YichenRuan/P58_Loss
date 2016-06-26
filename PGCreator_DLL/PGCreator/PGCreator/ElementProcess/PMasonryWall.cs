using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PMasonryWall
    {
        private static class MasonryWallRecognizer
        {
            private static Wall _wall;
            private static double _thickness;
            private static Direction _direction;
            private static double _length;
            private static int _floor_top, _floor_bottom;
            private static double _height;
            private static double _area;
            private static double _areaBase;

            private static Level _level_bottom, _level_top;
            private static double _offset_bottom, _offset_top;
            private static double _noConsHeight;
            private static Dictionary<string, int> _dictionary = new Dictionary<string, int>(7);       //Total num of FGs = 6

            private static Direction GetWallDirection(Wall wall)
            {
                if (ErrorCTRL_WallDirection < System.Math.Abs(wall.Orientation.X)) return Direction.Y;
                else if (ErrorCTRL_WallDirection < System.Math.Abs(wall.Orientation.Y)) return Direction.X;
                else return Direction.Undefined;
            }

            private static bool TryGetFGCode(out string FGCode)
            {
                FGCode = _addiInfo.defaultSet[(byte)DefaultSet.MasonryWall_Grout] == 0 ? "B1051." : "B1052.";
                if (4.0 * ConstSet.InchToFeet <= _thickness && _thickness <= 6.0 * ConstSet.InchToFeet)
                    FGCode += "00";
                else if (8.0 * ConstSet.InchToFeet < _thickness && _thickness <= 12.0 * ConstSet.InchToFeet)
                    FGCode += "01";
                else if (12.0 * ConstSet.InchToFeet < _thickness && _thickness <= 16.0 * ConstSet.InchToFeet)
                    FGCode += "02";
                else
                {
                    _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.MasonryWall_ThicknessOOR);
                    return false;
                }

                if (_addiInfo.defaultSet[(byte)DefaultSet.MasonryWall_Mechanics] == 0)
                {
                    if (_height <= 1.0)
                    {
                        FGCode += "1";
                        _areaBase = 100.0;
                    }
                    else
                    {
                        FGCode += "2";
                        _areaBase = 225.0;
                    }
                }
                else
                {
                    if (_height <= 1.0)
                    {
                        FGCode += "3";
                        _areaBase = 100.0;
                    }
                    else
                    {
                        FGCode += "4";
                        _areaBase = 225.0;
                    }
                }
                
                return true;
            }
            public static bool Recognization(Wall wall)
            {
                _wall = wall;
                _thickness = wall.Width;
                _length = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();

                _level_bottom =
                   _doc.GetElement(wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsElementId()) as Level;
                _level_top =
                    _doc.GetElement(wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId()) as Level;
                _offset_bottom = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                _offset_top = wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsDouble();
                _noConsHeight = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();

                _direction = GetWallDirection(wall);

                if (_direction == Direction.Undefined)
                {
                    _abandonWriter.WriteAbandonment(wall, AbandonmentTable.SkewWall);
                    return false;
                }

                bool isFound;
                _floor_bottom =
                    _myLevel.GetFloor(out isFound, _level_bottom, _offset_bottom);
                _floor_top =
                    _myLevel.GetWallTopFloor(out isFound, _level_bottom, _offset_bottom, _noConsHeight);
                if (!isFound) --_floor_top;

                if (!(MyLevel.isLegalFloorIndex(_floor_bottom) && MyLevel.isLegalFloorIndex(_floor_top)))
                {
                    _abandonWriter.WriteAbandonment(_wall as Element, AbandonmentTable.LevelNotFound);
                    return false;
                }

                if (_floor_top <= _floor_bottom)
                {
                    _abandonWriter.WriteAbandonment(wall as Element, AbandonmentTable.Wall_WallTooShort);
                    return false;
                }

                return true;
            }
            public static void UpdateToPGs()
            {
                int i = _floor_bottom + 1;
                int index;
                while (i <= _floor_top)
                {
                    string FGCode;
                    _floor_bottom = i - 1;
                    _height = _myLevel.GetElevation(i) - _myLevel.GetElevation(i - 1);
                    _area = _height * _length;
                    ++i;
                    if (!TryGetFGCode(out FGCode)) continue;
                    if (_dictionary.TryGetValue(FGCode + _direction.ToString(), out index))
                    {
                        _PGItems.ElementAt(index).Num[_floor_bottom] += _area / _areaBase;
                    }
                    else
                    {
                        PGItem pgItem = new PGItem();
                        pgItem.PGName = "砌体墙";
                        pgItem.PinYinSuffix = "QiTiQiang";
                        pgItem.Code = FGCode;
                        pgItem.direction = _direction;
                        pgItem.Num[_floor_bottom] += _area / _areaBase;
                        pgItem.Price = _addiInfo.prices[(byte)PGComponents.MasonryWall];
                        if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                        else pgItem.IfDefinePrice = true;
                        _PGItems.Add(pgItem);
                        _dictionary.Add(FGCode + _direction.ToString(), _PGItems.Count - 1);
                    }

                }
            }
        }

        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static List<Element> _masonryWalls;

        private static readonly double ErrorCTRL_WallDirection = System.Math.Cos(ConstSet.AngleTol);

        private static void ExtractObjects()
        {
            FilteredElementCollector Walls = new FilteredElementCollector(_doc);
            ElementFilter WallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            IList<ElementFilter> StruWallFilterList = new List<ElementFilter>();
            StruWallFilterList.Add(new StructuralWallUsageFilter(StructuralWallUsage.Bearing));
            StruWallFilterList.Add(new StructuralWallUsageFilter(StructuralWallUsage.Shear));
            StruWallFilterList.Add(new StructuralWallUsageFilter(StructuralWallUsage.Combined));
            LogicalOrFilter logicOrFilter = new LogicalOrFilter(StruWallFilterList);
            Walls.WherePasses(WallFilter).WherePasses(logicOrFilter);

            foreach (Wall wall in Walls)
            {
                Material material = _doc.GetElement
                    (wall.WallType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsElementId()) as Material;
                if (material == null) continue;
                if (material.MaterialCategory == _addiInfo.materialTypes[(byte)PGMaterialType.Masonry])
                {
                    _masonryWalls.Add(wall);
                }
            }
        }
        private static void Process()
        {
            foreach (Wall wall in _masonryWalls)
            {
                if (MasonryWallRecognizer.Recognization(wall))
                    MasonryWallRecognizer.UpdateToPGs();
            }
        }
        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _PGItems = new List<PGItem>(4);
            _masonryWalls = new List<Element>(10);

            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}
