using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public sealed class PMCC : AMEPEquip
    {
        private sealed class MCCRecognizer : AMEPRecognizer
        {
            public MCCRecognizer(int dicSize = 1) : base(dicSize) { }
            public override bool Recognization(FamilyInstance fi)
            {
                _fi = fi;
                if (TryGetFIFloor(_doc)) return true;
                else return false;
            }
            public override void UpdateToPGs()
            {
                int installValue = _addiInfo.defaultSet[(byte)DefaultSet.MCC_Install];
                int dmValue = _addiInfo.defaultSet[(byte)DefaultSet.MCC_DamageMode];
                if (installValue == 0)
                {
                    if (dmValue != 1)
                    {
                        _abandonWriter.WriteAbandonment(_fi, AbandonmentTable.MCC_InsDMConflict);
                        return;
                    }
                    else dmValue = 0;
                }
                string FGCode = "D5012.013";
                FGCode += ConstSet.Alphabet[installValue + dmValue];

                int index;
                if (_dictionary.TryGetValue(FGCode, out index))
                {
                    _PGItems.ElementAt(index).Num[_floor] += 1.0;
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = "电动机控制中心";
                    pgItem.PinYinSuffix = "DianDongJiKongZhiZhongXin";
                    pgItem.Code = FGCode;
                    pgItem.direction = Direction.Undefined;
                    pgItem.Num[_floor] += 1.0;
                    pgItem.Price = _addiInfo.prices[(byte)PGComponents.MCC];
                    if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                    else pgItem.IfDefinePrice = true;
                    _PGItems.Add(pgItem);
                    _dictionary.Add(FGCode, _PGItems.Count - 1);
                }
            }
        }
        public PMCC(Document doc, AdditionalInfo addiInfo) : base(doc, addiInfo)
        {
            _PGItems = new List<PGItem>(1);
            _equips = new List<FamilyInstance>(20);
            _mepComp = MEPComponents.MCC;
            _mepRecog = new MCCRecognizer();
        }
    }
}
