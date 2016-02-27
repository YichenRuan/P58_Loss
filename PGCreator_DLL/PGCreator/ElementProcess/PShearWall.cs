﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using P58_Loss.GlobalLib;
using System.Diagnostics;

namespace P58_Loss.ElementProcess
{
    public static class PShearWall
    {
        private static class ShearWallRecognizer
        {
            //Main fields
            private static double _aspectRatio;
            private static ShearWallBoundaryCondition _boundaryCondition;
            private static ShearWallReinforcement _reinforcement;
            private static double _thickness;
            private static double _height;
            private static double _area;
            private static int _floor_top, _floor_bottom;
            private static Direction _direction;
            //Secondary fields
            private static double _length;
            private static Level _level_bottom, _level_top;
            private static double _offset_bottom, _offset_top;
            private static BoundingBoxXYZ _boundingBox;
            private static Wall _wall;
            private static Dictionary<string, int> _dictionary = new Dictionary<string, int>(37);       //Total num of FGs = 36
            private static double _areaBase = 0.0;

            private static void GetBoundCond_and_Rein(int i)
            {
                #region Boundary Condition
                ElementName[] boundaryEle = { ElementName.Level, ElementName.Slab };         //[0] for left, [1] for right

                XYZ bottom = new XYZ(_boundingBox.Min.X, _boundingBox.Min.Y,
                    _myLevel.GetElavation(i - 1) + _height * ErrorCTRL_WallBoundingBox);
                XYZ top = new XYZ(_boundingBox.Max.X, _boundingBox.Max.Y,
                    _myLevel.GetElavation(i) - _height * ErrorCTRL_WallBoundingBox);
                Outline outline = new Outline(bottom, top);
                BoundingBoxIntersectsFilter bbFilter = new BoundingBoxIntersectsFilter(outline);
                FilteredElementCollector intersectedEle = new FilteredElementCollector(_doc);
                List<ElementFilter> listFilter = new List<ElementFilter>();
                listFilter.Add(new ElementCategoryFilter(BuiltInCategory.OST_Walls));
                listFilter.Add(new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns));
                LogicalOrFilter orFilter = new LogicalOrFilter(listFilter);
                intersectedEle.WherePasses(bbFilter).WherePasses(orFilter);


                foreach (Element ele in intersectedEle)
                {
                    //Note: only structural walls/columns are considered as boundary elements
                    if (ele.Category.Name == "墙")
                    #region Wall Intersection
                    {
                        Wall wall = ele as Wall;
                        if (GetWallDirection(wall) == _direction) continue;
                        if (wall.StructuralUsage == StructuralWallUsage.NonBearing) continue;
                        if (wall.get_BoundingBox(_doc.ActiveView).Max.Z - _myLevel.GetElavation(i - 1)
                                        < _height * ErrorCTRL_WallBoundaryEleHeight ||
                                         _myLevel.GetElavation(i) - wall.get_BoundingBox(_doc.ActiveView).Min.Z
                                        < _height * ErrorCTRL_WallBoundaryEleHeight) continue;
                        Curve wallCurve = ((LocationCurve)wall.Location).Curve;
                        switch (_direction)
                        {
                            case Direction.X:
                                if (System.Math.Abs(wallCurve.GetEndPoint(0).X - _boundingBox.Min.X)
                                    < ErrorCTRL_WallIntersection * _thickness)
                                {
                                    if (boundaryEle[0] != ElementName.StruColumn)
                                        boundaryEle[0] = ElementName.ShearWall;
                                }
                                else if (System.Math.Abs(wallCurve.GetEndPoint(0).X - _boundingBox.Max.X)
                                    < ErrorCTRL_WallIntersection * _thickness)
                                {
                                    if (boundaryEle[1] != ElementName.StruColumn)
                                        boundaryEle[1] = ElementName.ShearWall;
                                }
                                break;
                            case Direction.Y:
                                if (System.Math.Abs(wallCurve.GetEndPoint(0).Y - _boundingBox.Min.Y)
                                    < ErrorCTRL_WallIntersection * _thickness)
                                {
                                    if (boundaryEle[0] != ElementName.StruColumn)
                                        boundaryEle[0] = ElementName.ShearWall;
                                }
                                else if (System.Math.Abs(wallCurve.GetEndPoint(0).Y - _boundingBox.Max.Y)
                                    < ErrorCTRL_WallIntersection * _thickness)
                                {
                                    if (boundaryEle[1] != ElementName.StruColumn)
                                        boundaryEle[1] = ElementName.ShearWall;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    #endregion
                    else if (ele.Category.Name.ToString() == "结构柱")
                    #region Column Intersecion
                    {
                        if (ele.get_BoundingBox(_doc.ActiveView).Max.Z - _myLevel.GetElavation(i - 1)
                                        < _height * ErrorCTRL_WallBoundaryEleHeight ||
                                        _myLevel.GetElavation(i) - ele.get_BoundingBox(_doc.ActiveView).Min.Z
                                        < _height * ErrorCTRL_WallBoundaryEleHeight) continue;
                        XYZ columnOri = ((LocationPoint)ele.Location).Point;
                        switch (_direction)
                        {
                            case Direction.X:
                                if (System.Math.Abs(columnOri.X - _boundingBox.Min.X)
                                    < ErrorCTRL_WallIntersection * _thickness)
                                {
                                    boundaryEle[0] = ElementName.StruColumn;
                                }
                                else if (System.Math.Abs(columnOri.X - _boundingBox.Max.X)
                                    < ErrorCTRL_WallIntersection * _thickness)
                                {
                                    boundaryEle[1] = ElementName.StruColumn;
                                }
                                break;
                            case Direction.Y:
                                if (System.Math.Abs(columnOri.Y - _boundingBox.Min.Y)
                                    < ErrorCTRL_WallIntersection * _thickness)
                                {
                                    boundaryEle[0] = ElementName.StruColumn;
                                }
                                else if (System.Math.Abs(columnOri.Y - _boundingBox.Max.Y)
                                    < ErrorCTRL_WallIntersection * _thickness)
                                {
                                    boundaryEle[1] = ElementName.StruColumn;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    #endregion
                }

                if (boundaryEle[0] != boundaryEle[1]) _boundaryCondition = ShearWallBoundaryCondition.Rectangular;
                else if (boundaryEle[0] == ElementName.ShearWall) _boundaryCondition = ShearWallBoundaryCondition.ReturnFlange;
                else if (boundaryEle[0] == ElementName.StruColumn) _boundaryCondition = ShearWallBoundaryCondition.Column;
                #endregion
                #region Reinforcement
                ElementCategoryFilter fabricFilter = new ElementCategoryFilter(BuiltInCategory.OST_FabricReinforcement);
                FilteredElementCollector containedEle = new FilteredElementCollector(_doc);
                containedEle.WherePasses(bbFilter).WherePasses(fabricFilter);

                _reinforcement = ShearWallReinforcement.Unknown;
                if (containedEle.ToElements().Count == 0) return;
                _reinforcement = ShearWallReinforcement.SingleCurtain;
                int fabric_loc = containedEle.FirstElement().get_Parameter(BuiltInParameter.FABRIC_PARAM_LOCATION_WALL).AsInteger();
                double fabric_offset = containedEle.FirstElement().
                    get_Parameter(BuiltInParameter.FABRIC_PARAM_COVER_OFFSET).AsDouble();
                foreach (Element ele in containedEle)
                {
                    if (ele.get_Parameter(BuiltInParameter.FABRIC_PARAM_LOCATION_WALL).AsInteger() != fabric_loc
                        || ele.get_Parameter(BuiltInParameter.FABRIC_PARAM_COVER_OFFSET).AsDouble() != fabric_offset)
                    {
                        _reinforcement = ShearWallReinforcement.DoubleCurtain;
                        break;
                    }
                }
                #endregion                  //Return Unknown if no fabrics are found
            }
            private static bool TryGetFGCode(out string FGcode)
            {
                FGcode = "B1044.";
                if (2.0 < _aspectRatio)                  //Rectangular & low-rise walls
                {
                    switch(_boundaryCondition)
                    {
                        case ShearWallBoundaryCondition.Rectangular:
                            if (24 / 12 < _thickness)
                            {
                                _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.RectangularWallTooThick);
                                return false;
                            }
                            else if (16 / 12 < _thickness)
                            {
                                if (_reinforcement == ShearWallReinforcement.SingleCurtain)
                                {
                                    _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.ThickWallHasSingleCurtain);
                                    return false;
                                }
                                FGcode += "02";
                            }
                            else if (8 / 12 < _thickness)
                            {
                                if (_reinforcement == ShearWallReinforcement.SingleCurtain)
                                {
                                    _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.ThickWallHasSingleCurtain);
                                    return false;
                                }
                                FGcode += "01";
                            }
                            else
                            {
                                if (_reinforcement == ShearWallReinforcement.DoubleCurtain)
                                {
                                    _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.ThinWallHasDoubleCurtain);
                                    return false;
                                }
                                FGcode += "00";
                            }
                            break;
                        case ShearWallBoundaryCondition.ReturnFlange:
                            if (24 / 12 < _thickness)
                            {
                                _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.LowRiseWallTooThick);
                                return false;
                            }
                            else if (16 / 12 < _thickness) FGcode += "05";
                            else if (8 / 12 < _thickness)  FGcode += "04";
                            else FGcode += "03";
                            break;
                        case ShearWallBoundaryCondition.Column:
                            if (24 / 12 < _thickness)
                            {
                                _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.LowRiseWallTooThick);
                                return false;
                            }
                            else if (16 / 12 < _thickness) FGcode += "08";
                            else if (8 / 12 < _thickness) FGcode += "07";
                            else FGcode += "06";
                            break;
                        default:
                            break;
                    }

                    if (40 < _height)
                    {
                        _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.UnSlenderWallTooHigh);
                        return false;
                    }
                    else if (24 < _length) { FGcode += "3"; _areaBase = 900; }
                    else if (15 < _length) { FGcode += "2"; _areaBase = 400; }
                    else { FGcode += "1"; _areaBase = 144; }

                    return true;
                }
                else                                    //Slender Wall
                {
                    if (30 / 12 < _thickness)
                    {
                        _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.SlenderWallTooThick);
                        return false;
                    }
                    else if (18 / 12 < _thickness) FGcode += "11";
                    else if (12 / 12 < _thickness) FGcode += "10";
                    else FGcode += "09";

                    if (12 < _height)
                    {
                        _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.SlenderWallTooHigh);
                        return false;
                    }

                    if (30 < _length)
                    {
                        _abandonWriter.WriteAbandonment(_wall, AbandonmentTable.SlenderWallTooLong);
                        return false;
                    }
                    else if (20 < _length) FGcode += "3";
                    else if (15 < _length) FGcode += "2";
                    else FGcode += "1";

                    _areaBase = 144;
                    return true;
                }
            }
            public static void Recognization(Wall wall)
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

                _direction = GetWallDirection(wall);

                bool isFound;
                _floor_bottom =
                    _myLevel.GetFloor(out isFound, _level_bottom, _offset_bottom);
                _floor_top =
                    _myLevel.GetFloor(out isFound, _level_top, _offset_top);
                if (!isFound) --_floor_top;

                _boundingBox = wall.get_BoundingBox(_doc.ActiveView);
            }
            public static void UpdateToPGs()
            {
                if (_floor_top <= _floor_bottom)
                {
                    _abandonWriter.WriteAbandonment(_wall as Element, AbandonmentTable.WallTooShort);
                    return;
                }
                int i = _floor_bottom + 1;
                int index;
                while (i <= _floor_top)
                {
                    string FGCode;
                    _floor_bottom = i - 1;
                    _height = _myLevel.GetElavation(i) - _myLevel.GetElavation(i - 1);
                    _area = _height * _length;
                    _aspectRatio = _length / _height;
                    GetBoundCond_and_Rein(i);
                    //Default : double curtain
                    if (_reinforcement == ShearWallReinforcement.Unknown) _reinforcement = ShearWallReinforcement.DoubleCurtain;
                    ++i;
                    if (!TryGetFGCode(out FGCode)) continue;
                    if (_dictionary.TryGetValue(FGCode + _direction.ToString(), out index))
                    {
                        _PGItems.ElementAt(index).Num[_floor_bottom] += _area / _areaBase;
                    }
                    else
                    {
                        PGItem pgItem = new PGItem();
                        pgItem.PGName = "剪力墙";
                        pgItem.PinYinSuffix = "JianLiQiang";
                        pgItem.Code = FGCode;
                        pgItem.direction = _direction;
                        pgItem.Num[_floor_bottom] += _area / _areaBase;
                        if (_addiInfo.prices[(byte)PGComponents.ShearWall] != 0.0)
                        {
                            pgItem.IfDefinePrice = true;
                            pgItem.Price = _addiInfo.prices[(byte)PGComponents.ShearWall];
                        }
                        _PGItems.Add(pgItem);
                        _dictionary.Add(FGCode + _direction.ToString(), _PGItems.Count - 1);
                    }
                }
            }
        }

        private enum ShearWallBoundaryCondition : byte
        {
            Rectangular,
            ReturnFlange,
            Column
        }
        private enum ShearWallReinforcement : byte
        {
            Unknown,
            SingleCurtain,
            DoubleCurtain
        }

        private static readonly double ErrorCTRL_WallDirection = 0.05;
        private static readonly double ErrorCTRL_WallBoundingBox = 0.1;
        private static readonly double ErrorCTRL_WallBoundaryEleHeight = 0.9;
        private static readonly double ErrorCTRL_WallIntersection = 1.0;

        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static List<Wall> _ShearWalls;

        private static Direction GetWallDirection(Wall wall)
        {
            //assert: ABS(wall.orientation.#) <= 1
            if (1.0 - System.Math.Abs(wall.Orientation.X) < ErrorCTRL_WallDirection) return Direction.Y;
            else if (1.0 - System.Math.Abs(wall.Orientation.Y) < ErrorCTRL_WallDirection) return Direction.X;
            else return Direction.Undefined;
        }
        private static void ExtractObjects()
        {
            //All structural walls are considered as shear walls
            FilteredElementCollector ShearWallCollector = new FilteredElementCollector(_doc);
            ElementFilter WallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            StructuralWallUsageFilter BearingWallFilter = new StructuralWallUsageFilter(StructuralWallUsage.Bearing);
            StructuralWallUsageFilter ShearWallFilter = new StructuralWallUsageFilter(StructuralWallUsage.Shear);
            StructuralWallUsageFilter CombinedWallFilter = new StructuralWallUsageFilter(StructuralWallUsage.Combined);
            IList<ElementFilter> StruWallFilterList = new List<ElementFilter>();
            StruWallFilterList.Add(BearingWallFilter);
            StruWallFilterList.Add(ShearWallFilter);
            StruWallFilterList.Add(CombinedWallFilter);
            LogicalOrFilter logicOrFilter = new LogicalOrFilter(StruWallFilterList);
            ShearWallCollector.WherePasses(WallFilter).WherePasses(logicOrFilter);

            foreach (Wall wall in ShearWallCollector)
            {
                //Exclude shear walls whose structural material is NOT concrete (or undefined)
                Element paraEle = _doc.GetElement
                    (wall.WallType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsElementId());
                if (paraEle != null && paraEle.Name.ToString().ToLower().Contains("concrete"))
                {
                    _ShearWalls.Add(wall);
                }
                else
                {
                    _abandonWriter.WriteAbandonment(wall, AbandonmentTable.ShearWallNonConcrete);
                }
            }
        }
        private static void Process()
        {
            foreach (Wall wall in _ShearWalls)
            {
                ShearWallRecognizer.Recognization(wall);
                ShearWallRecognizer.UpdateToPGs();
            }
        }
        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _ShearWalls = new List<Wall>(20);
            _PGItems = new List<PGItem>(10);
            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}