using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PBeamColumnJoints
    {
        private static class JointsRecognizer
        {
            private static Element _column;
            private static List<Element> _intersectedBeams = new List<Element>(8);
            private static BoundingBoxXYZ _columnBox;
            private static int _bottom_floor, _top_floor;
            private static int[,] _num = new int[2,_numLevel];                      //Temp Recorder : [direction, levels]
            private static int[,] _size = new int[2,_numLevel];
            private static Stack<int>[] _levelRecord = new Stack<int>[2];           //Aided Recorder : record levels
            private static int[,,] _record = new int[4, 3, _numLevel];              //Global Recorder : [X1/X2/Y1/Y2,1by1/1by2/2by2,levels]

            private static void GetIntersectBeams()
            {
                FilteredElementCollector tempCollector = new FilteredElementCollector(_doc);
                ElementStructuralTypeFilter BeamFilter = new ElementStructuralTypeFilter(StructuralType.Beam);
                _intersectedBeams.Clear();
                //Do not consider lower beams
                XYZ left = new XYZ(_columnBox.Min.X - ErrorCTRL_BoundingBox,
                    _columnBox.Min.Y - ErrorCTRL_BoundingBox, 0.5 * (_myLevel.GetElevation(_bottom_floor) + _myLevel.GetElevation(_bottom_floor + 1)));
                XYZ right = new XYZ(_columnBox.Max.X + ErrorCTRL_BoundingBox,
                    _columnBox.Max.Y + ErrorCTRL_BoundingBox, _columnBox.Max.Z + ErrorCTRL_BoundingBox);
                Outline outline = new Outline(left, right);
                BoundingBoxIntersectsFilter bbfilter = new BoundingBoxIntersectsFilter(outline);
                tempCollector.WherePasses(bbfilter).WherePasses(BeamFilter);
                foreach (FamilyInstance beam in tempCollector)
                {
                    if (beam.StructuralMaterialType == StructuralMaterialType.Concrete)
                        _intersectedBeams.Add(beam);
                    else
                        _abandonWriter.WriteAbandonment(beam, AbandonmentTable.RCJoint_BeamNotConcrete);              
                }
            }
            private static bool IsBeamCrossColumn(FamilyInstance beam, Direction dire)
            {
                LocationPoint columnLocation = (LocationPoint)_column.Location;
                XYZ columnP = columnLocation.Point;
                LocationCurve Lcurve = (LocationCurve)beam.Location;
                Curve curve = Lcurve.Curve;
                XYZ beamP1 = curve.GetEndPoint(0);
                XYZ beamP2 = curve.GetEndPoint(1);
                double deltaX = _columnBox.Max.X - _columnBox.Min.X;
                double deltaY = _columnBox.Max.Y - _columnBox.Min.Y;
                double erroCtrl = 2;
                if (dire == Direction.X)
                {
                    erroCtrl = ErrorCTRL_IsCross * deltaX;
                    if (Math.Abs(beamP1.X - columnP.X) < erroCtrl || Math.Abs(beamP2.X - columnP.X) < erroCtrl)
                    {
                        return false;
                    }
                    return true;
                }

                else
                {
                    erroCtrl = ErrorCTRL_IsCross * deltaY;
                    if (Math.Abs(beamP1.Y - columnP.Y) < erroCtrl || Math.Abs(beamP2.Y - columnP.Y) < erroCtrl)
                    {
                        return false;
                    }
                    return true;
                }
            }
            private static BeamSize GetBeamSize(FamilyInstance beam, Direction beamDire)
            {
                double h = 0, b = 0;
                BoundingBoxXYZ beamBox = beam.get_BoundingBox(_doc.ActiveView);
                switch(beamDire)
                {
                    case Direction.X:
                        b = beamBox.Max.Y - beamBox.Min.Y;
                        break;
                    case Direction.Y:
                        b = beamBox.Max.X - beamBox.Min.X;
                        break;
                }
                h = beamBox.Max.Z - beamBox.Min.Z;

                if (h <= 2 && b <= 2) return BeamSize.TwoByTwo;
                else if ((h <= 2 && 2 < b) || (2 < h && b <= 2)) return BeamSize.TwoByThree;
                else return BeamSize.ThreeByThree;
            }
            private static Direction GetBeamDirection(FamilyInstance beam)
            {
                if (ErrorCTRL_BeamDirection < System.Math.Abs(beam.HandOrientation.X))
                    return Direction.X;
                else if (ErrorCTRL_BeamDirection < System.Math.Abs(beam.HandOrientation.Y))
                    return Direction.Y;
                else return Direction.Undefined;
            }
            private static void AddNewConnection(FamilyInstance beam)
            {   
                bool isFound;
                Level beamLevel = null;
                double bottom_offset = 0.0;
                ParameterSet paras = beam.Parameters;
                //Built-in parameter does not work
                foreach (Parameter para in paras)
                {
                    if (para.Definition.Name == "起点标高偏移")
                    { bottom_offset = para.AsDouble(); continue; }
                    if (para.Definition.Name == "参照标高")
                    { beamLevel = _doc.GetElement(para.AsElementId()) as Level; continue; }
                }
                int beamFloor = _myLevel.GetFloor(out isFound, beamLevel, bottom_offset);
                if (!isFound)
                {
                    _abandonWriter.WriteAbandonment(beam, AbandonmentTable.RCJoint_BeamNotConcrete);
                    return;
                }

                Direction beamDire = GetBeamDirection(beam);
                if (beamDire == Direction.Undefined)
                {
                    _abandonWriter.WriteAbandonment(beam, AbandonmentTable.SkewBeam);
                    return;
                }

                bool isCross = IsBeamCrossColumn(beam, beamDire);
                int q = isCross ? 2 : 1;

                BeamSize beamSize = GetBeamSize(beam, beamDire);
                int tempDire = (byte)beamDire;
                int tempSize = (byte)beamSize;
                if (_num[tempDire, beamFloor] == 0 || _size[tempDire, beamFloor] < tempSize)
                    _size[tempDire, beamFloor] = tempSize;
                _num[tempDire, beamFloor] += q;
                _levelRecord[tempDire].Push(beamFloor);
            }
            private static void InitTempRecord()
            {
                for (int i = 0; i < 2; ++i)
                {
                    while (_levelRecord[i].Count != 0) { _num[i, _levelRecord[i].Pop()] = 0; }
                }
            }
            private static void UpdateToGlobalRecord()
            {
                while (_bottom_floor <= _top_floor)
                {
                    if (_num[0, _bottom_floor] == 1) ++_record[0, (byte)_size[0, _bottom_floor], _bottom_floor];
                    else if (_num[0, _bottom_floor] == 2) ++_record[1, (byte)_size[0, _bottom_floor], _bottom_floor];
                    else if (2 < _num[0, _bottom_floor]) _abandonWriter.WriteAbandonment(_column, AbandonmentTable.Joint_TooManyBeamsAtOneJoint);
                    if (_num[1, _bottom_floor] == 1) ++_record[2, (byte)_size[1, _bottom_floor], _bottom_floor];
                    else if (_num[1, _bottom_floor] == 2) ++_record[3, (byte)_size[1, _bottom_floor], _bottom_floor];
                    else if (2 < _num[1, _bottom_floor]) _abandonWriter.WriteAbandonment(_column, AbandonmentTable.Joint_TooManyBeamsAtOneJoint);
                    ++_bottom_floor;
                }
            }
            public static void Recognization(Element column)
            {
                _column = column;
                bool isFound;
                ParameterSet paras = _column.Parameters;
                Level _bottom_level = null;
                Level _top_level = null;
                double _bottom_offset = 0;
                double _top_offset = 0;
                //Built-in parameter does not work
                foreach (Parameter para in paras)
                {
                    if (para != null && para.Definition.Name == "底部标高")
                    { _bottom_level = _doc.GetElement(para.AsElementId()) as Level; continue; }
                    if (para != null && para.Definition.Name == "顶部标高")
                    { _top_level = _doc.GetElement(para.AsElementId()) as Level; continue; }
                    if (para != null && para.Definition.Name == "底部偏移")
                    { _bottom_offset = para.AsDouble(); continue; }
                    if (para != null && para.Definition.Name == "顶部偏移")
                    { _top_offset = para.AsDouble(); continue; }
                }
                _bottom_floor = _myLevel.GetFloor(out isFound, _bottom_level, _bottom_offset);
                _top_floor = _myLevel.GetFloor(out isFound, _top_level, _top_offset);
                if (!isFound) --_top_floor;
                if (_top_floor <= _bottom_floor)
                {
                    _abandonWriter.WriteAbandonment(column, AbandonmentTable.Joint_ColumnTooShort);
                    return;
                }
                _columnBox = _column.get_BoundingBox(_doc.ActiveView);
                GetIntersectBeams();
                foreach (FamilyInstance beam in _intersectedBeams)
                {
                    AddNewConnection(beam);
                }
                UpdateToGlobalRecord();
                InitTempRecord();
            }
            public static void UpdateToPGs()
            {
                string FGCode = "B1041.";
                switch (_addiInfo.mfType)
                {
                    case MomentFrameType.SMF:
                        FGCode += "00";
                        break;
                    case MomentFrameType.confirmedMF:
                        FGCode += "01";
                        break;
                    case MomentFrameType.IMF:
                        FGCode += "02";
                        break;
                    case MomentFrameType.OMF:
                        FGCode += "03";
                        break;
                    case MomentFrameType.unconfirmedMF:
                        FGCode += "08";
                        break;
                }

                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        PGItem pgItem = new PGItem();
                        for (int k = 1; k < _numLevel; ++k)
                        {
                            pgItem.Num[k - 1] = _record[i, j, k];
                        }
                        if (pgItem.Num.Sum() == 0.0) continue;
                        pgItem.Code = FGCode + (j + 1).ToString() + ((i % 2 == 0) ? "a" : "b");
                        pgItem.direction = ((i <= 1) ? Direction.X : Direction.Y);
                        pgItem.PGName = "梁柱结点";
                        pgItem.PinYinSuffix = "LiangZhuJieDian";
                        pgItem.Price = _addiInfo.prices[(byte)PGComponents.BeamColumnJoint];
                        if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                        else pgItem.IfDefinePrice = true;
                        _PGItems.Add(pgItem);
                    }
                }

            }
            static JointsRecognizer()
            {
                _levelRecord[0] = new Stack<int>();
                _levelRecord[1] = new Stack<int>();
            }
        }

        private enum BeamSize : byte
        {
            TwoByTwo = 0,
            TwoByThree = 1,
            ThreeByThree = 2
        }

        private static readonly double ErrorCTRL_IsCross = 2.0;
        private static readonly double ErrorCTRL_BoundingBox = 0.5;
        private static readonly double ErrorCTRL_BeamDirection = System.Math.Cos(ConstSet.AngleTol);

        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static List<Element> _StruColumns;
        private static int _numLevel = MyLevel.GetLevelNum();

        private static void ExtractObjects()
        {
            FilteredElementCollector ColumnCollector = new FilteredElementCollector(_doc);
            ElementStructuralTypeFilter StruColumnFilter = new ElementStructuralTypeFilter(StructuralType.Column);
            IList<ElementFilter> StruMaterialFilterList = new List<ElementFilter>();
            StruMaterialFilterList.Add(new StructuralMaterialTypeFilter(StructuralMaterialType.Concrete));
            StruMaterialFilterList.Add(new StructuralMaterialTypeFilter(StructuralMaterialType.PrecastConcrete));
            LogicalOrFilter StruMaterialFilter = new LogicalOrFilter(StruMaterialFilterList);
            _StruColumns = ColumnCollector.WherePasses(StruColumnFilter).WherePasses(StruMaterialFilter).ToList();
        }
        private static void Process()
        {
            foreach (Element column in _StruColumns)
            {
                JointsRecognizer.Recognization(column);
            }
            JointsRecognizer.UpdateToPGs();
        }
        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _PGItems = new List<PGItem>(10);
            _StruColumns = new List<Element>(50);
            ExtractObjects();
            Process();
            return _PGItems;
        }

    }
}
