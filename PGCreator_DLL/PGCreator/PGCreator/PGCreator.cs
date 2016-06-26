using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Diagnostics;
using P58_Loss.GlobalLib;
using P58_Loss.ElementProcess;

namespace P58_Loss
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PGCreator : IExternalCommand
    {
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
            MEPHelper.WriteMEPToInFile(doc, ref inFile);
            IOHelper.Output(inFile, "PGCTF.IN");
        }
            
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            ErrorWriter.SetWriter();
            ErrorWriter errorWriter = ErrorWriter.GetWriter();
            //FireProtectionColl fireProColl = new FireProtectionColl();
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
                //fireProColl.OutputIN2(doc);
                Process process = Process.Start(PGPath.exeDirectory + "PGCreator.exe", PGPath.exeDirectory);
                process.WaitForExit();
                char[] outFile = IOHelper.Input("PGCTF.OUT");
                IOHelper.Output("1", "PGCTF.OUT");
                IOHelper.TryHideFile("PGCTF.OUT");
                //Process
                if (outFile[0] == '0')
                {
                    MEPHelper.ReadBinFile();
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

                    if (addiInfo.requiredComp[(byte)PGComponents.Duct])             pgWriter.UpdatePGs(PDuct.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.Pipe])             pgWriter.UpdatePGs(PPipe.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.Chiller])          pgWriter.UpdatePGs((new PChiller(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.CoolingTower])     pgWriter.UpdatePGs((new PCoolingTower(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.Compressor])       pgWriter.UpdatePGs((new PCompressor(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.HVACFan_InLine])   pgWriter.UpdatePGs((new PHVACFan_InLine(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.Diffuser])         pgWriter.UpdatePGs((new PDiffuser(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.VAV])              pgWriter.UpdatePGs((new PVAV(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.HVACFan])          pgWriter.UpdatePGs((new PHVACFan(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.AHU])              pgWriter.UpdatePGs((new PAHU(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.ControlPanel])     pgWriter.UpdatePGs((new PControlPanel(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.FireSprinkler])    pgWriter.UpdatePGs((new PFireSprinkler(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.Transformer])      pgWriter.UpdatePGs((new PTransformer(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.MCC])              pgWriter.UpdatePGs((new PMCC(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.LVS])              pgWriter.UpdatePGs((new PLVS(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.DistPanel])        pgWriter.UpdatePGs((new PDistPanel(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.BatteryRack])      pgWriter.UpdatePGs((new PBatteryRack(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.BatteryCharger])   pgWriter.UpdatePGs((new PBatteryCharger(doc, addiInfo)).GetPG());
                    if (addiInfo.requiredComp[(byte)PGComponents.DieselGen])        pgWriter.UpdatePGs((new PDieselGen(doc, addiInfo)).GetPG());

                    if (addiInfo.requiredComp[(byte)PGComponents.BracedFrame])      pgWriter.UpdatePGs(PBracedFrame.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.SteelBCJoint])     pgWriter.UpdatePGs(PStealBCJoints.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.FlatSlab])         pgWriter.UpdatePGs(PFlatSlab.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.LinkBeam])         pgWriter.UpdatePGs(PLinkBeam.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.Stair])            pgWriter.UpdatePGs(PStair.GetPG(doc, addiInfo));
                    if (addiInfo.requiredComp[(byte)PGComponents.Roof])             pgWriter.UpdatePGs(PRoof.GetPG(doc, addiInfo));
                    normalExit = true;
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
