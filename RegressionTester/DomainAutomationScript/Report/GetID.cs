using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.Advanced;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DevRegTest.domainVMApp;
using System.Management.Automation;
using Skyline.DataMiner.Net;
using System.Windows;

namespace DevRegTest.domainAutomationScript.Report
{
    class GetID
    {
        public GetDataMinerInfoResponseMessage GetInfoMessage(RemotingConnection connectie)
        {
            try
            {
                GetInfoMessage infoMessage = new GetInfoMessage();
                infoMessage.Type = InfoType.DataMinerInfo;

                GetDataMinerInfoResponseMessage mesg =
                    (GetDataMinerInfoResponseMessage)connectie.HandleSingleResponseMessage(infoMessage);

                return mesg;
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Problem get DMA ID", ex.Message));

                return null;
            }
        }
    }
}
