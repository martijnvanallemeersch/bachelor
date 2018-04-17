using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace DevRegTest.domainVMApp
{
    class Init
    {
        //Method to initialize the DMA in form1
        public static DMA initialisatie(bool cmdOurUiArg)
        {
            DMA DMAVU = new DMA();

            #region initInfoForm
            try
            {
                //Gets all the Configs of Dataminer under the "C:\Skyline DataMiner Configs" directory
                DirectoryInfo Configs = new DirectoryInfo(@"C:\Skyline DataMiner Configs");
                FileSystemInfo[] filesConfigs = Configs.GetDirectories().OrderByDescending(p => p.CreationTime).Where(d => !d.Name.StartsWith("Testing")).ToArray();

                // Adds all the configs to the application (combobox)
                foreach (FileSystemInfo path in filesConfigs)
                {
                    NameConfigs SC7 = new NameConfigs(path.Name);

                    DMAVU.AddConfig(SC7);
                }
            }
            catch (Exception ex)
            {
                if (cmdOurUiArg == true)
                {
                    MessageBox.Show("the folder 'C:\\Skyline DataMiner Configs' doesn't exists.",
                                   "folder problem.",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Error);
                    MessageBox.Show(ex.ToString());
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "folder problem.", "the folder 'C:\\Skyline DataMiner Configs' doesn't exists."));
                    App.CommandLog(ex.Message);
                }

                Environment.Exit(0);
            }

            try
            {
                //Gets all the directories with script of Dataminer under the shared directory
                DirectoryInfo DirectoriesScripts = new DirectoryInfo(@"\\NAS\Shares\Public\Software Development\Testing\Scripts");
                FileSystemInfo[] ScriptsDirectories = DirectoriesScripts.GetDirectories().OrderByDescending(p => p.CreationTime).ToArray();

                // Adds all the directories with script to the application (combobox)
                foreach (FileSystemInfo path in ScriptsDirectories)
                {
                    NameScriptsDirectories script = new NameScriptsDirectories(path.Name, path.FullName);

                    DMAVU.AddScriptsDirectory(script);
                }
            }
            catch (Exception ex)
            {
                if (cmdOurUiArg == true)
                {
                    MessageBox.Show("the folder '\\NAS\\Shares\\Public\\Software Development\\Testing\\Scripts' doesn't exists our you haven't connection with the Shares.",
                                   "folder problem.",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Error);
                    MessageBox.Show(ex.ToString());
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "folder problem.", "the folder '\\NAS\\Shares\\Public\\Software Development\\Testing\\Scripts' doesn't exists our you haven't connection with the Shares."));
                    App.CommandLog(ex.Message);
                }

                Environment.Exit(0);
            }

            try
            {
                //Gets all the versions of Dataminer under Shared directory
                DirectoryInfo info = new DirectoryInfo(@"\\NAS\Shares\Public\DataMiner Software\Upgrades");
                FileSystemInfo[] files = info.GetFiles().OrderByDescending(p => p.Name).Where(d => d.Extension == ".dmupgrade").ToArray();

                // Adds all the upgrade packages to the application (combobox)
                VersionDMA SC7_empty = new VersionDMA("");
                DMAVU.AddVersion(SC7_empty);

                foreach (FileSystemInfo path in files)
                {
                    if (path.Name.Contains("rc"))
                    {
                        string version = path.Name.Substring(path.Name.LastIndexOf('\\') + 1, path.Name.Length - path.Name.LastIndexOf('\\') - 1);
                        string pathToVersion = new DirectoryInfo(path.FullName).Parent.FullName;
                        string fileName = "DataMinerUpgradeUnAttend.bat";

                        VersionDMA SC7 = new VersionDMA(version, pathToVersion, "C:\\RegressionTester_Skyline\\Scripts", fileName, false, true);
                        SC7.DMAUpgrade = true;

                        DMAVU.AddVersion(SC7);
                    }
                }
            }
            catch (Exception ex)
            {
                if (cmdOurUiArg == true)
                {
                    MessageBox.Show("the folder '\\NAS\\Shares\\Public\\DataMiner Software\\Upgrades' doesn't exists our you haven't connection with the Shares.",
                                   "folder problem.",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Error);
                    MessageBox.Show(ex.ToString());
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "folder problem.", "the folder '\\NAS\\Shares\\Public\\DataMiner Software\\Upgrades' doesn't exists our you haven't connection with the Shares."));
                    App.CommandLog(ex.Message);
                }

                Environment.Exit(0);
            }

            try
            {
                //Gets all the delts of Dataminer under Shared directory
                DirectoryInfo infoDelt = new DirectoryInfo(@"\\NAS\Shares\Public\Software Development\Testing\Delt");
                string[] pathsDelt = Directory.GetFiles(@"\\NAS\Shares\Public\Software Development\Testing\Delt", "*.dmimport", SearchOption.TopDirectoryOnly);
                FileSystemInfo[] filesDelt = infoDelt.GetFiles().OrderByDescending(p => p.CreationTime).Where(d => d.Extension == ".dmimport").ToArray();

                // Adds all the delts to the application (combobox)
                foreach (FileSystemInfo path in filesDelt)
                {
                    NameScriptOrDelt SC7 = new NameScriptOrDelt(path.Name);

                    DMAVU.AddDelt(SC7);
                }
            }
            catch (Exception ex)
            {
                if (cmdOurUiArg == true)
                {
                    MessageBox.Show("the folder '\\NAS\\Shares\\Public\\Software Development\\Testing\\Delt' doesn't exists our you haven't connection with the Shares.",
                                   "folder problem.",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Error);
                    MessageBox.Show(ex.ToString());
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "folder problem.", "the folder '\\NAS\\Shares\\Public\\Software Development\\Testing\\Delt' doesn't exists our you haven't connection with the Shares."));
                    App.CommandLog(ex.Message);
                }

                Environment.Exit(0);
            }

            #endregion initInfoForm

            return DMAVU;
        }

        #region MakeSomeXMLfiles
        public static void MakeXmlCopyToShares(string Path)
        {
            try
            {
                if (File.Exists(Path))
                {
                    File.Delete(Path);
                }

                XDocument doc = new XDocument(new XDeclaration("1.0", "utf-16", "yes"));
                doc.Declaration.Encoding = "UTF-16";

                XNamespace reportDef = "http://schemas.microsoft.com/windows/2004/02/mit/task";
                XElement root = new XElement(reportDef + "Task");

                root.SetAttributeValue("version", 1.2);

                XElement Actions = new XElement(reportDef + "Actions");
                Actions.SetAttributeValue("Context", "Author");
                XElement Exec = new XElement(reportDef + "Exec");
                XElement Command = new XElement(reportDef + "Command", @"C:\RegressionTester_Skyline\Scripts\Copy.bat");

                //task.Add(new XElement(Action));
                root.Add(Actions);
                Actions.Add(Exec);
                Exec.Add(Command);

                doc.Add(root);

                doc.Save(Path);
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "problem with MakeXmlCopyToShares", ex.Message));
            }
        }

        public static void MakeXmlUpgrade(string Path)
        {
            try
            {
                if (File.Exists(Path))
                {
                    File.Delete(Path);
                }

                XDocument doc = new XDocument(new XDeclaration("1.0", "utf-16", "yes"));
                doc.Declaration.Encoding = "UTF-16";

                XNamespace reportDef = "http://schemas.microsoft.com/windows/2004/02/mit/task";
                XElement root = new XElement(reportDef + "Task");

                root.SetAttributeValue("version", 1.2);

                XElement RegistrationInfo = new XElement(reportDef + "RegistrationInfo");
                RegistrationInfo.Add(new XElement(reportDef + "Date", DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T" + DateTime.Now.ToLongTimeString()));
                RegistrationInfo.Add(new XElement(reportDef + "Author", "SKYLINE2\\" + Environment.UserName));
                root.Add(RegistrationInfo);

                XElement Triggers = new XElement(reportDef + "Triggers");
                root.Add(Triggers);

                XElement Principals = new XElement(reportDef + "Principals");
                XElement Principal = new XElement(reportDef + "Principal", new XAttribute("id", "Author"));
                Principal.Add(new XElement(reportDef + "UserId", "SKYLINE2\\" + Environment.UserName));
                Principal.Add(new XElement(reportDef + "LogonType", "InteractiveToken"));
                Principal.Add(new XElement(reportDef + "RunLevel", "HighestAvailable"));
                Principals.Add(Principal);
                root.Add(Principals);

                XElement Settings = new XElement(reportDef + "Settings");
                Settings.Add(new XElement(reportDef + "MultipleInstancesPolicy", "IgnoreNew"));
                Settings.Add(new XElement(reportDef + "DisallowStartIfOnBatteries", "true"));
                Settings.Add(new XElement(reportDef + "StopIfGoingOnBatteries", "true"));
                Settings.Add(new XElement(reportDef + "AllowHardTerminate", "true"));
                Settings.Add(new XElement(reportDef + "StartWhenAvailable", "false"));
                Settings.Add(new XElement(reportDef + "RunOnlyIfNetworkAvailable", "false"));

                XElement IdleSettings = new XElement(reportDef + "IdleSettings");
                IdleSettings.Add(new XElement(reportDef + "StopOnIdleEnd", "true"));
                IdleSettings.Add(new XElement(reportDef + "RestartOnIdle", "false"));

                Settings.Add(IdleSettings);
                Settings.Add(new XElement(reportDef + "AllowStartOnDemand", "true"));
                Settings.Add(new XElement(reportDef + "Enabled", "true"));
                Settings.Add(new XElement(reportDef + "Hidden", "false"));
                Settings.Add(new XElement(reportDef + "RunOnlyIfIdle", "false"));
                Settings.Add(new XElement(reportDef + "WakeToRun", "false"));
                Settings.Add(new XElement(reportDef + "ExecutionTimeLimit", "P3D"));
                Settings.Add(new XElement(reportDef + "Priority", "7"));
                root.Add(Settings);

                XElement Actions = new XElement(reportDef + "Actions");
                Actions.SetAttributeValue("Context", "Author");
                XElement Exec = new XElement(reportDef + "Exec");
                Exec.Add(new XElement(reportDef + "Command", @"C:\RegressionTester_Skyline\Scripts\DataMinerUpgradeUnAttend.bat"));
                Actions.Add(Exec);
                root.Add(Actions);

                doc.Add(root);
                doc.Save(Path);
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Porblem with MakeXmlUpgrade", ex.Message));
            }
        }
        #endregion MakeSomeXMLfiles
    }
}
