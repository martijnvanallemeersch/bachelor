using DevRegTest.domainVMApp;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevRegTest.domainAutomationScript.Element.Delete
{
    class DeleteElement
   {
        public int ElementId { get; set; }

        public int OldDataminerId { get; set; }

        public string Ip { get; set; }

        public string ElementName { get; set; }

        public bool DoNotMakeNewOne { get; set; }

        public int DMA_ID { get; set; }

        public DeleteElement( string ElementNameP, int ID_DMA)
        {
            Ip = "127.0.0.1";

            ElementName = ElementNameP;

            DoNotMakeNewOne = false;

            DMA_ID = ID_DMA;
        }

        private DMSMessage[] GetInMessage(RemotingConnection connectie)
        {
            GetInfoMessage infoMessage = new GetInfoMessage();
            infoMessage.DataMinerID = DMA_ID;
            infoMessage.Type = InfoType.ElementInfo;

            DMSMessage[] resp = connectie.HandleMessage(infoMessage);
            return resp;
        }

        public void Invoke(RemotingConnection connectie)
        {
            DMSMessage[] infMessages = GetInMessage(connectie);

            for (int aantal = 0; aantal < infMessages.Length; aantal++)
            {
                ElementInfoEventMessage tempMes = (ElementInfoEventMessage)infMessages[aantal];

                if (tempMes.Name == "TrendRegressionTester")
                {
                    DoNotMakeNewOne = true;
                    ElementId = tempMes.ElementID;
                    OldDataminerId = tempMes.DataMinerID;
                }
            }

            if(DoNotMakeNewOne == true)
            {
                SetElementStateMessage sesm = new SetElementStateMessage();
                sesm.BState = true;
                sesm.DataMinerID = OldDataminerId;
                sesm.ElementId = ElementId;
                sesm.State = ElementState.Deleted;
                sesm.HostingDataMinerID = DMA_ID;

                connectie.HandleSingleResponseMessage(sesm);
                DoNotMakeNewOne = false;
            }
        }
    }
}
