using System;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.IO;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PCeiling
    {
        private static class CeilingRecognizer
        {
            private static Ceiling _ceiling;
            private static double _area;
            private static Level _level;
            private static double _offset;
            private static int _floor;
            private static double _areaBase;
            private static Dictionary<string, int> _dictionary = new Dictionary<string, int>(13);       //Total num of FGs = 12
            private static int _num_Rece = 0;
            private static int _num_Pend = 0;
            private static bool[] isFoundLighting = {false,false};

            private static PGItem _rece, _pend;

            static CeilingRecognizer()
            {
                if (_addiInfo.requiredComp[(byte)PGComponents.CeilingLighting])
                {
                    _rece = new PGItem();
                    _rece.Code = "C3033.001";
                    _rece.PGName = "嵌入灯";
                    _rece.PinYinSuffix = "QianRuDeng";
                    _rece.direction = Direction.Undefined;
                    _rece.Price = _addiInfo.prices[(byte)PGComponents.CeilingLighting];
                    if (_rece.Price == 0.0) _rece.IfDefinePrice = false;
                    else _rece.IfDefinePrice = true;

                    _pend = new PGItem();
                    _pend.Code = "C3034.001";
                    _pend.PGName = "吊灯";
                    _pend.PinYinSuffix = "DiaoDeng";
                    _pend.direction = Direction.Undefined;
                    _pend.Price = _addiInfo.prices[(byte)PGComponents.CeilingLighting];
                    if (_pend.Price == 0.0) _pend.IfDefinePrice = false;
                    else _pend.IfDefinePrice = true;
                }
            }

            public static bool Recognization(Ceiling ceiling)
            {
                _ceiling = ceiling;
                _area = ceiling.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble();
                _level = _doc.GetElement(ceiling.get_Parameter(BuiltInParameter.LEVEL_PARAM).AsElementId()) as Level;
                _offset = ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).AsDouble();
                bool isFound;
                _floor = _myLevel.GetFloor(out isFound, _level, _offset) - 1;
                if (_floor == MyLevel.GetLevelNum() || _floor == -1)
                {
                    _abandonWriter.WriteAbandonment(ceiling, AbandonmentTable.CeilingLevelError);
                    return false;
                }
                return true;
            }
            public static void UpdateToPGs()
            {

                if (_addiInfo.requiredComp[(byte)PGComponents.Ceiling])
                {
                    string FGCode = "C3032.";
                    if (_addiInfo.sdc == SDC.A || _addiInfo.sdc == SDC.B) FGCode += "001";
                    else if (_addiInfo.sdc == SDC.C) FGCode += "002";
                    else FGCode += "004";

                    if (_area < 250)
                    {
                        FGCode += "a";
                        _areaBase = 250;
                    }
                    else if (250 < _area && _area < 1000)
                    {
                        FGCode += "b";
                        _areaBase = 600;
                    }
                    else if (1000 < _area && _area < 2500)
                    {
                        FGCode += "c";
                        _areaBase = 1800;
                    }
                    else
                    {
                        FGCode += "d";
                        _areaBase = 2500;
                    }

                    int index;
                    if (_dictionary.TryGetValue(FGCode, out index))
                    {
                        _PGItems.ElementAt(index).Num[_floor] += _area / _areaBase;
                    }
                    else
                    {
                        PGItem pgItem = new PGItem();
                        pgItem.PGName = "天花板";
                        pgItem.PinYinSuffix = "TianHuaBan";
                        pgItem.Code = FGCode;
                        pgItem.direction = Direction.Undefined;
                        pgItem.Num[_floor] += _area / _areaBase;
                        pgItem.Price = _addiInfo.prices[(byte)PGComponents.Ceiling];
                        if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                        else pgItem.IfDefinePrice = true;
                        _PGItems.Add(pgItem);
                        _dictionary.Add(FGCode, _PGItems.Count - 1);
                    }
                }

                if (_addiInfo.requiredComp[(byte)PGComponents.CeilingLighting])
                {
                    FilteredElementCollector lightingCollector = new FilteredElementCollector(_doc);
                    BoundingBoxXYZ bbCeiling = _ceiling.get_BoundingBox(_doc.ActiveView);
                    Outline ceilingOutline = new Outline(bbCeiling.Min, bbCeiling.Max);
                    BoundingBoxIntersectsFilter bbFilter = new BoundingBoxIntersectsFilter(ceilingOutline);
                    ElementCategoryFilter lightingFilter = new ElementCategoryFilter(BuiltInCategory.OST_LightingFixtures);
                    lightingCollector.WherePasses(bbFilter).WherePasses(lightingFilter);
                    foreach (Element lighting in lightingCollector)
                    {
                        BoundingBoxXYZ bbLighting = lighting.get_BoundingBox(_doc.ActiveView);
                        if (bbLighting.Max.Z - bbLighting.Min.Z < ErrorCtrl_Lighting)
                        {
                            ++_num_Rece;
                            isFoundLighting[0] = true;
                        }
                        else
                        {
                            ++_num_Pend;
                            isFoundLighting[1] = true;
                        }
                    }

                    _rece.Num[_floor] += _num_Rece;
                    _pend.Num[_floor] += _num_Pend;

                    _num_Rece = 0;
                    _num_Pend = 0;
                }
            }
            public static void AddLighting()
            {
                if(isFoundLighting[0]) _PGItems.Add(_rece);
                if (isFoundLighting[1]) _PGItems.Add(_pend);
            }
        }

        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static List<Element> _Ceiling;

        private static readonly double ErrorCtrl_Lighting = 0.5 / ConstSet.FeetToMeter;
        
        private static void ExtractObjects()
        {
            FilteredElementCollector CeilingCollector = new FilteredElementCollector(_doc);
            ElementClassFilter CeilingFilter = new ElementClassFilter(typeof(Ceiling));
            _Ceiling = CeilingCollector.WherePasses(CeilingFilter).ToList();
        }
        private static void Process()
        {
            foreach (Ceiling ceiling in _Ceiling)
            {
                if (CeilingRecognizer.Recognization(ceiling))
                    CeilingRecognizer.UpdateToPGs();
            }
            if (_addiInfo.requiredComp[(byte)PGComponents.CeilingLighting]) CeilingRecognizer.AddLighting();
        }
        
        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _PGItems = new List<PGItem>(4);
            _Ceiling = new List<Element>(20);

            ExtractObjects();
            Process();

            return _PGItems;
        }
    }
}
