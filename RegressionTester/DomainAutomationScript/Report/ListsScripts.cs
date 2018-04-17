using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevRegTest.domainAutomationScript.Report
{
    public class ListsScripts
    {
        public List<AlarmEventMessage> totalFails { get; set; }

        public List<AlarmEventMessage> totalWarnings { get; set; }

        public List<AlarmEventMessage> totalSuccesses { get; set; }

        public List<string> TotalFailsPath { get; set; }

        public List<string> TotalUnnoticedInformationEvents { get; set; }

        public List<string> TotalScriptWithParameters { get; set; }

        public List<string> TotalScriptWithProtocols { get; set; }

        public Dictionary<string, DateTime[]> DictionaryScript { get; set; }

        public List<AlarmEventMessage> TotalAlarmsAndEvents { get; set; }

        public List<string> TotalBatchFiles { get; set; }

        public List<string> TotalDelts { get; set; }

        public List<string> TotalRequestedTimeoutsFolders { get; set; }

        public List<string> TotalfailedToExcecuteScript { get; set; }

        public Dictionary<string, Dictionary<string, List<string>>> overview { get; set; }

        public ListsScripts()
        {
            totalFails = new List<AlarmEventMessage>();
            totalWarnings = new List<AlarmEventMessage>();
            totalSuccesses = new List<AlarmEventMessage>();
            TotalFailsPath = new List<string>();
            TotalBatchFiles = new List<string>();
            TotalDelts = new List<string>();
            TotalUnnoticedInformationEvents = new List<string>();
            TotalScriptWithParameters = new List<string>();
            TotalScriptWithProtocols = new List<string>();
            DictionaryScript = new Dictionary<string, DateTime[]>();
            TotalAlarmsAndEvents = new List<AlarmEventMessage>();
            TotalRequestedTimeoutsFolders = new List<string>();
            TotalfailedToExcecuteScript = new List<string>();
            overview = new Dictionary<string, Dictionary<string, List<string>>>();
        }

        public void AddToLists(List<AlarmEventMessage> fails, List<AlarmEventMessage> warnings, List<AlarmEventMessage> successes,
           string nameScript)
        {
            totalFails.AddRange(fails);
            totalWarnings.AddRange(warnings);
            totalSuccesses.AddRange(successes);

            if (fails.Count == 0 && successes.Count == 0 && warnings.Count == 0)
                TotalUnnoticedInformationEvents.Add(nameScript);
        }

        public void AddToLists(List<AlarmEventMessage> fails, List<AlarmEventMessage> warnings, List<AlarmEventMessage> successes, 
            string DestinationPath, string nameScript)
        {            
            totalFails.AddRange(fails);
            totalWarnings.AddRange(warnings);
            totalSuccesses.AddRange(successes);          

            if (fails.Count() != 0)
                TotalFailsPath.Add(DestinationPath);

            if (fails.Count == 0 && successes.Count == 0 && warnings.Count == 0)
                TotalUnnoticedInformationEvents.Add(nameScript);
        }

        public void AddToDictionary(string keyArg, DateTime[] timeScript)
        {
            if (!DictionaryScript.ContainsKey(keyArg))
                DictionaryScript.Add(keyArg, timeScript);
        }
      

        public void AddToListsGlobal(AlarmEventMessage AlarmAndEventArg)
        {
            TotalAlarmsAndEvents.Add(AlarmAndEventArg);
        }

        public void AddToListScriptsWithParameters(string scriptWithParametersArg)
        {
            TotalScriptWithParameters.Add(scriptWithParametersArg);
        }

        public void AddToListScriptsWithProtocols(string scriptWithProtocolsArg)
        {
            TotalScriptWithProtocols.Add(scriptWithProtocolsArg);
        }

        public void AddToListBatchFiles(string batchfileNameArg)
        {
            TotalBatchFiles.Add(batchfileNameArg);
        }

        public void AddToListDelts(string deltNameArg)
        {
            TotalDelts.Add(deltNameArg);
        }

        public void AddToListWithRequestedTimeoutsFolders(string Folder)
        {
            TotalRequestedTimeoutsFolders.Add(Folder);
        }

        public void AddToListWithFailedToExecuteScript(string script)
        {
            TotalfailedToExcecuteScript.Add(script);
        }

        public void AddToOverview(List<AlarmEventMessage> fails, List<AlarmEventMessage> warnings,
            List<AlarmEventMessage> successes, string nameScript, string duration)
        {
            if (!overview.ContainsKey(nameScript))
            {
                Dictionary<string, List<string>> subOverview = new Dictionary<string, List<string>>();

                List<string> failsDispalyValue = new List<string>();

                foreach (AlarmEventMessage message in fails)
                    failsDispalyValue.Add(message.DisplayValue);

                subOverview.Add("fails", failsDispalyValue);

                List<string> warningsDispalyValue = new List<string>();

                foreach (AlarmEventMessage message in warnings)
                    warningsDispalyValue.Add(message.DisplayValue);

                subOverview.Add("warnings", warningsDispalyValue);

                List<string> successesDispalyValue = new List<string>();

                foreach (AlarmEventMessage message in successes)
                    successesDispalyValue.Add(message.DisplayValue);

                subOverview.Add("successes", successesDispalyValue);

                subOverview.Add("duration", new List<string>() { duration });

                overview.Add(nameScript, subOverview);
            }
        }
    }
}
