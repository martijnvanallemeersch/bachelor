using DevRegTest.domainVMApp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace DevRegTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        public static bool IsAttached = false;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        public static int CommandExitCode = 1; //Default to failure.

        [STAThread]
        public static int Main(String[] args)
        {
            Dictionary<string, string> textCmd = new Dictionary<string, string>();
            Dictionary<string, bool> boolsCmd = new Dictionary<string, bool>();
            Dictionary<string, List<NameScriptsDirectories>> DirectorieWithActiveScriptsCmd = new Dictionary<string, List<NameScriptsDirectories>>();
            string singleInstanceKey = "RegressionTester_";
            bool resultMain = false;

            //Try attach to console if it's our parent.
            int parentID = ParentProcessUtilities.ParentProcessId();
            if (parentID != -1)
            {
                //Check if we're not currently attached. Try to detach if so.
                if (IsAttached)
                {
                    if (FreeConsole())
                        IsAttached = false;
                    else
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                if (AttachConsole(parentID))
                {
                    TextWriter writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                    Console.SetOut(writer);
                    IsAttached = true;
                }
            }

            if (SingleInstance<App>.InitializeAsFirstInstance(singleInstanceKey))
            {
                var application = new App();

                application.InitializeComponent();

                if (args.Length > 0)
                {
                    resultMain = HandleCommandLineArgs(args, textCmd, boolsCmd, DirectorieWithActiveScriptsCmd);
                    if (resultMain)
                    {
                        MainWindow mainwindow = new MainWindow(textCmd, boolsCmd, DirectorieWithActiveScriptsCmd);
                        mainwindow.Visibility = Visibility.Hidden;
                        application.Run(mainwindow);
                    }
                }
                else
                {
                    application.Run(new MainWindow(null, null, null));
                }

                SingleInstance<App>.Cleanup();
            }
            else
            {
                if (IsAttached)
                    CommandLog("Application RegressionTester already running. Only one instance of this application is allowed.");
                else
                    MessagboxShow("Application RegressionTester already running. Only one instance of this application is allowed.", "Instance already running");
            }

            return CommandExitCode;
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            return false; //We never want to activate from second instance.
        }

        public static void MessagboxShow(string message, string info)
        {
            if (!IsAttached)
                MessageBox.Show(message, info, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void CommandLog(string message)
        {
            if (IsAttached)
                Console.WriteLine(message);
        }

        private static bool HandleCommandLineArgs(IList<string> args, Dictionary<string, string> textCmd,
            Dictionary<string, bool> boolsCmd, Dictionary<string, List<NameScriptsDirectories>> DirectorieWithActiveScriptsCmd)
        {
            try
            {
                if (File.Exists(args[0]))
                {
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(File.ReadAllText(args[0]));

                    List<XmlNode> node = new List<XmlNode>();
                    foreach (XmlNode childnode1 in document.ChildNodes)
                    {
                        if (childnode1.Name == "root")
                            node.Add(childnode1);
                    }

                    foreach (XmlNode childnode2 in node[node.Count - 1].ChildNodes)
                    {
                        if (childnode2.Name.ToLower() == "gbSettings".ToLower())
                        {
                            foreach (XmlNode childnode3 in childnode2)
                            {
                                if (childnode3.Name.ToLower() == "combocopy".ToLower())
                                    textCmd.Add("combocopy".ToLower(), childnode3.InnerText);
                                else if (childnode3.Name.ToLower() == "comboVersion".ToLower())
                                    textCmd.Add("comboVersion".ToLower(), childnode3.InnerText);
                                else if (childnode3.Name.ToLower() == "comboVersionMinimum".ToLower())
                                    textCmd.Add("comboVersionMinimum".ToLower(), childnode3.InnerText);
                                else if (childnode3.Name.ToLower() == "comboConfigs".ToLower())
                                    textCmd.Add("comboConfigs".ToLower(), childnode3.InnerText);
                                else if (childnode3.Name.ToLower() == "cbcopyFiles".ToLower())
                                {
                                    if (childnode3.InnerText.ToLower() == "true".ToLower())
                                        boolsCmd.Add("cbcopyFiles".ToLower(), true);
                                    else if (childnode3.InnerText.ToLower() == "false".ToLower())
                                        boolsCmd.Add("cbcopyFiles".ToLower(), false);
                                }
                                else if (childnode3.Name.ToLower() == "cbVersionMinimum".ToLower())
                                {
                                    if (childnode3.InnerText.ToLower() == "true".ToLower())
                                        boolsCmd.Add("cbVersionMinimum".ToLower(), true);
                                    else
                                        boolsCmd.Add("cbVersionMinimum".ToLower(), false);
                                }
                                else if (childnode3.Name.ToLower() == "cbUpgradeSequential".ToLower())
                                {
                                    if (childnode3.InnerText.ToLower() == "true".ToLower())
                                        boolsCmd.Add("cbUpgradeSequential".ToLower(), true);
                                    else
                                        boolsCmd.Add("cbUpgradeSequential".ToLower(), false);
                                }
                                else if (childnode3.Name.ToLower() == "cbBinary".ToLower())
                                {
                                    if (childnode3.InnerText.ToLower() == "true".ToLower())
                                        boolsCmd.Add("cbBinary".ToLower(), true);
                                    else
                                        boolsCmd.Add("cbBinary".ToLower(), false);
                                }
                            }
                        }
                        else if (childnode2.Name.ToLower() == "gbScripts".ToLower())
                        {
                            foreach (XmlNode childnode3 in childnode2)
                            {
                                if (childnode3.Name.ToLower() == "cbSequentieel".ToLower())
                                {
                                    if (childnode3.InnerText.ToLower() == "true".ToLower())
                                        boolsCmd.Add("cbSequentieel".ToLower(), true);
                                    else if (childnode3.InnerText.ToLower() == "false".ToLower())
                                        boolsCmd.Add("cbSequentieel".ToLower(), false);
                                }
                                else if (childnode3.Name.ToLower() == "cbParallel".ToLower())
                                {
                                    if (childnode3.InnerText.ToLower() == "true".ToLower())
                                        boolsCmd.Add("cbParallel".ToLower(), true);
                                    else if (childnode3.InnerText.ToLower() == "false".ToLower())
                                        boolsCmd.Add("cbParallel".ToLower(), false);
                                }
                                else if (childnode3.Name.ToLower() == "cbMail".ToLower())
                                {
                                    if (childnode3.InnerText.ToLower() == "true".ToLower())
                                        boolsCmd.Add("cbMail".ToLower(), true);
                                    else if (childnode3.InnerText.ToLower() == "false".ToLower())
                                        boolsCmd.Add("cbMail".ToLower(), false);
                                }
                                else if (childnode3.Name.ToLower() == "cbAll".ToLower())
                                {
                                    if (childnode3.InnerText.ToLower() == "true".ToLower())
                                        boolsCmd.Add("cbAll".ToLower(), true);
                                    else if (childnode3.InnerText.ToLower() == "false".ToLower())
                                        boolsCmd.Add("cbAll".ToLower(), false);
                                }
                                else if (childnode3.Name.ToLower() == "tbEmailAddress".ToLower())
                                    textCmd.Add("tbEmailAddress".ToLower(), childnode3.InnerText);
                                else if (childnode3.Name.ToLower() == "ScriptsRun".ToLower())
                                {
                                    foreach (XmlNode childnode4 in childnode3.ChildNodes)
                                    {
                                        string key = childnode4.Name.ToString();
                                        List<NameScriptsDirectories> list = new List<NameScriptsDirectories>();

                                        foreach (XmlNode childnode5 in childnode4.ChildNodes)
                                        {
                                            string valuePath = string.Empty;
                                            string valueName = string.Empty;

                                            foreach (XmlNode childnode6 in childnode5.ChildNodes)
                                            {
                                                if (childnode6.Name.ToLower() == "Name".ToLower())
                                                    valueName = childnode6.InnerText;
                                                else if (childnode6.Name.ToLower() == "Path".ToLower())
                                                    valuePath = childnode6.InnerText;
                                            }
                                            list.Add(new NameScriptsDirectories(valueName, valuePath));
                                        }

                                        if (!DirectorieWithActiveScriptsCmd.ContainsKey(key))
                                            DirectorieWithActiveScriptsCmd.Add(key, list);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1}", DateTime.Now, "Import fail", "Failed import all the settings from the XML-file: "));

                    return false;
                }
            }
            catch (Exception ex)
            {
                App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Import fail", "Failed import all the settings from the XML-file: " + ex.Message));

                return false;
            }

            return true;
        }
    }
}
