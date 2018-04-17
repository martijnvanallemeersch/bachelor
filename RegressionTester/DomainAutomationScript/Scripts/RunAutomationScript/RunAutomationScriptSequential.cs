using DevRegTest.domainAutomationScript.Report;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace DevRegTest.domainAutomationScript.Element.RunAutomationScript
{
    static class RunAutomationScriptSequential
    {
        static private string ScriptNameSequential { get; set; }

        static public ListsScripts listsSequential { get; set; }

        static int scriptID;             

         static public void Execute(RemotingConnection connectie, ListsScripts listsSequentialArg, string scriptNameArg)
        {
            if(connectie != null)
            {
                listsSequential = listsSequentialArg;
                ScriptNameSequential = scriptNameArg;
                // set up callback to receive events
                connectie.OnNewMessage += HandleNewEventFromServer;

                var filter = new SubscriptionFilter(typeof(AlarmEventMessage));
                connectie.AddSubscription(filter);
                var filter2 = new SubscriptionFilter(typeof(ScriptProgressEventMessage));
                connectie.AddSubscription(filter2);

                RunScript(connectie);

                ManualResetEventSlim wait = new ManualResetEventSlim(false);
                wait.Wait(1000);

                connectie.RemoveSubscription(filter);
                connectie.RemoveSubscription(filter2);

                connectie.OnNewMessage -= HandleNewEventFromServer;
            }           
        }

        // called when server sends an event
        static void HandleNewEventFromServer(object sender, NewMessageEventArgs e)
        {
            if (e.Message is ScriptProgressEventMessage)
            {
                ScriptProgressEventMessage progressEvent = (ScriptProgressEventMessage)e.Message;

                switch (progressEvent.Type)
                {
                    case ScriptProgressEventType.Completed:
                    {
                        lock (_scriptHandles)
                        {
                            if (_scriptHandles.ContainsKey(progressEvent.ScriptID))
                            {
                                // set the event so thet RunScript method stops waiting
                                _scriptHandles[progressEvent.ScriptID].Set();
                            }

                            // bool succeeded = String.Equals(progressEvent.Data, "SUCCEEDED", StringComparison.InvariantCultureIgnoreCase); // "SUCCEEDED" OR "FAILED"
                            // string[] errors = progressEvent.ExtraData;
                         }
                         break;
                    }
                }
            }
            else if (e.Message is AlarmEventMessage)
                listsSequential.AddToListsGlobal((AlarmEventMessage)e.Message);
        }

        static void RunScript(RemotingConnection c)
        {
            // prepare the script
            ExecuteScriptMessage initRequest = new ExecuteScriptMessage()
            {
                ScriptName = ScriptNameSequential,
                Options = new SA(new string[] { "INTERACTIVE" })
            };

            ExecuteScriptResponseMessage initResponse = null;

            try
            {
                initResponse = c.HandleSingleResponseMessage(initRequest) as ExecuteScriptResponseMessage;

                //Started = false;
                scriptID = initResponse.ScriptID;

                ManualResetEvent waitHandle = new ManualResetEvent(false);
                lock (_scriptHandles)
                {
                    _scriptHandles[scriptID] = waitHandle;
                }

                // tell the script to start
                ScriptControlMessage launchRequest = new ScriptControlMessage() { ScriptID = scriptID, Type = ScriptControlType.Launch };
                DMSMessage[] i = c.HandleMessage(launchRequest);

                // wait for the script to complete (waitHandle will be set via HandleNewEventFromServer)
                waitHandle.WaitOne();

                // cleanup wait handle
                lock (_scriptHandles)
                {
                    _scriptHandles.Remove(scriptID);
                }

                waitHandle.Dispose();
                waitHandle = null;
            }
            catch(Exception ex)
            {
                listsSequential.AddToListWithFailedToExecuteScript(initRequest.ScriptName);
            }
        }

        static public void abortscript()
        {
           _scriptHandles[scriptID].Set();
        }

        static private Dictionary<int, ManualResetEvent> _scriptHandles = new Dictionary<int, ManualResetEvent>();
    }
}
