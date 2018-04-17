﻿using DevRegTest.domainAutomationScript.Connection.MakeOurCloseConnection;
using DevRegTest.domainAutomationScript.Element.AbortAutomationScript;
using DevRegTest.domainAutomationScript.Report;
using DevRegTest.DomainAutomationScript.Element.Import;
using DevRegTest.DomainAutomationScript.Report;
using DevRegTest.domainVMApp;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace DevRegTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        //String with the name of config you want to copy the directory 'file'
        public string ConfigCopyName { get; set; }

        //String to know which upgrade package you have selected
        public string UpgradeVersion { get; set; }       

        // string filled with the author (metadata kolomtreeview) of a script from the shares
        public string AuthorArg { get; set; }

        // string filled with the attached processes (metadata kolomtreeview) of a script from the shares
        public string ProcessArg { get; set; }

        // string filled with the version (metadata kolomtreeview) of a script from the shares
        public string VersionArg { get; set; }

        // string filled with extra meta info (metadata kolomtreeview) of a script from the shares
        public string ExtraInfo { get; set; }

        //bool to check if the application is started, to enable certain buttons
        public bool Runned { get; set; }

        //Bool to know if you want to get a mail with you successes, warnings and fails
        public bool IsCheckedMail { get; set; }

        //Bool to know if the DB type of the Testing Config is the same as the config DB type you selected
        public bool CheckDatabaseFileIsTheSameAsConfig { get; set; }

        //Bool to know if the program is runned from commandline or from GUI
        public bool CmdOurUi { get; set; }

        //The global instance from the class 'DMA'
        public DMA DMAVU { get; set; }

        //The global instance from the class 'ListsScripts'
        public ListsScripts listsScript { get; set; }

        //The global instance from the class 'MakeCloseConnection', to get our connection over the whole program
        public MakeCloseConnection GlobalConnection { get; set; } 

        //The global instance from the class 'AbortAndGetRunningScripts'
        public AbortAndGetRunningScripts abortAndGetRunningScripts { get; set; }

        //List with all the Regexes from the WPF window
        public ObservableCollection<RegexClass> listRegexes { get; set; }

        //List with all the Regexes from the WPF window
        public ObservableCollection<RegexClass> listRegexesCheck { get; set; }        

        // property with all the Items from the TreeViewMetaData
        public TreeViewMetaData TreeViewMetaDataItems { get; set; }

        public TreeViewMetaData TreeViewMetaDataItemsAfterSearch { get; set; }

        // Delegate to make it possible to write from background Thread to GUI
        public delegate void UpdateTextCallback(string message);

        public List<string> VersionsUpgrade { get; set; }

        // Dictionary with all the selected scripts in the TreeView
        Dictionary<string, List<NameScriptsDirectories>> DirectorieWithActiveScripts { get; set; }

        // property to make the overview XML file with all the Scripts runned over the different version with all there successes and fails.
        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> overviewVersion { get; set; }

        #endregion Properties

        #region Constructor
        public MainWindow(Dictionary<string, string> textCmdAr, Dictionary<string, bool> boolsCmdAr,
            Dictionary<string, List<NameScriptsDirectories>> DirectorieWithActiveScriptsCmdArg)
        {
            InitializeComponent();

            if (textCmdAr != null && boolsCmdAr != null && DirectorieWithActiveScriptsCmdArg != null)
            {
                CmdOurUi = false;
            }
            else
            {
                CmdOurUi = true;
            }

            //Initialise the current DMA class and set the user
            DMAVU = Init.initialisatie(CmdOurUi);
            string name = Environment.MachineName;
            string nameUser = Environment.UserName;

            DMAVU.SetUser(nameUser, "Skyline321");

            comboCopy.Items.Add("Testing_Cassandra");
            comboCopy.Items.Add("Testing_MySQL");
            comboCopy.SelectedIndex = 0;

            comboSearch.Items.Add("Scriptname");
            comboSearch.Items.Add("Author");
            comboSearch.Items.Add("Process");
            comboSearch.Items.Add("Version");
            comboSearch.SelectedIndex = 0;

            //Make directories and files for hold all the information that is need for the program
            DMAVU.CreateDirectoryWithoutDeleting(@"C:\RegressionTester_Skyline");
            DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Scripts");            
            DMAVU.CreateDirectoryWithoutDeleting(@"C:\RegressionTester_Skyline\Backup_Files_Directory");
            DMAVU.CreateDirectoryWithoutDeleting(@"C:\RegressionTester_Skyline\Clean_Configs");
            
            Init.MakeXmlCopyToShares(@"C:\RegressionTester_Skyline\Scripts\CopyToShares.xml");
            Init.MakeXmlUpgrade(@"C:\RegressionTester_Skyline\Scripts\ExecuteUpgradeDMTask.xml");

            //Here we declare the instance of the classes in the properties
            listsScript = new ListsScripts();
            abortAndGetRunningScripts = new AbortAndGetRunningScripts();
            DirectorieWithActiveScripts = new Dictionary<string, List<NameScriptsDirectories>>();
            listRegexes = new ObservableCollection<RegexClass>();
            listRegexesCheck = new ObservableCollection<RegexClass>();
            VersionsUpgrade = new List<string>();
            TreeViewMetaDataItems = new TreeViewMetaData() { SubItems = new List<TreeViewMetaData>() };
            TreeViewMetaDataItemsAfterSearch = new TreeViewMetaData() { SubItems = new List<TreeViewMetaData>() };
            overviewVersion = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();           

            Runned = false;
            AuthorArg = "";
            ProcessArg = "";
            VersionArg = "";
            ExtraInfo = "";

            //Update all the comboboxes and checkboxList
            UpdateCombobox(DMAVU);
            UpdateComboboxConfigConfiguration(DMAVU);
            treeViewDirectoriesScripts2.Items.Clear();
            FillTreeViews(DMAVU);

            //Our global connection for the whole program and look if there is an connection, otherwise we
            //Shut down the application
            GlobalConnection = new MakeCloseConnection();
            if (!GlobalConnection.MakeConnection(CmdOurUi))
                Environment.Exit(0);

            //Get the DMA ID from your own
            DMAVU.GetIDFunction(GlobalConnection.GetConnection());

            LinearGradientBrush gradientBrush = new LinearGradientBrush(Color.FromRgb(224, 224, 224), Color.FromRgb(255, 255, 255), new Point(0.5, 0), new Point(0.5, 1));
            UpperGrid.Background = gradientBrush;

            MultiColomTree.Items.Clear();
            MultiColomTree.ItemsSource = TreeViewMetaDataItems.SubItems;

            dataGridRegexes.Items.Clear();
            dataGridRegexes.ItemsSource = listRegexes;

            if (textCmdAr != null && boolsCmdAr != null && DirectorieWithActiveScriptsCmdArg != null)
            {
                OverallFunction(textCmdAr["combocopy".ToLower()], textCmdAr["comboVersion".ToLower()], textCmdAr["comboVersionMinimum".ToLower()], 
                    textCmdAr["comboConfigs".ToLower()], textCmdAr["tbEmailAddress".ToLower()], boolsCmdAr["cbcopyFiles".ToLower()], boolsCmdAr["cbSequentieel".ToLower()],
                    boolsCmdAr["cbParallel".ToLower()], boolsCmdAr["cbBinary".ToLower()], boolsCmdAr["cbVersionMinimum".ToLower()], boolsCmdAr["cbMail".ToLower()], 
                    boolsCmdAr["cbUpgradeSequential".ToLower()], DirectorieWithActiveScriptsCmdArg);
            }           
        }

        #endregion Constructor

        #region TreeViews_Functions
        private void FillTreeViews(DMA DMAVU)
        {
            foreach (NameScriptsDirectories directorie in DMAVU.ScriptsDirectories)
            {
                var rootDirectoryInfo = new DirectoryInfo(directorie.Path);

                TreeViewMetaDataItems.SubItems.Add(CreateDirectoryNode_MetaData(rootDirectoryInfo, DMAVU));
                treeViewDirectoriesScripts2.Items.Add(CreateDirectoryNode_Regex(rootDirectoryInfo, DMAVU));
            }
        }

        private TreeViewMetaData CreateDirectoryNode_MetaData(DirectoryInfo rootDirectoryInfo, DMA DMAVU)
        {
            TreeViewMetaData directoryNode = new TreeViewMetaData()
            {
                Name = new CheckBox()
                {
                    Content = rootDirectoryInfo.Name
                },
                SubItems = new List<TreeViewMetaData>()
            };

            (directoryNode.Name as CheckBox).Checked += Checkbox_Checked_TreeViewMetaData;
            (directoryNode.Name as CheckBox).Unchecked += Checkbox_Unchecked_TreeViewMetaData;

            Dictionary<string, List<string>> DictionaryXmlMetaData = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var file in rootDirectoryInfo.GetFiles().Where(d => d.Extension == ".xml" && d.Name.StartsWith("RT") || d.Extension == ".dmimport").OrderByDescending(p => p.CreationTime).ToArray())
            {
                List<XmlNode> li = new List<XmlNode>();

                foreach (FileInfo metadataFile in rootDirectoryInfo.GetFiles().Where(e => e.Name.EndsWith(file.Name.Substring(3)) && e.Name.StartsWith("MD")))
                {
                    XmlDocument xml = new XmlDocument();

                    try
                    {
                        xml.Load(metadataFile.FullName);

                        if (xml.DocumentElement.Name.ToLower() == "testinfo")
                        {
                            List<string> Nodes = new List<string>();
                            List<string> ListDuplicateNodes = new List<string>();
                            Dictionary<string, int> DictionaryDuplicateNodes = new Dictionary<string, int>();

                            foreach (XmlNode xmlNode in xml.DocumentElement.ChildNodes)
                            {
                                if (Nodes.Contains(xmlNode.Name))
                                {
                                    if (!DictionaryDuplicateNodes.ContainsKey(xmlNode.Name))
                                    {
                                        DictionaryDuplicateNodes.Add(xmlNode.Name, 2);
                                        ListDuplicateNodes.Add(xmlNode.Name);
                                    }
                                    else
                                    {
                                        int i = DictionaryDuplicateNodes[xmlNode.Name];
                                        i++;
                                        DictionaryDuplicateNodes[xmlNode.Name] = i;
                                    }
                                }
                                Nodes.Add(xmlNode.Name);
                            }

                            List<string>[] ListArrayContentDuplicateNodes = new List<string>[DictionaryDuplicateNodes.Count];
                            int indexList = 0;
                            string NameNode = "";
                            int OneTime = 0;

                            foreach (XmlNode xmlNode in xml.DocumentElement.ChildNodes)
                            {
                                if (DictionaryDuplicateNodes.ContainsKey(xmlNode.Name))
                                {
                                    if (OneTime == 0)
                                    {
                                        NameNode = xmlNode.Name;
                                    }
                                    OneTime = 1;

                                    if (NameNode == xmlNode.Name)
                                    {
                                        if (ListArrayContentDuplicateNodes[indexList] == null)
                                            ListArrayContentDuplicateNodes[indexList] = new List<string>() { xmlNode.InnerText };
                                        else
                                            ListArrayContentDuplicateNodes[indexList].Add(xmlNode.InnerText);
                                    }
                                    else
                                    {
                                        indexList++;
                                        if (ListArrayContentDuplicateNodes[indexList] == null)
                                            ListArrayContentDuplicateNodes[indexList] = new List<string>() { xmlNode.InnerText };
                                        else
                                            ListArrayContentDuplicateNodes[indexList].Add(xmlNode.InnerText);
                                    }

                                    NameNode = xmlNode.Name;
                                }
                                else
                                {
                                    DictionaryXmlMetaData.Add(xmlNode.Name, new List<string>(new string[] { xmlNode.InnerText }));
                                }
                            }

                            for (int x = 0; x < ListArrayContentDuplicateNodes.Count(); x++)
                            {
                                DictionaryXmlMetaData.Add(ListDuplicateNodes[x], ListArrayContentDuplicateNodes[x]);
                            }
                        }

                        foreach (var KeyItem in DictionaryXmlMetaData)
                        {
                            if (KeyItem.Key.ToLower() == "author")
                            {
                                foreach (string i in DictionaryXmlMetaData["author"])
                                {
                                    AuthorArg += i + " , ";
                                }
                                AuthorArg = AuthorArg.Remove(AuthorArg.Length - 3, 3);
                            }
                            else if (KeyItem.Key.ToLower() == "process")
                            {
                                foreach (string i in DictionaryXmlMetaData["Process"])
                                {
                                    ProcessArg += i + " , ";
                                }
                                ProcessArg = ProcessArg.Remove(ProcessArg.Length - 3, 3);
                            }
                            else if (KeyItem.Key.ToLower() == "version")
                            {
                                foreach (string i in DictionaryXmlMetaData["version"])
                                {
                                    VersionArg += i + " , ";
                                }
                                VersionArg = VersionArg.Remove(VersionArg.Length - 3, 3);
                            }
                            else
                            {
                                ExtraInfo += KeyItem.Key + ": ";

                                foreach (string i in DictionaryXmlMetaData[KeyItem.Key])
                                {
                                    ExtraInfo += i + " , ";
                                }

                                ExtraInfo = ExtraInfo.Remove(ExtraInfo.Length - 3, 3);
                                ExtraInfo += Environment.NewLine;
                            }
                        }

                        DictionaryXmlMetaData.Clear();
                    }
                    catch (XmlException ex)
                    {
                        LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "XML error", "Something wrong with the XML Syntax in " + metadataFile.FullName));
                    }
                    catch (Exception ex)
                    {
                        LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ex.InnerException.ToString());
                    }
                }

                TreeViewMetaData treenode = new TreeViewMetaData()
                {
                    Name = new CheckBox
                    {
                        Content = file.Name
                    },
                    Author = new TextBlock()
                    {
                        Text = AuthorArg
                    },
                    Process = new TextBlock()
                    {
                        Text = ProcessArg
                    },
                    Version = new TextBlock()
                    {
                        Text = VersionArg
                    }
                };

                if(ExtraInfo != "")
                {
                    treenode.Extra = new Image()
                    {
                        Source = new BitmapImage(new Uri("ButtonPlus.png", UriKind.Relative)),
                        ToolTip = ExtraInfo,
                        Width = 10
                    };
                }

                directoryNode.SubItems.Add(treenode);
                DMAVU.DirectoriesAndChilderenVar.Add(new NameScriptsDirectories(file.Name, file.FullName));

                AuthorArg = "";
                ProcessArg = "";
                VersionArg = "";
                ExtraInfo = "";
            }

            return directoryNode;
        }

        private TreeViewItem CreateDirectoryNode_Regex(DirectoryInfo rootDirectoryInfo, DMA DMAVU)
        {
            TreeViewItem directoryNode = new TreeViewItem()
            {
                Header = new CheckBox
                {
                    Content = rootDirectoryInfo.Name
                }
            };

            (directoryNode.Header as CheckBox).Checked += Checkbox_Checked_TreeViewRegex;
            (directoryNode.Header as CheckBox).Unchecked += Checkbox_Unchecked_TreeViewRegex;

            foreach (var file in rootDirectoryInfo.GetFiles().Where(d => d.Extension == ".xml" && d.Name.StartsWith("RT") || d.Extension == ".dmimport").OrderByDescending(p => p.CreationTime).ToArray())
            {
                var treenode = new TreeViewItem()
                {
                    Header = new CheckBox
                    {
                        Content = file.Name
                    }
                };

                directoryNode.Items.Add(treenode);
                DMAVU.DirectoriesAndChilderenVar.Add(new NameScriptsDirectories(file.Name, file.FullName));
            }

            return directoryNode;
        }

        //function to check all the underlying checkboxes if the main checkbox from a directory on the shares is unchecked in the treeview
        private void Checkbox_Checked_TreeViewMetaData(object sender, RoutedEventArgs e)
        {
            var item = sender as CheckBox;

            foreach (TreeViewMetaData itemChecked in MultiColomTree.Items)
            {
                if (item.Content == (itemChecked.Name as CheckBox).Content)
                {
                    CheckAllChildNodesMetaData(itemChecked, true);
                }
            }
        }

        //function to uncheck all the underlying checkboxes if the main checkbox from a directory on the shares is unchecked in the treeview
        private void Checkbox_Unchecked_TreeViewMetaData(object sender, RoutedEventArgs e)
        {
            var item = sender as CheckBox;

            foreach (TreeViewMetaData itemUnChecked in MultiColomTree.Items)
            {
                if (item.Content == (itemUnChecked.Name as CheckBox).Content)
                {
                    CheckAllChildNodesMetaData(itemUnChecked, false);
                }
            }
        }

        //function to check all the underlying checkboxes if the main checkbox from a directory on the shares is unchecked in the Regextreeview
        private void Checkbox_Checked_TreeViewRegex(object sender, RoutedEventArgs e)
        {
            var item = sender as CheckBox;

            foreach (TreeViewItem itemChecked in treeViewDirectoriesScripts2.Items)
            {
                if (item.Content == (itemChecked.Header as CheckBox).Content)
                {
                    CheckAllChildNodesRegex(itemChecked, true);
                }
            }
        }

        //function to uncheck all the underlying checkboxes if the main checkbox from a directory on the shares is unchecked in the Regextreeview
        private void Checkbox_Unchecked_TreeViewRegex(object sender, RoutedEventArgs e)
        {
            var item = sender as CheckBox;

            foreach (TreeViewItem itemUnChecked in treeViewDirectoriesScripts2.Items)
            {
                if (item.Content == (itemUnChecked.Header as CheckBox).Content)
                {
                    CheckAllChildNodesRegex(itemUnChecked, false);
                }
            }
        }

        private void CheckAllChildNodesMetaData(TreeViewMetaData treeNode, bool nodeChecked)
        {
            foreach (TreeViewMetaData node in treeNode.SubItems)
            {
                (node.Name as CheckBox).IsChecked = nodeChecked;

                if (node.SubItems != null)
                    if (node.SubItems.Count > 0)
                        this.CheckAllChildNodesMetaData(node, nodeChecked);
            }
        }

        private void CheckAllChildNodesRegex(TreeViewItem treeNode, bool nodeChecked)
        {
            foreach (TreeViewItem node in treeNode.Items)
            {
                (node.Header as CheckBox).IsChecked = nodeChecked;

                if (node.Items.Count > 0)
                    this.CheckAllChildNodesRegex(node, nodeChecked);
            }
        }

        #endregion TreeViews_Functions

        #region Functions

        private void RunningScripts(bool SequentialArg, bool ParallelArg,
           Dictionary<string, List<NameScriptsDirectories>> DirectorieWithActiveScriptsArg, string EmailAdressArg, string UpgradeVersion, string TextCombocopyArg)
        {
            if (GlobalConnection.GetConnection().IsAuthenticated)
            {
                // Timer with duration of 30 seconds (used to keep connection with Dataminer open).
                Timer timer = new Timer(Timer_Tick);
                timer.Change(30000, 30000);

                
                if (DMA.tokenCrash.IsCancellationRequested == false)
                {
                    TogetherAbortFromRunningScripts();
                }

                if (CmdOurUi == true)
                {
                    if (UpgradeVersion != string.Empty)
                    {
                        statusProgramStrip.Dispatcher.Invoke(
                             new UpdateTextCallback(this.UpdateTextStatusProgramStrip),
                             new object[] { "Make trend element on '" + UpgradeVersion + "'" });
                    }
                    else
                    {
                        statusProgramStrip.Dispatcher.Invoke(
                             new UpdateTextCallback(this.UpdateTextStatusProgramStrip),
                             new object[] { "Make trend element on '" + DMAVU.GetLastStartup(false) + "'" });
                    }
                }
                else
                {
                    App.CommandLog("Make trend element on '" + UpgradeVersion + "'");
                }


                if (DMA.tokenCrash.IsCancellationRequested == false)
                {
                    //Import delt with trend element in and microsoft protocol into local DMA
                    DMAVU.ImportElementAndCheckIfElementIsStarted(GlobalConnection.GetConnection());
                }

                ManualResetEventSlim wait1 = new ManualResetEventSlim(false);
                wait1.Wait(3000);

                if (DMA.tokenCrash.IsCancellationRequested == false)
                {
                    ManualResetEventSlim waitBetween = new ManualResetEventSlim(false);
                    waitBetween.Wait(1000);

                    if (CmdOurUi == true)
                    {
                        if (UpgradeVersion != string.Empty)
                        {
                            statusProgramStrip.Dispatcher.Invoke(
                                 new UpdateTextCallback(this.UpdateTextStatusProgramStrip),
                                 new object[] { "Run script(s) on '" + UpgradeVersion + "'" });
                        }
                        else
                        {
                            statusProgramStrip.Dispatcher.Invoke(
                                 new UpdateTextCallback(this.UpdateTextStatusProgramStrip),
                                 new object[] { "Run script(s) on '" + DMAVU.GetLastStartup(false) + "'" });
                        }
                    }
                    else
                    {
                        App.CommandLog("Run script(s) on '" + UpgradeVersion + "'");
                    }

                    string directoryPath = string.Empty;

                    if (SequentialArg == true)
                    {
                        if (DMA.tokenSourceCrash.IsCancellationRequested == false)
                        {
                            if (UpgradeVersion != string.Empty)
                            {
                                DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\Sequential_" + UpgradeVersion);
                                directoryPath = @"C:\RegressionTester_Skyline\Current_Test\Sequential_" + UpgradeVersion;
                            }
                            else
                            {
                                DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\Sequential_" + DMAVU.GetLastStartup(false));
                                directoryPath = @"C:\RegressionTester_Skyline\Current_Test\Sequential_" + DMAVU.GetLastStartup(false);
                            }
                        }

                        foreach (var directory in DirectorieWithActiveScriptsArg)
                        {
                            if (DMA.tokenSourceCrash.IsCancellationRequested == false)
                            {
                                if (UpgradeVersion != string.Empty)
                                {
                                    statusProgramStrip.Dispatcher.Invoke(
                                            new UpdateTextCallback(this.UpdateTextboxConsole),
                                            new object[] { string.Format("{0} : Starting with directory '{1}' on '{2}'.\n", DateTime.Now, directory.Key, UpgradeVersion) });
                                }
                                else
                                {
                                    statusProgramStrip.Dispatcher.Invoke(
                                             new UpdateTextCallback(this.UpdateTextboxConsole),
                                             new object[] { string.Format("{0} : Starting with directory '{1}' on '{2}'.\n", DateTime.Now, directory.Key, DMAVU.GetLastStartup(false)) });
                                }

                                //Run Bat files first, then import Delts add every script that has to run in listScriptXml
                                List<NameScriptsDirectories> listScriptsXml = DMAVU.MEnvironment(@"\\NAS\Shares\Public\Software Development\Testing\Scripts\" + directory.Key, directory.Value, GlobalConnection.GetConnection(), listsScript);

                                ManualResetEventSlim wait = new ManualResetEventSlim(false);
                                wait.Wait(500);

                                DMAVU.CreateDirectoryWithoutDeleting(directoryPath + "\\" + directory.Key.ToString());
                                
                                // Runs every script from listScriptXml sequential
                                DMAVU.RunAllScriptsSequential(directoryPath, directory.Key, listScriptsXml, GlobalConnection.GetConnection(), listsScript, listRegexes, UpgradeVersion, TextCombocopyArg);

                                if (DMA.tokenCrash.IsCancellationRequested == false)
                                {
                                    DMAVU.CEnvironment(@"\\NAS\Shares\Public\Software Development\Testing\Scripts\" + directory.Key, GlobalConnection.GetConnection(), listsScript);

                                    if (UpgradeVersion != string.Empty)
                                    {
                                        statusProgramStrip.Dispatcher.Invoke(
                                            new UpdateTextCallback(this.UpdateTextboxConsole),
                                            new object[] { string.Format("{0} : Ready with directory '{1}' on '{2}'.\n", DateTime.Now, directory.Key, UpgradeVersion) });
                                    }
                                    else
                                    {
                                        statusProgramStrip.Dispatcher.Invoke(
                                            new UpdateTextCallback(this.UpdateTextboxConsole),
                                            new object[] { string.Format("{0} : Ready with directory '{1}' on '{2}'.\n", DateTime.Now, directory.Key, DMAVU.GetLastStartup(false)) });
                                    }
                                }
                            }
                        }

                        if (DMA.tokenSourceCrash.IsCancellationRequested == false)
                        {
                            //Restart the trendElement (to put trenddata in DB)
                            DMAVU.RestartElements(GlobalConnection.GetConnection());

                            ManualResetEventSlim wait2 = new ManualResetEventSlim(false);
                            wait2.Wait(1000);

                            foreach (string key in listsScript.DictionaryScript.Keys)
                            {
                                if (DMA.tokenSourceFileNotFound.IsCancellationRequested == false)
                                {
                                    // Get trenddata sequential
                                    DMAVU.GetParametersSequential(key, GlobalConnection.GetConnection(), listsScript.DictionaryScript[key], DMAVU);
                                }
                                else
                                {
                                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1}", DateTime.Now, " TrendDeltImport not done, no trenddata measured."));
                                }
                            }
                        }
                    }
                    else if (ParallelArg == true)
                    {
                        if (DMA.tokenSourceCrash.IsCancellationRequested == false)
                        {
                            if (UpgradeVersion != string.Empty)
                            {
                                DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\Parallel_" + UpgradeVersion);
                                directoryPath = @"C:\RegressionTester_Skyline\Current_Test\Parallel_" + UpgradeVersion;
                            }
                            else
                            {
                                DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\Parallel_" + DMAVU.GetLastStartup(false));
                                directoryPath = @"C:\RegressionTester_Skyline\Current_Test\Parallel_" + DMAVU.GetLastStartup(false);
                            }
                        }

                        DMAVU.filesScriptsParallel.Clear();
                        CyclusParallelTime cyclusParallelTime = new CyclusParallelTime();

                        listsScript.TotalAlarmsAndEvents.Clear();

                        foreach (var directory in DirectorieWithActiveScriptsArg)
                        {
                            if (DMA.tokenSourceCrash.IsCancellationRequested == false)
                            {
                                if (UpgradeVersion != string.Empty)
                                {
                                    statusProgramStrip.Dispatcher.Invoke(
                                            new UpdateTextCallback(this.UpdateTextboxConsole),
                                            new object[] { string.Format("{0} : Starting with directory '{1}' on '{2}'.\n", DateTime.Now, directory.Key, UpgradeVersion) });
                                }
                                else
                                {
                                    statusProgramStrip.Dispatcher.Invoke(
                                             new UpdateTextCallback(this.UpdateTextboxConsole),
                                             new object[] { string.Format("{0} : Starting with directory '{1}' on '{2}'.\n", DateTime.Now, directory.Key, DMAVU.GetLastStartup(false)) });
                                }

                                //Run Bat files first, then import Delts add every script that has to run in listScriptXml
                                List<NameScriptsDirectories> listScriptsXml = DMAVU.MEnvironment(@"\\NAS\Shares\Public\Software Development\Testing\Scripts\" + directory.Key, directory.Value, GlobalConnection.GetConnection(), listsScript);

                                ManualResetEventSlim waiter = new ManualResetEventSlim(false);
                                waiter.Wait(500);

                                //Runs all script parallel from listScriptXml
                                DMAVU.RunAllScriptsParallel(listScriptsXml, GlobalConnection.GetConnection(), listsScript, directory.Key, directoryPath, UpgradeVersion);

                                if (DMA.tokenSourceCrash.IsCancellationRequested == false)
                                {
                                    DMAVU.FillListsXMLParallel(directoryPath, GlobalConnection.GetConnection(), listsScript, cyclusParallelTime);
                                    DMAVU.filesScriptsParallel.Clear();
                                    DMAVU.CEnvironment(@"\\NAS\Shares\Public\Software Development\Testing\Scripts\" + directory.Key, GlobalConnection.GetConnection(), listsScript);

                                    if (UpgradeVersion != string.Empty)
                                    {
                                        statusProgramStrip.Dispatcher.Invoke(
                                            new UpdateTextCallback(this.UpdateTextboxConsole),
                                            new object[] { string.Format("{0} : Ready with directory '{1}' on '{2}'.\n", DateTime.Now, directory.Key, UpgradeVersion) });
                                    }
                                    else
                                    {
                                        statusProgramStrip.Dispatcher.Invoke(
                                            new UpdateTextCallback(this.UpdateTextboxConsole),
                                            new object[] { string.Format("{0} : Ready with directory '{1}' on '{2}'.\n", DateTime.Now, directory.Key, DMAVU.GetLastStartup(false)) });
                                    }
                                }
                            }
                        }

                        if (DMA.tokenSourceCrash.IsCancellationRequested == false)
                        {
                            if(DMA.tokenSourceFileNotFound.IsCancellationRequested == false)
                            {
                                //Restart the trendElement (to put trenddata in DB)
                                DMAVU.RestartElements(GlobalConnection.GetConnection());

                                ManualResetEventSlim wait2 = new ManualResetEventSlim(false);
                                wait2.Wait(1000);

                                //Get parallel trenddata 
                                DMAVU.GetParametersParallel(directoryPath, GlobalConnection.GetConnection(), cyclusParallelTime, DMAVU);
                            }
                            else
                            {
                                LoggingErrors.AddExceptionToList(String.Format("{0}: {1}", DateTime.Now, " TrendDeltImport not done, no trenddata measured."));
                            }
                            
                            //Makes the directories with all the alarmevents,informationevents, all the scripts failed/succeeded
                            DMAVU.MakeXMLDocumentParallel(directoryPath, listsScript, UpgradeVersion, TextCombocopyArg);
                           
                        }
                           
                    }

                    if (DMA.tokenSourceCrash.IsCancellationRequested == false)
                    {
                        //this function sends a mail with the fails/successes
                        if (IsCheckedMail == true)
                            DMAVU.RunMail(GlobalConnection.GetConnection(), listsScript, EmailAdressArg, UpgradeVersion, TextCombocopyArg);

                        DMAVU.RunMail(GlobalConnection.GetConnection(), listsScript, "jochen.deleu@skyline.be", UpgradeVersion, TextCombocopyArg);
                        DMAVU.RunMail(GlobalConnection.GetConnection(), listsScript, "martijn.vanallemeersch@skyline.be", UpgradeVersion, TextCombocopyArg);

                        DMAVU.CreateXMLFileWithoutDeleting(directoryPath + "\\InformationRun.xml");
                        DMAVU.MakeXMLDocumentWithInfo(directoryPath + "\\InformationRun.xml", listsScript, SequentialArg, ParallelArg, UpgradeVersion);
                    }
                    //If crashdump is found in DataMiner(Crash in DMA)
                    else if (DMA.tokenSourceCrash.IsCancellationRequested == true)
                    {
                        //this function sends a mail with the fails/successes
                        if (IsCheckedMail == true)
                            DMAVU.RunMail(GlobalConnection.GetConnection(), listsScript, EmailAdressArg, UpgradeVersion, TextCombocopyArg);

                        DMAVU.RunMail(GlobalConnection.GetConnection(), listsScript, "jochen.deleu@skyline.be", UpgradeVersion, TextCombocopyArg);
                        DMAVU.RunMail(GlobalConnection.GetConnection(), listsScript, "martijn.vanallemeersch@skyline.be", UpgradeVersion, TextCombocopyArg);

                        if (Directory.Exists(@"C:\RegressionTester_Skyline\Current_Test\"))
                        {
                            if (SequentialArg == true)
                            {
                                if (UpgradeVersion != string.Empty)
                                {
                                    DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\Sequential_" + UpgradeVersion);

                                    //Make CrashErrors
                                    DMAVU.MakeXMLDocumentFail(@"C:\RegressionTester_Skyline\Current_Test\Sequential_" + UpgradeVersion, UpgradeVersion, TextCombocopyArg, GlobalConnection.GetConnection());
                                    
                                    //Copies the logging of the crash                                    
                                    CopyLoggingWhileCrash(UpgradeVersion, "Sequential_");
                                }
                                else
                                {
                                    DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\Sequential_" + DMAVU.GetLastStartup(false));

                                    //Make CrashErrors
                                    DMAVU.MakeXMLDocumentFail(@"C:\RegressionTester_Skyline\Current_Test\Sequential_" + DMAVU.GetLastStartup(false), UpgradeVersion, TextCombocopyArg, GlobalConnection.GetConnection());
                                    
                                    //Copies the logging of the crash                                 
                                    CopyLoggingWhileCrash(DMAVU.GetLastStartup(false), "Sequential_");
                                }
                            }
                            else if (ParallelArg == true)
                            {
                                if (UpgradeVersion != string.Empty)
                                {
                                    DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\Parallel_" + UpgradeVersion);
                                    
                                    //Make CrashErrors
                                    DMAVU.MakeXMLDocumentFail(@"C:\RegressionTester_Skyline\Current_Test\Parallel_" + UpgradeVersion, UpgradeVersion, TextCombocopyArg, GlobalConnection.GetConnection());
                                    
                                    //Copies the logging of the crash
                                    CopyLoggingWhileCrash(UpgradeVersion, "Parallel_");
                                }
                                else
                                {
                                    DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\Parallel_" + DMAVU.GetLastStartup(false));
                                    
                                    //Make CrashErrors
                                    DMAVU.MakeXMLDocumentFail(@"C:\RegressionTester_Skyline\Current_Test\Parallel_" + DMAVU.GetLastStartup(false), UpgradeVersion, TextCombocopyArg, GlobalConnection.GetConnection());
                                    
                                    //Copies the logging of the crash
                                    CopyLoggingWhileCrash(DMAVU.GetLastStartup(false), "Parallel_");                                  
                                }
                            }
                        }
                    }

                    timer.Dispose();
                }                
            }
            else
            {
                if (CmdOurUi == true)
                {
                    MessageBox.Show("Could not make the connection with the server. DMA is not running",
                                    "Connection problem.",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Connection problem.", "Could not close the connection with the server"));
                }

                Environment.Exit(0);
            }
        }

        public void CopyLoggingWhileCrash(string version,string runMethod)
        {
            using (PowerShell PSI = PowerShell.Create())
            {
                PSI.Streams.Error.Clear();
                int timeOut = 0;

                //try to copy the logging while crash happens, after 60(random number) fails abort
                do
                {
                    PSI.Streams.Error.Clear();

                    PSI.AddScript("Copy-Item -Path 'C:\\Skyline DataMiner\\logging' -Destination 'C:\\RegressionTester_Skyline\\Current_Test\\" + runMethod + version + "' -Recurse -Force;");
                    PSI.Invoke();
                    PSI.Commands.Clear();

                    ManualResetEventSlim wait = new ManualResetEventSlim(false);
                    wait.Wait(1000);

                    timeOut++;
                } while (PSI.Streams.Error.Count > 0 && timeOut < 60);
            }
        }

        private async void OverallFunction(string textCombocopyArg, string textComboVersionArg, string comboVersionMinimumArg, string textComboConfigsArg,
            string EmailAdress, bool checkcopyFilesArg, bool sequentialArg, bool parallelArg, bool BinaryArg, bool VersionMinimumArg, bool MailArg,
            bool UpgradeSequential, Dictionary<string, List<NameScriptsDirectories>> DirectorieWithActiveScriptsArg)
        {
            if (CmdOurUi == true)
            {
                gbScripts.IsEnabled = false;
                gbSettings.IsEnabled = false;
                AdvancedRegexOpen.IsEnabled = false;
                AdvancedRegexClose.IsEnabled = false;
                bSaveSettings.IsEnabled = false;
            }

            rtbConsole.Document.Blocks.Clear();

            //Regex that checks if the given emailadress in the GUI is a correct emailadres.
            Regex emailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            if (MailArg == true)
            {
                if (emailRegex.IsMatch(EmailAdress))
                {
                    IsCheckedMail = true;
                }
                else
                {
                    IsCheckedMail = false;

                    if (CmdOurUi == true)
                    {
                        MessageBox.Show("The specified email address is incorrect, the email will not be send.",
                                        "Email address incorrect.",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                    }
                    else
                    {
                        App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Email address incorrect", "The specified email address is incorrect, the email will not be send."));
                    }
                }
            }
            else
            {
                IsCheckedMail = false;
            }

            //checks if the file you try to copy are from the same DB as the ones you try to copy to.
            CheckDatabaseFileIsTheSameAsConfig = true;
            CheckDatabaseComboConfig(comboConfigs.Text, checkcopyFilesArg);

            if (CheckDatabaseFileIsTheSameAsConfig == true)
            {
                if (DirectorieWithActiveScriptsArg.Count > 0)
                {
                    DirectorieWithActiveScripts = DirectorieWithActiveScriptsArg;
                }
                else
                {
                    DMAVU.ActiveNodes.Clear();
                    DirectorieWithActiveScripts.Clear();

                    //Add all selected items in the treeview to a list
                    foreach (TreeViewMetaData node in MultiColomTree.Items)
                    {
                        foreach (TreeViewMetaData nodechild in node.SubItems)
                        {
                            CheckBox nodechildCheckbox = (nodechild.Name as CheckBox);
                            if (nodechildCheckbox != null)
                            {
                                if (nodechildCheckbox.IsChecked != null)
                                {
                                    if (nodechildCheckbox.IsChecked == true)
                                    {
                                        DMAVU.ActiveNodes.Add(DMAVU.DirectoriesAndChilderenVar.Where(d => d.Name == (string)nodechildCheckbox.Content).First());
                                    }
                                }
                            }
                        }
                    }

                    foreach (NameScriptsDirectories map in DMAVU.ActiveNodes)
                    {
                        string[] parts = map.Path.Split('\\');
                        string directory = parts[parts.Length - 2];
                        List<NameScriptsDirectories> lijst = new List<NameScriptsDirectories>();

                        foreach (NameScriptsDirectories script in DMAVU.ActiveNodes)
                        {
                            string[] partsScript = script.Path.Split('\\');

                            if (parts[parts.Length - 2] == partsScript[partsScript.Length - 2])
                            {
                                lijst.Add(script);
                            }
                        }

                        if (!DirectorieWithActiveScripts.ContainsKey(directory))
                            DirectorieWithActiveScripts.Add(directory, lijst);
                    }
                }

                if (VersionMinimumArg == true)
                {
                    List<string> list = new List<string>();

                    foreach (var version in DMAVU.Versions)
                        list.Add(version.Name);

                    if (list.IndexOf(textComboVersionArg) < list.IndexOf(comboVersionMinimumArg) && textComboVersionArg != "" && comboVersionMinimumArg != "")
                    {
                        foreach (string item in list)
                        {
                            if (list.IndexOf(item) >= list.IndexOf(textComboVersionArg) && list.IndexOf(item) <= list.IndexOf(comboVersionMinimumArg))
                            {
                                VersionsUpgrade.Add(item);
                            }
                        }
                    }
                    else
                    {
                        VersionsUpgrade.Clear();
                    }
                }
                else
                {
                    VersionsUpgrade.Add(textComboVersionArg);
                }

                listRegexesCheck.Clear();

                foreach (RegexClass item in listRegexes)
                {
                    if (((CheckBox)item.Check).IsChecked == true)
                    {
                        if (File.Exists(item.Output))
                        {
                            File.WriteAllText(item.Output, String.Empty);
                        }

                        listRegexesCheck.Add(item);
                    }
                }

                if (DirectorieWithActiveScripts.Count > 0)
                {
                    if (VersionsUpgrade.Count > 0)
                    {
                        Runned = true;
                        DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\");
                        DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\General_Information");
                        DMAVU.CreateXMLFileWithDeleting(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Configuration_switch_time.xml");
                        DMAVU.CreateXMLFileWithDeleting(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Scripts_running_time.xml");
                        DMAVU.CreateXMLFileWithDeleting(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Startup_DMA_time.xml");

                        DMAVU.CreateDirectory(@"C:\RegressionTester_Skyline\Current_Test\LoggingTool");
                        DMAVU.CreateTxtFile(@"C:\RegressionTester_Skyline\Current_Test\LoggingTool\Log.txt");

                        //When all versions are runned binary
                        if (BinaryArg == true && VersionMinimumArg == true)
                        {
                            bool BinaryReady = false;
                            double midVersion = 0.0;
                            int midVersionInt = 0;
                            int midVersionIntMin = 0;
                            int minVersion = VersionsUpgrade.Count - 1;
                            int maxVersion = 0;
                            List<string> UpgradedVersions = new List<string>();

                            while (BinaryReady == false)
                            {
                                await Task.Factory.StartNew(() =>
                                {
                                    StopDataMiner();
                                }).ContinueWith((prevTask) =>
                                {
                                    CopyDMA(textCombocopyArg);
                                }).ContinueWith((prevTask) =>
                                {
                                    prevTask.Wait();
                                    ChooseConfig(textCombocopyArg);
                                }).ContinueWith((prevTask) =>
                                {
                                    prevTask.Wait();
                                    UpgradeDMA(VersionsUpgrade[midVersionInt + maxVersion]);
                                    UpgradedVersions.Add(VersionsUpgrade[midVersionInt + maxVersion]);
                                }).ContinueWith((prevTask) =>
                                {
                                    prevTask.Wait();
                                    CopyFileDirectory(textComboConfigsArg, checkcopyFilesArg, textCombocopyArg);
                                }).ContinueWith((prevTask) =>
                                {
                                    prevTask.Wait();

                                    DMA.tokenSourceCrash = new CancellationTokenSource();
                                    DMA.tokenCrash = DMA.tokenSourceCrash.Token;
                                    DMA.tokenSourceFileNotFound = new CancellationTokenSource();
                                    DMA.tokenFileNotFound = DMA.tokenSourceFileNotFound.Token;

                                    RunningScripts(sequentialArg, parallelArg, DirectorieWithActiveScriptsArg, EmailAdress, VersionsUpgrade[midVersionInt + maxVersion], textCombocopyArg);
                                }).ContinueWith((prevTask) =>
                                {
                                    try
                                    {
                                        //(Binary versions)If no crashdump has been found, this function makes an overview of the successes and fails of the scripts runned
                                        if (DMA.tokenCrash.IsCancellationRequested == false)
                                        {
                                            foreach (string scriptName in listsScript.overview.Keys)
                                            {
                                                Dictionary<string, Dictionary<string, List<string>>> overViewVersionSub = new Dictionary<string, Dictionary<string, List<string>>>();
                                                Dictionary<string, List<string>> overViewVersionSubSub = new Dictionary<string, List<string>>();

                                                if (overviewVersion.ContainsKey(scriptName))
                                                {
                                                    foreach (var between in listsScript.overview[scriptName])
                                                    {
                                                        if (between.Key == "fails" || between.Key == "warnings" || between.Key == "successes" || between.Key == "duration")
                                                        {
                                                            overViewVersionSubSub.Add(between.Key, between.Value);
                                                        }
                                                    }

                                                    overviewVersion[scriptName].Add(VersionsUpgrade[midVersionInt + maxVersion], overViewVersionSubSub);
                                                }
                                                else
                                                {
                                                    foreach (var between in listsScript.overview[scriptName])
                                                    {
                                                        if (between.Key == "fails" || between.Key == "warnings" || between.Key == "successes" || between.Key == "duration")
                                                        {
                                                            overViewVersionSubSub.Add(between.Key, between.Value);
                                                        }
                                                    }

                                                    overViewVersionSub.Add(VersionsUpgrade[midVersionInt + maxVersion], overViewVersionSubSub);
                                                    overviewVersion.Add(scriptName, overViewVersionSub);
                                                }
                                            }
                                        }
                                        //(Binary versions) If a crashdump has been found (Crash in DMA), this function makes an overview of the crash
                                        else if (DMA.tokenCrash.IsCancellationRequested == true)
                                        {
                                            foreach (var name in DirectorieWithActiveScripts)
                                            {
                                                foreach (var Scriptname in name.Value)
                                                {
                                                    Dictionary<string, Dictionary<string, List<string>>> overViewVersionSub = new Dictionary<string, Dictionary<string, List<string>>>();
                                                    Dictionary<string, List<string>> overViewVersionSubSub = new Dictionary<string, List<string>>();

                                                    if (overviewVersion.ContainsKey(Scriptname.Name))
                                                    {
                                                        overViewVersionSubSub.Add("fails", new List<string>());
                                                        overViewVersionSubSub.Add("warnings", new List<string>());
                                                        overViewVersionSubSub.Add("successes", new List<string>());
                                                        overViewVersionSubSub.Add("duration", new List<string>() { "Crash" });

                                                        if (overviewVersion[Scriptname.Name].ContainsKey(VersionsUpgrade[midVersionInt + maxVersion]))
                                                            overviewVersion[Scriptname.Name].Add(VersionsUpgrade[midVersionInt + maxVersion], overViewVersionSubSub);
                                                    }
                                                    else
                                                    {
                                                        overViewVersionSubSub.Add("fails", new List<string>());
                                                        overViewVersionSubSub.Add("warnings", new List<string>());
                                                        overViewVersionSubSub.Add("successes", new List<string>());
                                                        overViewVersionSubSub.Add("duration", new List<string>() { "Crash" });

                                                        overViewVersionSub.Add(VersionsUpgrade[midVersionInt + maxVersion], overViewVersionSubSub);
                                                        overviewVersion.Add(Scriptname.Name, overViewVersionSub);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LoggingErrors.AddExceptionToList((String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to make overview.", ex.Message)));
                                    }

                                    //Calculate wich version has to run next (Binary run of the versions)
                                    if (listsScript.totalFails.Count != 0 || DMA.tokenCrash.IsCancellationRequested == true)
                                    {
                                        maxVersion = midVersionInt + maxVersion;
                                        midVersion = ((minVersion - maxVersion) / 2.0);
                                        midVersionInt = (int)Math.Round(midVersion, MidpointRounding.AwayFromZero);
                                        midVersionIntMin = (int)Math.Floor(midVersion);

                                        if (UpgradedVersions.Contains(VersionsUpgrade[midVersionInt + maxVersion]))
                                        {
                                            BinaryReady = true;
                                        }
                                        else
                                        {
                                            listsScript = new ListsScripts();
                                        }
                                    }
                                    else
                                    {
                                        if (midVersion == 0)
                                            BinaryReady = true;

                                        minVersion = minVersion - midVersionIntMin;

                                        midVersion = ((minVersion - maxVersion) / 2.0);
                                        midVersionInt = (int)Math.Round(midVersion, MidpointRounding.AwayFromZero);
                                        midVersionIntMin = (int)Math.Floor(midVersion);

                                        if (UpgradedVersions.Contains(VersionsUpgrade[midVersionInt + maxVersion]))
                                        {
                                            BinaryReady = true;
                                        }
                                        else
                                        {
                                            listsScript = new ListsScripts();
                                        }
                                    }
                                    //Switch config to the original configuration
                                    ChooseConfig(textComboConfigsArg);
                                });
                            }
                            UpgradedVersions.Clear();
                        }
                        //When all versions are runned sequential
                        else
                        {
                            foreach (string upgradePacket in VersionsUpgrade)
                            {
                                await Task.Factory.StartNew(() =>
                                {
                                    StopDataMiner();
                                }).ContinueWith((prevTask) =>
                                {
                                    CopyDMA(textCombocopyArg);
                                }).ContinueWith((prevTask) =>
                                {
                                    prevTask.Wait();
                                    ChooseConfig(textCombocopyArg);
                                }).ContinueWith((prevTask) =>
                                {
                                    prevTask.Wait();
                                    UpgradeDMA(upgradePacket);
                                }).ContinueWith((prevTask) =>
                                {
                                    prevTask.Wait();
                                    CopyFileDirectory(textComboConfigsArg, checkcopyFilesArg, textCombocopyArg);
                                }).ContinueWith((prevTask) =>
                                {
                                    prevTask.Wait();

                                    DMA.tokenSourceCrash = new CancellationTokenSource();
                                    DMA.tokenCrash = DMA.tokenSourceCrash.Token;
                                    DMA.tokenSourceFileNotFound = new CancellationTokenSource();
                                    DMA.tokenFileNotFound = DMA.tokenSourceFileNotFound.Token;

                                    RunningScripts(sequentialArg, parallelArg, DirectorieWithActiveScriptsArg, EmailAdress, upgradePacket, textCombocopyArg);
                                }).ContinueWith((prevTask) =>
                                {
                                    try
                                    {
                                        // (Sequential versions) if no crashdump has been found, this function makes an overview of the successes and fails of the scripts runned
                                        if (DMA.tokenCrash.IsCancellationRequested == false)
                                        {
                                            foreach (string scriptName in listsScript.overview.Keys)
                                            {
                                                Dictionary<string, Dictionary<string, List<string>>> overViewVersionSub = new Dictionary<string, Dictionary<string, List<string>>>();
                                                Dictionary<string, List<string>> overViewVersionSubSub = new Dictionary<string, List<string>>();

                                                if (overviewVersion.ContainsKey(scriptName))
                                                {
                                                    foreach (var between in listsScript.overview[scriptName])
                                                    {
                                                        if (between.Key == "fails" || between.Key == "warnings" || between.Key == "successes" || between.Key == "duration")
                                                        {
                                                            overViewVersionSubSub.Add(between.Key, between.Value);
                                                        }
                                                    }

                                                    overviewVersion[scriptName].Add(upgradePacket, overViewVersionSubSub);
                                                }
                                                else
                                                {
                                                    foreach (var between in listsScript.overview[scriptName])
                                                    {
                                                        if (between.Key == "fails" || between.Key == "warnings" || between.Key == "successes" || between.Key == "duration")
                                                        {
                                                            overViewVersionSubSub.Add(between.Key, between.Value);
                                                        }
                                                    }

                                                    overViewVersionSub.Add(upgradePacket, overViewVersionSubSub);
                                                    overviewVersion.Add(scriptName, overViewVersionSub);
                                                }
                                            }
                                        }
                                        // (Sequential versions) If a crashdump has been found (Crash in DMA), this function makes an overview of the crash
                                        else if (DMA.tokenCrash.IsCancellationRequested == true)
                                        {
                                            foreach (var name in DirectorieWithActiveScripts)
                                            {
                                                foreach (var Scriptname in name.Value)
                                                {
                                                    Dictionary<string, Dictionary<string, List<string>>> overViewVersionSub = new Dictionary<string, Dictionary<string, List<string>>>();
                                                    Dictionary<string, List<string>> overViewVersionSubSub = new Dictionary<string, List<string>>();

                                                    if (overviewVersion.ContainsKey(Scriptname.Name))
                                                    {
                                                        overViewVersionSubSub.Add("fails", new List<string>());
                                                        overViewVersionSubSub.Add("warnings", new List<string>());
                                                        overViewVersionSubSub.Add("successes", new List<string>());
                                                        overViewVersionSubSub.Add("duration", new List<string>() { "Crash" });

                                                        if (overviewVersion[Scriptname.Name].ContainsKey(upgradePacket))
                                                            overviewVersion[Scriptname.Name].Add(upgradePacket, overViewVersionSubSub);
                                                    }
                                                    else
                                                    {
                                                        overViewVersionSubSub.Add("fails", new List<string>());
                                                        overViewVersionSubSub.Add("warnings", new List<string>());
                                                        overViewVersionSubSub.Add("successes", new List<string>());
                                                        overViewVersionSubSub.Add("duration", new List<string>() { "Crash" });

                                                        overViewVersionSub.Add(upgradePacket, overViewVersionSubSub);
                                                        overviewVersion.Add(Scriptname.Name, overViewVersionSub);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LoggingErrors.AddExceptionToList((String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to make overview.", ex.Message)));
                                    }

                                    if (VersionsUpgrade.Count > 1 && VersionsUpgrade.IndexOf(upgradePacket) < (VersionsUpgrade.Count - 1))
                                    {
                                        listsScript = new ListsScripts();
                                    }

                                    //Switch config to the original configuration
                                    ChooseConfig(textComboConfigsArg);
                                });
                            }
                        }

                        DMAVU.MakeXMLDocumentWithOverview(overviewVersion);

                        if (CmdOurUi == true)
                        {
                            statusProgramStrip.Dispatcher.Invoke(
                                new UpdateTextCallback(this.UpdateTextStatusProgramStrip),
                                new object[] { "Ready" });
                        }

                        //Copy results to shares
                        DMAVU.CopyToShares();

                        if (CmdOurUi == true)
                        {
                            UpdateTextBoxWithFails(VersionsUpgrade);
                        }

                        if (CmdOurUi == true)
                        {
                            MessageBox.Show("All scripts have been runned, you must restart the application before running the application.",
                                            "Ready",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                        }
                        else
                        {
                            App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Ready", "All scripts have been runned, you must restart the application before running the application."));
                        }
                    }
                    else
                    {
                        if (CmdOurUi == true)
                        {
                            MessageBox.Show("You have selected an older/the same version in the initial upgrade package than in the mimimum version.",
                                        "Setting incorrect.",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                        }
                        else
                        {
                            App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Wrong selection", "You have selected an older/the same version in the initial upgrade package than in the mimimum version."));
                        }
                    }
                }
                else
                {
                    if (CmdOurUi == true)
                    {
                        MessageBox.Show("There must be a directory selected.",
                                        "Setting incorrect.",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                    else
                    {
                        App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Setting incorrect.", "There must be a directory selected."));
                    }
                }
            }

            if (CmdOurUi == true)
            {
                gbScripts.IsEnabled = true;
                gbSettings.IsEnabled = true;
                AdvancedRegexOpen.IsEnabled = true;
                AdvancedRegexClose.IsEnabled = true;
                bSaveSettings.IsEnabled = true;

                if (Runned == true)
                    bRunningScripts.IsEnabled = false;
            }
            else
            {
                Environment.Exit(0);
            }
        }

        //Upgrade the DMA with the selected upgrade package
        private void UpgradeDMA(string textComboVersion)
        {
            if (GlobalConnection.GetConnection().IsAuthenticated)
            {
                if (textComboVersion != "")
                {
                    if (CmdOurUi == true)
                    {
                        statusProgramStrip.Dispatcher.Invoke(
                            new UpdateTextCallback(this.UpdateTextStatusProgramStrip),
                            new object[] { "Upgrade DMA to version \"" + textComboVersion + "\"" });
                    }
                    else
                    {
                        App.CommandLog("Upgrade DMA to version \"" + textComboVersion + "\"");
                    }

                    UpgradeVersion = textComboVersion;

                    //Close the global connection
                    GlobalConnection.CloseConnection();

                    //Install the selected upgrade package
                    DMAVU.ExecuteInstallUpgrade(UpgradeVersion);

                    LoggingErrors.AddExceptionToList(Environment.NewLine + Environment.NewLine 
                        + DateTime.Now.ToString() + ": Upgrade to version " + UpgradeVersion 
                        + Environment.NewLine + Environment.NewLine);

                    //Make an connection but first wait till your DMA is started
                    GlobalConnection.MakeConnectionTryCatch();

                    //get the last startup time in startup.txt and versionHistory
                    //write it to the file where we want to know al the startup times
                    DMAVU.GetLastStartup(true);
                }
            }
            else
            {
                if (CmdOurUi == true)
                {
                    MessageBox.Show("Could not make the connection with the server. DMA is not running",
                                    "Connection problem.",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Connection problem", "Could not make the connection with the server. DMA is not running"));
                }

                Environment.Exit(0);
            }
        }

        //Change configuration from you DMA so you can switch between some configs
        private void ChooseConfig(string CleanConfigName)
        {     
                if (CmdOurUi == true)
                { 
                    statusProgramStrip.Dispatcher.Invoke(
                            new UpdateTextCallback(this.UpdateTextStatusProgramStrip),
                            new object[] { "Change configuration to \"" + CleanConfigName + "\"" });
                }
                else
                {
                    App.CommandLog("Change configuration to \"" + CleanConfigName + "\"");
                }

                ConfigCopyName = CleanConfigName;      

                //Here we are change our configuration
                DMAVU.ChooseConfig(CleanConfigName, ConfigCopyName, false, GlobalConnection.GetConnection());

                //Make an connection but first wait till your DMA is started
                GlobalConnection.MakeConnectionTryCatch();

                //Get the DMA ID from your own
                DMAVU.GetIDFunction(GlobalConnection.GetConnection());           
        }
        
        //function that copies the file directory from a DataMiner configuration
        private void CopyFileDirectory(string ConfigNameTocopyFiles, bool check, string CleanConfigNameArg)
        {
            if (GlobalConnection.GetConnection().IsAuthenticated)
            {
                if (check == true)
                {
                    if (CmdOurUi == true)
                    {
                        statusProgramStrip.Dispatcher.Invoke(
                            new UpdateTextCallback(this.UpdateTextStatusProgramStrip),
                            new object[] { "copy 'File' directory from \"" + ConfigNameTocopyFiles + "\" to \"" + CleanConfigNameArg + "\"" });
                    }
                    else
                    {
                        App.CommandLog("copy 'File' directory from \"" + ConfigNameTocopyFiles + "\" to \"" + CleanConfigNameArg + "\"");
                    }

                    ConfigCopyName = ConfigNameTocopyFiles;

                    //Here we are change our configuration
                    DMAVU.ChooseConfig(CleanConfigNameArg, ConfigCopyName, true, GlobalConnection.GetConnection());

                    //Make an connection but first wait till your DMA is started
                    GlobalConnection.MakeConnectionTryCatch();

                    ManualResetEventSlim wait = new ManualResetEventSlim();
                    wait.Wait(500);

                    //Get the DMA ID from your own
                    DMAVU.GetIDFunction(GlobalConnection.GetConnection());
                }
            }
            else
            {
                if (CmdOurUi == true)
                {
                    MessageBox.Show("Could not make the connection with the server. DMA is not running",
                                    "Connection problem.",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Connection problem", "Could not make the connection with the server. DMA is not running"));
                }

                Environment.Exit(0);
            }
        }     

        private void StopDataMiner()
        {
            DMA.StopDataMiner();
        }

        //Function that deletes a DataMiner configuration en copies a new one
        private void CopyDMA(string TextCombocopy)
        {
                if (TextCombocopy == "Testing_Cassandra" || TextCombocopy == "Testing_MySQL")
                {
                    if (CmdOurUi == true)
                    {
                        statusProgramStrip.Dispatcher.Invoke(
                            new UpdateTextCallback(this.UpdateTextStatusProgramStrip),
                            new object[] { "copy clean DMA \"" + TextCombocopy + "\"" });
                    }
                    else
                    {
                        App.CommandLog("copy clean DMA \"" + TextCombocopy + "\"");
                    }                

                    CleanDmas(TextCombocopy);
                    Lookup(TextCombocopy);
            }
        }       
        
        //Method that give you the ID from the running scripts and abort all this running scripts
        public void TogetherAbortFromRunningScripts()
        {
            List<int> runningScriptIDs = abortAndGetRunningScripts.GetRunningScriptIDs(GlobalConnection.GetConnection());

            foreach (int runningScriptID in runningScriptIDs)
                abortAndGetRunningScripts.AbortRunningScipts(runningScriptID, GlobalConnection.GetConnection());
        }

        //Method that allows you to clean the testing DMA's, after that you can show the testing DMA('s) in a combobox
        //And also show all the configs into another combobox
        public void Lookup(string textCombocopy)
        {
            try
            {
                if (textCombocopy == "Testing_Cassandra")
                {
                    DMAVU.copyDMA("Testing_Cassandra");
                }
                else if (textCombocopy == "Testing_MySQL")
                {
                    DMAVU.copyDMA("Testing_MySQL");
                }
            }
            catch (Exception ex)
            {
                if (CmdOurUi == true)
                {
                    MessageBox.Show(ex.Message,
                                  "Problem copy clean DMA.",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now,"Problem copy clean DMA." + ex.InnerException.ToString()));
                }
            }
        }

        public void CheckDatabaseComboConfig(string comboConfig, bool check)
        {
            if (check == true)
            {
                XmlDocument document = new XmlDocument();

                if (File.Exists(@"C:\Skyline DataMiner Configs\" + comboConfig + "\\Db.xml"))
                {
                    try
                    {
                        document.LoadXml(File.ReadAllText(@"C:\Skyline DataMiner Configs\" + comboConfig + "\\Db.xml"));

                        XmlNode dbNode = null;
                        foreach (XmlNode childnode in document.ChildNodes)
                        {
                            if (childnode.Name == "DataBases")
                                dbNode = childnode;
                        }

                        XmlNode nodeDataBase = null;
                        foreach (XmlNode childnode in dbNode.ChildNodes)
                        {
                            if (childnode.Name == "DataBase")
                                nodeDataBase = childnode;

                            break;
                        }

                        string attribute = string.Empty;
                        foreach (XmlAttribute childnode in nodeDataBase.Attributes)
                        {
                            if (childnode.Name == "type")
                                attribute = childnode.Value;
                        }


                        string[] Split = comboCopy.Text.Split('_');
                        string combocopySplit = Split[1];

                        if (attribute != combocopySplit)
                        {
                            if (CmdOurUi == true)
                            {
                                MessageBox.Show("Please just copy files from a config with the same database as the testing one",
                                               "incorrect DataBase.",
                                               MessageBoxButton.OK,
                                               MessageBoxImage.Error);
                            }
                            else
                            {
                                App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "incorrect DataBase.", "Please just copy files from a config with the same database as the testing one"));
                            }

                            CheckDatabaseFileIsTheSameAsConfig = false;
                        }
                    }
                    catch(Exception ex)
                    {
                        LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2}).", DateTime.Now.ToString(), @"C:\Skyline DataMiner Configs\" + comboConfig + "\\Db.xml", ex.Message));
                    }
                }
                else
                {
                    if(CmdOurUi == true)
                    { 
                    MessageBox.Show("We couldn't find C:\\Skyline DataMiner Configs\\" + comboConfig + "\\Db.xml, probably there's something wrong with your config",
                                          "File not found.",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                    }
                    else
                    {
                        App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "File not found.", "We couldn't find C:\\Skyline DataMiner Configs\\" + comboConfig + "\\Db.xml, probably there's something wrong with your config"));
                    }

                    Environment.Exit(0);
                }     
            }
        }

        //With this method you clean all the MYSQL and Cassandra DMA from the config folder under the C folder
        //So you can copy new DMA's from the shares and so you have a new and clean config
        public void CleanDmas(string textCombocopy)
        {
            if (File.Exists(@"C:\Skyline DataMiner\Db.xml"))
            {
                try
                {
                    var document = XDocument.Load(@"C:\Skyline DataMiner\Db.xml");
                    var nodes = document.Descendants().Where(e => e.Name.LocalName.Equals("DB"));

                    // .Descendants().Where(e => e.Name.LocalName.StartsWith("TESTING_MYSQL_SLDMADB")|| e.Name.LocalName.StartsWith("TESTING_CASSANDRA_SLDMADB"))

                    foreach (string name in nodes)
                    {
                        if (name == "TESTING_MYSQL_SLDMADB")
                        {
                            if (CmdOurUi == true)
                            {
                                MessageBox.Show("Please switch config to not a testing config so we are able to clean the testing configs",
                                              "incorrect config.",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error);
                            }
                            else
                            {
                                App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "incorrect config", "Please switch config to not a testing config so we are able to clean the testing configs"));
                            }

                            Environment.Exit(0);
                        }
                        else if (name == "TESTING_Cassandra_SLDMADB")
                        {
                            if (CmdOurUi == true)
                            {
                                MessageBox.Show("Please switch config to not a testing config so we are able to clean the testing configs",
                                              "incorrect config.",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error);
                            }
                            else
                            {
                                App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "incorrect config", "Please switch config to not a testing config so we are able to clean the testing configs"));
                            }

                            Environment.Exit(0);
                        }
                        else if (textCombocopy == "Testing_Cassandra")
                        {
                            DMAVU.ClearDMA("Testing_Cassandra");
                        }
                        else if (textCombocopy == "Testing_MySQL")
                        {
                            DMAVU.ClearDMA("Testing_MySQL");
                        }

                        break;
                    }
                }
                catch (Exception ex)
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1}", DateTime.Now, " problem with reading Db.xml."));
                }
            }
            else
            {
                if (CmdOurUi == true)
                {
                    MessageBox.Show("We couldn't find C:\\Skyline DataMiner\\Db.xml, probably there's something wrong with your config.",
                                "File not found.",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "File not found.", "We couldn't find C:\\Skyline DataMiner\\Db.xml, probably there's something wrong with your config."));
                }

                Environment.Exit(0);
            }
            
        }

        #endregion Functions

        #region Form_Events

        //With tis method you can run scripts sequential our parallel
        //First we get all the runningscripts and abort them so we are sure there are no scripts running otherwise the application would crash
        //Then we look if the email address is valid and if we must to send an email our not
        //The third step si to create all the directories and put the inforamtion into the directory after running the scripts
        private void bRunningTool_Click(object sender, RoutedEventArgs e)
        {
            OverallFunction(comboCopy.Text, comboVersion.Text, comboVersionMinimum.Text, comboConfigs.Text, tbEmailAddress.Text,
                (bool)cbCopyFiles.IsChecked, (bool)cbSequentieel.IsChecked, (bool)cbParallel.IsChecked, (bool)cbBinary.IsChecked, 
                (bool)cbVersionMinimum.IsChecked, (bool)cbMail.IsChecked, (bool)cbUpgradeSequential.IsChecked, DirectorieWithActiveScripts);
        }

        //To run the application from commandline this function is to save all the setting from the GUI in an XML
        //To start the application from commandline reference to this XML
        private void bSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, bool> settingsBool = new Dictionary<string, bool>();

                DMAVU.CreateDirectoryWithoutDeleting(@"C:\RegressionTester_Skyline\Task_Scheduler");
                DMAVU.CreateXMLFileWithDeleting(@"C:\RegressionTester_Skyline\Task_Scheduler\Scheduler_Task.xml");

                #region gbSettings
                if (cbCopyFiles.IsChecked == true)
                    settingsBool.Add(cbCopyFiles.Name, true);
                else
                    settingsBool.Add(cbCopyFiles.Name, false);

                if (cbVersionMinimum.IsChecked == true)
                    settingsBool.Add(cbVersionMinimum.Name, true);
                else
                    settingsBool.Add(cbVersionMinimum.Name, false);

                if (cbBinary.IsChecked == true)
                    settingsBool.Add(cbBinary.Name, true);
                else
                    settingsBool.Add(cbBinary.Name, false);

                if (cbUpgradeSequential.IsChecked == true)
                    settingsBool.Add(cbUpgradeSequential.Name, true);
                else
                    settingsBool.Add(cbUpgradeSequential.Name, false);
                #endregion gbSettings

                #region gBScripts
                if (cbSequentieel.IsChecked == true)
                    settingsBool.Add(cbSequentieel.Name, true);
                else
                    settingsBool.Add(cbSequentieel.Name, false);

                if (cbParallel.IsChecked == true)
                    settingsBool.Add(cbParallel.Name, true);
                else
                    settingsBool.Add(cbParallel.Name, false);

                if (cbMail.IsChecked == true)
                    settingsBool.Add(cbMail.Name, true);
                else
                    settingsBool.Add(cbMail.Name, false);

                if (cbAll.IsChecked == true)
                    settingsBool.Add(cbAll.Name, true);
                else
                    settingsBool.Add(cbAll.Name, false);

                DMAVU.ActiveNodes.Clear();
                DirectorieWithActiveScripts.Clear();

                foreach (TreeViewMetaData node in MultiColomTree.Items)
                {
                    foreach (TreeViewMetaData nodechild in node.SubItems)
                    {
                        CheckBox nodechildCheckbox = (nodechild.Name as CheckBox);
                        if (nodechildCheckbox != null)
                        {
                            if (nodechildCheckbox.IsChecked != null)
                            {
                                if (nodechildCheckbox.IsChecked == true)
                                {
                                    DMAVU.ActiveNodes.Add(DMAVU.DirectoriesAndChilderenVar.Where(d => d.Name == (string)nodechildCheckbox.Content).First());
                                }
                            }
                        }
                    }
                }

                foreach (NameScriptsDirectories map in DMAVU.ActiveNodes)
                {
                    string[] parts = map.Path.Split('\\');
                    string directory = parts[parts.Length - 2];
                    List<NameScriptsDirectories> lijst = new List<NameScriptsDirectories>();

                    foreach (NameScriptsDirectories script in DMAVU.ActiveNodes)
                    {
                        string[] partsScript = script.Path.Split('\\');

                        if (parts[parts.Length - 2] == partsScript[partsScript.Length - 2])
                        {
                            lijst.Add(script);
                        }
                    }

                    if (!DirectorieWithActiveScripts.ContainsKey(directory))
                        DirectorieWithActiveScripts.Add(directory, lijst);
                }
                #endregion gBScripts

                if (File.Exists(@"C:\RegressionTester_Skyline\Task_Scheduler\Scheduler_Task.xml"))
                {
                    XDocument doc = XDocument.Load(@"C:\RegressionTester_Skyline\Task_Scheduler\Scheduler_Task.xml");

                    XElement gbSettings = new XElement("gbSettings");
                    gbSettings.Add(new XElement("combocopy", comboCopy.Text));
                    gbSettings.Add(new XElement("comboVersion", comboVersion.Text));
                    gbSettings.Add(new XElement("comboVersionMinimum", comboVersionMinimum.Text));
                    gbSettings.Add(new XElement("cbVersionMinimum", settingsBool[cbVersionMinimum.Name]));
                    gbSettings.Add(new XElement("cbBinary", settingsBool[cbBinary.Name]));
                    gbSettings.Add(new XElement("cbUpgradeSequential", settingsBool[cbUpgradeSequential.Name]));
                    gbSettings.Add(new XElement("comboConfigs", comboConfigs.Text));
                    gbSettings.Add(new XElement("cbcopyFiles", settingsBool[cbCopyFiles.Name]));

                    XElement gbScripts = new XElement("gbScripts");
                    gbScripts.Add(new XElement("cbSequentieel", settingsBool[cbSequentieel.Name]));
                    gbScripts.Add(new XElement("cbParallel", settingsBool[cbParallel.Name]));
                    gbScripts.Add(new XElement("cbMail", settingsBool[cbMail.Name]));
                    gbScripts.Add(new XElement("tbEmailAddress", tbEmailAddress.Text));

                    XElement ScriptsRun = new XElement("ScriptsRun");
                    foreach (var name in DirectorieWithActiveScripts)
                    {
                        XElement DirectorieWithActiveScriptsSaveSettings = new XElement(name.Key);

                        foreach (var name2 in name.Value)
                        {
                            XElement Script = new XElement("Script");
                            Script.Add(new XElement("Name", new XCData(name2.Name)));
                            Script.Add(new XElement("Path", new XCData(name2.Path)));

                            DirectorieWithActiveScriptsSaveSettings.Add(Script);
                        }

                        ScriptsRun.Add(DirectorieWithActiveScriptsSaveSettings);
                    }

                    gbScripts.Add(ScriptsRun);

                    doc.Element("root").Add(gbSettings);
                    doc.Element("root").Add(gbScripts);
                    doc.Save(@"C:\RegressionTester_Skyline\Task_Scheduler\Scheduler_Task.xml");
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Setting incorrect", "There is an exception occured with making the XML file for the scheduler task" + ex.Message));
            }
        }

        private void cbcopyFiles_Click(object sender, RoutedEventArgs e)
        {
            if (cbCopyFiles.IsChecked == true)
                comboConfigs.IsEnabled = true;
            else
                comboConfigs.IsEnabled = false;
        }

        private void cbMail_Click(object sender, RoutedEventArgs e)
        {
            if (cbMail.IsChecked == false)
                tbEmailAddress.IsEnabled = false;
            else
                tbEmailAddress.IsEnabled = true;
        }

        private void checkBox_Click_All(object sender, RoutedEventArgs e)
        {
            if (cbAll.IsChecked == true)
            {
                foreach (TreeViewMetaData itemTreeView in MultiColomTree.Items)
                {
                    (itemTreeView.Name as CheckBox).IsChecked = true;

                    if(itemTreeView.SubItems != null)
                        CheckAllChildNodesMetaData(itemTreeView, true);
                }
            }
            else
            {
                foreach (TreeViewMetaData itemTreeView in MultiColomTree.Items)
                {
                    (itemTreeView.Name as CheckBox).IsChecked = false;

                    if (itemTreeView.SubItems != null)
                        CheckAllChildNodesMetaData(itemTreeView, false);
                }
            }
        }

        private void cbVersionMinimum_Click(object sender, RoutedEventArgs e)
        {
            if (cbVersionMinimum.IsChecked == true)
            {
                comboVersionMinimum.IsEnabled = true;
                cbBinary.IsEnabled = true;
                cbUpgradeSequential.IsEnabled = true;

                cbCopyFiles.IsChecked = false;
                cbCopyFiles.IsEnabled = false;
                comboConfigs.IsEnabled = false;
            }
            else
            {
                comboVersionMinimum.IsEnabled = false;
                cbBinary.IsEnabled = false;
                cbUpgradeSequential.IsEnabled = false;

                cbCopyFiles.IsEnabled = true;
                cbCopyFiles.IsChecked = true;
                comboConfigs.IsEnabled = true;

            }
        }

        private void button_Click_Regex_Open(object sender, RoutedEventArgs e)
        {
            MyPopupRegex.PlacementTarget = sender as UIElement;
            MyPopupRegex.IsOpen = true;

            tbRegex.IsEnabled = false;
            treeViewDirectoriesScripts2.IsEnabled = false;
            btnChooseFile.IsEnabled = false;
            btnAdd.IsEnabled = false;
            btnClear.IsEnabled = false;
        }

        private void button_Click_Regex_Close(object sender, RoutedEventArgs e)
        {
            MyPopupRegex.IsOpen = false;

            tbRegex.Text = "";
            btnChooseFile.Content = "...";

            foreach (TreeViewItem itemTreeView in treeViewDirectoriesScripts2.Items)
            {
                (itemTreeView.Header as CheckBox).IsChecked = false;

                CheckAllChildNodesRegex(itemTreeView, false);
            }
        }

        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            try
            {
                listRegexes.RemoveAt(dataGridRegexes.SelectedIndex);
            }
            catch (Exception ex)
            {
                if (CmdOurUi == true)
                {
                    MessageBox.Show(ex.Message, "Error removing regex.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Error removing regex.", ex.Message));
                }
            }

            MyPopupRegex.IsOpen = true;
        }

        private void Button_Click_New(object sender, RoutedEventArgs e)
        {
            tbRegex.IsEnabled = true;
            treeViewDirectoriesScripts2.IsEnabled = true;
            btnChooseFile.IsEnabled = true;
            btnAdd.IsEnabled = true;
            btnClear.IsEnabled = true;
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            List<NameScriptsDirectories> ActiveNodes2 = new List<NameScriptsDirectories>();
            Dictionary<string, List<NameScriptsDirectories>> DirectorieWithActiveScripts2 = new Dictionary<string, List<NameScriptsDirectories>>();
            List<NameScriptsDirectories> DirectoriesAndChilderenVarRegex = new List<NameScriptsDirectories>();
            DirectoriesAndChilderenVarRegex = DMAVU.DirectoriesAndChilderenVar;

            foreach (TreeViewItem node in treeViewDirectoriesScripts2.Items)
            {
                foreach (TreeViewItem nodechild in node.Items)
                {
                    CheckBox nodechildCheckbox = (nodechild.Header as CheckBox);
                    if (nodechildCheckbox != null)
                    {
                        if (nodechildCheckbox.IsChecked != null)
                        {
                            if (nodechildCheckbox.IsChecked == true)
                            {
                                ActiveNodes2.Add(DirectoriesAndChilderenVarRegex.Where(d => d.Name == (string)nodechildCheckbox.Content).First());
                            }
                        }
                    }
                }
            }

            foreach (NameScriptsDirectories map in ActiveNodes2)
            {
                string[] parts = map.Path.Split('\\');
                string directory = parts[parts.Length - 2];
                List<NameScriptsDirectories> lijst = new List<NameScriptsDirectories>();

                foreach (NameScriptsDirectories script in ActiveNodes2)
                {
                    string[] partsScript = script.Path.Split('\\');

                    if (parts[parts.Length - 2] == partsScript[partsScript.Length - 2])
                    {
                        lijst.Add(script);
                    }
                }

                if (!DirectorieWithActiveScripts2.ContainsKey(directory))
                    DirectorieWithActiveScripts2.Add(directory, lijst);
            }

            if (btnChooseFile.Content.ToString().ToLower().EndsWith(".txt") && DirectorieWithActiveScripts2.Count != 0)
            {
                string FilesString = "";

                foreach (var folder in DirectorieWithActiveScripts2)
                {
                    FilesString = FilesString + folder.Key + " ( " + folder.Value.Count.ToString() + " file('s) ) , ";

                    if (folder.ToString() == DirectorieWithActiveScripts2.Last().ToString())
                        FilesString.TrimEnd(new char[] { ',', ' ' });
                }
                dataGridRegexes.ClearValue(ItemsControl.ItemsSourceProperty);
                dataGridRegexes.Items.Clear();

                listRegexes.Add(new RegexClass() { Check = new CheckBox() { IsChecked = false }, Regex = tbRegex.Text, Collection = DirectorieWithActiveScripts2, Files = FilesString, Output = btnChooseFile.Content.ToString() });
                dataGridRegexes.ItemsSource = listRegexes;
            }
            else
            {
                if (CmdOurUi == true)
                {
                    MessageBox.Show("There is something wrong with your new regex.", "Failed to make regex.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to make regex.", "There is something wrong with your new regex."));
                }
            }

            tbRegex.Text = "";
            btnChooseFile.Content = "...";

            foreach (TreeViewItem itemTreeView in treeViewDirectoriesScripts2.Items)
            {
                (itemTreeView.Header as CheckBox).IsChecked = false;

                CheckAllChildNodesRegex(itemTreeView, false);
            }

            tbRegex.IsEnabled = false;
            treeViewDirectoriesScripts2.IsEnabled = false;
            btnChooseFile.IsEnabled = false;
            btnAdd.IsEnabled = false;
            btnClear.IsEnabled = false;
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            tbRegex.Text = "";
            btnChooseFile.Content = "...";

            foreach (TreeViewItem itemTreeView in treeViewDirectoriesScripts2.Items)
            {
                (itemTreeView.Header as CheckBox).IsChecked = false;

                CheckAllChildNodesRegex(itemTreeView, false);
            }
        }

        private void Button_Click_File_Choose(object sender, RoutedEventArgs e)
        {
            MyPopupRegex.IsOpen = false;

            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "TXT Files (*.txt)|*.TXT" };
            var result = ofd.ShowDialog();

            if (result == true)
                btnChooseFile.Content = ofd.FileName;
            else
                btnChooseFile.Content = "You haven't selected a file.";

            MyPopupRegex.IsOpen = true;
        }

        //Function to make hyperlinks
        private void Hyper_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink lin = (e.Source as Hyperlink);

            if (lin != null)
                System.Diagnostics.Process.Start("Explorer.exe", lin.NavigateUri.ToString());
        }

        //Method that allows you to close the connection and abort all the running script if you close you application
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GlobalConnection.GetConnection().IsAuthenticated)
            {
                TogetherAbortFromRunningScripts();
                DMAVU.DeleteElement(GlobalConnection.GetConnection());
                GlobalConnection.CloseConnection();
            }
        }

        private void Timer_Tick(object state)
        {  
            DMAVU.SendMessageDispatcher(GlobalConnection.GetConnection());           
        }      

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            TreeViewMetaDataItemsAfterSearch.SubItems.Clear();
            MultiColomTree.ClearValue(ItemsControl.ItemsSourceProperty);
            MultiColomTree.Items.Clear();

            foreach (TreeViewMetaData item in TreeViewMetaDataItems.SubItems)
            {
                TreeViewMetaData itemMD = new TreeViewMetaData() { SubItems = new List<TreeViewMetaData>(), Name = item.Name };
                foreach (TreeViewMetaData itemItem in item.SubItems)
                {
                    if (comboSearch.SelectedItem.ToString().ToLower() == "Scriptname".ToLower())
                    {
                        if (itemItem.Name.Content.ToString().ToLower().Contains(textBox.Text.ToLower()))
                            itemMD.SubItems.Add(itemItem);
                    }
                    else if (comboSearch.SelectedItem.ToString().ToLower() == "Author".ToLower())
                    {
                        if (itemItem.Author.Text.ToString().ToLower().Contains(textBox.Text.ToLower()))
                            itemMD.SubItems.Add(itemItem);
                    }
                    else if (comboSearch.SelectedItem.ToString().ToLower() == "Process".ToLower())
                    {
                        if (itemItem.Process.Text.ToString().ToLower().Contains(textBox.Text.ToLower()))
                            itemMD.SubItems.Add(itemItem);
                    }
                    else if (comboSearch.SelectedItem.ToString().ToLower() == "Version".ToLower())
                    {
                        if (itemItem.Version.Text.ToString().ToLower().Contains(textBox.Text.ToLower()))
                            itemMD.SubItems.Add(itemItem);
                    }
                }

                if (itemMD.SubItems.Count != 0)
                    TreeViewMetaDataItemsAfterSearch.SubItems.Add(itemMD);
            }

            if (textBox.Text == string.Empty)
                MultiColomTree.ItemsSource = TreeViewMetaDataItems.SubItems;
            else
                MultiColomTree.ItemsSource = TreeViewMetaDataItemsAfterSearch.SubItems;
        }

        #endregion Form_Events

        #region FormUpdateFunctions

        private void UpdateTextStatusProgramStrip(string message)
        {
            statusProgramStrip.Content = message;
        }

        private void UpdateTextboxConsole(string message)
        {
            tbConsole.AppendText(message);
            tbConsole.ScrollToEnd();
        }

        //Edits the list of all configs that are available on the Shares 
        public void UpdateComboboxConfigConfiguration(DMA DMAVU)
        {
            const string path = @"C:\Skyline DataMiner Configs";

            if (Directory.Exists(path))
            {
                comboConfigs.Items.Clear();
                IList<string> list = DMAVU.Configs.Select(a => a.Name).ToArray();

                foreach (string name in list)
                    comboConfigs.Items.Add(name);

                comboConfigs.SelectedIndex = 0;
            }
            else
            {
                if (CmdOurUi == true)
                {
                    MessageBox.Show("You don't have the directory: 'C:\\Skyline DataMiner Configs'.",
                                    "Error: directory.",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Error: directory.", "You don't have the directory: 'C:\\Skyline DataMiner Configs'."));
                }

                comboConfigs.Items.Clear();
            }
        }

        //Edits the list of available Upgrades that are situated on the Shares 
        public void UpdateCombobox(DMA DMAVU)
        {
            comboVersion.Items.Clear();
            IList<string> list = DMAVU.Versions.Select(m => m.Name).ToArray();

            foreach (string name in list)
            {
                comboVersion.Items.Add(name);
                comboVersionMinimum.Items.Add(name);
            }

            comboVersion.SelectedIndex = 0;
            comboVersionMinimum.SelectedIndex = 0;
        }

        //Fill the richtextbox with the fails that have occurred in the current run
        //This is the same method for parallel out sequential
        public void UpdateTextBoxWithFails(List<string> VersionsUpgrade)
        {
            rtbConsole.AppendText("There were " + listsScript.totalFails.Count + " fails in the runned scripts.");

            if (cbSequentieel.IsChecked == true)
            {
                for (int i = 0; i < listsScript.TotalFailsPath.Count; i++)
                {
                    if (i < listsScript.TotalFailsPath.Count - 1)
                    {
                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(listsScript.TotalFailsPath[i] + "\n");

                        Hyperlink hyper = new Hyperlink(paragraph.ContentStart, paragraph.ContentEnd);
                        hyper.NavigateUri = new Uri(new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text);
                        hyper.Click += Hyper_Click;
                        paragraph.Margin = new Thickness(0);
                        rtbConsole.Document.Blocks.Add(paragraph);
                    }
                    else
                    {
                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(listsScript.TotalFailsPath[i]);

                        Hyperlink hyper = new Hyperlink(paragraph.ContentStart, paragraph.ContentEnd);
                        hyper.NavigateUri = new Uri(new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text);
                        hyper.Click += Hyper_Click;
                        paragraph.Margin = new Thickness(0);
                        rtbConsole.Document.Blocks.Add(paragraph);
                    }
                }
            }
            else if (cbParallel.IsChecked == true)
            {
                if (listsScript.totalFails.Count != 0)
                {
                    Paragraph paragraph = new Paragraph();
                    paragraph.Inlines.Add(@"C:\RegressionTester_Skyline\Current_Test\Parallel_" + VersionsUpgrade.Last().ToString() + "\n");

                    Hyperlink hyper = new Hyperlink(paragraph.ContentStart, paragraph.ContentEnd);
                    hyper.NavigateUri = new Uri(new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text);
                    hyper.Click += Hyper_Click;
                    rtbConsole.Document.Blocks.Add(paragraph);
                }
            }

            Paragraph paragraphTotalUnnoticedInformationEvents = new Paragraph();
            paragraphTotalUnnoticedInformationEvents.Inlines.Add("There was/were " + listsScript.TotalUnnoticedInformationEvents.Count + " script(s) that not generate any succes, fail our warning.");

            for (int i = 0; i < listsScript.TotalUnnoticedInformationEvents.Count; i++)
            {
                if (i == 0)
                    paragraphTotalUnnoticedInformationEvents.Inlines.Add("\n");

                if (i < listsScript.TotalUnnoticedInformationEvents.Count - 1)
                {
                    paragraphTotalUnnoticedInformationEvents.Inlines.Add(listsScript.TotalUnnoticedInformationEvents[i] + "\n");
                }
                else
                {
                    paragraphTotalUnnoticedInformationEvents.Inlines.Add(listsScript.TotalUnnoticedInformationEvents[i]);
                }
            }
            rtbConsole.Document.Blocks.Add(paragraphTotalUnnoticedInformationEvents);

            Paragraph paragraphTotalScriptWithParameters = new Paragraph();
            paragraphTotalScriptWithParameters.Inlines.Add("There was/were " + listsScript.TotalScriptWithParameters.Count + " script(s) with parameters.");

            for (int i = 0; i < listsScript.TotalScriptWithParameters.Count; i++)
            {
                if (i == 0)
                    paragraphTotalScriptWithParameters.Inlines.Add("\n");

                if (i < listsScript.TotalScriptWithParameters.Count - 1)
                {
                    paragraphTotalScriptWithParameters.Inlines.Add(listsScript.TotalScriptWithParameters[i] + "\n");
                }
                else
                {
                    paragraphTotalScriptWithParameters.Inlines.Add(listsScript.TotalScriptWithParameters[i]);
                }
            }
            rtbConsole.Document.Blocks.Add(paragraphTotalScriptWithParameters);

            Paragraph paragraphTotalScriptWithProtocols = new Paragraph();
            paragraphTotalScriptWithProtocols.Inlines.Add("There was/were " + listsScript.TotalScriptWithProtocols.Count + " script(s) with Protocols.");

            for (int i = 0; i < listsScript.TotalScriptWithProtocols.Count; i++)
            {
                if (i == 0)
                    paragraphTotalScriptWithProtocols.Inlines.Add("\n");

                if (i < listsScript.TotalScriptWithProtocols.Count - 1)
                {
                    paragraphTotalScriptWithProtocols.Inlines.Add(listsScript.TotalScriptWithProtocols[i] + "\n");
                }
                else
                {
                    paragraphTotalScriptWithProtocols.Inlines.Add(listsScript.TotalScriptWithProtocols[i]);
                }
            }
            rtbConsole.Document.Blocks.Add(paragraphTotalScriptWithProtocols);

            Paragraph paragraphTotalBatchFiles = new Paragraph();
            paragraphTotalBatchFiles.Inlines.Add("There was/were " + listsScript.TotalBatchFiles.Count + " batch(es) file(s) runned.");

            for (int i = 0; i < listsScript.TotalBatchFiles.Count; i++)
            {
                if (i == 0)
                    paragraphTotalBatchFiles.Inlines.Add("\n");

                if (i < listsScript.TotalBatchFiles.Count - 1)
                {
                    paragraphTotalBatchFiles.Inlines.Add(listsScript.TotalBatchFiles[i] + "\n");
                }
                else
                {
                    paragraphTotalBatchFiles.Inlines.Add(listsScript.TotalBatchFiles[i]);
                }
            }
            rtbConsole.Document.Blocks.Add(paragraphTotalBatchFiles);

            Paragraph paragraphTotalDelts = new Paragraph();
            paragraphTotalDelts.Inlines.Add("There was/were " + listsScript.TotalDelts.Count + " delt(s) imported.");

            for (int i = 0; i < listsScript.TotalDelts.Count; i++)
            {
                if (i == 0)
                    paragraphTotalDelts.Inlines.Add("\n");

                if (i < listsScript.TotalDelts.Count - 1)
                {
                    paragraphTotalDelts.Inlines.Add(listsScript.TotalDelts[i] + "\n");
                }
                else
                {
                    paragraphTotalDelts.Inlines.Add(listsScript.TotalDelts[i]);
                }
            }
            rtbConsole.Document.Blocks.Add(paragraphTotalDelts);

            Paragraph paragraphTotalRequestedTimeoutsFolders = new Paragraph();
            paragraphTotalRequestedTimeoutsFolders.Inlines.Add("There was/were " + listsScript.TotalRequestedTimeoutsFolders.Count + " folder(s) with timeout request.");

            for (int i = 0; i < listsScript.TotalRequestedTimeoutsFolders.Count; i++)
            {
                if (i == 0)
                    paragraphTotalRequestedTimeoutsFolders.Inlines.Add("\n");

                if (i < listsScript.TotalRequestedTimeoutsFolders.Count - 1)
                {
                    paragraphTotalRequestedTimeoutsFolders.Inlines.Add(listsScript.TotalRequestedTimeoutsFolders[i] + "\n");
                }
                else
                {
                    paragraphTotalRequestedTimeoutsFolders.Inlines.Add(listsScript.TotalRequestedTimeoutsFolders[i]);
                }
            }
            rtbConsole.Document.Blocks.Add(paragraphTotalRequestedTimeoutsFolders);

            Paragraph paragraphTotalFailedScriptsSequential = new Paragraph();
            paragraphTotalFailedScriptsSequential.Inlines.Add("There was/were " + listsScript.TotalfailedToExcecuteScript.Count + " folder(s) with timeout request.");

            for (int i = 0; i < listsScript.TotalfailedToExcecuteScript.Count; i++)
            {
                if (i == 0)
                    paragraphTotalFailedScriptsSequential.Inlines.Add("\n");

                if (i < listsScript.TotalfailedToExcecuteScript.Count - 1)
                {
                    paragraphTotalFailedScriptsSequential.Inlines.Add(listsScript.TotalfailedToExcecuteScript[i] + "\n");
                }
                else
                {
                    paragraphTotalFailedScriptsSequential.Inlines.Add(listsScript.TotalfailedToExcecuteScript[i]);
                }
            }
            rtbConsole.Document.Blocks.Add(paragraphTotalFailedScriptsSequential);
        }

        #endregion FormUpdateFunctions 
    }
}
