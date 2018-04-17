using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevRegTest.domainAutomationScript.Element.RunAutomationScript
{
    class AddDelt
    { 
        ExecuteScriptMessage ScriptMessage;
        ScriptProgressEventMessage scriptProgressEventMessageObject;
        Dictionary<DELTType, int> dictionaryOptions = new Dictionary<DELTType, int>();

        public string PathC { get; set; }

        public string FileName { get; set; }

        public AddDelt(string filename)
        {
            FileName = filename;
            PathC = @"\\NAS\Shares\Public\Software Development\Testing\Delt\";
        }

        public void execute(RemotingConnection connectie)
        {
            dictionaryOptions.Add(DELTType.DELT_ELEMENT, (Int32)(DELT_ELEMENT_OPTIONS.DELT_ELEMENT_DB_DATA | DELT_ELEMENT_OPTIONS.DELT_ELEMENT_DB_INFO));
            byte[] BytePathToPackage = File.ReadAllBytes(PathC + FileName);
            //byte[] BytePathToPackage = File.ReadAllBytes(@"C:\development\RT_Tests\RT_Alarm_Restart\RT_Alarm_Restart.dmimport");
            connectie.Subscribe(new SubscriptionFilter("importProgressEventMessage"));
            connectie.Subscribe(new SubscriptionFilter("ImportRequestMessage"));
            connectie.Subscribe(new SubscriptionFilter("ScriptProgressEventMessage"));

            ImportRequestMessage importRequestMessage = new ImportRequestMessage(BytePathToPackage, dictionaryOptions, null, null);

            connectie.HandleSingleResponseMessage(importRequestMessage);
        }        
    }
}
