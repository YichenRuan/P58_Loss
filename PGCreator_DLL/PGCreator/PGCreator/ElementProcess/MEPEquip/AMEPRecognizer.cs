using Autodesk.Revit.DB;
using System.Collections.Generic;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public abstract class AMEPRecognizer
    {
        protected FamilyInstance _fi;
        protected int _floor;
        protected static Dictionary<string, int> _dictionary;

        public AMEPRecognizer(int dicSize = 1)
        {
            _dictionary = new Dictionary<string, int>(dicSize);
        }
        public abstract bool Recognization(FamilyInstance fi);
        public abstract void UpdateToPGs();
        protected bool TryGetFIFloor(Document doc)
        {
            Level level = doc.GetElement(_fi.LevelId) as Level;
            double offset = _fi.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).AsDouble();
            bool isFound;
            _floor = MyLevel.GetMyLevel().GetFloor(out isFound, level, offset);
            if (MyLevel.GetLevelNum() <= _floor)
            {
                AbandonmentWriter.GetWriter().WriteAbandonment(_fi, AbandonmentTable.LevelOutOfRoof);
                return false;
            }
            else return true;
        }
    }
}
