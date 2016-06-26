using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public sealed class PLVS : AMEPEquip
    {
        private sealed class LVSRecognizer : AMEPRecognizer
        {
            public LVSRecognizer(int dicSize = 1) : base(dicSize) { }
            public override bool Recognization(FamilyInstance fi)
            {
                _fi = fi;
                if (TryGetFIFloor(_doc)) return true;
                else return false;
            }
            public override void UpdateToPGs()
            {
                int installValue = _addiInfo.defaultSet[(byte)DefaultSet.LVS_Install];
                int capacityValue = _addiInfo.defaultSet[(byte)DefaultSet.LVS_Capacity];
                int dmValue = _addiInfo.defaultSet[(byte)DefaultSet.LVS_DamageMode];
                int factor = 3;
                if (installValue == 0)
                {
                    if (dmValue != 1)
                    {
                        _abandonWriter.WriteAbandonment(_fi, AbandonmentTable.LVS_InsDMConflict);
                        return;
                    }
                    else
                    {
                        --installValue;
                        dmValue = 0;
                        factor = 1;
                    }
                }
                string FGCode = "D5012.02" + (installValue + 2).ToString() + ConstSet.Alphabet[factor * capacityValue + dmValue];

                int index;
                if (_dictionary.TryGetValue(FGCode, out index))
                {
                    _PGItems.ElementAt(index).Num[_floor] += 1.0;
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = "低压开关";
                    pgItem.PinYinSuffix = "DiYaKaiGuan";
                    pgItem.Code = FGCode;
                    pgItem.direction = Direction.Undefined;
                    pgItem.Num[_floor] += 1.0;
                    pgItem.Price = _addiInfo.prices[(byte)PGComponents.LVS];
                    if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                    else pgItem.IfDefinePrice = true;
                    _PGItems.Add(pgItem);
                    _dictionary.Add(FGCode, _PGItems.Count - 1);
                }
            }
        } 
        public PLVS(Document doc, AdditionalInfo addiInfo) : base(doc, addiInfo)
        {
            _PGItems = new List<PGItem>(1);
            _equips = new List<FamilyInstance>(20);
            _mepComp = MEPComponents.LVS;
            _mepRecog = new LVSRecognizer();
        }
    }
}
