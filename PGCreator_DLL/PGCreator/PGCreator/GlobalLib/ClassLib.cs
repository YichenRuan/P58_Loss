using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace P58_Loss.GlobalLib
{
    public struct ConstSet
    {
        private static bool[] isSet = { false, false };         //0: angle tol, 1: floor tol
        public static readonly double FeetToMeter = 0.3048;
        public static double AngleTol;
        public static double FloorTol;
        public static readonly double InchToFeet = 1.0 / 12.0;
        public static readonly int MAX_BUFF = 10240;
        public static readonly string[] Alphabet = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l" };
        public static void SetAngleTol(double angleTol)
        {
            if (isSet[0]) return;
            AngleTol = angleTol / 180.0 * Math.PI;
            isSet[0] = true;
        }
        public static void SetFloorTol(double floorTol)
        {
            if (isSet[1]) return;
            FloorTol = floorTol;
            isSet[1] = true;
            MyLevel.SetFloorTol(floorTol);
        }
    }

    public class PGPath
    {
        public static readonly string exeDirectory = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "PGCreator\\";
    }

    public class MyLevel
    {
        private static List<double> _elevations;
        private static List<double> _elevations_adj = null;
        private static int _num;
        private static MyLevel _myLevel = null;
        private static double _aveHeight;
        private static double ErrorCTRL_Offset;

        private MyLevel(FilteredElementCollector levelColls)
        {
            _elevations = new List<double>();

            foreach (Level level in levelColls)
            {
                _elevations.Add(level.Elevation);
            }
            _num = _elevations.Count;
            _elevations.Sort();
        }
        public static void SetFloorTol(double floorTol)
        {
            ErrorCTRL_Offset = floorTol;
        }
        public static void SetMyLevel(FilteredElementCollector levelColls)
        {
            if (_myLevel == null) _myLevel = new MyLevel(levelColls);
        }
        public static MyLevel GetMyLevel()
        {
            return _myLevel;
        }
        public int GetFloor(out bool isFound, Level oriLevel, double offset = 0)
        {                                                                         
            return GetFloor(out isFound, oriLevel.Elevation, offset);
        }
        public int GetFloor(out bool isFound, double oriElevation, double offset = 0)   //Return the next larger item if not found.
        {                                                                               //unsafe for walls, use GetWallHighFloor() instead
            int rank = 0;
            double ele = oriElevation + offset;
            double actualOffset = 0.0;
            isFound = false;
            rank = _elevations_adj.BinarySearch(ele);   //Automatically throw an excepetion if _elevation_adj == null
            if (rank < 0)
            {
                rank = ~rank;                           //C# ref: return the bitwise complement of the next larger item if not found
                if (rank != 0)
                {
                    actualOffset = ele - _elevations_adj[rank - 1];
                    if (actualOffset < _aveHeight * ErrorCTRL_Offset)
                    {
                        --rank;
                        isFound = true;
                    }
                    else if (_aveHeight - actualOffset < _aveHeight * ErrorCTRL_Offset && rank < _num) isFound = true;
                }
                else
                {
                    actualOffset = _elevations_adj[0] - ele;
                    if (actualOffset < _aveHeight * ErrorCTRL_Offset) isFound = true;
                }
            }
            else isFound = true;

            return rank;
        }
        public int GetWallTopFloor(out bool isFound, Level bottomLevel, double bottomOffset, double height)
        {
            double topElevation = bottomLevel.Elevation + bottomOffset + height;
            return GetFloor(out isFound, topElevation);
        }
        public double GetElevation(int levelNo)
        {
            return _elevations_adj.ElementAt(levelNo);
        }
        public static int GetLevelNum()                 //Note: num_floor = num_level - 1
        {
            if (_myLevel == null) throw (new Exception("In ClassLib.MyLevel.GetLevelNum(): MyLevel has not yet initialized"));
            return _num;
        }
        public static void WriteLevelsToInFile(ref string inFile)
        {
            foreach(double ela in _elevations)
            {
                inFile += (ela * ConstSet.FeetToMeter).ToString() + "\t";
            }
            inFile += "\r\n";
        }
        public static void AdjustLevels(AdditionalInfo addiInfo)
        {
            _elevations_adj = new List<double>(_num);
            for (int i = 0; i < _num; ++i)
            {
                if (addiInfo.unCheckedLevel[i] == 0)
                {
                    _elevations_adj.Add(_elevations[i]);
                }            
            }
            _num = _elevations_adj.Count;
            _aveHeight = (_elevations_adj[_num - 1] - _elevations_adj[0]) / (_num - 1);
        }
        public static bool isLegalFloorIndex(int index)
        {
            return 0 <= index && index <= (_num - 2);
        }
    }

    public class ErrorWriter
    {
        private static string _directory;
        private static string _fileName;
        private static ErrorWriter _errorWriter = null;
        private static string _error;
        private ErrorWriter(string dir = null)
        {
            _directory = PGPath.exeDirectory;
            _fileName = @"\ErrorLog.log";
            _error = DateTime.Now.ToString() + "\r\n";
            if (dir != null) _directory = dir;
        }
        public static void SetWriter(string dir = null)
        {
            if (_errorWriter == null) _errorWriter = new ErrorWriter(dir);
        }
        public static ErrorWriter GetWriter()
        {
            return _errorWriter;
        }
        public void WriteError(Exception e)
        {
            _error += e.ToString();
        }
        public void WriteError(string e)
        {
            _error += e;
        }
        public static void Output()
        {
            IOHelper.Output(_error, _fileName, _directory);
            _errorWriter = null;
        }
    }

    public class PGItem
    {
        public string PGName;
        public string Code;
        public Direction direction;
        public bool IfDefinePrice;
        public string PinYinSuffix;
        public double Price;            //USD
        public List<double> Num;        //May not be int
        private static int _num_floor;

        static PGItem()
        {
            _num_floor = MyLevel.GetLevelNum() - 1;
        }
        public PGItem()
        {
            PGName = "\t";
            Code = "\t";
            IfDefinePrice = false;
            PinYinSuffix = "\t";
            Price = 0.0;
            Num = new List<double>(_num_floor);
            for (int i = 0; i < _num_floor; ++i)
            {
                Num.Add(0.0);
            }
        }
    }

    public class PGWriter
    {
        private static string _directory;
        private static string _fileName;
        private static int _num;
        private static PGWriter _PGWriter = null;
        private static string _PGInfo;
        private static AdditionalInfo _addiInfo;

        private PGWriter(string dir = null)
        {
            _directory = PGPath.exeDirectory;
            _fileName = "BuildingPGs";
            _num = 0;
            _PGInfo = "";
            if (dir != null) _directory = dir;
        }
        public static void SetWriter(AdditionalInfo addiInfo)
        {
            if (_PGWriter == null)
            {
                _addiInfo = addiInfo;
                _PGWriter = new PGWriter(_addiInfo.outPath + "\\");
            }
        }
        public static PGWriter GetWriter()
        {
            return _PGWriter;
        }
        public void UpdatePGs(List<PGItem> pgItems)
        {
            foreach (PGItem pgItem in pgItems)
            {
                ++_num;
                _PGInfo += pgItem.PGName + "\t"
                        + pgItem.Code + "\t"
                        + ((byte)pgItem.direction).ToString() + "\t"
                        + ((pgItem.IfDefinePrice)?"1":"0") + "\t"
                        + pgItem.PinYinSuffix + "\t"
                        + string.Format("{0:f}",pgItem.Price) + "\t";
                foreach (double num in pgItem.Num)
                {
                    _PGInfo += num + "\t";
                }
                _PGInfo += "\r\n";
            }
        }
        public static void Output()
        {
            string head = "1\t建筑名称\t" + _addiInfo.bldgName + "\t行数\t" + _num.ToString()
                + "\t层数\t" + (MyLevel.GetLevelNum() - 1).ToString() + "\t\t\t\t\t\r\n"
                + "PG名称\t编码\t方向\t是否自定义价格\t拼音后缀\t单价（美元）\t";
            int i = 1;
            while (i < MyLevel.GetLevelNum())
            {
                head += i.ToString() + "\t";
                ++i;
            }
            head += "\r\n";
            _PGInfo = head + _PGInfo;
            _fileName += "_" + _addiInfo.rvtFileName + ".txt";
            IOHelper.Output(_PGInfo, _fileName, _directory);
            _PGWriter = null;
        }
    }

    public class AbandonmentWriter
    {
        private static string _directory;
        private static string _fileName;
        private static int _num;
        private static AbandonmentWriter _abandonmentWriter;
        private static string _abandonment;
        private static AdditionalInfo _addiInfo = null;

        private AbandonmentWriter(string dir = null)
        {
            _directory = PGPath.exeDirectory;
            _fileName = "AbandonedElements";
            _num = 0;
            _abandonment = null;
            if (dir != null) _directory = dir;
        }
        public static void SetWriter(AdditionalInfo addiInfo)
        {
            if (_abandonmentWriter == null)
            {
                _addiInfo = addiInfo;
                _abandonmentWriter = new AbandonmentWriter(_addiInfo.outPath + "\\");
            }
        }
        public static AbandonmentWriter GetWriter()
        {
            return _abandonmentWriter;
        }
        public void WriteAbandonment(Element ele, AbandonmentTable abonTable)
        {
            string eleName = "N/A";
            string eleId = "N/A";
            if (ele != null)
            {
                eleName = ele.Name.ToString();
                eleId = ele.Id.ToString();
            }
            _abandonment += ++_num + "\t" + eleName + "\t" + eleId + "\t" 
                + ((int)abonTable).ToString().PadLeft(4,'0') + "\r\n";
        }
        public static void Output()
        {
            _fileName += "_" + _addiInfo.rvtFileName + ".txt";
            string head = _addiInfo.bldgName + "\t" + _addiInfo.bldgUse + "\t" + _addiInfo.builtYear + "\r\n";
            _abandonment = head + _abandonment;
            IOHelper.Output(_abandonment, _fileName, _directory);
            _abandonmentWriter = null;
        }
    }

    public class AdditionalInfo
    {
        private static int _num_comp = System.Enum.GetNames(typeof(PGComponents)).Length;
        private static int _num_material = System.Enum.GetNames(typeof(PGMaterialType)).Length;
        private static int _num_setting = System.Enum.GetNames(typeof(DefaultSet)).Length;
        public string outPath;
        public string rvtFileName;
        public string bldgName;
        public string bldgUse;
        public string struType;
        public string builtYear;
        public MomentFrameType mfType;
        public SDC sdc;
        public int[] unCheckedLevel = new int[MyLevel.GetLevelNum()];
        public bool[] requiredComp = new bool[_num_comp];
        public double[] prices = new double[_num_comp];
        public string[] materialTypes = new string[_num_material];
        public int[] defaultSet = new int[_num_setting];
        public AdditionalInfo(char[] outFile)
        {
            /*
            outFile format:
            0th line: 0 + '\r\n'
            1st line: output path + '\r\n'
            2nd line: rvt file name + '\t' + bldg name + '\t' + bldg usage + '\t' + structural type + '\t' + built year + '\t\r\n'
            3rd line: unchecked level index(uli1) + '\t' + uli2 + '\t' + ...(num uncertain) + '\r\n'
                                    uli indicates the index of the corresponding level in MyLevel
            4th line: moment frame type + '\t' + sdc + '\t' + angle tol + '\t' + floor tol + '\t\r\n'...(so far 4) + '\r\n'
                        MFtype: 0-SMF, 1-comfirmedMF, 2-IMF, 3-OMF, 4-uncomfirmedMF
                        SDC: 0-A, 1-B, 2-C, 3-D, 4-E, 5-F, 6-OSHPD
            5th line: checked component(cc1) + '\t' + price1 + '\t' + cc2 + '\t' + price2 + '\t' + ...(num uncertain) + '\r\n'
                        cc: 0-beam column joints, 1-shear wall, 2-gyp walls, 3-curtain walls
                        price: 0.00 if default
            6th line: material type name(mtn1) + '\t' + mtn2 + '\t' + ... + '\r\n'
                        check EnumLib.PGMaterialType for names and sequence
            7th line: default setting(ds1) + '\t' + ds2 + '\t' + ... + '\r\n'
            */
            int i = 3, hot = 3;
            string temp = null;
            int tempIndex = 0;
            //1: output path
            while (outFile[i] != '\r') ++i;
            outPath = new string(outFile, hot, i - hot);
            hot = i += 2;
            //2: basic info
            while (outFile[i] != '\t') ++i;
            rvtFileName = new string(outFile, hot, i - hot);
            hot = ++i;
            while (outFile[i] != '\t') ++i;
            bldgName = new string(outFile, hot, i - hot);
            hot = ++i;
            while (outFile[i] != '\t') ++i;
            bldgUse = new string(outFile, hot, i - hot);
            hot = ++i;
            while (outFile[i] != '\t') ++i;
            struType = new string(outFile, hot, i - hot);
            hot = ++i;
            while (outFile[i] != '\t') ++i;
            builtYear = new string(outFile, hot, i - hot);
            hot = i += 3;
            //3: levels
            if (outFile[i] == '\r')
            {
                hot = i += 2;
            }
            else
            {
                while (outFile[i] != '\r')
                {
                    ++i;
                    if (outFile[i] == '\t')
                    {
                        temp = new string(outFile, hot, i - hot);
                        unCheckedLevel[(int.Parse(temp))] = 1;
                        hot = ++i;
                    }
                }
                hot = i += 2;
            }
            //4: structural info
            //MFType
            switch (outFile[i])
            {
                case '0':
                    mfType = MomentFrameType.SMF;
                    break;
                case '1':
                    mfType = MomentFrameType.confirmedMF;
                    break;
                case '2':
                    mfType = MomentFrameType.IMF;
                    break;
                case '3':
                    mfType = MomentFrameType.OMF;
                    break;
                case '4':
                    mfType = MomentFrameType.unconfirmedMF;
                    break;
                default:
                    ErrorWriter.GetWriter().WriteError("ClassLib.AdditionalInfo: MFType not found.");
                    break;
            }
            //SDC
            i += 2;
            switch (outFile[i])
            {
                case '0':
                    sdc = SDC.A;
                    break;
                case '1':
                    sdc = SDC.B;
                    break;
                case '2':
                    sdc = SDC.C;
                    break;
                case '3':
                    sdc = SDC.D;
                    break;
                case '4':
                    sdc = SDC.E;
                    break;
                case '5':
                    sdc = SDC.F;
                    break;
                case '6':
                    sdc = SDC.OSHPD;
                    break;
                default:
                    ErrorWriter.GetWriter().WriteError("ClassLib.AdditionalInfo: SDC not found.");
                    break;
            }
            hot = i += 2;
            while (outFile[i] != '\t') ++i;
            ConstSet.SetAngleTol(Double.Parse(new string(outFile, hot, i - hot)));
            hot = ++i;
            while (outFile[i] != '\t') ++i;
            ConstSet.SetFloorTol(Double.Parse(new string(outFile, hot, i - hot)));  
            hot = i += 3;
            //5: component
            if (outFile[i] != '\r')
            {
                while (outFile[i] != '\r')
                {
                    ++i;
                    if (outFile[i] == '\t')
                    {
                        temp = new string(outFile, hot, i - hot);
                        tempIndex = int.Parse(temp);
                        requiredComp[tempIndex] = true;
                        hot = ++i;
                        while (outFile[i] != '\t') ++i;
                        temp = new string(outFile, hot, i - hot);
                        prices[tempIndex] = double.Parse(temp);
                        hot = ++i;
                    }
                }
            }
            //6: material type
            hot = i += 2;
            int count_material = 0;
            while (outFile[i] != '\r')
            {
                ++i;
                if (outFile[i] == '\t')
                {
                    materialTypes[count_material++] = new string(outFile, hot, i - hot);
                    hot = ++i;
                }
            }
            //7: default setting
            hot = i += 2;
            int count_setting = 0;
            while (outFile[i] != '\r')
            {
                ++i;
                if (outFile[i] == '\t')
                {
                    defaultSet[count_setting++] = int.Parse(new string(outFile, hot, i - hot));
                    hot = ++i;
                }
            }
            //End
        }
    }

    public static class IOHelper
    {
        private static FileStream _lastFs;
        private static BinaryReader _lastBr;
        private static string _lastFileName;

        public static bool TryHideFile(string fileName, string directory)
        {
            try 
            { 
                File.SetAttributes(directory + fileName, FileAttributes.Hidden);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool TryHideFile(string fileName)
        {
            return TryHideFile(fileName, PGPath.exeDirectory);
        }
        public static void Output(string content, string fileName, string directory)
        {
            try { File.SetAttributes(directory + fileName, FileAttributes.Normal); }
            catch { }
            FileStream fs = new FileStream(directory + fileName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);           //Write in ANSI
            sw.Write(content);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        public static void Output(string content, string fileName)
        {
            Output(content, fileName, PGPath.exeDirectory);
        }
        public static char[] Input(string fileName)
        {
            Stream instream = File.OpenRead(PGPath.exeDirectory + fileName);
            BufferedStream bfs = new BufferedStream(instream);
            byte[] buffer = new byte[ConstSet.MAX_BUFF];
            bfs.Read(buffer, 0, buffer.Length);
            bfs.Close();
            instream.Close();
            File.SetAttributes(PGPath.exeDirectory + fileName, FileAttributes.Hidden);
            return System.Text.Encoding.Default.GetString(buffer).ToCharArray();
        }
        public static BinaryReader BInput(string fileName)
        {
            _lastFileName = fileName;
            _lastFs = new FileStream(PGPath.exeDirectory + fileName, FileMode.Open);
            _lastBr = new BinaryReader(_lastFs);
            return _lastBr;
        }
        public static void BCloseLast()
        {
            File.SetAttributes(PGPath.exeDirectory + _lastFileName, FileAttributes.Hidden);
            _lastBr.Close();
            _lastFs.Close();
        }
    }

    public class MEPId
    {
        public List<BuiltInCategory> builtInCategories = new List<BuiltInCategory>(1);
        public List<ElementType> elementTypes = new List<ElementType>(1);
    }

    public class MEPHelper
    {
        private class Node
        {
            private class TypeComparer : IComparer<ElementType>
            {
                public int Compare(ElementType x, ElementType y)
                {
                    return x.FamilyName.CompareTo(y.FamilyName);
                }
            }
            private static TypeComparer _typeComparer = new TypeComparer();
            public BuiltInCategory cate;
            public List<ElementType> types = new List<ElementType>(5);
            public string cateName;
            public bool empty = false;
            
            public Node(BuiltInCategory category)
            {
                cate = category;
                FilteredElementCollector symbolColl = new FilteredElementCollector(_doc);
                types = symbolColl.OfCategory(cate).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();
                try
                {
                    cateName = types.First().Category.Name;
                    types.Sort(_typeComparer);
                }
                catch(InvalidOperationException e)
                {
                    empty = true;
                }
            }
        }

        private static Document _doc;
        private static BuiltInCategory[] _cates = 
            { BuiltInCategory.OST_ElectricalEquipment, BuiltInCategory.OST_DuctTerminal, BuiltInCategory.OST_PipeAccessory, BuiltInCategory.OST_Sprinklers, BuiltInCategory.OST_CableTray, BuiltInCategory.OST_MechanicalEquipment};
        private static List<Node> _listNode = new List<Node>(_cates.Length);
        private static List<int[]>[] _map = new List<int[]>[System.Enum.GetNames(typeof(MEPComponents)).Length];

        static MEPHelper()
        {
            int num = System.Enum.GetNames(typeof(MEPComponents)).Length;
            for (int i = 0; i < num; ++i)
            {
                _map[i] = new List<int[]>(1);
            }
        }

        private static void CreateDataBase()
        {
            foreach (BuiltInCategory cate in _cates)
            {
                Node temp = new Node(cate);
                if (temp.empty) continue;           //assert: all nodes in _listNode are non-empty
                else _listNode.Add(temp);
            }
        }

        private static string GetReport()
        {
            string report = "";
            report += _listNode.Count.ToString() + "\r\n";
            foreach (Node node in _listNode)
            {
                report += node.cateName + "\t";
                foreach (ElementType type in node.types)
                {
                    string familyName = type.FamilyName;
                    if (familyName == type.Name) familyName = "";
                    else familyName += ":";
                    report += familyName + type.Name + "\t";
                }
                report += "\r\n";
            }
            
            return report;
        }

        public static void WriteMEPToInFile(Document doc, ref string inFile)
        {
            _doc = doc;
            CreateDataBase();
            inFile += GetReport();
        }

        public static void ReadBinFile(string fileName = "MEPTF.dat")
        {
            BinaryReader br = IOHelper.BInput(fileName);
            while (true)
            {
                try
                {
                    int mepCode = br.ReadInt32();
                    int[] temp = new int[2];
                    temp[0] = br.ReadInt32();
                    temp[1] = br.ReadInt32();
                    _map[mepCode].Add(temp);
                }
                catch (EndOfStreamException e)
                {
                    break;
                }
            }
            IOHelper.BCloseLast();
        }

        public static MEPId GetId(MEPComponents mepComponent)
        {
            MEPId mepId = new MEPId();
            List<int[]> index = _map[(byte)mepComponent];
            if (index.Count == 0)
            {
                throw new Exception("No such MEP Comp is found");
            }
            foreach (int[] bundle in index)
            {
                mepId.builtInCategories.Add(_listNode[bundle[0]].cate);
                mepId.elementTypes.Add(_listNode[bundle[0]].types[bundle[1]]);
            }
            return mepId;
        }
    }

    public class SDCConverter
    {
        private static int[] _map = { 0, 0, 1, 2, 2, 2, 3 };
        public static int Get4LevelIndex(SDC sdc)
        {
            return _map[(byte)sdc];
        }
    }
}
