using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Filters;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.Advanced;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace DevRegTest.domainAutomationScript.Report
{
    public class MakeXMLFail
    {
        #region properties
        public string DestinationPath { get; set; }

        string _strServerName = "localhost";

        const string css = "<style type = \"text/css\">"
                            + "html, body {"
                                + "margin: 0px;"
                                + "padding: 0px;"
                                + "background-color: #eee;"
                            + "}"
                            + "table.outer {"
                                + "padding: 30px;"
                                + "margin: 0px;"
                                + "border: 0px;"
                                + "width: 100%;"
                                + "background-color: #eee;"
                            + "}"
                            + "table.inner {"
                                + "margin: 0px;"
                                + "padding: 0px;"
                                + "width: 100%;"
                                + "background-color: #fff;"
                            + "}"
                            + ".wrapper {"
                                + "border: 1px solid #ddd;"
                                + "border-right: 2px #aaa;"
                                + "border-bottom: 2px #aaa;"
                                + "background-color: #fff;"
                                + "color: #000;"
                                + "padding: 10px;"
                                + "margin: 0px;"
                            + "}"
                            + "body, .wrapper, .contents table, td, tr, ul, li {"
                                + "font-family: verdana, arial, sans-serif;"
                                + "font-size: 8pt;"
                            + "}"
                            + "td {"
                                + "font-size: 7pt;"
                            + "}"
                            + "h1 {"
                                + "font-size: 16pt;"
                            + "}"
                            + "h2 {"
                                + "font-size: 14pt;"
                            + "}"
                            + "h3 {"
                                + "font-size: 12pt;"
                            + "}"
                            + "h4 {"
                                + "font-size: 10pt;"
                            + "}"
                            + "h1, h2, h3, h4 {"
                                + "border-bottom: 1px solid black;"
                            + "}"
                            + ".contents table"
                            + "{"
                                + "border-collapse: collapse;"
                                + "border-spacing: 0;"
                                + "width: 100%;"
                                + "max-width: 800px;"
                                + "background-color: #fff;"
                            + "}"
                            + ".contents tr td, .contents tr th {"
                                + "background-color: #fff;"
                                + "margin: 0px;"
                                + "padding: 0.7em 0.5em;"
                                + "border: solid #193e7f;"
                                + "border-width: 1px;"
                            + "}"
                            + ".contents table"
                            + "{"
                                + "border: solid #193e7f;"
                                + "border-width: 1px;"
                            + "}"
                            + ".contents tr.alternate td {"
                                + "background - color: #edf2fc;"
                            + "}"
                            + ".contents tr.header th {"
                                + "background - color: #d5e1f7;"
                                + "border - width: 1px;"
                                + "white - space: nowrap;"
                            + "}"
                            + ".generator {"
                                + "margin-top: 10px;"
                                + "border-top: 1px solid black;"
                                + "padding: 3px;"
                                + "text-align: right;"
                            + "}"
                            /* 
			                    severities (1 = critical, 5=normal) 
		
			                    1 = critical
			                    ...
			                    5 = normal
			                    13 = information
			                    24 = error
			                    28 = notice
		                    */
                            + ".severity1 { background-color: #f88 !important; color: #fff !important; }"
                            + ".severity2 { background-color: #ff8 !important; }"
                            + ".severity3 { background-color: #8ff !important; }"
                            + ".severity4 { background-color: #88f !important; color: #fff !important;}"
                            + ".severity5 { background-color: #8f8 !important; }"
                            + ".severity13 { background-color: #eee !important; }"
                            + ".severity17 { background-color: #f90 !important; color: #fff !important;}"
                            + ".severity24 { background-color: #c0c0c0 !important; color: #fff !important; }"
                            + ".severity28 { background-color: #c0c0c0 !important; }"
                            /*
                                overrides 
                            */
                            /*
                                DO NOT EDIT THIS FILE - CHANGES WILL BE LOST WHEN DATAMINER RESTARTS
                            */
                            + "td.alarmstate1, .severity1, td.alarmstate1 a"
                            + "{"
                                + "background-color: #f88 !important;"
                                + "color: #fff !important;"
                            + "}"
                            + "td.alarmstate2, .severity2, td.alarmstate2 a"
                            + "{"
                                + "background-color: #ff8 !important;"
                                + "color: #000 !important;"
                            + "}"
                            + "td.alarmstate3, .severity3, td.alarmstate3 a"
                            + "{"
                                + "background-color: #8ff !important;"
                                + "color: #000 !important;"
                            + "}"
                            + "td.alarmstate4, .severity4, td.alarmstate4 a"
                            + "{"
                                + "background-color: #88f !important;"
                                + "color: #fff !important;"
                            + "}"
                            + "td.alarmstate5, .severity5, td.alarmstate5 a"
                            + "{"
                                + "background-color: #8f8 !important;"
                                + "color: #000 !important;"
                            + "}"
                            + "td.alarmstate13, .severity13, td.alarmstate13 a"
                            + "{"
                                + "background-color: #ccc !important;"
                                + "color: #000 !important;"
                            + "}"
                            + ".severity24, td.alarmstate24, td.alarmstate24 a"
                            + "{"
                                + "background-color: #c0c0c0 !important;"
                                + "color: #000 !important;"
                            + "}"
                            + ".severity25, td.alarmstate25, td.alarmstate25 a"
                            + "{"
                                + "background-color: #808 !important;"
                                + "color: #fff !important;"
                            + "}"
                            + "td.alarmstate28, .severity28, td.alarmstate28 a"
                            + "{"
                                + "background-color: #c0c0c0 !important;"
                                + "color: #000 !important;"
                            + "}"
                            + "td.masked, td.es_masked {"
                                + "border: 1px dotted #808;"
                                + "background-color: #fff !important;"
                                + "color: #808 !important;"
                            + "}"
                            + "td.es_stop, td.es_inactive, td.es_notinited {"
                                + "background-color: #f2f2f2 !important;"
                                + "color: #000 !important;"
                            + "}"
                            + "td.alarmstate17, .severity17, td.es_timeout, td.alarmstate17 a"
                            + "{"
                                + "background-color: #f90 !important;"

                                + "color: #fff !important;"
                            + "}"
                            + "td.es_notemplate {"
                                + "background-color: #ddd !important;"
                                + "color: #000 !important;"
                            + "}"
                            + "table.outer { padding: 0px; }"
                            + "table.inner { background-color: #eee; }"
                            + ".wrapper { background-color: #fff; margin: 30px; }"
                            + "</style>";
        #endregion properties

        public MakeXMLFail(string DestinationPathArg)
        {
            DestinationPath = DestinationPathArg;
        }

        public void ExecuteFail(string upgradeVersion, string textCombocopyArg, RemotingConnection conn, int DMA_ID)
        {
            generateResultPathFail(DestinationPath, upgradeVersion, textCombocopyArg, conn, DMA_ID);
        }

        public void generateResultPathFail(string destinationPath, string upgradeVersion, string textCombocopyArg, RemotingConnection conn, int DMA_ID)
        {
            string _DBType = "";
            if (textCombocopyArg == "Testing_Cassandra")
                _DBType = "Cassandra";
            else if (textCombocopyArg == "Testing_MySQL")
                _DBType = "MySQL";

            string strSubject = string.Format("Error(s) detected on {0} ({1}) so no script(s) could be runned!", upgradeVersion, _DBType);
            string strMessage = ConvertMessagesToHTMLTable(checkAlarmRecords(conn, DMA_ID), "Active alarms");

            foreach (AlarmEventMessage mes in checkAlarmRecords(conn, DMA_ID))
                LoggingErrors.AddExceptionToList(mes.TimeOfArrival.ToString(AlarmEventMessage.DateTimeFormat) + ": " + mes.Value + ".");

            CreateDirectory(destinationPath);

            destinationPath += "\\Error.HTML";

            using (var stream = new FileStream(destinationPath, FileMode.OpenOrCreate))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("<html><head><meta http - equiv =\"Content-Type\" content=\"text/html; charset=utf-8\"><title>Notification</title>" +
                                               css + "</head><body>" +
                                               "<table class=\"outer\" cellpadding=\"0\" cellspacing=\"0\"><tr class=\"outer\"><td class=\"outer\">" +
                                               "<table class=\"inner\" cellpadding=\"0\" cellspacing=\"0\"><tr class=\"inner\"><td class=\"inner\">" +
                                               "<div class=\"wrapper\">" +
                                               "<h1>" + strSubject + "</h1>" +
                                               "<div class=\"contents\">" + "<h2>" + strMessage + "</h2></div>" +
                                               "<div class=\"generator\">This notification was generated by the <a href=\"http://www.skyline.be/\">Skyline DataMiner</a> monitoring and control system.</div>" +
                                               "</div></td></tr></table></td></tr></table></body></html>");
                }
            }
        }

        public void ExecuteToken(string upgradeVersion, string textCombocopyArg, RemotingConnection conn, int DMA_ID)
        {
            generateResultPathToken(DestinationPath, upgradeVersion, textCombocopyArg, conn, DMA_ID);
        }

        public void generateResultPathToken(string destinationPath, string upgradeVersion, string textCombocopyArg, RemotingConnection conn, int DMA_ID)
        {
            string _DBType = "";
            if (textCombocopyArg == "Testing_Cassandra")
                _DBType = "Cassandra";
            else if (textCombocopyArg == "Testing_MySQL")
                _DBType = "MySQL";

            string strSubject = string.Format("Error(s) detected on {0} ({1}) so script(s) are aborted!", upgradeVersion, _DBType);
            string strMessage = "There are folder(s) detected in the folders: 'CrashDump', 'MiniDump' and 'WatchDog'.";

            CreateDirectory(destinationPath);

            destinationPath += "\\Token.HTML";

            using (var stream = new FileStream(destinationPath, FileMode.OpenOrCreate))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("<html><head><meta http - equiv =\"Content-Type\" content=\"text/html; charset=utf-8\"><title>Notification</title>" +
                                               css + "</head><body>" +
                                               "<table class=\"outer\" cellpadding=\"0\" cellspacing=\"0\"><tr class=\"outer\"><td class=\"outer\">" +
                                               "<table class=\"inner\" cellpadding=\"0\" cellspacing=\"0\"><tr class=\"inner\"><td class=\"inner\">" +
                                               "<div class=\"wrapper\">" +
                                               "<h1>" + strSubject + "</h1>" +
                                               "<div class=\"contents\">" + "<h2>" + strMessage + "</h2></div>" +
                                               "<div class=\"generator\">This notification was generated by the <a href=\"http://www.skyline.be/\">Skyline DataMiner</a> monitoring and control system.</div>" +
                                               "</div></td></tr></table></td></tr></table></body></html>");
                }
            }
        }

        public string ConvertMessagesToHTMLTable(List<AlarmEventMessage> aem, string Title = "")
        {
            // If no AlarmEventMessages, return nothing
            if (aem.Count == 0) return "";

            string strContent = "";

            // If Title is given, set the header of the table
            if (Title.Length > 0)
            {
                strContent += string.Format("<th>{0}</th>", WebUtility.HtmlEncode(Title));
            }

            // Go over every message to fill the table
            foreach (var message in aem)
            {
                strContent += string.Format("<tr><td>{0}</td><td>{1}</td></tr>", WebUtility.HtmlEncode(message.TimeOfArrival.ToString(AlarmEventMessage.DateTimeFormat)), WebUtility.HtmlEncode(message.Value));
            }

            // Return content in table
            return string.Format("<table>{0}</table>", strContent);
        }

        public List<AlarmEventMessage> checkAlarmRecords(RemotingConnection conn, int DMA_ID)
        {
            List<AlarmEventMessage> list = new List<AlarmEventMessage>();

            GetActiveAlarmsMessage message = new GetActiveAlarmsMessage()
            {
                DataMinerID = DMA_ID,
                HostingDataMinerID = DMA_ID
            };

            ActiveAlarmsResponseMessage response = (ActiveAlarmsResponseMessage)conn.HandleSingleResponseMessage(message);
            list.AddRange(response.ActiveAlarms);

            return list;
        }

        public void CreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    dir.Delete(true);
                }
            }

            Directory.CreateDirectory(path);
        }
    }
}
