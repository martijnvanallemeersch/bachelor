using DevRegTest.domainAutomationScript.Report;
using Skyline.DataMiner.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevRegTest.domainAutomationScript.Element.Import
{
    class ClearEnvironment
    {
        private string Dir { get; set; }

        #region Constructor
        public ClearEnvironment(string DirArg)
        {
            Dir = DirArg;
        }
        #endregion Constructor

        public void Execute(RemotingConnection connectie, ListsScripts listsScriptArg)
        {
            string[] BatchFiles = Directory.GetFiles(Dir, "*.bat", SearchOption.TopDirectoryOnly);

            using (PowerShell PSI = PowerShell.Create())
            {
                PSI.AddScript("Set-ExecutionPolicy Unrestricted");
                PSI.Invoke();
                PSI.Commands.Clear();

                foreach (string file in BatchFiles.Where(d => d.ToLower().EndsWith("stop.bat")))
                {
                    listsScriptArg.AddToListBatchFiles(Path.GetFileName(file));

                    PSI.AddScript("$action = New-ScheduledTaskAction -Execute '" + file + "'");
                    PSI.AddScript("Register-ScheduledTask -Action $action  -TaskName \"clearEnvironment\" -Description \"Clear installers true .BAT\"");
                    PSI.Invoke();
                    PSI.Commands.Clear();
                    PSI.AddScript("Start-ScheduledTask -TaskPath \"\\\" -TaskName \"clearEnvironment\"");
                    PSI.Invoke();

                    PSI.AddScript("$Global:clearEnvironment=(Get-ScheduledTask -TaskName \"clearEnvironment\").State");
                    PSI.Invoke();

                    var stateTask = PSI.Runspace.SessionStateProxy.PSVariable.GetValue("clearEnvironment");
                    Console.WriteLine(PSI.Runspace.SessionStateProxy.PSVariable.GetValue("clearEnvironment"));

                    while (stateTask.ToString().ToLower() != "ready")
                    {
                        ManualResetEventSlim wait = new ManualResetEventSlim(false);
                        wait.Wait(new TimeSpan(0, 0, 5));
                        PSI.Commands.Clear();
                        PSI.AddScript("$Global:clearEnvironment=(Get-ScheduledTask -TaskName \"clearEnvironment\").State");
                        PSI.Invoke();
                        stateTask = PSI.Runspace.SessionStateProxy.PSVariable.GetValue("clearEnvironment");
                    }

                    PSI.AddScript("Unregister-ScheduledTask -TaskName \"clearEnvironment\" -Confirm:$false");
                    PSI.Invoke();
                }
            }
        }
    }
}
