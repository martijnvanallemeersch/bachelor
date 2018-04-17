using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevRegTest.DomainAutomationScript.Element.Import
{
    static class ImportDelt
    {
        static Dictionary<DELTType, int> dictionaryOptions = new Dictionary<DELTType, int>();       

        static public void Import(RemotingConnection connectie, string Path)
        {
            try
            {
                connectie.OnNewMessage += HandleNewEventFromServer;
                var filter = new SubscriptionFilter(typeof(ImportProgressEventMessage));
                connectie.AddSubscription(filter);


                if (!dictionaryOptions.ContainsKey(DELTType.DELT_ELEMENT))
                    dictionaryOptions.Add(DELTType.DELT_ELEMENT, (Int32)(DELT_ELEMENT_OPTIONS.DELT_ELEMENT_DB_DATA | DELT_ELEMENT_OPTIONS.DELT_ELEMENT_DB_INFO));

                if (!dictionaryOptions.ContainsKey(DELTType.DELT_PROTOCOL))
                    dictionaryOptions.Add(DELTType.DELT_PROTOCOL, (Int32)(DELT_PROTOCOL_OPTIONS.DELT_PROTOCOL_IMPORT_ALARM_TEMPLATE | DELT_PROTOCOL_OPTIONS.DELT_PROTOCOL_IMPORT_TRENDING_TEMPLATE | DELT_PROTOCOL_OPTIONS.DELT_PROTOCOL_INFORMATION_TEMPLATE));

                byte[] BytePathToPackage = File.ReadAllBytes(Path);
                ImportRequestMessage importRequestMessage = new ImportRequestMessage(BytePathToPackage, dictionaryOptions, null, null);
                Guid importProgressIdentifier = importRequestMessage.GetProgressIdentifier();
                connectie.HandleSingleResponseMessage(importRequestMessage);

                ManualResetEvent waitHandle = new ManualResetEvent(false);

                lock (_importHandles)
                {
                    _importHandles[importProgressIdentifier] = waitHandle;
                }

                waitHandle.WaitOne(60000);

                // cleanup wait handle
                lock (_importHandles)
                {
                    _importHandles.Remove(importProgressIdentifier);
                }

                waitHandle.Dispose();
                waitHandle = null;

                ManualResetEventSlim wait = new ManualResetEventSlim(false);
                wait.Wait(1000);

                connectie.RemoveSubscription(filter);
                //connectie.OnNewMessage -= HandleNewEventFromServer;
            }
            catch(Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Failed to import Delt: " + ".", ex.Message));
            }
        }

        // called when server sends an event
        static private void HandleNewEventFromServer(object sender, NewMessageEventArgs e)
        {
            if (e.Message is ImportProgressEventMessage)
            {
                ImportProgressEventMessage progressEvent = (ImportProgressEventMessage)e.Message;

                switch (progressEvent.Type)
                {
                    case ImportProgressEventType.SUCCESS:
                        {
                            lock (_importHandles)
                            {
                                if (_importHandles.ContainsKey(progressEvent.GetProgressIdentifier()))
                                {
                                    // set the event so thet RunScript method stops waiting
                                    _importHandles[progressEvent.GetProgressIdentifier()].Set();
                                }
                            }
                            break;
                        }
                    //case ImportProgressEventType.PROGRESS:
                    //    {
                    //        lock (_importHandles)
                    //        {
                    //            if (_importHandles.ContainsKey(progressEvent.GetProgressIdentifier()))
                    //            {
                    //                // set the event so thet RunScript method stops waiting
                    //                _importHandles[progressEvent.GetProgressIdentifier()].Set();
                    //            }
                    //        }
                    //        break;
                    //    }
                }
            }
        }

        static private Dictionary<Guid, ManualResetEvent> _importHandles = new Dictionary<Guid, ManualResetEvent>();
    }
}
