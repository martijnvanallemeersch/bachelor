using Skyline.DataMiner.Net.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevRegTest.domainVMApp;
using Skyline.DataMiner.Net;
using VMApp.domainAutomationScript.Report;
using VMApp.domainAutomationScript;
using DevRegTest.domainAutomationScript.Scripts.Import;
using Skyline.DataMiner.Net.Messages;
using DevRegTest.domainAutomationScript.Element.RunAutomationScript;
using Skyline.DataMiner.Net.Filters;
using System.Net.Mail;
using System.Net;
using Skyline.DataMiner.Automation;
using DevRegTest.domainAutomationScript.Report;
using System.Diagnostics;
using System.ServiceProcess;
using VMApp.domainAutomationScript;
using DevRegTest.domainAutomationScript.Element.AbortAutomationScript;
using DevRegTest.domainAutomationScript.Connection.MakeOurCloseConnection;
using DevRegTest.domainAutomationScript.Element.Delete;
using Skyline.DataMiner.Net.Messages.Advanced;
using System.Drawing;
using System.Xml.Linq;
using System.Xml;
using DevRegTest.domainAutomationScript.Element.Import;
using DevRegTest.domainAutomationScript.Element.Restart;
using System.Windows;
using System.Collections.ObjectModel;
using DevRegTest.DomainAutomationScript.Report;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using DevRegTest.DomainAutomationScript.Element.Import;


namespace DevRegTest.domainVMApp
{
    public class DMA
    {   
        #region Properties
        //Lijst van alle beschikbare directories with scripts
        public IList<NameScriptsDirectories> ScriptsDirectories { get; set; }

        public List<NameScriptsDirectories> DirectoriesAndChilderenVar { get; set; }

        public List<NameScriptsDirectories> ActiveNodes { get; set; }

        //Lijst van alle beschikbare configs
        public IList<NameConfigs> Configs { get; set; }

        //Lijst van alle beschikbare scripts
        public IList<VersionDMA> Versions { get; set; }

        //Lijst van alle beschikbare automationDelts
        public IList<NameScriptOrDelt> Delts { get; set; }  

        //Contains list of active machines
        public IList<DevRegTest.domainVMApp.Machine> Machines { get; set; }              
 
        //Specifies a list of available protocols to be inserted
        public List<Protocol> ProtocolList { get; set; }

        //List with all the scripts that we run in parallel
        public List<NameScriptsDirectories> filesScriptsParallel { get; set; }

        //DMA ID from the local Dataminer
        public int DMA_ID { get; set; }

        int _amountFailures;

        //credentials of user
        public string UserName { get; set; }
        public string Password { get; set; }

        public static CancellationTokenSource tokenSourceCrash;
        public static CancellationToken tokenCrash;

        public static CancellationTokenSource tokenSourceFileNotFound;
        public static CancellationToken tokenFileNotFound;


        public ManualResetEvent waitHandle;

        public event InstallationDoneHandler InstallDoneEvent;
        public EventArgs e = null;
        public delegate void InstallationDoneHandler(Machine machineInstall, EventArgs e);

        #endregion

        #region constructor
        public DMA()
        {
            Versions = new List<VersionDMA>();
            Configs = new List<NameConfigs>();
            Machines = new List<Machine>();
            ProtocolList = new List<Protocol>();
            Delts = new List<NameScriptOrDelt>();
            ScriptsDirectories = new List<NameScriptsDirectories>();
            DirectoriesAndChilderenVar = new List<NameScriptsDirectories>();
            ActiveNodes = new List<NameScriptsDirectories>();
            filesScriptsParallel = new List<NameScriptsDirectories>();
            _amountFailures = 0;
        }
        #endregion constructor

        #region AddToListsFunctions
        //Add directory with Cassandra scripts to the environment
        public void AddScriptsDirectory(NameScriptsDirectories directory)
        {
            if (!ScriptsDirectories.Contains(directory))
            {
                ScriptsDirectories.Add(directory);
            }
        }

        //Add config to the environment
        public void AddConfig(NameConfigs config)
        {
            if (!Configs.Contains(config))
            {
                Configs.Add(config);
            }
        }

        //Add version to the environment
        public void AddVersion(VersionDMA version)
        {
            if (!Versions.Contains(version))
            {
                Versions.Add(version);
            }
        }

        //Add a Delt to the environment
        public void AddDelt(NameScriptOrDelt delt)
        {
            if (!Delts.Contains(delt))
            {
                Delts.Add(delt);
            }
        }

        #endregion

        #region ChangeConfigAndFunctionality

        //The method for running a process
        private static void RunProcess(string file, string arguments = "", bool shellExecute = false)
        {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = file;
            pi.Arguments = arguments;
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            pi.CreateNoWindow = true;
            pi.UseShellExecute = shellExecute;
            Process process = Process.Start(pi);
            process.WaitForExit();
            process.Close();
        }

        //method that we use to stop the DMA
        public static void StopDataMiner()
        {
            try
            {
                string killPath = @"C:\Skyline DataMiner\Files\SLKill.exe";
                if (File.Exists(killPath))
                {
                    RunProcess(killPath, "sl");
                    RunProcess(killPath, "upgradehelper");
                }
                else
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2}).", DateTime.Now.ToString(), " File not found", killPath));
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2}).", DateTime.Now, "Failed to stop DataMiner", ex.Message));
            }
        }

        //method that we use to unregister our DMA
        public static void UnregisterDataMiner()
        {
            try
            {
                string unregisterServicePath = @"C:\Skyline DataMiner\Tools\UnRegister DataMiner.bat";
                if (File.Exists(unregisterServicePath))
                    RunProcess(unregisterServicePath, "/nb", true);
                else
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, " File not found.", "Could not unregister services.\r\nCould not find " + unregisterServicePath + "."));
                }

                string unregisterDLLsPath = @"C:\Skyline DataMiner\Tools\UnRegister dll's of DataMiner.bat";
                if (File.Exists(unregisterDLLsPath))
                    RunProcess(unregisterDLLsPath, "/nb", true);
                else
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, " File not found.", "Could not unregister DLL's.\r\nCould not find " + unregisterDLLsPath + "."));
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, " Failed to unregister DataMiner.", ex.Message));
            }
        }

        //method that we use to make the link from "C:\Skyline DataMiner" to the config we have picked
        public static bool SetLinkdInfo(String destination)
        {
            bool result = false;

            try
            {
                Process fileProcess = new Process();
                fileProcess.StartInfo = new ProcessStartInfo
                {
                    //FileName = String.Format(@"{0}linkd.exe", @"C:\\DataMiner Configs\"),
                    FileName = "linkd.exe",
                    Arguments = String.Format("\"C:\\Skyline DataMiner\" \"{0}\"", destination),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                };

                bool started = fileProcess.Start();

                if (started)
                {
                    fileProcess.WaitForExit();
                    String output = fileProcess.StandardOutput.ReadToEnd();

                    if (output.StartsWith(String.Format("Link created at: ")))
                    {
                        result = true;
                    }
                }
                else
                {
                    throw new Exception("Couldn't start linkd command");
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to Setup a link", ex.Message));
            }

            return result;
        }

        //method that we use to get the link from "C:\Skyline DataMiner"
        public static String GetLinkdInfo()
        {
            String linkedPath = String.Empty;

            try
            {
                Process fileProcess = new Process();
                fileProcess.StartInfo = new ProcessStartInfo
                {
                    //FileName = String.Format(@"{0}linkd.exe", @"C:\\DataMiner Configs\"),
                    FileName = "linkd.exe",
                    Arguments = "\"C:\\Skyline DataMiner\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                };

                bool started = fileProcess.Start();

                if (started)
                {
                    fileProcess.WaitForExit();
                    String linkedPathOutput = fileProcess.StandardOutput.ReadToEnd();

                    if (linkedPathOutput.StartsWith(String.Format("Source  C:\\Skyline DataMiner is linked to")))
                    {
                        String[] content = linkedPathOutput.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        if (content.Length == 2)
                            linkedPath = content[1];
                    }
                }
                else
                {
                    throw new Exception("Couldn't start linkd command");
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, " Failed to retrieve the linkd info", ex.Message));
            }

            return linkedPath;
        }

        //method that we use to register our DMA
        public static void RegisterDataMiner()
        {
            try
            {
                string registerDLLsPath = @"C:\Skyline DataMiner\Tools\Register dll's of DataMiner (silent).bat";
                if (File.Exists(registerDLLsPath))
                    RunProcess(registerDLLsPath, "/nb", true);
                else
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "File not found.", "Could not register DLL's.\r\nCould not find " + registerDLLsPath + "."));
                }

                string registerServicePath = @"C:\Skyline DataMiner\Tools\Register DataMiner as Service.bat";
                if (File.Exists(registerServicePath))
                    RunProcess(registerServicePath, "/nb", true);
                else
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "File not found.", "Could not register services.\r\nCould not find " + registerServicePath + "."));
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to register DataMiner.", ex.Message));
            }
        }

        //method that we van use to start our local DMA
        public static void StartDataMiner(bool restart = false)
        {
            try
            {
                ServiceController service = new ServiceController("SLDataMiner");
                if (restart || service.Status != ServiceControllerStatus.Running)
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(30000);

                    if (service.Status != ServiceControllerStatus.Stopped)
                        service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                    service.Start();

                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to start DataMiner.", ex.Message));

            }
        }

        public void ChooseConfig(string CleanConfigName, string ConfigName, bool copyFileBool, RemotingConnection connectionArg)
        {
            List<string> Iplist = new List<string>();
            Iplist.Add("127.0.0.1");

            try
            {
                String linkedFolderName = Path.GetFileName(GetLinkdInfo());
                if (!CleanConfigName.Equals(linkedFolderName) || copyFileBool == true)
                {
                    DateTime startTimeChangeConfiguration = DateTime.Now;

                    StopDataMiner();

                    if (copyFileBool == false)
                    {
                        UnregisterDataMiner();
                    }

                    if (copyFileBool == true)
                    {
                        copyFiles(CleanConfigName, ConfigName);
                    }

                    if (copyFileBool == false)
                    {
                        SetLinkdInfo(@"C:\Skyline DataMiner Configs\" + CleanConfigName);
                        RegisterDataMiner();
                    }

                    StartDataMiner(true);

                    DateTime endTimeChangeConfiguration = DateTime.Now;

                    if (copyFileBool == false)
                    {
                        if (File.Exists(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Configuration_switch_time.xml"))
                        {
                            try
                            {
                                XDocument doc = XDocument.Load(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Configuration_switch_time.xml");
                                XElement root = new XElement("ConfigurationSwitch");
                                root.Add(new XElement("OldConfiguration", linkedFolderName));
                                root.Add(new XElement("NewConfiguration", CleanConfigName));
                                root.Add(new XElement("StartTime", startTimeChangeConfiguration.ToString()));
                                root.Add(new XElement("EndTime", endTimeChangeConfiguration.ToString()));
                                root.Add(new XElement("Duration", (endTimeChangeConfiguration - startTimeChangeConfiguration).ToString()));
                                doc.Element("root").Add(root);
                                doc.Save(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Configuration_switch_time.xml");
                            }
                            catch (Exception ex)
                            {
                                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2}).", DateTime.Now.ToString(), " problem in making or loading the XML file for the configuration switch timing", ex.Message));
                            }
                        }
                        else
                        {
                            LoggingErrors.AddExceptionToList(String.Format("{0}: {1}.", DateTime.Now.ToString(), " could not found C:\\RegressionTester_Skyline\\Current_Test\\General_Information\\Configuration_switch_time.xml"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2}).", DateTime.Now.ToString(), "Error in Choose Config", ex.Message));
            }

        }

        #endregion ChangeConfigAndFunctionality

        #region InstallUpgrade
        //Execute scripts on selected machines
        public void ExecuteInstallUpgrade(String nameDMA)
        {
            if (Password != "" && UserName != "")
            {
                Machine machineToBeModified = new Machine();

                VersionDMA sDMA = Versions.Where(i => i.Name == nameDMA).FirstOrDefault();
                if (sDMA != null)
                {
                    machineToBeModified.AddVersion(sDMA);
                }

                machineToBeModified.ExecuteInstallUpgrade(UserName, Password);
            }
            else
            {
                LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": Credentials are needed.");
            }
        }

        #endregion InstallUpgrade

        #region importTrendDelt

        public void ImportElementAndCheckIfElementIsStarted(RemotingConnection connectie)
        {
            waitHandle = new ManualResetEvent(false);
            var filter = new SubscriptionFilter(typeof(ElementStateEventMessage));
            try
            {
                connectie.OnNewMessage += HandleNewEventFromServer;
                connectie.AddSubscription(filter);
            }
            catch(Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "failed to subscribe on connection", ex.Message));
            }

            if(File.Exists(@"\\NAS\Shares\Public\Software Development\Testing\Delt\ElementDevRegTest.dmimport"))
                ImportDelt.Import(connectie, @"\\NAS\Shares\Public\Software Development\Testing\Delt\ElementDevRegTest.dmimport");
            else
            {
                try
                {
                    tokenSourceFileNotFound.Cancel();
                }
                catch (Exception ex)
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "error canceling token :", ex.ToString()));
                }
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1}", DateTime.Now, "File not exist :" + @"\\NAS\Shares\Public\Software Development\Testing\Delt\ElementDevRegTest.dmimport"));
            }

            ManualResetEventSlim wait = new ManualResetEventSlim(false);
            wait.Wait(650);

            waitHandle.WaitOne(60000);

            try
            {
                connectie.RemoveSubscription(filter);
                connectie.OnNewMessage -= HandleNewEventFromServer;                
            }
            catch(Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "failed to unsubscribe on connection", ex.Message));   
            }

            waitHandle.Dispose();

            try
            {
                WaitUntilFirstPointIsMeasured wufpim = new WaitUntilFirstPointIsMeasured(DMA_ID);
                //Wait till our trend measure the first point
                wufpim.Check(connectie);
            }
            catch(Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "failed to check if Element is measuring", ex.Message));
            }
        }
        // called when server sends an event
        void HandleNewEventFromServer(object sender, NewMessageEventArgs e)
        {
            if (e.Message is ElementStateEventMessage)
            {
                ElementStateEventMessage StateElement = (ElementStateEventMessage)e.Message;

                switch (StateElement.State)
                {
                    case ElementState.Active:
                        {
                            try
                            {
                                waitHandle.Set();
                            }
                            catch(Exception ex)
                            {
                                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "failed to set waitHandler when TrendElement was active", ex.Message));
                            }
                            break;
                        }
                }
            }           
        }

        #endregion importTrendDelt

        #region RestartDeleteElement
        //The method to delete an element
        public void DeleteElement(RemotingConnection connectionArg)
        {
            List<string> Iplist = new List<string>();
            Iplist.Add("127.0.0.1");

            DeleteElement doe = new DeleteElement("TrendRegressionTester", DMA_ID);
            doe.Invoke(connectionArg);
        }

        //Method that restart all the elements in the current DMA
        public void RestartElements(RemotingConnection remotingConnection)
        {
            RestartAllElements restartAllElements = new RestartAllElements(DMA_ID);
            restartAllElements.Execute(remotingConnection);
        }

        #endregion RestartDeleteElement

        #region CreateDirectoriesAndFiles

        //The method to create a directory and look if the directory exist and if it exists then delete all the directory and 
        //Files under this directory so we can create some new files and directory under this directory
        public void CreateDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);

                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }

                        dir.Delete(true);
                    }
                }

                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to create a directory", ex.Message));
            }
        }

        //The method to create a directory
        public void CreateDirectoryWithoutDeleting(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to create directory: " + path + ".", ex.Message));
            }
        }

        //The method to create a file
        public void CreateXMLFileWithoutDeleting(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    XmlTextWriter writer = new XmlTextWriter(path, System.Text.Encoding.UTF8);
                    writer.WriteStartElement("root");
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to create file: " + path + ".", ex.Message));
            }
        }

        public void CreateXMLFileWithDeleting(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                XmlTextWriter writer = new XmlTextWriter(path, System.Text.Encoding.UTF8);
                writer.WriteStartElement("root");
                writer.Close();
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to create file: " + path + ".", ex.Message));
            }
        }

        public void CreateTxtFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    File.Create(path);
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to create file: " + path, ex.Message));
            }
        }

        //Method that we use to make the file with all the fails, successess and warnings
        public void MakeXMLDocumentParallel(string SelectedDirectory, ListsScripts ListsScriptsArg, string upgradeVersion, string textCombocopyArg)
        {
            MakeXMLParallel mxmlp = new MakeXMLParallel(SelectedDirectory);
            mxmlp.Execute(ListsScriptsArg, upgradeVersion, textCombocopyArg);
        }

        public void MakeXMLDocumentFail(string SelectedDirectory, string upgradeVersion, string textCombocopyArg, RemotingConnection conn)
        {
            MakeXMLFail mxmlf = new MakeXMLFail(SelectedDirectory);

            if (tokenCrash.IsCancellationRequested == false)
            {
                mxmlf.ExecuteFail(upgradeVersion, textCombocopyArg, conn, DMA_ID);
            }
            else
            {
                mxmlf.ExecuteToken(upgradeVersion, textCombocopyArg, conn, DMA_ID);
            }
        }

        public void MakeXMLDocumentWithOverview(Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> overviewVersionArg)
        {
            CreateXMLFileWithoutDeleting(@"C:\RegressionTester_Skyline\Current_Test\Overview.xml");

            ManualResetEventSlim wait = new ManualResetEventSlim();
            wait.Wait(500);

            if (File.Exists(@"C:\RegressionTester_Skyline\Current_Test\Overview.xml"))
            {
                try
                {
                    XDocument doc = XDocument.Load(@"C:\RegressionTester_Skyline\Current_Test\Overview.xml");

                    foreach (var script in overviewVersionArg.Keys)
                    {
                        XElement ItemScript = new XElement("script");
                        ItemScript.SetAttributeValue("name", script);

                        foreach (var version in overviewVersionArg[script])
                        {
                            XElement ItemVersion = new XElement("version");

                            if (version.Key != "")
                                ItemVersion.Add(new XElement("VersionName", version.Key));
                            else
                                ItemVersion.Add(new XElement("VersionName", GetLastStartup(false)));

                            XElement VersionInfo = new XElement("VersionInfo");

                            foreach (var info in version.Value)
                            {
                                XElement ItemInfo = new XElement(info.Key);

                                foreach (var item in info.Value)
                                {
                                    XElement ItemItem = null;

                                    if (info.Key == "fails")
                                        ItemItem = new XElement("fail");
                                    else if (info.Key == "warnings")
                                        ItemItem = new XElement("warning");
                                    else if (info.Key == "successes")
                                        ItemItem = new XElement("succes");
                                    else if (info.Key == "duration")
                                        ItemItem = new XElement("duration");

                                    if (ItemItem != null)
                                    {
                                        ItemItem.SetValue(item);
                                        ItemInfo.Add(ItemItem);
                                    }
                                }
                                VersionInfo.Add(ItemInfo);
                            }
                            ItemVersion.Add(VersionInfo);
                            ItemScript.Add(ItemVersion);
                        }
                        doc.Element("root").Add(ItemScript);
                    }
                    doc.Save(@"C:\RegressionTester_Skyline\Current_Test\Overview.xml");
                }
                catch (Exception ex)
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now.ToString(), " problem in making or loading the XML file for the overview.", ex.Message));
                }
            }
        }

        public void MakeXMLDocumentWithInfo(string pathArg, ListsScripts listsScript, bool SequentialArg, bool ParallelArg, string UpgradeVersion)
        {
            if (File.Exists(pathArg))
            {
                try
                {
                    XDocument doc = XDocument.Load(pathArg);

                    XElement fails = new XElement("Fails");
                    if (listsScript.totalFails.Count == 0)
                        fails.Value = "0";
                    else
                    {
                        if (SequentialArg == true)
                        {
                            foreach (string text in listsScript.TotalFailsPath)
                            {
                                XElement FailsItem = new XElement("FailsItem");
                                FailsItem.Value = text;

                                fails.Add(FailsItem);
                            }
                        }
                        else if (ParallelArg == true)
                        {
                            XElement FailsItem = new XElement("FailsItem");
                            FailsItem.Value = @"C:\RegressionTester_Skyline\Current_Test\Parallel_" + UpgradeVersion;

                            fails.Add(FailsItem);
                        }
                    }

                    XElement TotalUnnoticedInfo = new XElement("Unnoticed");
                    if (listsScript.TotalUnnoticedInformationEvents.Count == 0)
                        TotalUnnoticedInfo.Value = "0";
                    else
                    {
                        foreach (string text in listsScript.TotalUnnoticedInformationEvents)
                        {
                            XElement UnnoticedItem = new XElement("UnnoticedItem");
                            UnnoticedItem.Value = text;

                            TotalUnnoticedInfo.Add(UnnoticedItem);
                        }
                    }

                    XElement ScriptsWithParameters = new XElement("ScriptsWithParameters");
                    if (listsScript.TotalScriptWithParameters.Count == 0)
                        ScriptsWithParameters.Value = "0";
                    else
                    {
                        foreach (string text in listsScript.TotalScriptWithParameters)
                        {
                            XElement ScriptWithParametersItem = new XElement("ScriptWithParametersItem");
                            ScriptWithParametersItem.Value = text;

                            ScriptsWithParameters.Add(ScriptWithParametersItem);
                        }
                    }

                    XElement ScriptsWithProtocol = new XElement("ScriptsWithProtocol");
                    if (listsScript.TotalScriptWithProtocols.Count == 0)
                        ScriptsWithProtocol.Value = "0";
                    else
                    {
                        foreach (string text in listsScript.TotalScriptWithProtocols)
                        {
                            XElement ScriptWithProtocolItem = new XElement("ScriptWithProtocolItem");
                            ScriptWithProtocolItem.Value = text;

                            ScriptsWithProtocol.Add(ScriptWithProtocolItem);
                        }
                    }

                    XElement BatchFiles = new XElement("BatchFiles");
                    if (listsScript.TotalBatchFiles.Count == 0)
                        BatchFiles.Value = "0";
                    else
                    {
                        foreach (string text in listsScript.TotalBatchFiles)
                        {
                            XElement BatchFileItem = new XElement("BatchFile");
                            BatchFileItem.Value = text;

                            BatchFiles.Add(BatchFileItem);
                        }
                    }

                    XElement Delts = new XElement("Delts");
                    if (listsScript.TotalDelts.Count == 0)
                        Delts.Value = "0";
                    else
                    {
                        foreach (string text in listsScript.TotalDelts)
                        {
                            XElement DeltItem = new XElement("Delt");
                            DeltItem.Value = text;

                            Delts.Add(DeltItem);
                        }
                    }

                    XElement TimeOuts = new XElement("Timeouts");
                    if (listsScript.TotalRequestedTimeoutsFolders.Count == 0)
                        TimeOuts.Value = "0";
                    else
                    {
                        foreach (string text in listsScript.TotalRequestedTimeoutsFolders)
                        {
                            XElement TimeOutsItem = new XElement("Timeout");
                            TimeOutsItem.Value = text;

                            TimeOuts.Add(TimeOutsItem);
                        }
                    }

                    XElement failedToExcecuteScripts = new XElement("failedScripts");
                    if (listsScript.TotalfailedToExcecuteScript.Count == 0)
                        failedToExcecuteScripts.Value = "0";
                    else
                    {
                        foreach (string text in listsScript.TotalfailedToExcecuteScript)
                        {
                            XElement failedToExcecuteScriptsItem = new XElement("failedScript");
                            failedToExcecuteScriptsItem.Value = text;

                            failedToExcecuteScripts.Add(failedToExcecuteScriptsItem);
                        }
                    }

                    doc.Element("root").Add(fails);
                    doc.Element("root").Add(TotalUnnoticedInfo);
                    doc.Element("root").Add(ScriptsWithParameters);
                    doc.Element("root").Add(ScriptsWithProtocol);
                    doc.Element("root").Add(BatchFiles);
                    doc.Element("root").Add(Delts);
                    doc.Element("root").Add(TimeOuts);
                    doc.Element("root").Add(failedToExcecuteScripts);

                    doc.Save(pathArg);
                }
                catch (Exception ex)
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now.ToString(), " problem in making or loading the XML file for the information.", ex.Message));
                }
            }
        }

        #endregion CreateDirectoriesAndFiles

        #region CopyAndClearFunctions

        //Method to get copy the directory 'file' from the local DMA to the new testing DMA
        private void copyFiles(string cleanConfigName, string configNameCopy)
        {
            try
            {
                using (PowerShell PSI = PowerShell.Create())
                {
                    if (Directory.Exists("C:\\Skyline DataMiner Configs\\" + cleanConfigName + "\\Files"))
                    {
                        if (Directory.Exists(@"C:\\RegressionTester_Skyline\\Backup_Files_Directory\\Files"))
                        {
                            PSI.AddScript("rm -r 'C:\\RegressionTester_Skyline\\Backup_Files_Directory\\Files' -Force;");
                            PSI.Invoke();
                        }

                        PSI.AddScript("Copy-Item -Path 'C:\\Skyline DataMiner Configs\\" + cleanConfigName + "\\Files' -Destination 'C:\\RegressionTester_Skyline\\Backup_Files_Directory' -Recurse -Force;");
                        PSI.Invoke();

                        PSI.AddScript("rm -r 'C:\\Skyline DataMiner Configs\\" + cleanConfigName + "\\Files' -Force;");
                        PSI.Invoke();

                        PSI.AddScript("Copy-Item -Path 'C:\\Skyline DataMiner Configs\\" + configNameCopy + "\\Files' -Destination 'C:\\Skyline DataMiner Configs\\" + cleanConfigName + "' -Recurse -Force;");
                        PSI.Invoke();

                        PSI.Commands.Clear();

                    }

                    if (PSI.HadErrors)
                    {
                        foreach (System.Management.Automation.ErrorRecord errorMessage in PSI.Streams.Error)
                        {
                            if(!errorMessage.ToString().Contains("Windows PowerShell updated your execution policy successfully"))
                            LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": " + errorMessage.ToString() + ".");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to copy File directory", ex.Message));
            }
        }

        internal void CopyToShares()
        {
            //Copy to Shares
            string destinationPathShares = @"\\nas\Shares\Public\Software Development\Testing\ResultDataTool\" + System.Environment.MachineName + "_" + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second;
            string pathToXmlCopyShares = @"C:\RegressionTester_Skyline\Scripts\CopyToShares.xml";
            string folderScriptsC = @"C:\RegressionTester_Skyline\Scripts\";

            using (PowerShell PSI = PowerShell.Create())
            {
                if (!File.Exists(@"C:\RegressionTester_Skyline\Scripts\exclude.txt"))
                {
                    PSI.AddScript("New-Item '" + folderScriptsC + "exclude.txt' -type file;");
                    PSI.Invoke();
                    PSI.Commands.Clear();
                }

                ManualResetEventSlim wait3 = new ManualResetEventSlim(false);
                wait3.Wait(1000);

                try
                {
                    using (var writer = new StreamWriter(@"C:\RegressionTester_Skyline\Scripts\exclude.txt", false))
                    {
                        writer.WriteLine("logging");
                    }
                }
                catch
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Path exception.", "Could not find path C:\\RegressionTester_Skyline\\Scripts\\exclude.txt ."));
                }

                if (!File.Exists(@"C:\RegressionTester_Skyline\Scripts\Copy.bat"))
                {
                    PSI.AddScript("New-Item '" + folderScriptsC + "Copy.bat' -type file;");
                    PSI.Invoke();
                    PSI.Commands.Clear();
                }

                ManualResetEventSlim wait4 = new ManualResetEventSlim(false);
                wait3.Wait(1000);

                try
                {
                    using (var writer = new StreamWriter(@"C:\RegressionTester_Skyline\Scripts\Copy.bat", false))
                    {
                        writer.WriteLine("powershell.exe XCopy C:\\RegressionTester_Skyline\\Current_Test '" + destinationPathShares + "' /EXCLUDE:C:\\RegressionTester_Skyline\\Scripts\\exclude.txt /E /C /I /F /R /Y");
                    }
                }
                catch
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Path exception.", "Could not find path C:\\RegressionTester_Skyline\\Scripts\\Copy.bat."));
                }

                ManualResetEventSlim wait5 = new ManualResetEventSlim(false);
                wait4.Wait(1000);

                PSI.AddScript("Register-ScheduledTask -Xml (Get-Content '" + pathToXmlCopyShares + "' | Out-String) -TaskName \"test\"");
                PSI.Invoke();
                PSI.Commands.Clear();
                PSI.AddScript("Start-ScheduledTask -TaskPath \"\\\" -TaskName \"test\"");
                PSI.Invoke();
                PSI.Commands.Clear();
                PSI.AddScript("Unregister-ScheduledTask -TaskName \"test\" -Confirm:$false");
                PSI.Invoke();

         
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

        public void copyDMA(string DbVersion)
        {
            using (PowerShell PSI = PowerShell.Create())
            {
                if (Directory.Exists(@"C:\RegressionTester_Skyline\Clean_Configs\" + DbVersion))
                {
                    PSI.AddScript("Copy-Item -Path 'C:\\RegressionTester_Skyline\\Clean_Configs\\" + DbVersion + "' -Destination 'C:\\Skyline DataMiner Configs' -Recurse -Force;");
                    PSI.Invoke();
                    PSI.Commands.Clear();
                }
                else
                {
                    PSI.AddScript("Copy-Item -Path '\\\\NAS\\Shares\\Public\\Software Development\\Testing\\CleanDMAs\\" + DbVersion + "' -Destination 'C:\\RegressionTester_Skyline\\Clean_Configs\\" + DbVersion + "' -Recurse -Force;");
                    PSI.Invoke();
                    PSI.Commands.Clear();
                    PSI.AddScript("Copy-Item -Path 'C:\\RegressionTester_Skyline\\Clean_Configs\\" + DbVersion + "' -Destination 'C:\\Skyline DataMiner Configs' -Recurse -Force;");
                    PSI.Invoke();
                    PSI.Commands.Clear();
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

        //Method to delete the testing MYSQL DMA 
        public void ClearDMA(string DbVersion)
        {
            using (PowerShell PSI = PowerShell.Create())
            {
                try
                {
                    foreach (Process proc in Process.GetProcessesByName("SLTaskbarUtility"))
                    {
                        proc.Kill();
                    }
                }
                catch (Exception errorMessage)
                {
                    LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": failed to kill SLTaskbarUtility " + Environment.NewLine + errorMessage.ToString() + ".");
                }

                try
                {
                    foreach (Process proc in Process.GetProcessesByName("w3wp"))
                    {
                        proc.Kill();
                    }
                }
                catch (Exception errorMessage)
                {
                    LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": failed to kill w3wp " + Environment.NewLine + errorMessage.ToString() + ".");
                }

                for (int index = 0; index < 3; index++)
                {

                    if (Directory.Exists(@"C:\Skyline DataMiner Configs\" + DbVersion))
                    {
                        PSI.AddScript("rm -r 'C:\\Skyline DataMiner Configs\\" + DbVersion + "' -Force;");
                        PSI.Invoke();
                        PSI.Commands.Clear();
                    }
                }

                try
                {
                    Process.Start(@"C:\Program Files (x86)\Skyline Communications\Skyline Taskbar Utility\SLTaskbarUtility.exe");
                }
                catch (Exception errorMessage)
                {
                    LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": failed to start SLTaskbarUtility " + Environment.NewLine + errorMessage.ToString() + ".");
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

        #endregion CopyAndClearFunctions

        #region XMLFunctions
        //Method that we use to make the file with all the fails, successess and warnings
        //In this method we also fill the lists with the fails, successess and warnings
        //We also want to know how long a script have been running in this method
        //Als last function in this method we make 2 excel files: one for the inforamtion events and one for the alarms
        public DateTime[] CallCLassMakeXmlSequential(string SelectedDirectory, RemotingConnection connectionArg,
            string name, ListsScripts listsSequentialArg, string upgradeVersionArg, string textCombocopyArg, string pathArg)
        {
            DateTime[] realRunningTime = new DateTime[] { DateTime.Now, DateTime.Now };

            MakeXMLSequential mxmls = new MakeXMLSequential(SelectedDirectory);
            realRunningTime = mxmls.GetRunningTime(name, listsSequentialArg, pathArg);
            mxmls.Execute(upgradeVersionArg, textCombocopyArg);

            return realRunningTime;
        }

        //In this method we fill the lists with the fails, successess and warnings
        //We also want to know how long a script have been running in this method
        //Als last function in this method we make 2 excel files: one for the inforamtion events and one for the alarms
        public void FillListsXMLParallel(string SelectedDirectory, RemotingConnection connectionArg, ListsScripts ListsScriptsArg, CyclusParallelTime cyclusParallelTimeArg)
        {
            foreach (NameScriptsDirectories path in filesScriptsParallel)
            {
                if (path.Name.EndsWith(".xml"))
                {
                    try
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(path.Path);
                        XmlNodeList ParametersNode = xml.FirstChild.SelectNodes("/DMSScript/Parameters");

                        if (!ParametersNode[0].HasChildNodes)
                        {
                            string[] splitString = path.Path.Split('\\');
                            string SelectedFolderPlusFileNameWithExtension = splitString[splitString.Length - 2] + "\\" + splitString[splitString.Length - 1];

                            MakeXMLParallel mxmlp = new MakeXMLParallel(SelectedDirectory, SelectedFolderPlusFileNameWithExtension);
                            mxmlp.GetRunningTime(path.Name, ListsScriptsArg, cyclusParallelTimeArg, path.Path);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingErrors.AddExceptionToList(String.Format("{0}: {1} : {2} .", DateTime.Now.ToString(), " error in read xml" + Environment.NewLine, ex.ToString()));
                    }

                }
                else if (path.Name.EndsWith(".dmimport"))
                {
                    if (File.Exists("C:\\Skyline DataMiner\\Scripts\\Script_" + Path.GetFileNameWithoutExtension(path.Name) + ".xml"))
                    {
                        try
                        {
                            XmlDocument xml = new XmlDocument();
                            xml.Load("C:\\Skyline DataMiner\\Scripts\\Script_" + Path.GetFileNameWithoutExtension(path.Name) + ".xml");
                            XmlNodeList ParametersNode = xml.FirstChild.SelectNodes("/DMSScript/Parameters");

                            if (!ParametersNode[0].HasChildNodes)
                            {
                                string[] splitString = path.Path.Split('\\');
                                string SelectedFolderPlusFileNameWithExtension = splitString[splitString.Length - 2] + "\\" + splitString[splitString.Length - 1];

                                MakeXMLParallel mxmlp = new MakeXMLParallel(SelectedDirectory, SelectedFolderPlusFileNameWithExtension);
                                mxmlp.GetRunningTime(path.Name, ListsScriptsArg, cyclusParallelTimeArg, path.Path);
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingErrors.AddExceptionToList(String.Format("{0}: {1} : {2} .", DateTime.Now.ToString(), " error in read xml" + Environment.NewLine, ex.ToString()));
                        }
                    }
                }
            }
        }

        #endregion XMLFunctions     
        
        #region RunScripts

        //The main method for running and get the parameters for running scripts sequential
        public void RunAllScriptsSequential(string directoryPath, string directory, List<NameScriptsDirectories> listWithScriptsToRun,
            RemotingConnection connectionArg, ListsScripts listsScriptArg, ObservableCollection<RegexClass> listRegexesArg,
            string upgradeVersion, string textCombocopyArg)
        {
            List<string> Iplist = new List<string>();
            Iplist.Add("127.0.0.1");

            DateTime[] TimeScriptRunning = new DateTime[2];

            foreach (NameScriptsDirectories script in listWithScriptsToRun)
            {
                if (tokenCrash.IsCancellationRequested == false)
                {
                    if (script.Name.EndsWith(".xml"))
                    {
                        try
                        {
                            XmlDocument xml = new XmlDocument();
                            xml.Load(script.Path);
                            XmlNodeList ParametersNode = xml.FirstChild.SelectNodes("/DMSScript/Parameters");
                            XmlNodeList ProtocolNode = xml.FirstChild.SelectNodes("/DMSScript/Protocols");

                            if (!ParametersNode[0].HasChildNodes)
                            {
                                if (!ProtocolNode[0].HasChildNodes)
                                {
                                    listsScriptArg.TotalAlarmsAndEvents.Clear();

                                    string[] partsScript = script.Path.Split('\\');

                                    CreateDirectory(directoryPath + "\\" + directory + "\\" + partsScript[partsScript.Length - 1]);

                                    RunScriptSequential(connectionArg, listsScriptArg, script.Name);

                                    if (tokenCrash.IsCancellationRequested == false)
                                    {
                                    TimeScriptRunning = CallCLassMakeXmlSequential(directoryPath + "\\" + directory + "\\" + script.Name + "\\", connectionArg, script.Name, listsScriptArg, upgradeVersion, textCombocopyArg, script.Path);
                                        listsScriptArg.AddToDictionary(directoryPath + "\\" + directory + "\\" + script.Name + "\\", TimeScriptRunning);

                                        using (PowerShell PSI = PowerShell.Create())
                                        {
                                            PSI.AddScript("Copy-Item -Path 'C:\\Skyline DataMiner\\logging' -Destination '" + directoryPath + "\\" + directory + "\\" + script.Name + "\\" + "' -Recurse -Force;");
                                            PSI.Invoke();
                                            PSI.Commands.Clear();
                                        }

                                        foreach (RegexClass item in listRegexesArg)
                                        {
                                            foreach (var item2 in item.Collection)
                                            {
                                                if (directory == item2.Key)
                                                {
                                                    foreach (var item3 in item2.Value)
                                                    {
                                                        if (item3.Name == script.Name)
                                                        {
                                                            if (item3.Path == script.Path)
                                                            {
                                                                if (File.Exists(item.Output))
                                                                {
                                                                    using (StreamWriter sw = File.AppendText(item.Output))
                                                                    {
                                                                        sw.WriteLine("Regex '{0}' (tijdstip: {1}) on the logging of {2} on '{3}'",
                                                                            item.Regex, DateTime.Now, item3.Name, upgradeVersion);

                                                                        foreach (string file in Directory.GetFiles(directoryPath + "\\" + directory + "\\" + script.Name + "\\logging"))
                                                                        {
                                                                            using (StreamReader fileLogging = new StreamReader(file))
                                                                            {
                                                                                string line;
                                                                                Regex reg = new Regex(item.Regex);

                                                                                while ((line = fileLogging.ReadLine()) != null)
                                                                                {
                                                                                    if (reg.IsMatch(line))
                                                                                    {
                                                                                        sw.WriteLine(line + "(" + Path.GetFileName(file) + ")");
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        sw.WriteLine();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    listsScriptArg.AddToListScriptsWithProtocols(script.Name);
                                }
                            }
                            else
                            {
                                listsScriptArg.AddToListScriptsWithParameters(script.Name);
                            }
                        }
                        catch(Exception ex)
                        {
                            LoggingErrors.AddExceptionToList(String.Format("{0}: {1} : {2} .", DateTime.Now.ToString(), " error in run script" + Environment.NewLine, ex.ToString()));
                        }
                    }
                    else if (script.Name.EndsWith(".dmimport"))
                    {
                        if (File.Exists("C:\\Skyline DataMiner\\Scripts\\Script_" + Path.GetFileNameWithoutExtension(script.Name) + ".xml"))
                        {
                            try
                            {
                                XmlDocument xml = new XmlDocument();
                                xml.Load("C:\\Skyline DataMiner\\Scripts\\Script_" + Path.GetFileNameWithoutExtension(script.Name) + ".xml");
                                XmlNodeList ParametersNode = xml.FirstChild.SelectNodes("/DMSScript/Parameters");

                                if (!ParametersNode[0].HasChildNodes)
                                {
                                    listsScriptArg.TotalAlarmsAndEvents.Clear();

                                    string[] partsScript = script.Path.Split('\\');

                                    CreateDirectory(directoryPath + "\\" + directory + "\\" + partsScript[partsScript.Length - 1]);

                                    RunScriptSequential(connectionArg, listsScriptArg, script.Name);

                                    if (tokenCrash.IsCancellationRequested == false)
                                    {
                                    TimeScriptRunning = CallCLassMakeXmlSequential(directoryPath + "\\" + directory + "\\" + script.Name + "\\", connectionArg, script.Name, listsScriptArg, upgradeVersion, textCombocopyArg, script.Path);
                                        listsScriptArg.AddToDictionary(directoryPath + "\\" + directory + "\\" + script.Name + "\\", TimeScriptRunning);

                                        using (PowerShell PSI = PowerShell.Create())
                                        {
                                            PSI.AddScript("Copy-Item -Path 'C:\\Skyline DataMiner\\logging' -Destination '" + directoryPath + "\\" + directory + "\\" + script.Name + "\\" + "' -Recurse -Force;");
                                            PSI.Invoke();
                                            PSI.Commands.Clear();
                                        }

                                        foreach (RegexClass item in listRegexesArg)
                                        {
                                            foreach (var item2 in item.Collection)
                                            {
                                                if (directory == item2.Key)
                                                {
                                                    foreach (var item3 in item2.Value)
                                                    {
                                                        if (item3.Name == script.Name)
                                                        {
                                                            if (item3.Path == script.Path)
                                                            {
                                                                if (File.Exists(item.Output))
                                                                {
                                                                    using (StreamWriter sw = File.AppendText(item.Output))
                                                                    {
                                                                        sw.WriteLine("Regex '{0}' (tijdstip: {1}) on the logging of {2} on '{3}'",
                                                                            item.Regex, DateTime.Now, item3.Name, upgradeVersion);

                                                                        foreach (string file in Directory.GetFiles(directoryPath + "\\" + directory + "\\" + script.Name + "\\logging"))
                                                                        {
                                                                            using (StreamReader fileLogging = new StreamReader(file))
                                                                            {
                                                                                string line;
                                                                                Regex reg = new Regex(item.Regex);

                                                                                while ((line = fileLogging.ReadLine()) != null)
                                                                                {
                                                                                    if (reg.IsMatch(line))
                                                                                    {
                                                                                        sw.WriteLine(line + "(" + Path.GetFileName(file) + ")");
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        sw.WriteLine();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} : {2} .", DateTime.Now.ToString(), " error in run script from Delt" + Environment.NewLine,ex.ToString()));
                            }
                        }
                    }
                }
            }
        }

        public void RunScriptSequential( RemotingConnection connectie, ListsScripts listsScriptArg, string ScriptDirectoryAndName)
        {
            string scriptnameWithoutExtensionSequential = Path.GetFileNameWithoutExtension(ScriptDirectoryAndName);
            string scriptnameWithoutExtraInfoSequential = scriptnameWithoutExtensionSequential.Substring(7);
            string DirectorynameSequential = Path.GetDirectoryName(ScriptDirectoryAndName);

            //RunAutomationScriptSequential ras = new RunAutomationScriptSequential(scriptnameWithoutExtensionSequential);
            RunAutomationScriptSequential.Execute(connectie, listsScriptArg, scriptnameWithoutExtensionSequential);
        }

        //The main method for running scripts parallel
        public void RunAllScriptsParallel(List<NameScriptsDirectories> listWithScriptsToRun, RemotingConnection connectionArg, 
            ListsScripts listsScriptArg, string ScriptDirectoryAndName, string directoryPathArg, string upgradeVersionArg)
        {
            if (tokenCrash.IsCancellationRequested == false)
            {
                List<string> Iplist = new List<string>();
                Iplist.Add("127.0.0.1");

                RunAutomationScriptParallel ra = new RunAutomationScriptParallel(listWithScriptsToRun, listsScriptArg);
                filesScriptsParallel.AddRange(listWithScriptsToRun);
                ra.execute(connectionArg, listsScriptArg, ScriptDirectoryAndName);
            }
        }
        
        #endregion RunScripts

        #region GetParameter

        //The method for get the parameters for running scripts sequential
        public void GetParametersSequential(string destinationPath, RemotingConnection connectionArg, DateTime[] TimeScriptRunning, DMA DMA_Arg)
        {
            List<string> Iplist = new List<string>();
            Iplist.Add("127.0.0.1");
           
                GetParameter gps = new GetParameter(DMA_ID, TimeScriptRunning, DMA_Arg);
                gps.Execute(destinationPath, connectionArg);
           
            
        }

        //The method for get the parameters for running scripts parallel
        public void GetParametersParallel(string destinationPath, RemotingConnection connectionArg, CyclusParallelTime cyclusTime, DMA DMA_Arg)
        {
            List<string> Iplist = new List<string>();
            Iplist.Add("127.0.0.1");

            DateTime[] timeCyclus = { cyclusTime.StartCyclus, cyclusTime.StopCyclus };

            GetParameter gpp = new GetParameter(DMA_ID, timeCyclus, DMA_Arg);
            gpp.Execute(destinationPath, connectionArg);
        }

        #endregion GetParameter              

        #region Environment
        //Method to make the environment, this is all the scripts,Delts and Bat installers.
        public List<NameScriptsDirectories> MEnvironment(string dir, List<NameScriptsDirectories> scripts, RemotingConnection connectionArg, ListsScripts listsScriptArg)
        {
            List<string> Iplist = new List<string>();
            Iplist.Add("127.0.0.1");

            MakeEnvironment isid = new MakeEnvironment(dir, scripts);
            List<NameScriptsDirectories> listScriptsRun = isid.Execute(connectionArg, listsScriptArg);

            return listScriptsRun;
        }

        //Method to Clear the environment you created with Bat installers in the MEnvironment function
        public void CEnvironment(string dir, RemotingConnection connectionArg, ListsScripts listsScriptArg)
        {
            List<string> Iplist = new List<string>();
            Iplist.Add("127.0.0.1");

            ClearEnvironment isid = new ClearEnvironment(dir);
            isid.Execute(connectionArg, listsScriptArg);
        }

        #endregion Environment

        //Method that make and send the mail if we run all the scripts
        public void RunMail(RemotingConnection connectionArg, ListsScripts listsScriptArg, string emailAddressArg, string upgradeVersion, string textCombocopyArg)
        {
            SendMailWithReport smwrs = new SendMailWithReport();

            if (tokenCrash.IsCancellationRequested == false)
                smwrs.Execute(connectionArg, listsScriptArg, emailAddressArg, upgradeVersion, textCombocopyArg);
            else
                smwrs.ExecuteToken(connectionArg, emailAddressArg, upgradeVersion, textCombocopyArg, DMA_ID);

        }
        //Saves credentials that have all the rights on the remote computers, can also be network admin
        public void SetUser(String userName, String password)
        {
            //The domain must be in username so the right domain account gets added to the unattended.xml for the syspreps
            if (userName.Contains("SKYLINE2\\"))
            {
                UserName = userName;
            }
            else
            {
                UserName = "SKYLINE2\\" + userName;
            }
            Password = password;
        }
       
        //Method to get the last startup time in startup.txt and the last startup moment in versionhistory.txt
        //Combine these 2 datas and write it to the file where we want to know al the startup times
        public string GetLastStartup(bool writeArg)
        {
            string VersionDMA = "";

            if (File.Exists(@"C:\Skyline DataMiner\Upgrades\VersionHistory.txt"))
            {
                if (File.Exists(@"C:\Skyline DataMiner\System Cache\SLNet\startup.txt"))
                {
                    if (File.Exists(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Startup_DMA_time.xml"))
                    {
                        try
                        {
                            string[] linesVersionHistory = File.ReadAllLines(@"C:\Skyline DataMiner\Upgrades\VersionHistory.txt");
                            string[] partsVersionHistory = linesVersionHistory[linesVersionHistory.Length - 1].Split(';');

                            string[] partsStartup = null;

                            VersionDMA = partsVersionHistory[partsVersionHistory.Length - 1];

                            using (StreamReader streamReader = new StreamReader(@"C:\Skyline DataMiner\System Cache\SLNet\startup.txt"))
                            {
                                partsStartup = streamReader.ReadToEnd().Split(';');
                            }

                            ManualResetEventSlim wait = new ManualResetEventSlim(false);
                            wait.Wait(500);

                            if (writeArg == true)
                            {
                                XDocument doc = XDocument.Load(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Startup_DMA_time.xml");
                                XElement root = new XElement("Startup");
                                root.Add(new XElement("Version", VersionDMA));
                                root.Add(new XElement("StartTime", partsVersionHistory[0]));
                                root.Add(new XElement("Duration", partsStartup[partsStartup.Length - 1]));
                                doc.Element("root").Add(root);
                                doc.Save(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Startup_DMA_time.xml");
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "GetlastStartup problem.", ex.Message));
                        }
                    }
                    else
                    {
                        LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + @" Could not find file: C:\RegressionTester_Skyline\Current_Test\General_Information\Startup_DMA_time.xml");
                    }
                }
                else
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "file not found exception", @"C:\Skyline DataMiner\System Cache\SLNet\startup.txt"));
                }
            }
            else
            {
                LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + @" Could not find file: C:\Skyline DataMiner\Upgrades\VersionHistory.txt");
            }

            return VersionDMA;
        }

        public void SendMessageDispatcher(RemotingConnection remotingConnection)
        {  
            DateTime boundary = DateTime.Now - TimeSpan.FromDays(1);

            try
            {
                CheckFiles(@"c:\skyline dataminer\logging\crashdump", "*.zip", boundary, "Crashdumps detected");
                CheckFiles(@"c:\skyline dataminer\logging\minidump", "*_Processes Disappeared.zip", boundary, "Process Disappearances detected");
                CheckFiles(@"c:\skyline dataminer\logging\minidump", "*_SL*.zip", boundary, "RTEs detected");

                if (_amountFailures == 0)
                {
                    try
                    {
                        GetInfoMessage message = new GetInfoMessage(DMA_ID, InfoType.DataMinerInfo);
                        GetDataMinerInfoResponseMessage messageResponse = remotingConnection.HandleSingleResponseMessage(message) as GetDataMinerInfoResponseMessage;
                    }
                    catch (Exception ex)
                    {
                        LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed in timer", ex.Message));
                    }
                }
                else
                {
                    _amountFailures = 0;
                    tokenSourceCrash.Cancel();
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1}", DateTime.Now, "CrashDump or MiniDump found"));
                    RunAutomationScriptSequential.abortscript();
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed in checking for crashfiles", ex.Message));
            }
        }

        private void CheckFiles(string path, string searchPattern, DateTime boundary, string message)
        {
            string[] files = Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
            if (Tools.Length(files) > 0)
            {
                List<string> rawFileNames = new List<string>();
                Array.ForEach(files,
                    fullPath =>
                    {
                        FileInfo fi = new FileInfo(fullPath);
                        if (fi.CreationTime > boundary)
                        {
                            rawFileNames.Add(System.IO.Path.GetFileNameWithoutExtension(fullPath));
                        }
                    }
                    );

                if (rawFileNames.Count > 0)
                {
                    _amountFailures++;
                }
            }
        }

        public void GetIDFunction(RemotingConnection connectionArg)
        {
            List<string> Iplist = new List<string>();
            Iplist.Add("127.0.0.1");

            GetID gi = new GetID();
            GetDataMinerInfoResponseMessage message = gi.GetInfoMessage(connectionArg);

            if (message != null)
                DMA_ID = message.ID;
            else
            {
                DMA_ID = -1;
            }
        }


    }
}
