using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PBracedFrame
    {
        private static bool IsTheSamePoint(XYZ p1, XYZ p2)
        {
            double distance = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            if (distance < ErrorCTRL_Overlap) return true;
            else return false;
        }
        private static bool TryGetFGCode(out string FGCode)
        {
            FGCode = "";
            int ds_type = _addiInfo.defaultSet[(byte)DefaultSet.BracedFrame_FrameType];
            int ds_crossSection = _addiInfo.defaultSet[(byte)DefaultSet.BracedFrame_CrossSection];
            if((ds_type == 0 && 5 <= ds_crossSection) || (ds_type == 1 && ds_crossSection <= 4))
            {
                _abandonWriter.WriteAbandonment(null, AbandonmentTable.BracedFrame_TypeCrossConflict);
                return false;
            }
            else
            {
                int ds_brace = 0;
                int ds_plf = 0;
                if (_Braces.Count != 0)
                {
                    int numOverlap = 0;
                    int numColumn = 0;
                    FamilyInstance brace = (FamilyInstance)_Braces.First();
                    Curve curve = (brace.Location as LocationCurve).Curve;
                    XYZ p0 = curve.GetEndPoint(0);
                    XYZ p1 = curve.GetEndPoint(1);
                    XYZ deltaXYZ = new XYZ(ErrorCTRL_Overlap, ErrorCTRL_Overlap, ErrorCTRL_Overlap);
                    BoundingBoxIntersectsFilter bbFilter = new BoundingBoxIntersectsFilter(
                        new Outline(brace.get_BoundingBox(_doc.ActiveView).Min - deltaXYZ, brace.get_BoundingBox(_doc.ActiveView).Max + deltaXYZ));
                    FilteredElementCollector fec = new FilteredElementCollector(_doc);
                    fec.WherePasses(bbFilter).WherePasses(new ElementStructuralTypeFilter(StructuralType.Brace));
                    foreach (Element brace2 in fec)
                    {
                        if (brace2.Id != brace.Id)
                        {
                            Curve curve2 = (brace2.Location as LocationCurve).Curve;
                            XYZ p2_0 = curve2.GetEndPoint(0);
                            XYZ p2_1 = curve2.GetEndPoint(1);
                            if (IsTheSamePoint(p2_0, p0) || IsTheSamePoint(p2_0, p1)) ++numOverlap;
                            if (IsTheSamePoint(p2_1, p0) || IsTheSamePoint(p2_1, p1)) ++numOverlap;
                            break;
                        }
                    }
                    fec = new FilteredElementCollector(_doc);
                    fec.WherePasses(bbFilter).WherePasses(new ElementStructuralTypeFilter(StructuralType.Column));
                    numColumn = fec.Count();
                    if (numOverlap == 1 && numColumn == 1) ds_brace = 1;    //Chevron
                    else if (numOverlap == 2) ds_brace = 3;                 //X
                    else ds_brace = 2;                                      //single

                    try
                    {
                        IList<Parameter> paras = brace.Symbol.GetParameters("A");
                        double area = paras.First().AsDouble();
                        if (area == 0.0) throw new Exception();
                        else
                        {
                            double plf = area * density_steel;
                            if (plf <= 40.0) ds_plf = 0;
                            else if (100.0 <= plf) ds_plf = 2;
                            else ds_plf = 1;
                        }
                    }
                    catch
                    {
                        ds_plf = _addiInfo.defaultSet[(byte)DefaultSet.BracedFrame_PLF];
                    }

                }
                else
                {
                    ds_brace = _addiInfo.defaultSet[(byte)DefaultSet.BracedFrame_Brace];
                    ds_plf = _addiInfo.defaultSet[(byte)DefaultSet.BracedFrame_PLF];
                }
                if (ds_crossSection != 8)
                {
                    FGCode = "B1033.0";
                    FGCode += ds_crossSection.ToString() + ds_brace.ToString() + ConstSet.Alphabet[ds_plf];
                }
                else
                {
                    FGCode = "B1033.1";
                    if (ds_brace == 3)
                    {
                        _abandonWriter.WriteAbandonment(null, AbandonmentTable.BracedFrame_BRBwXBrace);
                        return false;
                    }
                    else
                    {
                        FGCode += (ds_brace - 1).ToString() + "1" + ConstSet.Alphabet[ds_plf];
                    }
                }
                return true;
            }

        }
        private static void GetNMK(IList<ElementId> columnIds, IList<ElementId> beamIds, out int n, out int m, out int k)          //floor starts from 0
        {
            int num_IndividualColumn;
            k = DFS(columnIds, beamIds, out num_IndividualColumn);
            FilteredElementCollector beamFec = new FilteredElementCollector(_doc, beamIds);
            beamFec.OfClass(typeof(FamilyInstance));
            n = columnIds.Count() - num_IndividualColumn;
            m = beamFec.Count();
            foreach (Element beam in beamFec)
            {
                BoundingBoxXYZ beamBB = beam.get_BoundingBox(_doc.ActiveView);
                XYZ deltaXYZ = new XYZ(ErrorCTRL_BB, ErrorCTRL_BB, ErrorCTRL_BB);
                BoundingBoxIntersectsFilter beamBBFilter = new BoundingBoxIntersectsFilter(new Outline(beamBB.Min - deltaXYZ, beamBB.Max + deltaXYZ));
                FilteredElementCollector fec = new FilteredElementCollector(_doc, columnIds);
                int delta = fec.WherePasses(beamBBFilter).Count() - 2;
                if (delta < 0) delta = -1;      //net of cantilever beam or illegal unsupported beam
                m += delta;
            }
        }
        private static int DFS(IList<ElementId> columnIds, IList<ElementId> beamIds, out int num_IndividualColumn)
        {
            num_IndividualColumn = 0;
            List<Element> columns = (new FilteredElementCollector(_doc, columnIds)).OfClass(typeof(FamilyInstance)).ToList();
            List<Element> beams = (new FilteredElementCollector(_doc, beamIds)).OfClass(typeof(FamilyInstance)).ToList();
            int k = 0;
            LinkedList<Element> open = new LinkedList<Element>();
            HashSet<ElementId> discovered = new HashSet<ElementId>();
            foreach(Element column in columns)
            {
                if (!discovered.Contains(column.Id))
                {
                    ++k;
                    open.AddFirst(column);
                    discovered.Add(column.Id);
                    while (open.Count != 0)
                    {
                        Element e = open.First();
                        open.RemoveFirst();
                        BoundingBoxXYZ bbXYZ = e.get_BoundingBox(_doc.ActiveView);
                        XYZ deltaXYZ = new XYZ(ErrorCTRL_BB, ErrorCTRL_BB, _maxBB.Max.Z);
                        BoundingBoxIntersectsFilter bbFilter = new BoundingBoxIntersectsFilter(new Outline(bbXYZ.Min - deltaXYZ, bbXYZ.Max + deltaXYZ));
                        FilteredElementCollector fec = null;
                        if (((FamilyInstance)e).StructuralUsage == StructuralInstanceUsage.Column)
                        {
                            fec = new FilteredElementCollector(_doc, beamIds);
                            fec.WherePasses(bbFilter);
                            if (fec.Count() == 0)
                            {
                                ++num_IndividualColumn;
                                --k;
                            }
                        }
                        else
                        {
                            fec = new FilteredElementCollector(_doc, columnIds);
                            fec.WherePasses(bbFilter);
                        }
                        
                        foreach (Element intersectedEle in fec)
                        {
                            if (!discovered.Contains(intersectedEle.Id))
                            {
                                open.AddFirst(intersectedEle);
                                discovered.Add(intersectedEle.Id);
                            }      
                        }
                    }
                }
            }
            return k;
        }
        
        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static IList<ElementId> _StruColumns;
        private static IList<ElementId> _StruBeams;
        private static List<Element> _Braces;
        private static BoundingBoxXYZ _maxBB;
        private static readonly double ErrorCTRL_BB = 0.5 / ConstSet.FeetToMeter;
        private static readonly double ErrorCTRL_Overlap = 1.0 / ConstSet.FeetToMeter;
        private static readonly double density_steel = 490.0;                                //pound per cubic foot

        private static void ExtractObjects()
        {
            StructuralMaterialTypeFilter steelFilter = new StructuralMaterialTypeFilter(StructuralMaterialType.Steel);
            _StruColumns = (new FilteredElementCollector(_doc)).WherePasses(new ElementStructuralTypeFilter(StructuralType.Column)).WherePasses(steelFilter).ToElementIds().ToList();
            _StruBeams = (new FilteredElementCollector(_doc)).WherePasses(new ElementStructuralTypeFilter(StructuralType.Beam)).WherePasses(steelFilter).ToElementIds().ToList();
            _Braces = (new FilteredElementCollector(_doc)).WherePasses(new ElementStructuralTypeFilter(StructuralType.Brace)).WherePasses(steelFilter).ToList();
            _maxBB = new BoundingBoxXYZ();
        }
        private static void Process()
        {
            if (_StruColumns.Count() * _StruBeams.Count() == 0) return;
            string FGCode;
            if (!TryGetFGCode(out FGCode)) return;
            int floorNum = MyLevel.GetLevelNum() - 1;
            int[] num = new int[floorNum];
            int totalNum = 0;
            for (int i = 0; i < floorNum; ++i)
            {
                int n, m, k;
                double lowerLevel = _myLevel.GetElevation(i);
                double upperLevel = _myLevel.GetElevation(i + 1);
                double deltaHi = upperLevel - lowerLevel;
                double columnLL = lowerLevel + deltaHi * 0.3;
                double columnUL = upperLevel - deltaHi * 0.3;
                XYZ columnBottom = new XYZ(_maxBB.Min.X, _maxBB.Min.Y, columnLL);
                XYZ columnTop = new XYZ(_maxBB.Max.X, _maxBB.Max.Y, columnUL);
                BoundingBoxIntersectsFilter columnBBFilter = new BoundingBoxIntersectsFilter(new Outline(columnBottom, columnTop));
                FilteredElementCollector columnFec = new FilteredElementCollector(_doc, _StruColumns);
                columnFec.WherePasses(columnBBFilter);
                double beamLL = lowerLevel + deltaHi * 0.7;
                double beamUL = upperLevel + deltaHi * 0.3;
                XYZ beamBottom = new XYZ(_maxBB.Min.X, _maxBB.Min.Y, beamLL);
                XYZ beamTop = new XYZ(_maxBB.Max.X, _maxBB.Max.Y, beamUL);
                BoundingBoxIntersectsFilter beamBBFilter = new BoundingBoxIntersectsFilter(new Outline(beamBottom, beamTop));
                FilteredElementCollector beamFec = new FilteredElementCollector(_doc, _StruBeams);
                beamFec.WherePasses(beamBBFilter);
                if (columnFec.Count() * beamFec.Count() == 0) continue;
                GetNMK(columnFec.ToElementIds().ToList(), beamFec.ToElementIds().ToList(), out n, out m, out k);
                num[i] = m - n + k;
                totalNum += num[i];
            }
            if (totalNum != 0)
            {
                PGItem pgItem = new PGItem();
                pgItem.Code = FGCode;
                pgItem.direction = Direction.Undefined;
                pgItem.PGName = "支撑刚架";
                pgItem.PinYinSuffix = "ZhiChengGangJia";
                pgItem.Price = _addiInfo.prices[(byte)PGComponents.BracedFrame];
                if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                else pgItem.IfDefinePrice = true;
                for (int i = 0; i < floorNum; ++i)
                {
                    pgItem.Num[i] = num[i];
                }
                _PGItems.Add(pgItem);
            }
        }

        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _PGItems = new List<PGItem>(1);
            _StruColumns = new List<ElementId>(50);
            _StruBeams = new List<ElementId>(150);
            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}
