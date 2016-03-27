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
    public static class PDuct
    {
        private static class DuctRecognizer
        {
            private static double _area;
            private static double _length;
            private static int _floor;
            private static readonly double _lengthBase = 1000.0;
            private static Dictionary<string, int> _dictionary = new Dictionary<string, int>(11);   //num = 8

            private static Level _level;
            private static double _offset;

            public static bool Recognization(MEPCurve duct)
            {
                //assert: duct's cross-section is either circular or rectangular
                try
                {
                    _area = Math.PI * duct.Diameter * duct.Diameter / 4;
                }
                catch
                {
                    _area = Math.PI * duct.Width * duct.Height;
                }
                _length = duct.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                _level = _doc.GetElement(duct.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).AsElementId()) as Level;
                _offset = duct.get_Parameter(BuiltInParameter.RBS_START_OFFSET_PARAM).AsDouble();
                bool isFound;
                _floor = _myLevel.GetFloor(out isFound,_level,_offset) - 1;
                if (_floor == MyLevel.GetLevelNum())
                {
                    _abandonWriter.WriteAbandonment(duct, AbandonmentTable.DuctLevelError);
                    return false;
                }
                return true;
            }
            public static void UpdateToPGs()
            {
                string FGCode = "D3041.";
                if (_addiInfo.defaultSet[(byte)DefaultSet.Duct_Material] == 0) FGCode += "01";
                else FGCode += "02";
                if (_area <= 6.0) FGCode += "1";
                else FGCode += "2";
                if (_addiInfo.sdc == SDC.A || _addiInfo.sdc == SDC.B) FGCode += "a";
                else if (_addiInfo.sdc == SDC.C) FGCode += "b";
                else if (_addiInfo.sdc == SDC.OSHPD) FGCode += "d";
                else FGCode += "c";

                int index;
                if (_dictionary.TryGetValue(FGCode, out index))
                {
                    _PGItems.ElementAt(index).Num[_floor] += _length / _lengthBase;
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = "风管";
                    pgItem.PinYinSuffix = "FengGuan";
                    pgItem.Code = FGCode;
                    pgItem.direction = Direction.Undefined;
                    pgItem.Num[_floor] += _length / _lengthBase;
                    pgItem.Price = _addiInfo.prices[(byte)PGComponents.Duct];
                    if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                    else pgItem.IfDefinePrice = true;
                    _PGItems.Add(pgItem);
                    _dictionary.Add(FGCode, _PGItems.Count - 1);
                }
            }
        }


        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static List<MEPCurve> _ducts;

        private static void ExtractObjects()
        {
            ElementFilter ductFilter = new ElementCategoryFilter(BuiltInCategory.OST_DuctCurves);
            ElementFilter faminsFilter = new ElementClassFilter(typeof(MEPCurve));
            FilteredElementCollector ductCollector = new FilteredElementCollector(_doc);
            ductCollector.WherePasses(ductFilter).WherePasses(faminsFilter);
            foreach (MEPCurve duct in ductCollector) _ducts.Add(duct);
        }
        private static void Process()
        {
            foreach (MEPCurve duct in _ducts)
            {
                if (DuctRecognizer.Recognization(duct));
                    DuctRecognizer.UpdateToPGs();
            }
        }

        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _PGItems = new List<PGItem>(3);
            _ducts = new List<MEPCurve>(50);

            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}
