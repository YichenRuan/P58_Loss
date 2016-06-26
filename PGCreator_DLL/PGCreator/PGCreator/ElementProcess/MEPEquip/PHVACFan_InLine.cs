using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public sealed class PHVACFan_InLine : AMEPEquip
    {
        private sealed class HVACFanILRecognizer : AMEPRecognizer
        {
            public HVACFanILRecognizer(int dicSize = 1) : base(dicSize) { }
            public override bool Recognization(FamilyInstance fi)
            {
                _fi = fi;
                if (TryGetFIFloor(_doc)) return true;
                else return false;
            }
            public override void UpdateToPGs()
            {
                string FGCode = "D3041.00";
                FGCode += (_addiInfo.defaultSet[(byte)DefaultSet.HVACFan_InLine_Install] + 1).ToString();
                int sdcIndex = SDCConverter.Get4LevelIndex(_addiInfo.sdc);
                FGCode += ConstSet.Alphabet[sdcIndex];

                int index;
                if (_dictionary.TryGetValue(FGCode, out index))
                {
                    _PGItems.ElementAt(index).Num[_floor] += 0.1;       //costing per 10 units
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = "管道风机";
                    pgItem.PinYinSuffix = "GuanDaoFengJi";
                    pgItem.Code = FGCode;
                    pgItem.direction = Direction.Undefined;
                    pgItem.Num[_floor] += 0.1;
                    pgItem.Price = _addiInfo.prices[(byte)PGComponents.HVACFan_InLine];
                    if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                    else pgItem.IfDefinePrice = true;
                    _PGItems.Add(pgItem);
                    _dictionary.Add(FGCode, _PGItems.Count - 1);
                }
            }
        }
        public PHVACFan_InLine(Document doc, AdditionalInfo addiInfo) : base(doc, addiInfo)
        {
            _PGItems = new List<PGItem>(1);
            _equips = new List<FamilyInstance>(100);
            _mepComp = MEPComponents.HVACFan_InLine;
            _mepRecog = new HVACFanILRecognizer();
        }
    }
}
