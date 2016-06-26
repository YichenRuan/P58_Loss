using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public sealed class PFireSprinkler : AMEPEquip
    {
        private sealed class FireSprinkler : AMEPRecognizer
        {
            public FireSprinkler(int dicSize = 1) : base(dicSize) { }
            public override bool Recognization(FamilyInstance fi)
            {
                _fi = fi;
                if (TryGetFIFloor(_doc))
                {
                    --_floor;
                    if (_floor < 0) return false;
                    else return true;
                }
                else return false;
            }
            public override void UpdateToPGs()
            {
                int bracingValue = _addiInfo.defaultSet[(byte)DefaultSet.FireSprinkler_Bracing];
                int ceilingValue = _addiInfo.defaultSet[(byte)DefaultSet.FireSprinkler_Ceilling];
                int sdcIndex = SDCConverter.Get4LevelIndex(_addiInfo.sdc);
                if((bracingValue == 0 && ceilingValue == 1) || (bracingValue == 1 && ceilingValue == 0)
                    || (bracingValue == 1 && 2 <= sdcIndex) || (bracingValue == 2 && sdcIndex <= 1))
                {
                    _abandonWriter.WriteAbandonment(_fi, AbandonmentTable.FireSprinkler_Conflict);
                    return;
                }
                else
                {
                    string FGCode = "D4011.0" + (bracingValue + 3).ToString() + sdcIndex.ToString() + "a";

                    int index;
                    if (_dictionary.TryGetValue(FGCode, out index))
                    {
                        _PGItems.ElementAt(index).Num[_floor] += 0.01;          //costing per 100 units
                    }
                    else
                    {
                        PGItem pgItem = new PGItem();
                        pgItem.PGName = "消防喷淋";
                        pgItem.PinYinSuffix = "XiaoFangPenLin";
                        pgItem.Code = FGCode;
                        pgItem.direction = Direction.Undefined;
                        pgItem.Num[_floor] += 0.01;
                        pgItem.Price = _addiInfo.prices[(byte)PGComponents.FireSprinkler];
                        if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                        else pgItem.IfDefinePrice = true;
                        _PGItems.Add(pgItem);
                        _dictionary.Add(FGCode, _PGItems.Count - 1);
                    }
                }
            }
        }
        public PFireSprinkler(Document doc, AdditionalInfo addiInfo) : base(doc, addiInfo)
        {
            _PGItems = new List<PGItem>(1);
            _equips = new List<FamilyInstance>(200);
            _mepComp = MEPComponents.FireSprinkler;
            _mepRecog = new FireSprinkler();
        }
    }
}
