using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PFlatSlab
    {
        private static class FlatSlabRecognizer
        {
            private static Floor _slab;
            private static List<Element> _columns;
            private static int _floor;
            private static bool _isPrestress;
            private static BoundingBoxIntersectsFilter bbif;
            private static Dictionary<string, int> _dictionary = new Dictionary<string, int>(15);       //num = 14
            private static int ds_shearRein;
            private static int ds_VgVo;
            private static int ds_contiRein;

            static FlatSlabRecognizer()
            {
                ds_shearRein = _addiInfo.defaultSet[(byte)DefaultSet.FlatSlab_ShearRein];
                ds_VgVo = _addiInfo.defaultSet[(byte)DefaultSet.FlatSlab_VgVo];
                ds_contiRein = _addiInfo.defaultSet[(byte)DefaultSet.FlatSlab_ContiRein];
            }
            private static bool TryGetFGCode(out string FGCode)
            {
                FGCode = "B1049.0";
                XYZ adj = new XYZ(0, 0 , ErrorCTRL_SlabBB);
                bool hasDropPanel = false;
                foreach (Element column in _columns)
                {
                    BoundingBoxXYZ bbXYZ = column.get_BoundingBox(_doc.ActiveView);
                    BoundingBoxIntersectsFilter bbif = new BoundingBoxIntersectsFilter(new Outline(bbXYZ.Min + adj, bbXYZ.Max + adj));
                    FilteredElementCollector fec = new FilteredElementCollector(_doc);
                    fec.WherePasses(bbif).OfCategory(BuiltInCategory.OST_Floors);
                    foreach (Floor f in fec)
                    {
                        if (f.Id != _slab.Id)
                        {
                            hasDropPanel = true;
                            break;
                        }
                    }
                }
                if (!_isPrestress && !hasDropPanel)
                {
                    if (ds_shearRein == 0)
                        FGCode += "0" + (ds_VgVo + 1).ToString() + ConstSet.Alphabet[ds_contiRein];
                    else
                    {
                        if (ds_VgVo == 0)
                        {
                            _abandonWriter.WriteAbandonment(_slab, AbandonmentTable.FlatSlab_ReinVgVoConflict);
                            return false;
                        }
                        FGCode += "1" + ds_VgVo.ToString();
                    }
                }
                else if (_isPrestress)
                {
                    if (ds_shearRein == 0)
                    {
                        if (ds_VgVo == 0)
                        {
                            _abandonWriter.WriteAbandonment(_slab, AbandonmentTable.FlatSlab_ReinVgVoConflict);
                            return false;
                        }
                        FGCode += "2" + ds_VgVo.ToString() + ConstSet.Alphabet[ds_contiRein];
                    }    
                    else
                    {
                        int temp = ds_VgVo == 0 ? 1 : ds_VgVo;
                        FGCode += "3" + temp.ToString();
                    }
                }
                else
                {
                     int temp = ds_VgVo == 0 ? 1 : ds_VgVo;
                     FGCode += "4" + temp.ToString() + ConstSet.Alphabet[ds_contiRein];
                }
                return true;
            }

            public static bool Recognization(Floor slab)
            {
                _slab = slab;
                XYZ minAdj = new XYZ(-ErrorCTRL_SlabBB, -ErrorCTRL_SlabBB, -ErrorCTRL_SlabBB);
                XYZ maxAdj = new XYZ(ErrorCTRL_SlabBB, ErrorCTRL_SlabBB, -ErrorCTRL_SlabBB);
                BoundingBoxXYZ bbXYZ = slab.get_BoundingBox(_doc.ActiveView);
                BoundingBoxIntersectsFilter bbif =
                    new BoundingBoxIntersectsFilter(new Outline(bbXYZ.Min + minAdj, bbXYZ.Max + maxAdj));
                if ((new FilteredElementCollector(_doc)).WherePasses(bbif).
                    WherePasses(new ElementStructuralTypeFilter(StructuralType.Beam)).Count() != 0)
                    return false;
                _columns = (new FilteredElementCollector(_doc)).WherePasses(bbif).OfCategory(BuiltInCategory.OST_Columns).ToList();
                
                Level level = (Level)_doc.GetElement(slab.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM).AsElementId());
                double offset = slab.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
                bool isFound;
                _floor = _myLevel.GetFloor(out isFound, level, offset);
                if (!isFound)
                {
                    _abandonWriter.WriteAbandonment(slab, AbandonmentTable.LevelNotFound);
                    return false;
                }  

                if (_doc.GetElement(slab.FloorType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsElementId()).Name
                            == _addiInfo.materialTypes[(byte)PGMaterialType.PrestressConcrete])
                    _isPrestress = true;
                else _isPrestress = false;
                return true;
            }
            public static void UpdateToPGs()
            {
                string FGCode;
                if (TryGetFGCode(out FGCode))
                {
                    int index;
                    if (_dictionary.TryGetValue(FGCode, out index))
                    {
                        _PGItems.ElementAt(index).Num[_floor] += 1.0;
                    }
                    else
                    {
                        PGItem pgItem = new PGItem();
                        pgItem.PGName = "无梁楼盖";
                        pgItem.PinYinSuffix = "WuLiangLouGai";
                        pgItem.Code = FGCode;
                        pgItem.direction = Direction.Undefined;
                        pgItem.Num[_floor] += 1.0;
                        pgItem.Price = _addiInfo.prices[(byte)PGComponents.FlatSlab];
                        if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                        else pgItem.IfDefinePrice = true;
                        _PGItems.Add(pgItem);
                        _dictionary.Add(FGCode, _PGItems.Count - 1);
                    }
                }
            }
        }

        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static List<Floor> _slabs;

        private static readonly double ErrorCTRL_SlabBB = 0.1 / ConstSet.FeetToMeter;

        private static void ExtractObjects()
        {
            FilteredElementCollector fec = new FilteredElementCollector(_doc);
            IList<ElementFilter> StruMaterialFilterList = new List<ElementFilter>();
            StruMaterialFilterList.Add(new StructuralMaterialTypeFilter(StructuralMaterialType.Concrete));
            StruMaterialFilterList.Add(new StructuralMaterialTypeFilter(StructuralMaterialType.PrecastConcrete));
            LogicalOrFilter StruMaterialFilter = new LogicalOrFilter(StruMaterialFilterList);
            _slabs = fec.OfCategory(BuiltInCategory.OST_Floors).OfClass(typeof(Floor)).
                WherePasses(StruMaterialFilter).Cast<Floor>().ToList();
        }
        private static void Process()
        {
            foreach (Floor floor in _slabs)
            {
                if (FlatSlabRecognizer.Recognization(floor))
                    FlatSlabRecognizer.UpdateToPGs();
            }
        }


        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _slabs = new List<Floor>(20);
            _PGItems = new List<PGItem>(10);
            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}
