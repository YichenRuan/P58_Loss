using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using System.IO;

namespace P58_Loss
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class PGAP : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            try
            {
                string exeDirectory = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "PGCreator\\";
                byte[] pgcBytes = File.ReadAllBytes(exeDirectory + "PGCreator.dll");
                Assembly assembly = Assembly.Load(pgcBytes);
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass)
                    {
                        if (type.Name == "PGCreator")
                        {
                            Object pgc = Activator.CreateInstance(type);
                            Object[] args = { revit, message, elements };
                            Object result = type.InvokeMember("Execute", BindingFlags.Default | BindingFlags.InvokeMethod, null, pgc, args);
                            return (Result)result;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("PGAP", e.Message);
            }
            return Result.Failed;
        }
    }
}
