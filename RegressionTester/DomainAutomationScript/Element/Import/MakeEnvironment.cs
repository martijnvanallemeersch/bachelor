using DevRegTest.domainAutomationScript.Report;
using DevRegTest.DomainAutomationScript.Element.Import;
using DevRegTest.domainVMApp;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;


namespace DevRegTest.domainAutomationScript.Scripts.Import
{
    class MakeEnvironment
    {
        //String with the directory path that you want to import all the scripts into you DMA 
        private string Dir { get; set; }

        public List<NameScriptsDirectories> scripts { get; set; }

        ExecuteScriptMessage ScriptMessage;
        ScriptProgressEventMessage scriptProgressEventMessageObject;

        #region Constructor
        public MakeEnvironment(string DirArg, List<NameScriptsDirectories> scriptsArg)
        {
            Dir = DirArg;
            scripts = scriptsArg;
        }
        #endregion Constructor

        //Method that we use to import all the script from a directory
        public List<NameScriptsDirectories> Execute(RemotingConnection connectie, ListsScripts listsScriptArg)
        {
            List<NameScriptsDirectories> xmlFiles = new List<NameScriptsDirectories>();
            List<NameScriptsDirectories> xmlCorrectedFiles = new List<NameScriptsDirectories>();

            foreach(string ScriptArg in Directory.GetFiles(Dir, "*.xml", SearchOption.TopDirectoryOnly))
            {
                foreach (NameScriptsDirectories script in scripts)
                {
                    if (ScriptArg == script.Path)
                    {
                        xmlFiles.Add(script);                        
                    }
                }
            }

            string[] DeltFiles = Directory.GetFiles(Dir, "*.dmimport", SearchOption.TopDirectoryOnly);

            foreach (string DeltArg in Directory.GetFiles(Dir, "*.dmimport", SearchOption.TopDirectoryOnly))
            {
                foreach (NameScriptsDirectories Delt in scripts)
                {
                    if (DeltArg == Delt.Path)
                    {
                        xmlCorrectedFiles.Add(Delt);
                    }
                }
            }

            string[] BatchFiles = Directory.GetFiles(Dir, "*.bat", SearchOption.TopDirectoryOnly);

            foreach (string BatArg in Directory.GetFiles(Dir, "*.bat", SearchOption.TopDirectoryOnly))
            {
                foreach (NameScriptsDirectories Bat in scripts)
                {
                    if (BatArg == Bat.Path)
                    {
                        xmlCorrectedFiles.Add(Bat);
                    }
                }
            }

            using (PowerShell PSI = PowerShell.Create())
            {
                PSI.AddScript("Set-ExecutionPolicy Unrestricted");
                PSI.Invoke();
                PSI.Commands.Clear();           
               
                foreach (string file in BatchFiles.Where(d => d.ToLower().EndsWith("start.bat") ||
                                                              d.ToLower().EndsWith("startpredelt.bat")))
                {
                    listsScriptArg.AddToListBatchFiles(Path.GetFileName(file));

                    PSI.AddScript("$action = New-ScheduledTaskAction -Execute '"+ file +"'");                   
                    PSI.AddScript("Register-ScheduledTask -Action $action  -TaskName \"AppLog\" -Description \"Daily dump of Applog\"");
                    PSI.Invoke();
                    PSI.Commands.Clear();
                    PSI.AddScript("Start-ScheduledTask -TaskPath \"\\\" -TaskName \"AppLog\"");
                    PSI.Invoke();                    

                    PSI.AddScript("$Global:AppLog=(Get-ScheduledTask -TaskName \"AppLog\").State");
                    PSI.Invoke();

                    var stateTask = PSI.Runspace.SessionStateProxy.PSVariable.GetValue("AppLog");
                    Console.WriteLine(PSI.Runspace.SessionStateProxy.PSVariable.GetValue("AppLog"));

                    while (stateTask.ToString().ToLower() != "ready")
                    {
                        ManualResetEventSlim wait = new ManualResetEventSlim(false);
                        wait.Wait(new TimeSpan(0, 0, 5));
                        PSI.Commands.Clear();
                        PSI.AddScript("$Global:AppLog=(Get-ScheduledTask -TaskName \"AppLog\").State");
                        PSI.Invoke();
                        stateTask = PSI.Runspace.SessionStateProxy.PSVariable.GetValue("AppLog"); 
                    }  

                    PSI.AddScript("Unregister-ScheduledTask -TaskName \"AppLog\" -Confirm:$false");
                    PSI.Invoke();
                }

                if (PSI.HadErrors)
                {
                    foreach (System.Management.Automation.ErrorRecord errorMessage in PSI.Streams.Error)
                    {
                        if (!errorMessage.ToString().Contains("Windows PowerShell updated your execution policy successfully"))
                            LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": " + errorMessage.ToString() + ".");
                    }
                }
            }

            foreach (string file in DeltFiles)
            {
                //listsScriptArg.AddToListDelts(Path.GetFileName(file));
                //xmlCorrectedFiles.Add();
                try
                {
                    ImportDelt.Import(connectie, file);
                }
                catch (Exception ex)
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Error while importing Delts", ex.Message));
                }
            }

            foreach (NameScriptsDirectories file in xmlFiles)
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(file.Path);
                    XmlNodeList ParametersNode = xml.FirstChild.SelectNodes("/DMSScript/Parameters");

                    if (!ParametersNode[0].HasChildNodes)
                    {
                        var contents = File.ReadAllLines(file.Path, Encoding.UTF8);

                        var cpfm = new CreateProtocolFileMessage() { Sa = new SA(new string[] { Path.GetFileNameWithoutExtension(file.Path) }), What = 2, strInfo = String.Join("\n", contents) };
                        var resp = connectie.HandleSingleResponseMessage(cpfm);
                        xmlCorrectedFiles.Add(file);                        
                    }
                }
                catch (Exception ex)
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Error while loading script", ex.Message));
                }             
            }

            using (PowerShell PSI = PowerShell.Create())
            {
                PSI.AddScript("Set-ExecutionPolicy Unrestricted");
                PSI.Invoke();
                PSI.Commands.Clear();

                foreach (string file in BatchFiles.Where(d => d.ToLower().EndsWith("startpostdelt.bat")))
                {
                    listsScriptArg.AddToListBatchFiles(Path.GetFileName(file));

                    PSI.AddScript("$action = New-ScheduledTaskAction -Execute '" + file + "'");
                    PSI.AddScript("Register-ScheduledTask -Action $action  -TaskName \"AppLog\" -Description \"Daily dump of Applog\"");
                    PSI.Invoke();
                    PSI.Commands.Clear();
                    PSI.AddScript("Start-ScheduledTask -TaskPath \"\\\" -TaskName \"AppLog\"");
                    PSI.Invoke();

                    PSI.AddScript("$Global:AppLog=(Get-ScheduledTask -TaskName \"AppLog\").State");
                    PSI.Invoke();

                    var stateTask = PSI.Runspace.SessionStateProxy.PSVariable.GetValue("AppLog");
                    Console.WriteLine(PSI.Runspace.SessionStateProxy.PSVariable.GetValue("AppLog"));

                    while (stateTask.ToString().ToLower() != "ready")
                    {
                        ManualResetEventSlim wait = new ManualResetEventSlim(false);
                        wait.Wait(new TimeSpan(0, 0, 5));
                        PSI.Commands.Clear();
                        PSI.AddScript("$Global:AppLog=(Get-ScheduledTask -TaskName \"AppLog\").State");
                        PSI.Invoke();
                        stateTask = PSI.Runspace.SessionStateProxy.PSVariable.GetValue("AppLog");
                    }

                    PSI.AddScript("Unregister-ScheduledTask -TaskName \"AppLog\" -Confirm:$false");
                    PSI.Invoke();
                }

                if (PSI.HadErrors)
                {
                    foreach (System.Management.Automation.ErrorRecord errorMessage in PSI.Streams.Error)
                    {
                        if (!errorMessage.ToString().Contains("Windows PowerShell updated your execution policy successfully"))
                            LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": " + errorMessage.ToString() + ".");
                    }
                }
            }

            return xmlCorrectedFiles;
        }    
    }
}
