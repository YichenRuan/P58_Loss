using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public sealed class PDistPanel : AMEPEquip
    {
        private sealed class DistPanelRecognizer : AMEPRecognizer
        {
            public DistPanelRecognizer(int dicSize = 1) : base(dicSize) { }
            public override bool Recognization(FamilyInstance fi)
            {
                _fi = fi;
                if (TryGetFIFloor(_doc)) return true;
                else return false;
            }
            public override void UpdateToPGs()
            {
                int installValue = _addiInfo.defaultSet[(byte)DefaultSet.DistPanel_Install];
                int capacityValue = _addiInfo.defaultSet[(byte)DefaultSet.DistPanel_Capacity];
                int dmValue = _addiInfo.defaultSet[(byte)DefaultSet.DistPanel_DamageMode];
                int factor = 3;
                if (installValue == 0)
                {
                    if (dmValue != 1)
                    {
                        _abandonWriter.WriteAbandonment(_fi, AbandonmentTable.DistPanel_InsDMConflict);
                        return;
                    }
                    else
                    {
                        --installValue;
                        dmValue = 0;
                        factor = 1;
                    }
                }
                string FGCode = "D5012.03" + (installValue + 2).ToString() + ConstSet.Alphabet[factor * capacityValue + dmValue];

                int index;
                if (_dictionary.TryGetValue(FGCode, out index))
                {
                    _PGItems.ElementAt(index).Num[_floor] += 1.0;
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = "配电盘";
                    pgItem.PinYinSuffix = "PeiDianPan";
                    pgItem.Code = FGCode;
                    pgItem.direction = Direction.Undefined;
                    pgItem.Num[_floor] += 1.0;
                    pgItem.Price = _addiInfo.prices[(byte)PGComponents.DistPanel];
                    if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                    else pgItem.IfDefinePrice = true;
                    _PGItems.Add(pgItem);
                    _dictionary.Add(FGCode, _PGItems.Count - 1);
                }
            }
        }
        public PDistPanel(Document doc, AdditionalInfo addiInfo) : base(doc, addiInfo)
        {
            _PGItems = new List<PGItem>(1);
            _equips = new List<FamilyInstance>(20);
            _mepComp = MEPComponents.DistPanel;
            _mepRecog = new DistPanelRecognizer();
        }
    }
}
