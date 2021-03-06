﻿using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PCurtainWall
    {
        private static class CurtainWallRecognizer
        {
            private static Wall _wall;
            private static int _floor_top, _floor_bottom;
            private static Direction _direction;
            private static int _num_glass = 2;                                                  //1-monolithic, 2-dual panel

            private static Level _level_bottom, _level_top;
            private static double _offset_bottom, _offset_top;
            private static double _noConsHeight;

            private static Direction GetWallDirection(Wall wall)
            {
                if (ErrorCTRL_WallDirection < System.Math.Abs(wall.Orientation.X)) return Direction.Y;
                else if (ErrorCTRL_WallDirection < System.Math.Abs(wall.Orientation.Y)) return Direction.X;
                else return Direction.Undefined;
            }

            public static bool Recognization(Wall wall)
            {
                _wall = wall;

                ElementId panelId = wall.WallType.get_Parameter(BuiltInParameter.AUTO_PANEL_WALL).AsElementId();
                PanelType panelType = _doc.GetElement(panelId) as PanelType;
                if (panelType != null)                                                          //as a panel system
                {
                    Material panelMaterial = 
                        _doc.GetElement(panelType.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM).AsElementId()) as Material;
                    if (panelMaterial.MaterialCategory != _addiInfo.materialTypes[(byte)PGMaterialType.Glass])
                    {
                        return false;
                    }
                }
                else                                                                            //panels built individually
                {
                    ICollection<ElementId> panelIds = _wall.CurtainGrid.GetPanelIds();
                    Panel panel = _doc.GetElement(panelIds.First()) as Panel;
                    if (panel == null) return false;
                    ICollection<ElementId> panelMaterialIds = panel.GetMaterialIds(false);
                    Material panelMaterial = null;
                    bool isContainGlass = false;
                    foreach (ElementId materialId in panelMaterialIds)
                    {
                        panelMaterial = _doc.GetElement(materialId) as Material;

                        if (panelMaterial.MaterialCategory == _addiInfo.materialTypes[(byte)PGMaterialType.Glass])
                        {
                            isContainGlass = true;
                            break;
                        }
                    }
                    if (!isContainGlass)
                    {
                        return false;
                    }
                }

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

                if (!(MyLevel.isLegalFloorIndex(_floor_bottom) && MyLevel.isLegalFloorIndex(_floor_top)))
                {
                    _abandonWriter.WriteAbandonment(_wall as Element, AbandonmentTable.LevelNotFound);
                    return;
                }
                
                if (_floor_top <= _floor_bottom)
                {
                    _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.Wall_WallTooShort);
                    return;
                }
                if (!isFound)
                {
                    --_floor_top;
                    if (_floor_top == MyLevel.GetLevelNum())
                    {
                        _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.LevelOutOfRoof);
                        return;
                    }
                }
                if (_num_glass == 1)
                {
                    _isSetPGItem[(byte)_direction] = true;
                    while(_floor_bottom < _floor_top)
                        _PGItems[(byte)_direction].Num[_floor_bottom++] += 1.0;
                }
                else
                {
                    _isSetPGItem[(byte)_direction + 2] = true;
                    while (_floor_bottom < _floor_top)
                        _PGItems[(byte)_direction + 2].Num[_floor_bottom++] += 1.0;
                }
            }
        }
        
        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static List<Wall> _CurtainWalls;
        private static bool[] _isSetPGItem;

        private static readonly double ErrorCTRL_WallDirection = System.Math.Cos(ConstSet.AngleTol);

        private static void Process()
        {
            foreach (Wall wall in _CurtainWalls)
            {
                if (CurtainWallRecognizer.Recognization(wall))
                    CurtainWallRecognizer.UpdateToPGs();
            }
            for (int i = 3; 0 <= i; --i)
            {
                if (!_isSetPGItem[i]) _PGItems.RemoveAt(i);
            }
        }
        private static void ExtractObjects()
        {
            FilteredElementCollector Walls = new FilteredElementCollector(_doc);
            ElementFilter WallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ElementFilter NonStruWallFilter = new StructuralWallUsageFilter(StructuralWallUsage.NonBearing);
            Walls.WherePasses(WallFilter).WherePasses(NonStruWallFilter);
            foreach (Wall wall in Walls)
            {
                if (wall.WallType.Kind == WallKind.Curtain)
                _CurtainWalls.Add(wall);
            }    
        }

        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _PGItems = new List<PGItem>(4);
            _CurtainWalls = new List<Wall>(10);
            _isSetPGItem = new bool[4];

            double Price = addiInfo.prices[(byte)PGComponents.CurtainWall];
            bool IfDefinePrice = Price == 0.0 ? false : true;
            string[] temp_code = { "B2022.001", "B2022.002" };
            Direction[] temp_dire = { Direction.X, Direction.Y };
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    PGItem pgItem = new PGItem();
                    pgItem.Code = temp_code[i];
                    pgItem.direction = temp_dire[j];
                    pgItem.PGName = "玻璃幕墙";
                    pgItem.PinYinSuffix = "BoLiMuQiang";
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
