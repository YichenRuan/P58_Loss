using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PPipe
    {
        private static class PipeRecognizer
        {
            private enum PipeType : byte
            {
                ColdWater,
                HotWater,
                SanitaryWater,
                ChilledWater,
                Steam,
                FireSprinkler,
                Unknown
            }

            private static Pipe _pipe;
            private static double _diameter;                                                       //in inch
            private static double _length;
            private static int _floor;
            private static string _material;
            private static readonly double _lengthBase = 1000.0;
            private static Dictionary<string, int> _dictionary = new Dictionary<string, int>(5);   //num = 4
            private static PipeType _pipeType;
            private static readonly string[] _typeMap = { "D2021.0", "D2022.0", "D2031.0", "D2051.0", "D2061.0", "D4011.0" };
            private static readonly string[] _nameZH = { "冷水管", "热水管", "污水管", "制冷管", "蒸汽管","消防管" };
            private static readonly string[] _namePY = { "LengShuiGuan", "ReShuiGuan", "WuShuiGuan", "ZhiLengGuan", "ZhengQiGuan", "XiaoFangGuan" };
            private static readonly string[] _mateList;

            private static Level _level;
            private static double _offset;
            private static bool IsValidMaterial(string matl)
            {
                foreach (string name in _mateList)
                {
                    if (matl == name)
                        return true;
                }
                return false;
            }

            static PipeRecognizer()
            {
                _mateList = new string[5];
                _mateList[0] = _addiInfo.materialTypes[(byte)PGMaterialType.ThreadedSteel];
                _mateList[1] = _addiInfo.materialTypes[(byte)PGMaterialType.WeldedSteel];
                _mateList[2] = _addiInfo.materialTypes[(byte)PGMaterialType.VitaulicSteel];
                _mateList[3] = _addiInfo.materialTypes[(byte)PGMaterialType.CastIron_FC];
                _mateList[4] = _addiInfo.materialTypes[(byte)PGMaterialType.CastIron_BSC];
            }

            public static bool Recognization(Pipe pipe)
            {
                _pipe = pipe;
                try
                {
                    _diameter = pipe.Diameter * 12;
                }
                catch
                {
                    _abandonWriter.WriteAbandonment(pipe, AbandonmentTable.Pipe_NonCircular);
                    return false;
                }
                _length = pipe.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                _level = _doc.GetElement(pipe.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).AsElementId()) as Level;
                _offset = pipe.get_Parameter(BuiltInParameter.RBS_START_OFFSET_PARAM).AsDouble();
                bool isFound;
                _floor = _myLevel.GetFloor(out isFound, _level, _offset) - 1;
                if (_floor == MyLevel.GetLevelNum() || _floor < 0)
                {
                    _abandonWriter.WriteAbandonment(pipe, AbandonmentTable.LevelOutOfRoof);
                    return false;
                }
                SDC sdc = _addiInfo.sdc;
                _material = ((Material)_doc.GetElement(pipe.get_Parameter(BuiltInParameter.RBS_PIPE_MATERIAL_PARAM).AsElementId())).MaterialCategory;
                _pipeType = PipeType.Unknown;
                try
                {
                    PipingSystem pipingSys = pipe.MEPSystem as PipingSystem;
                    PipingSystemType pipingSysType = _doc.GetElement(pipingSys.GetTypeId()) as PipingSystemType;
                    FluidType fluidType = _doc.GetElement(pipingSysType.FluidType) as FluidType;
                    String pstName = pipingSysType.Name;

                    if (pstName.Contains("冷水")) _pipeType = PipeType.ColdWater;
                    if (pstName.Contains("热水")) _pipeType = PipeType.HotWater;
                    if (pstName.Contains("卫生")) _pipeType = PipeType.SanitaryWater;
                    if (pstName.Contains("冷却")) _pipeType = PipeType.ChilledWater;
                    if (pstName.Contains("蒸汽")) _pipeType = PipeType.Steam;
                    if (pstName.Contains("消防")) _pipeType = PipeType.FireSprinkler;

                    if (_pipeType == PipeType.Unknown)
                    {
                        double temperature = pipingSysType.FluidTemperature - 273.15;
                        if (5 < temperature && temperature < 15)         _pipeType = PipeType.ColdWater;
                        else if (30 < temperature && temperature < 100)  _pipeType = PipeType.HotWater;
                        else if (temperature <= 5)                       _pipeType = PipeType.ChilledWater;
                        else if (100 <= temperature)                     _pipeType = PipeType.Steam;
                        else
                        {
                            _abandonWriter.WriteAbandonment(pipe, AbandonmentTable.Pipe_TypeUnknown);
                            return false;
                        }
                    }
                }
                catch
                {
                    _abandonWriter.WriteAbandonment(pipe, AbandonmentTable.Pipe_TypeUnknown);
                    return false;
                }
                if (_pipeType != PipeType.ColdWater && !IsValidMaterial(_material))
                {
                    _abandonWriter.WriteAbandonment(_pipe, AbandonmentTable.Pipe_MatlOOR);
                    return false;
                }

                return true;
            }
            public static void UpdateToPGs()
            {       
                //assert: all materials are valid
                if( (_pipeType == PipeType.SanitaryWater && (_material != _addiInfo.materialTypes[(byte)PGMaterialType.CastIron_FC] && _material != _addiInfo.materialTypes[(byte)PGMaterialType.CastIron_BSC]))
                 || (_pipeType == PipeType.FireSprinkler && (_material != _addiInfo.materialTypes[(byte)PGMaterialType.ThreadedSteel] && _material != _addiInfo.materialTypes[(byte)PGMaterialType.VitaulicSteel])))
                {
                    _abandonWriter.WriteAbandonment(_pipe, AbandonmentTable.Pipe_TypeMateConflict);
                    return;
                }
                if (_pipeType == PipeType.SanitaryWater && _material == _addiInfo.materialTypes[(byte)PGMaterialType.CastIron_FC])
                {
                    if (_addiInfo.defaultSet[(byte)DefaultSet.Pipe_FragilityType] == 0)
                    {
                        _abandonWriter.WriteAbandonment(_pipe, AbandonmentTable.Pipe_MateFragConflict);
                        return;
                    }
                }
                if (  _pipeType != PipeType.ColdWater &&
                    ((_material == _addiInfo.materialTypes[(byte)PGMaterialType.ThreadedSteel] && 2.5 < _diameter)
                  || (_material == _addiInfo.materialTypes[(byte)PGMaterialType.WeldedSteel] && _diameter <= 2.5)))
                {
                    _abandonWriter.WriteAbandonment(_pipe, AbandonmentTable.Pipe_DiameterMateConflict);
                    return;
                }
                string FGCode = _typeMap[(byte)_pipeType];
                if (_pipeType == PipeType.ColdWater)
                {
                    if (_diameter <= 2.5)
                    {
                        _abandonWriter.WriteAbandonment(_pipe, AbandonmentTable.Pipe_DiameterOOR);
                        return;
                    }
                    else
                    {
                        FGCode += "1";
                    }
                }
                else
                {
                    if (_material == _addiInfo.materialTypes[(byte)PGMaterialType.ThreadedSteel])       FGCode += "1";
                    else if (_material == _addiInfo.materialTypes[(byte)PGMaterialType.VitaulicSteel])  FGCode += "1";
                    else if (_material == _addiInfo.materialTypes[(byte)PGMaterialType.WeldedSteel])    FGCode += "2";
                    else if (_material == _addiInfo.materialTypes[(byte)PGMaterialType.CastIron_FC])    FGCode += "1";
                    else if (_material == _addiInfo.materialTypes[(byte)PGMaterialType.CastIron_BSC])   FGCode += "2";
                }

                FGCode += (SDCConverter.Get4LevelIndex(_addiInfo.sdc) + 1).ToString();
                FGCode += ConstSet.Alphabet[_addiInfo.defaultSet[(byte)DefaultSet.Pipe_FragilityType]];
                int index_pipeType = (byte)_pipeType;

                int index;
                if (_dictionary.TryGetValue(FGCode, out index))
                {
                    _PGItems.ElementAt(index).Num[_floor] += _length / _lengthBase;
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = _nameZH[index_pipeType];
                    pgItem.PinYinSuffix = _namePY[index_pipeType];
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
