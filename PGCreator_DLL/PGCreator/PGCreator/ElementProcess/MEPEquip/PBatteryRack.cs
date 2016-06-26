using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public sealed class PBatteryRack : AMEPEquip
    {
        private sealed class BatteryRackRecognizer : AMEPRecognizer
        {
            public BatteryRackRecognizer(int dicSize = 1) : base(dicSize) { }
            public override bool Recognization(FamilyInstance fi)
            {
                _fi = fi;
                if (TryGetFIFloor(_doc)) return true;
                else return false;
            }
            public override void UpdateToPGs()
            {
                int installValue = _addiInfo.defaultSet[(byte)DefaultSet.BatteryRack_Install];
                int dmValue = _addiInfo.defaultSet[(byte)DefaultSet.BatteryRack_DamageMode];
                if (installValue == 0)
                {
                    if (dmValue != 1)
                    {
                        _abandonWriter.WriteAbandonment(_fi, AbandonmentTable.BatteryRack_InsDMConflict);
                        return;
                    }
                    else
                    {
                        --installValue;
                        dmValue = 0;
                    }
                }
                string FGCode = "D5092.01";
                FGCode += (installValue + 2).ToString();
                FGCode += ConstSet.Alphabet[dmValue];

                int index;
                if (_dictionary.TryGetValue(FGCode, out index))
                {
                    _PGItems.ElementAt(index).Num[_floor] += 1.0;
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = "电池架";
                    pgItem.PinYinSuffix = "DianChiJia";
                    pgItem.Code = FGCode;
                    pgItem.direction = Direction.Undefined;
                    pgItem.Num[_floor] += 1.0;
                    pgItem.Price = _addiInfo.prices[(byte)PGComponents.BatteryRack];
                    if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                    else pgItem.IfDefinePrice = true;
                    _PGItems.Add(pgItem);
                    _dictionary.Add(FGCode, _PGItems.Count - 1);
                }
            }
        }
        public PBatteryRack(Document doc, AdditionalInfo addiInfo) : base(doc, addiInfo)
        {
            _PGItems = new List<PGItem>(1);
            _equips = new List<FamilyInstance>(20);
            _mepComp = MEPComponents.BatteryRack;
            _mepRecog = new BatteryRackRecognizer();
        }
    }
}
