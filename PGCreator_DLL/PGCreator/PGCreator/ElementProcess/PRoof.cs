using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PRoof
    {
        private static class RoofRecognizer
        {
            private static int ds_secure;
            private static int _matlIndex;
            private static double _area;
            private static int _floor;

            private static readonly double _areaBase = 100;

            static RoofRecognizer()
            {
                ds_secure = _addiInfo.defaultSet[(byte)DefaultSet.Roof_Secure];
            }
            public static bool Recognization(RoofBase roof)
            {
                Level level = (Level)_doc.GetElement(roof.get_Parameter(BuiltInParameter.ROOF_BASE_LEVEL_PARAM).AsElementId());
                double offset = roof.get_Parameter(BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM).AsDouble();
                bool isFound;
                _floor = _myLevel.GetFloor(out isFound, level, offset);
                --_floor;
                if (MyLevel.GetLevelNum() - 2 < _floor || _floor < 0)
                {
                    _abandonWriter.WriteAbandonment(roof, AbandonmentTable.LevelNotFound);
                    return false;
                }
                
                CompoundStructure cs = roof.RoofType.GetCompoundStructure();
                try
                {
                    IList<CompoundStructureLayer> layers = cs.GetLayers();
                    Material matl = (Material)_doc.GetElement(layers.First().MaterialId);
                    if (matl.MaterialCategory == _addiInfo.materialTypes[(byte)PGMaterialType.ConcreteTile])  _matlIndex = 1;
                    else if (matl.MaterialCategory == _addiInfo.materialTypes[(byte)PGMaterialType.ClayTile]) _matlIndex = 2;
                    else
                    {
                        _abandonWriter.WriteAbandonment(roof, AbandonmentTable.Roof_MatlOOR);
                        return false;
                    }
                }
                catch
                {
                    _abandonWriter.WriteAbandonment(roof, AbandonmentTable.Roof_MatlOOR);
                    return false;
                }
                _area = roof.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble();
                return true;
            }
            public static void UpdateToPGs()
            {
                string FGCode = "B3011.01" + ((ds_secure * 2) + _matlIndex).ToString();
                PGItem pgItem = new PGItem();
                pgItem.PGName = "屋顶";
                pgItem.PinYinSuffix = "WuDing";
                pgItem.Code = FGCode;
                pgItem.direction = Direction.Undefined;
                pgItem.Num[_floor] += _area / _areaBase;
                pgItem.Price = _addiInfo.prices[(byte)PGComponents.Roof];
                if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                else pgItem.IfDefinePrice = true;
                _PGItems.Add(pgItem);
            }
        }

        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static List<RoofBase> _roofs;

        private static void ExtractObjects()
        {
            FilteredElementCollector fec = new FilteredElementCollector(_doc);
            _roofs = fec.OfClass(typeof(RoofBase)).OfCategory(BuiltInCategory.OST_Roofs).Cast<RoofBase>().ToList();
        }
        private static void Process()
        {
            foreach (RoofBase roof in _roofs)
            {
                if (RoofRecognizer.Recognization(roof))
                    RoofRecognizer.UpdateToPGs();
            }
        }
        
        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _roofs = new List<RoofBase>(1);
            _PGItems = new List<PGItem>(1);
            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}
