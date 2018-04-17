using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Filters;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DevRegTest.domainAutomationScript.Report
{
    class SendMailWithReport
    {
        #region properties
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

        public SendMailWithReport()
        { }

        public void Execute(RemotingConnection connectie, ListsScripts listsSequentialArg, string emailAddressArg, string upgradeVersion, string textCombocopyArg)
        {
            generateResultMail(listsSequentialArg, emailAddressArg, upgradeVersion, textCombocopyArg);
        }

        //public void ExecuteFail(RemotingConnection connectie, string emailAddressArg, string upgradeVersion, string textCombocopyArg, int DMA_ID)
        //{
        //    generateResultMailFail(emailAddressArg, upgradeVersion, textCombocopyArg, connectie, DMA_ID);
        //}

        public void ExecuteToken(RemotingConnection connectie, string emailAddressArg, string upgradeVersion, string textCombocopyArg, int DMA_ID)
        {
            generateResultMailToken(emailAddressArg, upgradeVersion, textCombocopyArg, connectie, DMA_ID);
        }

        //generates the mail with the Script results
        public void generateResultMail(ListsScripts listsSequentialArg, string emailAddressArg, string upgradeVersion, string textCombocopyArg)
        {
            string strFailsTable = "";
            foreach (var fail in listsSequentialArg.totalFails)
            {
                strFailsTable += string.Format("<tr><td>{0}</td><td>{1}</td></tr>", fail.TimeOfArrival, WebUtility.HtmlEncode(fail.Value));
            }

            string _DBType = "";
            if (textCombocopyArg == "Testing_Cassandra")
                _DBType = "Cassandra";
            else if (textCombocopyArg == "Testing_MySQL")
                _DBType = "MySQL";

            string strSubject = string.Format("RT_FAILS {0}({1}): -{2}/~{3}/+{4} {5}", _strServerName, _DBType, listsSequentialArg.totalFails.Count, 
                    listsSequentialArg.totalWarnings.Count, listsSequentialArg.totalSuccesses.Count, upgradeVersion);
            string strMessage = GetScoreBoard(listsSequentialArg.totalSuccesses.Count, listsSequentialArg.totalFails.Count, listsSequentialArg.totalWarnings.Count);
            strMessage += ConvertMessagesToHTMLTable(listsSequentialArg.totalFails, "RT_FAIL");
            strMessage += ConvertMessagesToHTMLTable(listsSequentialArg.totalWarnings, "RT_WARNING");
            strMessage += ConvertMessagesToHTMLTable(listsSequentialArg.totalSuccesses, "RT_SUCCES");

            string to = emailAddressArg;
            string from = emailAddressArg;
            MailMessage message = new MailMessage(from, to);
            message.Subject = strSubject;
            message.Body = "<html><head><meta http - equiv =\"Content-Type\" content=\"text/html; charset=utf-8\"><title>Notification</title>" +
                                               css + "</head><body>" +
                                               "<table class=\"outer\" cellpadding=\"0\" cellspacing=\"0\"><tr class=\"outer\"><td class=\"outer\">" +
                                               "<table class=\"inner\" cellpadding=\"0\" cellspacing=\"0\"><tr class=\"inner\"><td class=\"inner\">" +
                                               "<div class=\"wrapper\">" +
                                               "<h1>" + strSubject + "</h1>" +
                                               "<div class=\"contents\">" + "<h2>" + strMessage + "</h2></div>" +
                                               "<div class=\"generator\">This notification was generated by the <a href=\"http://www.skyline.be/\">Skyline DataMiner</a> monitoring and control system.</div>" +
                                               "</div></td></tr></table></td></tr></table></body></html>";
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("10.12.0.3");

            client.UseDefaultCredentials = true;

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now.ToString() + " Error in sending mail " + ex.Message));
            }
        }

        //public void generateResultMailFail(string emailAddressArg, string upgradeVersion, string textCombocopyArg, RemotingConnection conn, int DMA_ID)
        //{
        //    string _DBType = "";
        //    if (textCombocopyArg == "Testing_Cassandra")
        //        _DBType = "Cassandra";
        //    else if (textCombocopyArg == "Testing_MySQL")
        //        _DBType = "MySQL";

        //    string strSubject = string.Format("Error(s) detected on {0} ({1}) so no script(s) could be runned!", upgradeVersion, _DBType);
        //    string strMessage = ConvertMessagesToHTMLTable(checkAlarmRecords(conn, DMA_ID), "Active alarms");

        //    string to = emailAddressArg;
        //    string from = emailAddressArg;
        //    MailMessage message = new MailMessage(from, to);
        //    message.Subject = strSubject;
        //    message.Body = "<html><head><meta http - equiv =\"Content-Type\" content=\"text/html; charset=utf-8\"><title>Notification</title>" +
        //                                       css + "</head><body>" +
        //                                       "<table class=\"outer\" cellpadding=\"0\" cellspacing=\"0\"><tr class=\"outer\"><td class=\"outer\">" +
        //                                       "<table class=\"inner\" cellpadding=\"0\" cellspacing=\"0\"><tr class=\"inner\"><td class=\"inner\">" +
        //                                       "<div class=\"wrapper\">" +
        //                                       "<h1>" + strSubject + "</h1>" +
        //                                       "<div class=\"contents\">" + "<h2>" + strMessage + "</h2></div>" +
        //                                       "<div class=\"generator\">This notification was generated by the <a href=\"http://www.skyline.be/\">Skyline DataMiner</a> monitoring and control system.</div>" +
        //                                       "</div></td></tr></table></td></tr></table></body></html>";
        //    message.IsBodyHtml = true;
        //    SmtpClient client = new SmtpClient("10.12.0.3");

        //    client.UseDefaultCredentials = true;

        //    try
        //    {
        //        client.Send(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now.ToString() + " Error in sending mail " + ex.Message));
        //    }
        //}

        public void generateResultMailToken(string emailAddressArg, string upgradeVersion, string textCombocopyArg, RemotingConnection conn, int DMA_ID)
        {
            string _DBType = "";
            if (textCombocopyArg == "Testing_Cassandra")
                _DBType = "Cassandra";
            else if (textCombocopyArg == "Testing_MySQL")
                _DBType = "MySQL";

            string strSubject = string.Format("Error(s) detected on {0} ({1}) so script(s) are aborted!", upgradeVersion, _DBType);
            string strMessage = "There are folder(s) detected in the folders: 'CrashDump', 'MiniDump' and 'WatchDog'.";

            string to = emailAddressArg;
            string from = emailAddressArg;
            MailMessage message = new MailMessage(from, to);
            message.Subject = strSubject;
            message.Body = "<html><head><meta http - equiv =\"Content-Type\" content=\"text/html; charset=utf-8\"><title>Notification</title>" +
                                               css + "</head><body>" +
                                               "<table class=\"outer\" cellpadding=\"0\" cellspacing=\"0\"><tr class=\"outer\"><td class=\"outer\">" +
                                               "<table class=\"inner\" cellpadding=\"0\" cellspacing=\"0\"><tr class=\"inner\"><td class=\"inner\">" +
                                               "<div class=\"wrapper\">" +
                                               "<h1>" + strSubject + "</h1>" +
                                               "<div class=\"contents\">" + "<h2>" + strMessage + "</h2></div>" +
                                               "<div class=\"generator\">This notification was generated by the <a href=\"http://www.skyline.be/\">Skyline DataMiner</a> monitoring and control system.</div>" +
                                               "</div></td></tr></table></td></tr></table></body></html>";
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("10.12.0.3");

            client.UseDefaultCredentials = true;

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now.ToString() + " Error in sending mail " + ex.Message));
            }
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

        public string GetScoreBoard(int SuccesCount, int FailCount, int WarningCount)
        {
            return string.Format("<h3>RT_FAIL: {0}</h3>"
                               + "<h3>RT_WARNING: {1}</h3>"
                               + "<h3>RT_SUCCES: {2}</h3>", FailCount, WarningCount, SuccesCount);
        }
    }
}
