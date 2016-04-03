using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PPipe
    {
        private static class PipeRecognizer
        {
           
            private static double _area;
            private static double _length;
            private static int _floor;
            private static readonly double _lengthBase = 1000.0;
            private static Dictionary<string, int> _dictionary = new Dictionary<string, int>(5);   //num = 4

            private static Level _level;
            private static double _offset;


            public static bool Recognization(Pipe pipe)
            {
                try
                {
                    double diameter = pipe.Diameter;
                    if (diameter <= 2.5 * ConstSet.InchToFeet)
                    {
                        _abandonWriter.WriteAbandonment(pipe, AbandonmentTable.PipeDiameterTooSmall);
                        return false;
                    }
                    _area = Math.PI * diameter * diameter / 4;
                }
                catch
                {
                    _abandonWriter.WriteAbandonment(pipe, AbandonmentTable.PipeNonCircular);
                    return false;
                }
                _length = pipe.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                _level = _doc.GetElement(pipe.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).AsElementId()) as Level;
                _offset = pipe.get_Parameter(BuiltInParameter.RBS_START_OFFSET_PARAM).AsDouble();
                bool isFound;
                _floor = _myLevel.GetFloor(out isFound, _level, _offset) - 1;
                if (_floor == MyLevel.GetLevelNum())
                {
                    _abandonWriter.WriteAbandonment(pipe, AbandonmentTable.PipeLevelError);
                    return false;
                }
                SDC sdc = _addiInfo.sdc;
                if (sdc == SDC.A || sdc == SDC.B || sdc == SDC.C)
                {
                    _abandonWriter.WriteAbandonment(pipe, AbandonmentTable.PipeSDCNotOOR);
                    return false;
                }
                /*
                PipingSystem pipingSys = pipe.MEPSystem as PipingSystem;
                PipingSystemType pipingSysType = _doc.GetElement(pipingSys.GetTypeId()) as PipingSystemType;
                ErrorWriter.GetWriter().WriteError(pipingSysType.FluidTemperature.ToString() + "\r\n");
                */

                return true;
            }
            public static void UpdateToPGs()
            {
                string FGCode = "D2061.02";
                SDC sdc = _addiInfo.sdc;

                if (sdc == SDC.D || sdc == SDC.E || sdc == SDC.F) FGCode += "3";
                else FGCode += "4";
                if (_addiInfo.defaultSet[(byte)DefaultSet.Pipe_FragilityType] == 0) FGCode += "a";
                else FGCode += "b";

                int index;
                if (_dictionary.TryGetValue(FGCode, out index))
                {
                    _PGItems.ElementAt(index).Num[_floor] += _length / _lengthBase;
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = "蒸汽管";
                    pgItem.PinYinSuffix = "ZhengQiGuan";
                    pgItem.Code = FGCode;
                    pgItem.direction = Direction.Undefined;
                    pgItem.Num[_floor] += _length / _lengthBase;
                    pgItem.Price = _addiInfo.prices[(byte)PGComponents.Pipe];
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
        private static List<MEPCurve> _pipes;

        private static void ExtractObjects()
        {
            ElementFilter pipeFilter = new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);
            ElementFilter faminsFilter = new ElementClassFilter(typeof(Pipe));
            FilteredElementCollector pipeCollector = new FilteredElementCollector(_doc);
            pipeCollector.WherePasses(pipeFilter).WherePasses(faminsFilter);
            foreach (Pipe pipe in pipeCollector) _pipes.Add(pipe);
        }
        private static void Process()
        {
            foreach (Pipe pipe in _pipes)
            {
                if (PipeRecognizer.Recognization(pipe))
                    PipeRecognizer.UpdateToPGs();
            }
        }

        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _PGItems = new List<PGItem>(3);
            _pipes = new List<MEPCurve>(50);

            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}
