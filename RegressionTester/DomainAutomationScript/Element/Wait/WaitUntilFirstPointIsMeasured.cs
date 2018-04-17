using DevRegTest.domainVMApp;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevRegTest.domainAutomationScript.Report
{
    class WaitUntilFirstPointIsMeasured
    {
        public int DMA_ID { get; set; }

        GetTrendDataResponseMessage response;

        public WaitUntilFirstPointIsMeasured(int DMA_ID_arg)
        {
            DMA_ID = DMA_ID_arg;
            response = null;
        }

        public GetTrendDataResponseMessage Check(RemotingConnection connectie)
        {
            try
            {
                GetTrendDataMessage trendDataMessage = new GetTrendDataMessage();

                trendDataMessage.StartTime = DateTime.Now.AddMinutes(1);
                trendDataMessage.EndTime = new DateTime(9999, 12, 31);
                trendDataMessage.HostingDataMinerID = DMA_ID;
                trendDataMessage.DataMinerID = DMA_ID;

                DMSMessage[] infMessages = GetInMessage(connectie);

                for (int aantal = 0; aantal < infMessages.Length; aantal++)
                {
                    ElementInfoEventMessage tempMes = (ElementInfoEventMessage)infMessages[aantal];
                    if (tempMes.Name == "TrendRegressionTester")
                    {

                        GetTrendingTemplateInfoResponseMessage templates = GetTrendPairs(tempMes, connectie);
                        GetElementProtocolResponseMessage protocol = GetProtocol(tempMes, connectie);

                        // trendDataMessage.ReturnAsObjects = true;
                        trendDataMessage.ElementID = tempMes.ElementID;
                        trendDataMessage.Fields = new string[] { "iPid", "chIndex", "iStatus", "dtFirst", "chValue" };
                        trendDataMessage.TrendingType = TrendingType.Realtime;

                        if (templates != null)
                        {
                            foreach (ParamTrendingInfo info in templates.Parameters.Where(p => p.ParameterID == 107))
                            {
                                trendDataMessage.Parameters = new ParameterIndexPair[] { new ParameterIndexPair(info.ParameterID) };
                                string nameOfProtocol = protocol.AllParameters.Where(pi => pi.ID == info.ParameterID).FirstOrDefault().Name;

                                if (!nameOfProtocol.Contains('('))
                                {
                                    nameOfProtocol = nameOfProtocol.Replace(" ", String.Empty);
                                    nameOfProtocol = nameOfProtocol.Replace("/", "");

                                    //Will pull data from all processes out of the TableIndicesSLProcesses
                                    if (info.Filter == "*")
                                    {
                                        bool started = false;

                                        while (started == false)
                                        {
                                            try
                                            {
                                                if (DMA.tokenCrash.IsCancellationRequested == false)
                                                {
                                                    trendDataMessage.Parameters = new ParameterIndexPair[] { new ParameterIndexPair(107, "SLElement.0") };
                                                    trendDataMessage.DateTimeUTC = false;
                                                    response = (GetTrendDataResponseMessage)connectie.HandleMessage(trendDataMessage)[0];
                                                    DMSMessage[] allResponses = connectie.HandleMessage(trendDataMessage);
                                                    if (response.Values.Length >= 5)
                                                    {
                                                        started = true;
                                                    }

                                                    ManualResetEventSlim wait2 = new ManualResetEventSlim(false);
                                                    wait2.Wait(1000);
                                                }
                                                else
                                                {
                                                    started = true;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                ManualResetEventSlim wait = new ManualResetEventSlim(false);
                                                wait.Wait(1000);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": " + ex.ToString() + ".");
            }

            return response;
        }

        //Retrevies general info for other messages
        private DMSMessage[] GetInMessage(RemotingConnection Conn)
        {
            GetInfoMessage infoMessage = new GetInfoMessage();
            infoMessage.DataMinerID = DMA_ID;
            infoMessage.Type = InfoType.ElementInfo;

            DMSMessage[] resp = null;

            try
            {
                resp = Conn.HandleMessage(infoMessage);
            }
            catch(Exception ex)
            {
                LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": " + ex.ToString() + ".");
            }

            return resp;
        }

        // Retrieves trend info from an element
        private GetTrendingTemplateInfoResponseMessage GetTrendPairs(ElementInfoEventMessage mess, RemotingConnection Conn)
        {
            GetTrendingTemplateInfoMessage GTTIM = new GetTrendingTemplateInfoMessage(mess.Protocol, mess.ProtocolVersion, mess.Trending);

            DMSMessage[] resp = null;

            try
            {
                resp = Conn.HandleMessage(GTTIM);
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": " + ex.ToString() + ".");
            }
            return (GetTrendingTemplateInfoResponseMessage)resp[0];
        }

        //Is needed to link the parameter id with a name
        private GetElementProtocolResponseMessage GetProtocol(ElementInfoEventMessage mess, RemotingConnection Conn)
        {
            GetElementProtocolMessage GEPM = new GetElementProtocolMessage(mess.DataMinerID, mess.ElementID);

            DMSMessage[] resp = null;

            try
            {
                resp = Conn.HandleMessage(GEPM);
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(DateTime.Now.ToString() + ": " + ex.ToString() + ".");
            }

            return (GetElementProtocolResponseMessage)resp[0];
        }
    }
}
