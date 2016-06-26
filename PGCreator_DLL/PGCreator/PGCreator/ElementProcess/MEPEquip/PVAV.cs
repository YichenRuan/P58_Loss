using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public sealed class PVAV : AMEPEquip
    {
        private sealed class VAVRecognizer : AMEPRecognizer
        {
            public VAVRecognizer(int dicSize = 1) : base(dicSize) { }
            public override bool Recognization(FamilyInstance fi)
            {
                _fi = fi;
                return true;
            }
            public override void UpdateToPGs()
            {
                int sdcIndex = SDCConverter.Get4LevelIndex(_addiInfo.sdc);
                if (2 <= sdcIndex)
                {
                    _abandonWriter.WriteAbandonment(_fi, AbandonmentTable.VAV_SDCNonABC);
                    return;
                }
                else
                {
                    string FGCode = "D3041.041";
                    FGCode += ConstSet.Alphabet[sdcIndex];

                    int index;
                    if (_dictionary.TryGetValue(FGCode, out index))
                    {
                        _PGItems.ElementAt(index).Num[_floor] += 0.1;       //costing per 10 units
                    }
                    else
                    {
                        PGItem pgItem = new PGItem();
                        pgItem.PGName = "变风量箱";
                        pgItem.PinYinSuffix = "BianFengLiangXiang";
                        pgItem.Code = FGCode;
                        pgItem.direction = Direction.Undefined;
                        pgItem.Num[_floor] += 0.1;
                        pgItem.Price = _addiInfo.prices[(byte)PGComponents.VAV];
                        if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                        else pgItem.IfDefinePrice = true;
                        _PGItems.Add(pgItem);
                        _dictionary.Add(FGCode, _PGItems.Count - 1);
                    }
                }
            }
        }
        public PVAV(Document doc, AdditionalInfo addiInfo) : base(doc, addiInfo)
        {
            _PGItems = new List<PGItem>(1);
            _equips = new List<FamilyInstance>(100);
            _mepComp = MEPComponents.VAV;
            _mepRecog = new VAVRecognizer();
        }
    }
}
