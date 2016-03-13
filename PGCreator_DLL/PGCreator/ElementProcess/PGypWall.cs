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
        private static class GypWallRecognizer
        {
            private static Wall _wall;
            private static int _floor_top, _floor_bottom;
            private static Direction _direction;
            private static double _length;

            private static Level _level_bottom, _level_top;
            private static double _offset_bottom, _offset_top;
            private static double _noConsHeight;
            private static readonly double _areaBase = 1300;

            private static Direction GetWallDirection(Wall wall)
            {
                if (ErrorCTRL_WallDirection < System.Math.Abs(wall.Orientation.X)) return Direction.Y;
                else if (ErrorCTRL_WallDirection < System.Math.Abs(wall.Orientation.Y)) return Direction.X;
                else return Direction.Undefined;
            }

            public static bool Recognization(Wall wall)
            {
                _wall = wall;

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
                    if(_floor_top == MyLevel.GetLevelNum())
                    {
                        _abandonWriter.WriteAbandonment(_wall,AbandonmentTable.WallBeyondRoof);
                    }
                    else                                                //partial height
                    {
                        _isSetPGItem[(byte)_direction + 2] = true;
                        _PGItems[(byte)_direction + 2].Num[_floor_top] +=
                            _length * (_level_top.Elevation + _offset_top - _myLevel.GetElevation(_floor_top)) / _areaBase;
                    }
                }
                while (_floor_bottom < _floor_top)                      //full height
                {
                    _isSetPGItem[(byte)_direction] = true;
                    _PGItems[(byte)_direction].Num[_floor_bottom] +=
                            _length * (_myLevel.GetElevation(_floor_top) - _myLevel.GetElevation(_floor_bottom)) / _areaBase;
                    ++_floor_bottom;
                }
            }
        }

        private static readonly double ErrorCTRL_WallDirection = System.Math.Cos(ConstSet.AngleTol);

        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;               //0:aX, 1:aY, 2:bX, 3:bY
        private static List<Element> _GypWalls;
        private static bool[] _isSetPGItem;

        private static void ExtractObjects()
        {
            List<ElementId> materials = null;
            Material material = null;
            FilteredElementCollector Walls = new FilteredElementCollector(_doc);
            ElementFilter WallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ElementFilter NonStruWallFilter = new StructuralWallUsageFilter(StructuralWallUsage.NonBearing);
            Walls.WherePasses(WallFilter).WherePasses(NonStruWallFilter);
            foreach (Wall wall in Walls)
            {
                materials = wall.GetMaterialIds(false).ToList();
                foreach (ElementId eleId in materials)
                {
                    material = _doc.GetElement(eleId) as Material;
                    if (material.Name.ToLower().Contains("gyp") || material.Name.ToLower().Contains("石膏"))
                    {
                        _GypWalls.Add(wall);
                        break;
                    }
                }
            }
        }
        private static void Process()
        {
            foreach (Wall wall in _GypWalls)
            {
                if (GypWallRecognizer.Recognization(wall))
                    GypWallRecognizer.UpdateToPGs();
            }
            for (int i = 3; 0 <= i; --i)
            {
                if (!_isSetPGItem[i]) _PGItems.RemoveAt(i);
            }
        }
        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _PGItems = new List<PGItem>(4);
            _GypWalls = new List<Element>(10);
            _isSetPGItem = new bool[4];

            double Price = addiInfo.prices[(byte)PGComponents.GypWall];
            bool IfDefinePrice = Price == 0.0 ? false : true;
            string[] temp_code = { "C1011.001a", "C1011.001b" };
            Direction[] temp_dire = { Direction.X, Direction.Y };
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    PGItem pgItem = new PGItem();
                    pgItem.Code = temp_code[i];
                    pgItem.direction = temp_dire[j];
                    pgItem.PGName = "石膏板隔墙";
                    pgItem.PinYinSuffix = "ShiGaoBanGeQiang";
                    pgItem.IfDefinePrice = IfDefinePrice;
                    pgItem.Price = Price;

                    _PGItems.Add(pgItem);
                }
            }

            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}
