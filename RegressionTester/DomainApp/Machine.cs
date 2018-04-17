using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace DevRegTest.domainVMApp
{
    public class Machine
    {
        #region Properties  
      
        //Is initial null, when machine is started this can be set to a value
        public string IP { get; set; }
       
        //keeps a list of all errors
        public List<String> Errors { get; set; }

        //Should be one at the time
        public VersionDMA VersionUpgrade { get; set; }

        //Contains info about installed version
        public String CurrentVersionDMA { get; set; }

        //Checks whether the machine had an error executing or not
        public bool HadErrors { get; set; }
      
        #endregion

        #region Constructor
        public Machine()
        { }
        #endregion

        public void AddVersion(VersionDMA version)
        {
            VersionUpgrade = version;
        }

        //Method to install a DMA version
        //
        //Requirements:
        //to run scripts remotly on a machine => Set-ExecutionPolicy Unrestricted  must be run within current PSI + Guest (invoke command)
        //Remote user must be administrator on remote pc (can be configured in autounattend in sysprep)
        //Notifications on remote machine to change windows components must be disabled (can also be configured in autounattend)
        public void ExecuteInstallUpgrade(string userName, string password)
        {
            HadErrors = false;
            Errors = new List<string>();

            IP = "127.0.0.1";
            using (PowerShell PSI = PowerShell.Create())
            {
                String SharedPathOnVm = VersionUpgrade.EndPath;                

                PSI.AddScript("Set-ExecutionPolicy Unrestricted;");
                //Specific for installing a new dataminer             
               
                CopyBatchFile(PSI, VersionUpgrade, SharedPathOnVm, userName);
                CurrentVersionDMA = VersionUpgrade.GenerateVersion();
                PSI.AddScript("Copy-Item -Path '" + VersionUpgrade.PathVersion + "\\" + VersionUpgrade.Name + "' -Destination '" + SharedPathOnVm + "'  -Force;");
                
                PSI.Invoke();
                PSI.Commands.Clear();

                //PSI.AddScript("Set-ExecutionPolicy Unrestricted");
                //PSI.Invoke();
                //PSI.Commands.Clear();

                           
                PSI.AddScript("Register-ScheduledTask -Xml (Get-Content 'C:\\RegressionTester_Skyline\\Scripts\\ExecuteUpgradeDMTask.xml' | Out-String) -TaskName \"ExecuteUpgradeTaskDM\"");                   
                PSI.Invoke();
                PSI.Commands.Clear();
                PSI.AddScript("Start-ScheduledTask -TaskPath \"\\\" -TaskName \"ExecuteUpgradeTaskDM\"");
                PSI.Invoke();
                PSI.Commands.Clear();
                PSI.AddScript("$Global:taskEx=(Get-ScheduledTask -TaskName \"ExecuteUpgradeTaskDM\").State;");
                PSI.Invoke();
                var stateTask = PSI.Runspace.SessionStateProxy.PSVariable.GetValue("taskEx");
                Console.WriteLine(PSI.Runspace.SessionStateProxy.PSVariable.GetValue("taskEx"));

                while (stateTask.ToString().ToLower() != "ready")
                {
                        ManualResetEventSlim wait = new ManualResetEventSlim(false);
                        wait.Wait(new TimeSpan(0, 0, 2));
                        PSI.Commands.Clear();
                        PSI.AddScript("$Global:taskEx=(Get-ScheduledTask -TaskName \"ExecuteUpgradeTaskDM\").State");
                        PSI.Invoke();
                        stateTask = PSI.Runspace.SessionStateProxy.PSVariable.GetValue("taskEx"); ;
                }

                PSI.AddScript("Unregister-ScheduledTask -TaskName \"ExecuteUpgradeTaskDM\" -Confirm:$false");
                PSI.Invoke();   
               

                if (PSI.Commands.Commands.Count != 0)
                {
                    PSI.Invoke();
                    CheckErrors(PSI, null);
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
        }

        //Copies a batch file to a guest OS and configured so it executes the right EXE file
        private void CopyBatchFile(PowerShell PSI, VersionDMA version, string temp, string userName)
        {
            string pathToBatch = @"C:\RegressionTester_Skyline\Scripts\DataMinerUpgradeUnAttend.bat";
            string indexBar = version.PathVersion.Substring(version.PathVersion.LastIndexOf('\\') + 1, version.PathVersion.Length - version.PathVersion.LastIndexOf('\\') - 1);

            if(File.Exists(pathToBatch))
            {
                File.Delete(pathToBatch);
            }

            ManualResetEventSlim wait2 = new ManualResetEventSlim(false);
            wait2.Wait(500);

            PSI.AddScript("New-Item '" + pathToBatch + "' -type file;");
            PSI.Invoke();           

            ManualResetEventSlim wait = new ManualResetEventSlim(false);
            wait.Wait(500);

            using (var stream = new FileStream(pathToBatch, FileMode.Truncate))
            {
                using (var writer = new StreamWriter(stream))
                {
                    // If its a DMAupgrade then it needs a specific command
                    // http://intranet/DataMiner/Lists/Release%20Notes/DispForm.aspx?ID=12454 
                    //Upgrade with command line only available after version 9.0.1611.4
                    //Writes the cmd commands to the file specific for each dataminer upgrade
                    if (version.DMAUpgrade)
                    {
                        writer.WriteLine("cd c:\\Program Files (x86)\\Skyline Communications\\Skyline Taskbar Utility");
                        writer.WriteLine("start SLTaskbarUtility ");
                        writer.WriteLine("timeout 10");
                        writer.WriteLine("start /wait SLTaskbarUtility -upgrade \"" + version.EndPath + "\\" + version.Name +  "\"");
                        writer.WriteLine("echo %errorlevel%");
                    }

                    writer.Close();
                }
                stream.Close();
            }
        }
      
        // Checks PSI.Stream if any errors were found
        public void CheckErrors(PowerShell PSI, string message)
        {
            if (PSI.HadErrors && message == null)
            {
                this.HadErrors = true;
                PSI.Streams.Error.ToList().ForEach(t => Errors.Add(t.Exception.Message));
            }
            else if (PSI.HadErrors && message != null)
            {
                this.HadErrors = true;
                Errors.Add(message + " " + PSI.Streams.Error.FirstOrDefault().Exception.Message);
            }
            else if (PSI.HadErrors && message != null)
            {
                this.HadErrors = true;
                Errors.Add(message + " " + PSI.Streams.Error.FirstOrDefault().Exception.Message);
            }
        }
    }
}
