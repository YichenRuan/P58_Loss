using System.Linq;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public abstract class AMEPEquip
    {
        protected static Document _doc;
        protected static AdditionalInfo _addiInfo;
        protected static MyLevel _myLevel;
        protected static AbandonmentWriter _abandonWriter;
        protected static List<PGItem> _PGItems;
        protected static List<FamilyInstance> _equips;
        protected static MEPComponents _mepComp;
        protected AMEPRecognizer _mepRecog;

        protected virtual void ExtractObjects(MEPComponents mepComp)
        {
            MEPId mepId = null;
            int num = 0;
            try
            {
                mepId = MEPHelper.GetId(mepComp);
                num = mepId.builtInCategories.Count;
            }
            catch { }
            for (int i = 0; i < num; ++i)
            {
                BuiltInCategory bic = mepId.builtInCategories[i];
                ElementType et = mepId.elementTypes[i];
                FilteredElementCollector coll = new FilteredElementCollector(_doc);
                FamilyInstanceFilter fif = new FamilyInstanceFilter(_doc, et.Id);
                _equips.AddRange(coll.OfCategory(bic).WherePasses(fif).Cast<FamilyInstance>().ToList());
            }
        }
        protected virtual void Process()
        {
            foreach (FamilyInstance fi in _equips)
            {
                if (_mepRecog.Recognization(fi))
                    _mepRecog.UpdateToPGs();
            }
        }

        public AMEPEquip(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _PGItems = null;
            _equips = null;
            _mepRecog = null;
        }
        public List<PGItem> GetPG()
        {
            ExtractObjects(_mepComp);
            Process();
            return _PGItems;
        }
    }
}
