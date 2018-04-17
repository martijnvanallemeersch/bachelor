using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.Advanced;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DevRegTest.domainAutomationScript.Element.AbortAutomationScript
{
    public class AbortAndGetRunningScripts
    {
        public SetAutomationInfoMessage AbortScriptMessage { get; private set; }

        public SetAutomationInfoResponseMessage AbortScriptMessageResponse { get; private set; }

        #region Constructor
        public AbortAndGetRunningScripts()
        { }
        #endregion Constructor

        //Method to know what that the ID's are from the running scripts at this moment
        public List<int> GetRunningScriptIDs(RemotingConnection connectie)
        {
            List<int> runningIDs = new List<int>();

            try
            {
                AbortScriptMessage = new SetAutomationInfoMessage();

                AbortScriptMessage.What = 31;

                AbortScriptMessage.Sa = new SA(new string[]
                    {
                    "LIST_SCRIPTIDS"
                    });

                AbortScriptMessageResponse = connectie.HandleSingleResponseMessage(AbortScriptMessage) as SetAutomationInfoResponseMessage;

                foreach (string Sa in AbortScriptMessageResponse.saRet.Sa)
                {
                    string[] parts = Sa.Split(';');
                    runningIDs.Add(int.Parse(parts[0]));
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Problem to get Running scripts", "Could not get the ID's from the running Scripts"));
            }

            return runningIDs;
        }

        //Method to abort the running scripts at this moment
        public void AbortRunningScipts(int ID, RemotingConnection connectie)
        {
            try
            {
                AbortScriptMessage = new SetAutomationInfoMessage();

                AbortScriptMessage.What = 31;

                AbortScriptMessage.Sa = new SA(new string[]
                    {
                    "ABORT_SCRIPT",
                    ID.ToString()
                    });

                AbortScriptMessageResponse = connectie.HandleSingleResponseMessage(AbortScriptMessage) as SetAutomationInfoResponseMessage;
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Problem to abort the running scripts", "Could not abort the running Scripts"));
            }
        }
    }
}
