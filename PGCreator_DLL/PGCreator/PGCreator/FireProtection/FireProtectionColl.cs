using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using P58_Loss.GlobalLib;

namespace P58_Loss.FireProtection
{
    public class FireProtectionColl
    {
        private class ParaComparer : IComparer<Parameter>
        {
            public int Compare(Parameter x, Parameter y)
            {
                return x.Definition.Name.CompareTo(y.Definition.Name);
            }
        }
        
        private class FamilyNode
        {
            public string familyName;
            private List<FamilyInstance> _fis = new List<FamilyInstance>(_num_instances);
            private static ParaComparer _paraComparer = new ParaComparer();

            public void AddFI(FamilyInstance familyInstance)
            {
                _fis.Add(familyInstance);
            }
            public FamilyNode(FamilyInstance familyInstance)            //auto add the symbol instance
            {
                familyName = familyInstance.Symbol.Family.Name;
                AddFI(familyInstance);
            }
            public string ReportFamily()
            {
                string report = "序号\t族类别";
                {
                    List<Parameter> paraList = new List<Parameter>(_num_paras);               
                    ParameterSet paras1 = _fis.First().Parameters;
                    ParameterSet paras2 = _fis.First().Symbol.Parameters;
                    foreach (Parameter para in paras1)
                    {
                        paraList.Add(para);  
                    }
                    foreach (Parameter para in paras2)
                    {
                        paraList.Add(para);
                    }
                    paraList.Sort(_paraComparer);
                    foreach (Parameter para in paraList)
                    {
                        report += "\t" + para.Definition.Name;
                    }
                    report += "\r\n";
                }
                
                int count = 1;
                foreach(FamilyInstance fi in _fis)
                {
                    report += count++.ToString() + "\t" + fi.Symbol.Name;
                    List<Parameter> paraList = new List<Parameter>(_num_paras);
                    ParameterSet paras1 = fi.Parameters;
                    ParameterSet paras2 = fi.Symbol.Parameters;
                    foreach (Parameter para in paras1)
                    {
                        paraList.Add(para);
                    }
                    foreach (Parameter para in paras2)
                    {
                        paraList.Add(para);
                    }
                    paraList.Sort(_paraComparer);
                    foreach (Parameter para in paraList)
                    {
                        string paraValue = para.AsValueString();
                        if (paraValue == null) paraValue = "<空缺>";
                        report += "\t" + paraValue;
                    }
                    report += "\r\n";
                }

                return report;
            }
        }

        private static class FPMana
        {
            private static List<string> _cateNames = new List<string>(_num_cates);
            private static List<List<FamilyNode>> _cateNodes = new List<List<FamilyNode>>(_num_cates);
            private static int _currNode = -1;
            private static Dictionary<string, int> _currFamilyRecord = new Dictionary<string, int>(_num_families);

            public static void CreateNewCategory(FamilyInstance familyInstance)
            {
                _cateNames.Add(familyInstance.Category.Name);
                _cateNodes.Add(new List<FamilyNode>(_num_families));
                ++_currNode;
                _currFamilyRecord.Clear();
            }
            public static void AddInstance(FamilyInstance familyInstance)
            {
                int index;
                if (_currFamilyRecord.TryGetValue(familyInstance.Symbol.Family.Name, out index))
                {
                    _cateNodes[_currNode][index].AddFI(familyInstance);
                }
                else
                {
                    _cateNodes[_currNode].Add(new FamilyNode(familyInstance));
                    _currFamilyRecord.Add(familyInstance.Symbol.Family.Name, _cateNodes[_currNode].Count - 1);
                }
            }
            public static string GetSoftReport()
            {
                string report = "";
                int num_cate = _cateNames.Count;
                for (int i = 0; i < num_cate; ++i)
                {
                    report += _cateNames[i] + "\t";
                    foreach (FamilyNode familyNode in _cateNodes[i])
                    {
                        report += familyNode.familyName + "\t";
                    }
                    report += "\r\n";
                }
                return report;
            }
            public static string GetHardReport(List<List<int>> indexs_cate)
            {
                string report = "";
                int count_cate = -1;
                foreach (List<int> indexs_fami in indexs_cate)
                {
                    ++count_cate;
                    if (indexs_fami.Count == 0) break;
                    else
                    {
                        report += (count_cate + 1).ToString() + " " + _cateNames[count_cate] + "\r\n";
                        List<FamilyNode> fns = _cateNodes[count_cate];
                        int count_fami = 1;
                        foreach (int index_fami in indexs_fami)
                        {
                            FamilyNode fn = fns[index_fami];
                            report += (count_cate + 1).ToString() + "." + count_fami++.ToString() + " " + fn.familyName + "\r\n";
                            report += fn.ReportFamily();
                        }
                    }  
                }
                return report;
            }
        }

        private static int _num_cates = 3;
        private static int _num_families = 10;
        private static int _num_instances = 10;
        private static int _num_paras = 30;
        private static int _emptyCates = 0;
        private char[] _out2;
        
        private string OutputSoft(Document doc)
        {
            List<FilteredElementCollector> collectors = new List<FilteredElementCollector>(_num_cates);
            for (int i = 0; i < _num_cates; ++i)
            {
                collectors.Add(new FilteredElementCollector(doc));
            }
            ElementClassFilter familyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));
            List<ElementCategoryFilter> filters = new List<ElementCategoryFilter>(_num_cates);
            filters.Add(new ElementCategoryFilter(BuiltInCategory.OST_FireAlarmDevices));
            filters.Add(new ElementCategoryFilter(BuiltInCategory.OST_ElectricalEquipment));
            filters.Add(new ElementCategoryFilter(BuiltInCategory.OST_CableTray));
            for (int i = 0; i < _num_cates; ++i)
            {
                collectors[i].WherePasses(filters[i]).WherePasses(familyInstanceFilter);
                try
                {
                    FPMana.CreateNewCategory((FamilyInstance)collectors[i].First());
                    foreach (FamilyInstance familyInstance in collectors[i])
                    {
                        FPMana.AddInstance(familyInstance);
                    }
                }
                catch
                {
                    //no category-level nodes will be created if an exception is thrown
                    ++_emptyCates;
                }
            }
            return (_num_cates - _emptyCates).ToString() + "\r\n" + FPMana.GetSoftReport();   
        }
        private List<List<int>> InterpretOUT2()
        {
            int num_validCate = _num_cates - _emptyCates;
            List<List<int>> indexs_cate = new List<List<int>>(num_validCate);
            if (_out2 == null) throw new Exception("OUT2 file not found");
            int posi = 0;
            int hot = posi;
            string temp;
            for (int i = 0; i < num_validCate; ++i)
            {
                indexs_cate.Add(new List<int>(_num_families));
                while (_out2[posi] != '\n')
                {
                    if (_out2[posi] == '\t')
                    {
                        temp = new string(_out2, hot, posi - hot);
                        indexs_cate[i].Add(int.Parse(temp));
                        hot = ++posi;
                    }
                    ++posi;
                }
                hot = ++posi;
            }
            return indexs_cate;
        }
        private string OutputHard()
        {
            return FPMana.GetHardReport(InterpretOUT2());
        }

        public void OutputIN2(Document doc, string fileName = "FPTF.IN2")
        {
            IOHelper.Output(OutputSoft(doc), fileName);
        }
        public void InputOUT2(string fileName = "FPTF.OUT2")
        {
            _out2 = IOHelper.Input(fileName);
        }
        public void OutputFP(string fileName = "FireProtection.txt")
        {
            IOHelper.Output(OutputHard(), fileName);
        }
    }
}
