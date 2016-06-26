using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PLinkBeam
    {
        private static class LinkBeamRecognizer
        {
            static LinkBeamRecognizer()
            {
                ds_rein = _addiInfo.defaultSet[(byte)DefaultSet.LinkBeam_Rein];
            }
            private static HashSet<ElementId> _checkedBeams = new HashSet<ElementId>();
            private static List<FamilyInstance> _linkBeams = new List<FamilyInstance>(10);
            private static int ds_rein;
            private static Direction GetBeamDirection(FamilyInstance beam)
            {
                if (ErrorCTRL_BeamDirection < System.Math.Abs(beam.HandOrientation.X))
                    return Direction.X;
                else if (ErrorCTRL_BeamDirection < System.Math.Abs(beam.HandOrientation.Y))
                    return Direction.Y;
                else return Direction.Undefined;
            }
            private static Dictionary<string, int> _dictionary = new Dictionary<string, int>(13);       //num = 12
            private static void UpdateToPGs(Element beam, int floor, Direction direction, double width, double aspectRatio)
            {
                string FGCode = "B1042.0";
                if (width < 16 / 12 * ConstSet.FeetToMeter) FGCode += "0";
                else if (width <= 24 / 12 * ConstSet.FeetToMeter) FGCode += "1";
                else FGCode += "2";
                FGCode += (ds_rein + 1).ToString() + ConstSet.Alphabet[(int)(aspectRatio / 2.0)];

                int index;
                if (_dictionary.TryGetValue(FGCode + direction.ToString(), out index))
                {
                    _PGItems.ElementAt(index).Num[floor] += 1.0;
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = "混凝土连梁";
                    pgItem.PinYinSuffix = "HunNingTuLianLiang";
                    pgItem.Code = FGCode;
                    pgItem.direction = direction;
                    pgItem.Num[floor] += 1.0;
                    pgItem.Price = _addiInfo.prices[(byte)PGComponents.LinkBeam];
                    if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                    else pgItem.IfDefinePrice = true;
                    _PGItems.Add(pgItem);
                    _dictionary.Add(FGCode + direction.ToString(), _PGItems.Count - 1);
                }
            }
            public static void Recognization(Wall shearWall)
            {
                BoundingBoxXYZ bbXYZ = shearWall.get_BoundingBox(_doc.ActiveView);
                BoundingBoxIntersectsFilter bbif = new BoundingBoxIntersectsFilter(new Outline(bbXYZ.Min - _adjXYZ, bbXYZ.Max + _adjXYZ));
                FilteredElementCollector fec = new FilteredElementCollector(_doc);
                List<FamilyInstance> possibleBeams = fec.WherePasses(bbif).WherePasses(new ElementStructuralTypeFilter(StructuralType.Beam)).Cast<FamilyInstance>().ToList();
                foreach (FamilyInstance beam in possibleBeams)
                {
                    ElementId id = beam.Id;
                    if (_checkedBeams.Contains(id)) continue;
                    else
                    {
                        _checkedBeams.Add(id);
                        FilteredElementCollector beamFEC = new FilteredElementCollector(_doc);
                        BoundingBoxXYZ beamBBXYZ = beam.get_BoundingBox(_doc.ActiveView);
                        BoundingBoxIntersectsFilter beamBBIF = 
                            new BoundingBoxIntersectsFilter(new Outline(beamBBXYZ.Min - _adjXYZ, beamBBXYZ.Max + _adjXYZ));
                        beamFEC.WherePasses(beamBBIF).OfClass(typeof(Wall));

                        int count = 0;
                        foreach (Wall wall in beamFEC)
                        {
                            if (wall.Id != shearWall.Id) ++count;
                        }
                        if (count == 1) _linkBeams.Add(beam);
                    }
                }
            }
            public static void UpdateToPGs()
            {
                foreach (FamilyInstance beam in _linkBeams)
                {
                    BoundingBoxXYZ bbXYZ = beam.get_BoundingBox(_doc.ActiveView);
                    double depth = bbXYZ.Max.Z - bbXYZ.Min.Z;
                    if (30 / 12 <= depth)
                    {
                        _abandonWriter.WriteAbandonment(beam, AbandonmentTable.LinkBeam_BeamTooHi);
                        continue;
                    }
                    Direction dire = GetBeamDirection(beam);
                    if (dire == Direction.Undefined)
                    {
                        _abandonWriter.WriteAbandonment(beam, AbandonmentTable.SkewBeam);
                        continue;
                    }
                    double width = Math.Min(bbXYZ.Max.Y - bbXYZ.Min.Y, bbXYZ.Max.X - bbXYZ.Min.X);
                    double aspectRatio = depth / width;
                    if (aspectRatio < 1.0 || 4.0 < aspectRatio)
                    {
                        _abandonWriter.WriteAbandonment(beam, AbandonmentTable.LinkBeam_AspectRatioOOR);
                        continue;
                    }

                    Level level = (Level)_doc.GetElement(beam.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM).AsElementId());
                    double offset = beam.get_Parameter(BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION).AsDouble();
                    bool isFound;
                    int floor = _myLevel.GetFloor(out isFound, level, offset);
                    if (!isFound)
                    {
                        _abandonWriter.WriteAbandonment(beam, AbandonmentTable.LevelNotFound);
                        continue;
                    }  
                    UpdateToPGs(beam, floor, dire, width, aspectRatio);
                }
            }
        }

        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static List<Wall> _walls;

        private static readonly double ErrorCTRL_Wall = 0.1 / ConstSet.FeetToMeter;
        private static readonly double ErrorCTRL_BeamDirection = System.Math.Cos(ConstSet.AngleTol);
        private static XYZ _adjXYZ = new XYZ(ErrorCTRL_Wall, ErrorCTRL_Wall, ErrorCTRL_Wall);
        
        private static void ExtractObjects()
        {
            FilteredElementCollector fec = new FilteredElementCollector(_doc);
            _walls = fec.OfClass(typeof(Wall)).WherePasses(new StructuralWallUsageFilter(StructuralWallUsage.Shear)).Cast<Wall>().ToList();
        }
        private static void Process()
        {
            foreach (Wall shearWall in _walls)
            {
                LinkBeamRecognizer.Recognization(shearWall);
            }
            LinkBeamRecognizer.UpdateToPGs();
        }
        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _walls = new List<Wall>(20);
            _PGItems = new List<PGItem>(10);
            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}
