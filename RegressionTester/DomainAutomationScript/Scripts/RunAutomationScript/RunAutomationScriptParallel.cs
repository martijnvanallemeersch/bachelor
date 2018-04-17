using DevRegTest.domainAutomationScript.Report;
using DevRegTest.domainVMApp;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace DevRegTest.domainAutomationScript.Element.RunAutomationScript
{
    class RunAutomationScriptParallel
    {
        ExecuteScriptMessage[] ScriptMessage;
        Dictionary<DELTType, int> dictionaryOptions = new Dictionary<DELTType, int>();
        List<int> indexOfParallelScripts;

        private List<NameScriptsDirectories> listScripts { get; set; }

        public ListsScripts ListsParallel { get; set; }

        public RunAutomationScriptParallel(List<NameScriptsDirectories> listScriptsArg, ListsScripts listsParallelArg)
        {
            ListsParallel = listsParallelArg;

            indexOfParallelScripts = new List<int>();

            for (int index = 0; index < listScriptsArg.Count; index++)
            {
                if (listScriptsArg[index].Name.EndsWith(".xml"))
                {
                    try
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(listScriptsArg[index].Path);
                        XmlNodeList ParametersNode = xml.FirstChild.SelectNodes("/DMSScript/Parameters");
                        XmlNodeList ProtocolsNode = xml.FirstChild.SelectNodes("/DMSScript/Protocols");


                        if (!ParametersNode[0].HasChildNodes)
                        {
                            if (!ProtocolsNode[0].HasChildNodes)
                            {
                                indexOfParallelScripts.Add(index);
                            }
                            else
                            {
                                ListsParallel.AddToListScriptsWithProtocols(listScriptsArg[index].Name);
                            }
                        }
                        else
                        {
                            ListsParallel.AddToListScriptsWithParameters(listScriptsArg[index].Name);
                        }
                    }
                    catch(Exception ex)
                    {
                        LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Error while loading" + listScriptsArg[index].Path, ex.Message));
                    }

                }
                else if (listScriptsArg[index].Name.EndsWith(".dmimport"))
                {
                    if (File.Exists("C:\\Skyline DataMiner\\Scripts\\Script_" + Path.GetFileNameWithoutExtension(listScriptsArg[index].Name) + ".xml"))
                    {
                        try
                        {
                            XmlDocument xml = new XmlDocument();
                            xml.Load("C:\\Skyline DataMiner\\Scripts\\Script_" + Path.GetFileNameWithoutExtension(listScriptsArg[index].Name) + ".xml");

                            XmlNodeList ParametersNode = xml.FirstChild.SelectNodes("/DMSScript/Parameters");
                            XmlNodeList ProtocolsNode = xml.FirstChild.SelectNodes("/DMSScript/Protocols");

                            if (!ParametersNode[0].HasChildNodes)
                            {
                                if (!ProtocolsNode[0].HasChildNodes)
                                {
                                    indexOfParallelScripts.Add(index);
                                }
                                else
                                {
                                    ListsParallel.AddToListScriptsWithProtocols(listScriptsArg[index].Name);
                                }
                            }
                            else
                            {
                                ListsParallel.AddToListScriptsWithParameters(listScriptsArg[index].Name);
                            }
                        } 
                           catch (Exception ex)
                        {
                            LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Error while loading" + listScriptsArg[index].Path, ex.Message));
                        }
                    }
                }
            }

            listScripts = listScriptsArg;
            ScriptMessage = new ExecuteScriptMessage[indexOfParallelScripts.Count];
        }

        public void execute(RemotingConnection connectie, ListsScripts listsScriptsArg, string folderArg)
        {
            try
            {
                connectie.OnNewMessage += Connectie_OnNewMessage;
                var filter = new SubscriptionFilter(typeof(AlarmEventMessage));
                connectie.AddSubscription(filter);

                int NumberOfScriptWithParameters = 0;

                for (int index = 0; index < indexOfParallelScripts.Count; index++)
                {
                    if (listScripts[indexOfParallelScripts[index]].Name.EndsWith(".xml"))
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(listScripts[indexOfParallelScripts[index]].Path);
                        XmlNodeList ParametersNode = xml.FirstChild.SelectNodes("/DMSScript/Parameters");

                        if (!ParametersNode[0].HasChildNodes)
                        {
                            ScriptMessage[index - NumberOfScriptWithParameters] = new ExecuteScriptMessage();
                            string scriptnameWithoutExtension = Path.GetFileNameWithoutExtension(listScripts[indexOfParallelScripts[index]].Name);

                            ScriptMessage[index - NumberOfScriptWithParameters].DataMinerID = -1;

                            ScriptMessage[index - NumberOfScriptWithParameters].Options = new SA(new string[]
                              {
                        "OPTIONS:0",
                        "CHECKSETS: TRUE",
                        "EXTENDED_ERROR_INFO",
                        "DEFER:FALSE"
                              });

                            ScriptMessage[index - NumberOfScriptWithParameters].ScriptName = scriptnameWithoutExtension;
                        }
                        else
                        {
                            NumberOfScriptWithParameters++;
                        }
                    }

                    if (listScripts[indexOfParallelScripts[index]].Name.EndsWith(".dmimport"))
                    {
                        if (File.Exists("C:\\Skyline DataMiner\\Scripts\\Script_" + Path.GetFileNameWithoutExtension(listScripts[indexOfParallelScripts[index]].Name) + ".xml"))
                        {
                            XmlDocument xml = new XmlDocument();
                            xml.Load("C:\\Skyline DataMiner\\Scripts\\Script_" + Path.GetFileNameWithoutExtension(listScripts[indexOfParallelScripts[index]].Name) + ".xml");
                            XmlNodeList ParametersNode = xml.FirstChild.SelectNodes("/DMSScript/Parameters");

                            if (!ParametersNode[0].HasChildNodes)
                            {
                                ScriptMessage[index - NumberOfScriptWithParameters] = new ExecuteScriptMessage();
                                string scriptnameWithoutExtension = Path.GetFileNameWithoutExtension(listScripts[indexOfParallelScripts[index]].Name);

                                ScriptMessage[index - NumberOfScriptWithParameters].DataMinerID = -1;

                                ScriptMessage[index - NumberOfScriptWithParameters].Options = new SA(new string[]
                                  {
                                "OPTIONS:0",
                                "CHECKSETS: TRUE",
                                "EXTENDED_ERROR_INFO",
                                "DEFER:FALSE"
                                  });

                                ScriptMessage[index - NumberOfScriptWithParameters].ScriptName = scriptnameWithoutExtension;
                            }
                            else
                            {
                                NumberOfScriptWithParameters++;
                            }
                        }
                    }
                }

                ExecuteScriptResponseMessage[] initResponse = connectie.HandleMessages(ScriptMessage) as ExecuteScriptResponseMessage[];


                ManualResetEventSlim wait = new ManualResetEventSlim(false);
                wait.Wait(1000);

                connectie.RemoveSubscription(filter);

                connectie.OnNewMessage -= Connectie_OnNewMessage;

            }
            catch (Exception ex)
            {
                listsScriptsArg.AddToListWithRequestedTimeoutsFolders(folderArg);
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1}.", DateTime.Now.ToString(), ex.Message));
            }
        }

        private void Connectie_OnNewMessage(object sender, NewMessageEventArgs e)
        {
            if (e.Message is AlarmEventMessage)
                ListsParallel.AddToListsGlobal((AlarmEventMessage)e.Message);
        }
    }
}
