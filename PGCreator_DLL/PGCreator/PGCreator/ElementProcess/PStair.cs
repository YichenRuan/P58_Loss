using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using P58_Loss.GlobalLib;

namespace P58_Loss.ElementProcess
{
    public static class PStair
    {    
        private static class StairRecognizer
        {
            private static int _floor;
            private static Direction _direction;
            private static int ds_matl;
            private static int ds_joint;

            private static Dictionary<string, int> _dictionary = new Dictionary<string, int>(11);       //num = 10

            static StairRecognizer()
            {
                ds_matl = _addiInfo.defaultSet[(byte)DefaultSet.Stair_Matl];
                ds_joint = _addiInfo.defaultSet[(byte)DefaultSet.Stair_Joint];
                if (ds_matl == 0 && ds_joint <= 1)
                {
                    _abandonWriter.WriteAbandonment(null, AbandonmentTable.Stair_MatlJointConflict);
                    throw new Exception();
                }
            }
            private static Direction GetDirection(Stairs stair)
            {
                ICollection<ElementId> runIds = stair.GetStairsRuns();
                StairsRun sr = (StairsRun)_doc.GetElement(runIds.First());
                BoundingBoxXYZ bbXYZ = sr.get_BoundingBox(_doc.ActiveView);
                double lx = bbXYZ.Max.X - bbXYZ.Min.X;
                double ly = bbXYZ.Max.Y - bbXYZ.Min.Y;
                if (lx < ly) return Direction.Y;
                else return Direction.X;
            }
            public static bool Recognization(Stairs stair)
            {
                Level level = (Level)_doc.GetElement(stair.get_Parameter(BuiltInParameter.STAIRS_BASE_LEVEL_PARAM).AsElementId());
                double offset = stair.get_Parameter(BuiltInParameter.STAIRS_BASE_OFFSET).AsDouble();
                bool isFound;
                _floor = _myLevel.GetFloor(out isFound, level, offset);
                if (!isFound)
                {
                    _abandonWriter.WriteAbandonment(stair, AbandonmentTable.LevelNotFound);
                    return false;
                }
                _direction = GetDirection(stair);
                return true;
            }
            public static void UpdateToPGs()
            {
                string FGCode = "C2011.0";
                int temp = ds_matl == 2 ? ds_joint - 2 : ds_joint;
                FGCode += ds_matl.ToString() + "1" + ConstSet.Alphabet[temp];

                int index;
                if (_dictionary.TryGetValue(FGCode + _direction.ToString(), out index))
                {
                    _PGItems.ElementAt(index).Num[_floor] += 1.0;
                }
                else
                {
                    PGItem pgItem = new PGItem();
                    pgItem.PGName = "楼梯";
                    pgItem.PinYinSuffix = "LouTi";
                    pgItem.Code = FGCode;
                    pgItem.direction = _direction;
                    pgItem.Num[_floor] += 1.0;
                    pgItem.Price = _addiInfo.prices[(byte)PGComponents.Stair];
                    if (pgItem.Price == 0.0) pgItem.IfDefinePrice = false;
                    else pgItem.IfDefinePrice = true;
                    _PGItems.Add(pgItem);
                    _dictionary.Add(FGCode + _direction.ToString(), _PGItems.Count - 1);
                }
            }
        }
    
        private static Document _doc;
        private static AdditionalInfo _addiInfo;
        private static MyLevel _myLevel;
        private static AbandonmentWriter _abandonWriter;
        private static List<PGItem> _PGItems;
        private static List<Stairs> _stairs;

        private static void ExtractObjects()
        {
            FilteredElementCollector fec = new FilteredElementCollector(_doc);
            _stairs = fec.OfClass(typeof(Stairs)).OfCategory(BuiltInCategory.OST_Stairs).Cast<Stairs>().ToList();
        }
        private static void Process()
        {
            try
            {
                foreach (Stairs stair in _stairs)
                {
                    if (StairRecognizer.Recognization(stair))
                        StairRecognizer.UpdateToPGs();
                }
            }
            catch (Exception e) { }
        }

        public static List<PGItem> GetPG(Document doc, AdditionalInfo addiInfo)
        {
            _doc = doc;
            _addiInfo = addiInfo;
            _myLevel = MyLevel.GetMyLevel();
            _abandonWriter = AbandonmentWriter.GetWriter();
            _stairs = new List<Stairs>(20);
            _PGItems = new List<PGItem>(10);
            ExtractObjects();
            Process();
            return _PGItems;
        }
    }
}
