using System;
using System.Threading;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using P58_Loss.GlobalLib;
using P58_Loss.ElementProcess;

namespace P58_Loss
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PGCreator : IExternalCommand
    {
        private static readonly int MAX_BUFF = 10240;           //10 KB
        private bool normalExit = false;
        
        private void DoOutput(Document doc)
        {
            /*
            inFile format:
            1st line: 0 + "\r\n"
            2nd line: rvt file name + "\t" + bldg name + "\t\r\n"
            3rd line: level(l1) + "\t" + l2 + "\t" + ... + "\r\n"
            */
            string inFile = "0\r\n";
            string title = doc.Title.Remove(doc.Title.Length - 4).Replace("\r\n", "");
            if (15 < title.Length) title = title.Remove(15);
            string bldgName = doc.ProjectInformation.BuildingName.Replace("\r\n", "");
            if (15 < bldgName.Length) title = bldgName.Remove(15);
            inFile += title + "\t"
                   + bldgName + "\t\r\n";
            MyLevel.WriteLevelsToInFile(ref inFile);
            try { File.SetAttributes(PGPath.exeDirection + "PGCTF.IN", FileAttributes.Normal); }
            catch { }

            FileStream fs = new FileStream(PGPath.exeDirection + "PGCTF.IN", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);           //Write in ANSI
            sw.Write(inFile);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        private char[] DoInput(string fileName)
        {
            Stream instream = File.OpenRead(PGPath.exeDirection + fileName);
            BufferedStream bfs = new BufferedStream(instream);
            byte[] buffer = new byte[MAX_BUFF];
            bfs.Read(buffer, 0, buffer.Length);
            bfs.Close();
            instream.Close();
            File.SetAttributes(PGPath.exeDirection + fileName, FileAttributes.Hidden);
            return System.Text.Encoding.Default.GetString(buffer).ToCharArray();
        }   
            
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            ErrorWriter.SetWriter();
            ErrorWriter errorWriter = ErrorWriter.GetWriter();
            try
            {
                //Get Doc
                UIDocument uidoc = revit.Application.ActiveUIDocument;
                Document doc = uidoc.Document;
                //Set Level
                FilteredElementCollector LevelCollector = new FilteredElementCollector(doc);
                ElementFilter LevelFilter = new ElementClassFilter(typeof(Level));
                LevelCollector.WherePasses(LevelFilter);
                MyLevel.SetMyLevel(LevelCollector);
                //IO
                DoOutput(doc);
                Process process = Process.Start(PGPath.exeDirection + "PGCreator.exe");
                process.WaitForExit();
                char[] outFile = DoInput("PGCTF.OUT");
                //Process
                if (outFile[0] == '0')
                {
                    AdditionalInfo addiInfo = new AdditionalInfo(outFile);
                    MyLevel.AdjustLevels(addiInfo);
                    PGWriter.SetWriter(addiInfo);
                    AbandonmentWriter.SetWriter(addiInfo);
                    PGWriter pgWriter = PGWriter.GetWriter();

                    if (addiInfo.requiredComp[(byte)PGComponents.BeamColumnJoint])  pgWriter.UpdatePGs(PBeamColumnJoints.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.ShearWall])        pgWriter.UpdatePGs(PShearWall.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.GypWall]
                     || addiInfo.requiredComp[(byte)PGComponents.WallFinish])       pgWriter.UpdatePGs(PGypWall.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.CurtainWall])      pgWriter.UpdatePGs(PCurtainWall.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.Storefront])       pgWriter.UpdatePGs(PStorefront.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.Ceiling]
                     || addiInfo.requiredComp[(byte)PGComponents.CeilingLighting])  pgWriter.UpdatePGs(PCeiling.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.MasonryWall])      pgWriter.UpdatePGs(PMasonryWall.GetPG(doc, addiInfo));
                    normalExit = true;
                    if (addiInfo.requiredComp[(byte)PGComponents.Duct]) pgWriter.UpdatePGs(PDuct.GetPG(doc, addiInfo));
                }
 
                //Test
                FilteredElementCollector coll = new FilteredElementCollector(doc);
                ElementClassFilter filter = new ElementClassFilter(typeof(FamilySymbol));
                coll.WherePasses(filter);
                foreach (FamilySymbol fi in coll)
                {
                    errorWriter.WriteError(fi.FamilyName + "\r\n");
                }


            }
            catch (Exception e)
            {
                errorWriter.WriteError(e);
                normalExit = false;
                TaskDialog.Show("PGCreator", "未能正确导出性能组，请与软件提供者联系");
            }
            finally
            {
                ErrorWriter.Output();
                if (normalExit)
                {
                    AbandonmentWriter.Output();
                    PGWriter.Output();
                    TaskDialog.Show("PGCreator", "性能组导出成功!");
                }
            }
            return Result.Succeeded;
        }
    }
}
