using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DevRegTest.domainAutomationScript.Element.Restart
{
    public class RestartAllElements
    {
        public int DMA_ID { get; set; }

        private DMSMessage[] GetInMessage(RemotingConnection Conn)
        {
            GetInfoMessage infoMessage = new GetInfoMessage();
            infoMessage.DataMinerID = DMA_ID;
            infoMessage.Type = InfoType.ElementInfo;

            DMSMessage[] resp = Conn.HandleMessage(infoMessage);
            return resp;
        }

        public RestartAllElements(int ID_DMA)
        {
            DMA_ID = ID_DMA;
        }

        public void Execute(RemotingConnection Conn)
        {
            try
            {
                DMSMessage[] infMessages = GetInMessage(Conn);

                for (int aantal = 0; aantal < infMessages.Length; aantal++)
                {
                    ElementInfoEventMessage tempMes = (ElementInfoEventMessage)infMessages[aantal];

                    SetElementStateMessage setElementStateMessage = new SetElementStateMessage();
                    setElementStateMessage.DataMinerID = tempMes.DataMinerID;
                    setElementStateMessage.ElementId = tempMes.ElementID;
                    setElementStateMessage.HostingDataMinerID = DMA_ID;
                    setElementStateMessage.State = ElementState.Restart;

                    DMSMessage resp = Conn.HandleSingleResponseMessage(setElementStateMessage);
                }
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Fail!", ex.Message));
            }
        }
    }
}
