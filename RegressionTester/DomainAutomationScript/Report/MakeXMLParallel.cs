using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Filters;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.Advanced;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace DevRegTest.domainAutomationScript.Report
{
    public class MakeXMLParallel
    {
        #region properties
        public string DestinationPath { get; set; }

        public DateTime[] RealRunningTime { get; set; }

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

        public string SelectedFolderPlusFileNameWithExtension { get; set; }
        #endregion properties

        public MakeXMLParallel(string DestinationPathArg,string SelectedFolderArg)
        {
            DestinationPath = DestinationPathArg;
            SelectedFolderPlusFileNameWithExtension = SelectedFolderArg;
            RealRunningTime = new DateTime[] { DateTime.Now, DateTime.Now };
        }

        public MakeXMLParallel(string DestinationPathArg)
        {
            DestinationPath = DestinationPathArg;
        }

        public void GetRunningTime(string name, ListsScripts ListsScriptsArg, CyclusParallelTime cyclusParallelTimeArg, string path)
        {
            string nameScriptWithoutExtension = Path.GetFileNameWithoutExtension(path);
            List<AlarmEventMessage> successes = checkInfoRecords("RT_SUCCES", nameScriptWithoutExtension, ListsScriptsArg);
            List<AlarmEventMessage> failsFail = checkInfoRecords("RT_FAIL", nameScriptWithoutExtension, ListsScriptsArg);
            List<AlarmEventMessage> failsFailure = checkInfoRecords("Script Failure", nameScriptWithoutExtension, ListsScriptsArg);
            List<AlarmEventMessage> fails = new List<AlarmEventMessage>();
            fails.AddRange(failsFail);
            fails.AddRange(failsFailure);
            List<AlarmEventMessage> warnings = checkInfoRecords("RT_WARNING", nameScriptWithoutExtension, ListsScriptsArg);

            if (fails.Count != 0 && successes.Count != 0)
            {
                if (fails[0].CreationTime > successes[0].CreationTime)
                {
                    RealRunningTime[1] = fails.First().CreationTime;
                }
                else if (fails[0].CreationTime < successes[0].CreationTime)
                {
                    RealRunningTime[1] = successes.First().CreationTime;
                }
                else
                {
                    RealRunningTime[1] = successes.First().CreationTime;
                }
            }
            else if(fails.Count == 0 && successes.Count != 0)
            {
                RealRunningTime[1] = successes.First().CreationTime;
            }
            else if (fails.Count != 0 && successes.Count == 0)
            {
                RealRunningTime[1] = fails.First().CreationTime;
            }

            //Look when the script is beginning.
            List<AlarmEventMessage> all = checkInfoRecords("", "", ListsScriptsArg);
            List<AlarmEventMessage> allStarted = new List<AlarmEventMessage>();
            DateTime begintijd = RealRunningTime[1];

            foreach (AlarmEventMessage message in all)
            {
                if (message.ParameterName.ToLower() == "script started")
                {
                    allStarted.Add(message);
                }
            }

            foreach(AlarmEventMessage message in allStarted)
            {
                if (message.DisplayValue.Contains(nameScriptWithoutExtension))
                {
                    begintijd = message.CreationTime;
                    RealRunningTime[0] = message.CreationTime;
                }
            }

            if (File.Exists(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Scripts_running_time.xml"))
            {
                try
                {
                    if ((RealRunningTime[1] - RealRunningTime[0]).TotalSeconds < 0)
                    {
                        XDocument doc = XDocument.Load(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Scripts_running_time.xml");
                        XElement root = new XElement("ScriptRun");
                        root.Add(new XElement("ScriptName", name));
                        root.Add(new XElement("StartTime", RealRunningTime[0].ToString()));
                        root.Add(new XElement("EndTime", RealRunningTime[1].ToString()));
                        root.Add(new XElement("Duration", "00:00:00"));
                        doc.Element("root").Add(root);
                        doc.Save(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Scripts_running_time.xml");
                    }
                    else
                    {
                        XDocument doc = XDocument.Load(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Scripts_running_time.xml");
                        XElement root = new XElement("ScriptRun");
                        root.Add(new XElement("ScriptName", name));
                        root.Add(new XElement("StartTime", RealRunningTime[0].ToString()));
                        root.Add(new XElement("EndTime", RealRunningTime[1].ToString()));
                        root.Add(new XElement("Duration", (RealRunningTime[1] - RealRunningTime[0]).ToString()));
                        doc.Element("root").Add(root);
                        doc.Save(@"C:\RegressionTester_Skyline\Current_Test\General_Information\Scripts_running_time.xml");
                    }
                }
                catch (Exception ex)
                {
                    LoggingErrors.AddExceptionToList(String.Format("{0}: {1}", DateTime.Now, " problem with making or loading the XML file with the time a script(s) runned."));
                }
            }

            cyclusParallelTimeArg.changeStartCyclus(RealRunningTime[0]);
            cyclusParallelTimeArg.changeStopCyclus(RealRunningTime[1]);
            ListsScriptsArg.AddToLists(fails, warnings, successes, name);

            ListsScriptsArg.AddToDictionary(name, RealRunningTime);
            ListsScriptsArg.AddToOverview(fails, warnings, successes, name, (RealRunningTime[1] - RealRunningTime[0]).ToString());

            ManualResetEventSlim wait = new ManualResetEventSlim(false);
            wait.Wait(1000);
        }

        public void Execute(ListsScripts ListsScriptsArg, string upgradeVersion, string textCombocopyArg)
        {
            AllInformationAndAlarmEvents(DestinationPath, ListsScriptsArg, true, false);
            AllInformationAndAlarmEvents(DestinationPath, ListsScriptsArg, false, true);
            generateResultPath(DestinationPath, ListsScriptsArg, upgradeVersion, textCombocopyArg);
        }

        private void AllInformationAndAlarmEvents(string destinationPath, ListsScripts ListsScriptsArg, bool EventsArg, bool AlarmsArg)
        {
            List<AlarmEventMessage> all = null;

            if (EventsArg == true)
            {
                all = checkInfoRecords("", "", ListsScriptsArg);

                destinationPath += "\\InformationEvents\\";
                CreateDirectory(destinationPath);

            }
            else if (AlarmsArg == true)
            {
                all = checkAlarmRecords("", "", ListsScriptsArg);

                destinationPath += "\\AlarmEvents\\";
                CreateDirectory(destinationPath);
            }

            Excel.Application xlApp = new Excel.Application();

            if (xlApp == null)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1}", DateTime.Now, "Excel is possible not properly installed!"));

                return;
            }

            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = Missing.Value;

            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = xlWorkBook.ActiveSheet;

            xlWorkSheet.Cells[1, 1] = "AlarmID:";
            xlWorkSheet.Cells[1, 2] = "BaseAlarms:";
            xlWorkSheet.Cells[1, 3] = "Comments:";
            xlWorkSheet.Cells[1, 4] = "DataMinerID:";
            xlWorkSheet.Cells[1, 5] = "DisplayValue";
            xlWorkSheet.Cells[1, 6] = "ElementID:";
            xlWorkSheet.Cells[1, 7] = "ElementName:";
            xlWorkSheet.Cells[1, 8] = "ElementType:";
            xlWorkSheet.Cells[1, 9] = "IsDeleted:";
            xlWorkSheet.Cells[1, 10] = "IsNew:";
            xlWorkSheet.Cells[1, 11] = "IsLastHistory:";
            xlWorkSheet.Cells[1, 12] = "Links:";
            xlWorkSheet.Cells[1, 13] = "Owner:";
            xlWorkSheet.Cells[1, 14] = "ParameterID:";
            xlWorkSheet.Cells[1, 15] = "ParameterName:";
            xlWorkSheet.Cells[1, 16] = "ParameterRCALevel:";
            xlWorkSheet.Cells[1, 17] = "ParentServices:";
            xlWorkSheet.Cells[1, 18] = "PrevAlarmID:";
            xlWorkSheet.Cells[1, 19] = "Properties:";
            xlWorkSheet.Cells[1, 20] = "RCALevel:";
            xlWorkSheet.Cells[1, 21] = "RootAlarmID:";
            xlWorkSheet.Cells[1, 22] = "RootTime:";
            xlWorkSheet.Cells[1, 23] = "RootCreationTime:";
            xlWorkSheet.Cells[1, 24] = "ServiceRCALevel:";
            xlWorkSheet.Cells[1, 25] = "Services:";
            xlWorkSheet.Cells[1, 26] = "SeverityID:";
            xlWorkSheet.Cells[1, 27] = "SeverityRangeID:";
            xlWorkSheet.Cells[1, 28] = "SourceID:";
            xlWorkSheet.Cells[1, 29] = "StatusID:";
            xlWorkSheet.Cells[1, 30] = "TableIndex:";
            xlWorkSheet.Cells[1, 31] = "TimeOfArrival:";
            xlWorkSheet.Cells[1, 32] = "TypeID:";
            xlWorkSheet.Cells[1, 33] = "UserStatusID:";
            xlWorkSheet.Cells[1, 34] = "Value:";
            xlWorkSheet.Cells[1, 35] = "_tableIdxPK:";
            xlWorkSheet.Cells[1, 36] = "Category";
            xlWorkSheet.Cells[1, 37] = "Description:";
            xlWorkSheet.Cells[1, 38] = "CorrectiveAction:";
            xlWorkSheet.Cells[1, 39] = "InterpretTableIdx:";
            xlWorkSheet.Cells[1, 40] = "Interfaces:";
            xlWorkSheet.Cells[1, 41] = "ParentInterfaces:";
            xlWorkSheet.Cells[1, 42] = "KeyPoint:";
            xlWorkSheet.Cells[1, 43] = "OfflineImpact:";
            xlWorkSheet.Cells[1, 44] = "ComponentInfo:";
            xlWorkSheet.Cells[1, 45] = "CorrelationReferences:";
            xlWorkSheet.Cells[1, 46] = "ViewImpactInfo:";

            xlWorkSheet.Cells[1, 46].EntireRow.Font.Bold = true;

            var columnHeadingsRange = xlWorkSheet.Range[
            xlWorkSheet.Cells[1, 1],
            xlWorkSheet.Cells[1, 46]];
            columnHeadingsRange.Interior.Color = Excel.XlRgbColor.rgbGreen;
            columnHeadingsRange.Font.Color = Excel.XlRgbColor.rgbWhite;

            for (int index = 2; index < (all.Count + 2); index++)
            {
                xlWorkSheet.Cells[index, 1] = all[index - 2].AlarmID.ToString();

                if (all[index - 2].BaseAlarms != null)
                    xlWorkSheet.Cells[index, 2] = all[index - 2].BaseAlarms.ToString();
                else
                    xlWorkSheet.Cells[index, 2] = "";

                if (all[index - 2].Comments != null)
                    xlWorkSheet.Cells[index, 3] = all[index - 2].Comments.ToString();
                else
                    xlWorkSheet.Cells[index, 3] = "";

                xlWorkSheet.Cells[index, 4] = all[index - 2].DataMinerID.ToString();

                if (all[index - 2].DisplayValue != null)
                    xlWorkSheet.Cells[index, 5] = all[index - 2].DisplayValue.ToString();
                else
                    xlWorkSheet.Cells[index, 5] = "";

                xlWorkSheet.Cells[index, 6] = all[index - 2].ElementID.ToString();

                if (all[index - 2].ElementName != null)
                    xlWorkSheet.Cells[index, 7] = all[index - 2].ElementName.ToString();
                else
                    xlWorkSheet.Cells[index, 7] = "";

                if (all[index - 2].ElementType != null)
                    xlWorkSheet.Cells[index, 8] = all[index - 2].ElementType.ToString();
                else
                    xlWorkSheet.Cells[index, 8] = "";

                xlWorkSheet.Cells[index, 9] = all[index - 2].IsDeleted.ToString();
                xlWorkSheet.Cells[index, 10] = all[index - 2].IsNew.ToString();
                xlWorkSheet.Cells[index, 11] = all[index - 2].IsLastHistory.ToString();

                if (all[index - 2].Links != null)
                    xlWorkSheet.Cells[index, 12] = all[index - 2].Links.ToString();
                else
                    xlWorkSheet.Cells[index, 12] = "";

                if (all[index - 2].Owner != null)
                    xlWorkSheet.Cells[index, 13] = all[index - 2].Owner.ToString();
                else
                    xlWorkSheet.Cells[index, 13] = "";

                xlWorkSheet.Cells[index, 14] = all[index - 2].ParameterID.ToString();

                if (all[index - 2].ParameterName != null)
                    xlWorkSheet.Cells[index, 15] = all[index - 2].ParameterName.ToString();
                else
                    xlWorkSheet.Cells[index, 15] = "";

                xlWorkSheet.Cells[index, 16] = all[index - 2].ParameterRCALevel.ToString();

                if (all[index - 2].ParentServices != null)
                    xlWorkSheet.Cells[index, 17] = all[index - 2].ParentServices.ToString();
                else
                    xlWorkSheet.Cells[index, 17] = "";

                xlWorkSheet.Cells[index, 18] = all[index - 2].PrevAlarmID.ToString();

                if (all[index - 2].Properties != null)
                    xlWorkSheet.Cells[index, 19] = all[index - 2].Properties.ToString();
                else
                    xlWorkSheet.Cells[index, 19] = "";

                xlWorkSheet.Cells[index, 20] = all[index - 2].RCALevel.ToString();
                xlWorkSheet.Cells[index, 21] = all[index - 2].RootAlarmID.ToString();

                if (all[index - 2].RootTime != null)
                    xlWorkSheet.Cells[index, 22] = all[index - 2].RootTime.ToString();
                else
                    xlWorkSheet.Cells[index, 22] = "";

                if (all[index - 2].RootCreationTime != null)
                    xlWorkSheet.Cells[index, 23] = all[index - 2].RootCreationTime.ToString();
                else
                    xlWorkSheet.Cells[index, 23] = "";

                xlWorkSheet.Cells[index, 24] = all[index - 2].ServiceRCALevel.ToString();

                if (all[index - 2].Services != null)
                    xlWorkSheet.Cells[index, 25] = all[index - 2].Services.ToString();
                else
                    xlWorkSheet.Cells[index, 25] = "";

                xlWorkSheet.Cells[index, 26] = all[index - 2].SeverityID.ToString();
                xlWorkSheet.Cells[index, 27] = all[index - 2].SeverityRangeID.ToString();
                xlWorkSheet.Cells[index, 28] = all[index - 2].SourceID.ToString();
                xlWorkSheet.Cells[index, 29] = all[index - 2].StatusID.ToString();

                if (all[index - 2].TableIndex != null)
                    xlWorkSheet.Cells[index, 30] = all[index - 2].TableIndex.ToString();
                else
                    xlWorkSheet.Cells[index, 30] = "";

                if (all[index - 2].TimeOfArrival != null)
                    xlWorkSheet.Cells[index, 31] = all[index - 2].TimeOfArrival.ToString();
                else
                    xlWorkSheet.Cells[index, 31] = "";

                xlWorkSheet.Cells[index, 32] = all[index - 2].TypeID.ToString();

                if (all[index - 2].UserStatus != null)
                    xlWorkSheet.Cells[index, 33] = all[index - 2].UserStatus.ToString();
                else
                    xlWorkSheet.Cells[index, 33] = "";

                if (all[index - 2].Value != null)
                    xlWorkSheet.Cells[index, 34] = all[index - 2].Value.ToString();
                else
                    xlWorkSheet.Cells[index, 34] = "";

                if (all[index - 2].TableIdxPK != null)
                    xlWorkSheet.Cells[index, 35] = all[index - 2].TableIdxPK.ToString();
                else
                    xlWorkSheet.Cells[index, 35] = "";

                if (all[index - 2].Category != null)
                    xlWorkSheet.Cells[index, 36] = all[index - 2].Category.ToString();
                else
                    xlWorkSheet.Cells[index, 36] = "";

                if (all[index - 2].Description != null)
                    xlWorkSheet.Cells[index, 37] = all[index - 2].Description.ToString();
                else
                    xlWorkSheet.Cells[index, 37] = "";

                if (all[index - 2].CorrectiveAction != null)
                    xlWorkSheet.Cells[index, 38] = all[index - 2].CorrectiveAction.ToString();
                else
                    xlWorkSheet.Cells[index, 38] = "";

                xlWorkSheet.Cells[index, 39] = all[index - 2].InterpretTableIdx.ToString();

                if (all[index - 2].Interfaces != null)
                    xlWorkSheet.Cells[index, 40] = all[index - 2].Interfaces.ToString();
                else
                    xlWorkSheet.Cells[index, 40] = "";

                if (all[index - 2].ParentInterfaces != null)
                    xlWorkSheet.Cells[index, 41] = all[index - 2].ParentInterfaces.ToString();
                else
                    xlWorkSheet.Cells[index, 41] = "";

                if (all[index - 2].KeyPoint != null)
                    xlWorkSheet.Cells[index, 42] = all[index - 2].KeyPoint.ToString();
                else
                    xlWorkSheet.Cells[index, 42] = "";

                xlWorkSheet.Cells[index, 43] = all[index - 2].OfflineImpact.ToString();

                if (all[index - 2].ComponentInfo != null)
                    xlWorkSheet.Cells[index, 44] = all[index - 2].ComponentInfo.ToString();
                else
                    xlWorkSheet.Cells[index, 44] = "";

                if (all[index - 2].CorrelationReferences != null)
                    xlWorkSheet.Cells[index, 45] = all[index - 2].CorrelationReferences.ToString();
                else
                    xlWorkSheet.Cells[index, 45] = "";

                if (all[index - 2].ViewImpactInfo != null)
                    xlWorkSheet.Cells[index, 46] = all[index - 2].ViewImpactInfo.ToString();
                else
                    xlWorkSheet.Cells[index, 46] = "";
            }

            xlWorkSheet.Columns.AutoFit();

            if (EventsArg == true)
            {
                xlWorkBook.SaveAs(destinationPath + "Events.xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            }
            else if (AlarmsArg == true)
            {
                xlWorkBook.SaveAs(destinationPath + "Alarms.xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            }

            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();
        }

        //generates the directory to put the HTML5 file with the Script results
        public void generateResultPath(string destinationPath, ListsScripts ListsScriptsArg, string upgradeVersion, string textCombocopyArg)
        {
            string strFailsTable = "";
            foreach (var fail in ListsScriptsArg.totalFails)
            {
                strFailsTable += string.Format("<tr><td>{0}</td><td>{1}</td></tr>", fail.TimeOfArrival, WebUtility.HtmlEncode(fail.Value));
            }

            string _DBType = "";
            if (textCombocopyArg == "Testing_Cassandra")
                _DBType = "Cassandra";
            else if (textCombocopyArg == "Testing_MySQL")
                _DBType = "MySQL";

            string strSubject = string.Format("RT_FAILS {0}({1}): -{2}/~{3}/+{4} {5}", 
                _strServerName, _DBType, ListsScriptsArg.totalFails.Count, ListsScriptsArg.totalWarnings.Count, ListsScriptsArg.totalSuccesses.Count, upgradeVersion);
            string strMessage = GetScoreBoard(ListsScriptsArg.totalSuccesses.Count, ListsScriptsArg.totalFails.Count, ListsScriptsArg.totalWarnings.Count);
            strMessage += ConvertMessagesToHTMLTable(ListsScriptsArg.totalFails, "RT_FAIL");
            strMessage += ConvertMessagesToHTMLTable(ListsScriptsArg.totalWarnings, "RT_WARNING");
            strMessage += ConvertMessagesToHTMLTable(ListsScriptsArg.totalSuccesses, "RT_SUCCES");

            CreateDirectory(destinationPath + "\\ScriptsSuceededAndFailed\\");

            destinationPath += "\\ScriptsSuceededAndFailed\\Scripts.HTML";

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

        public string GetScoreBoard(int SuccesCount, int FailCount, int WarningCount)
        {
            return string.Format("<h3>RT_FAIL: {0}</h3>"
                               + "<h3>RT_WARNING: {1}</h3>"
                               + "<h3>RT_SUCCES: {2}</h3>", FailCount, WarningCount, SuccesCount);
        }

        public List<AlarmEventMessage> checkInfoRecords(string arrFilter1, string arrFilter2, ListsScripts ListsScriptsArg)
        {
            List<AlarmEventMessage> list = new List<AlarmEventMessage>();

            Regex regex = new Regex("[^\n]+"); ;

            if (arrFilter1 == "RT_SUCCES" || arrFilter1 == "RT_FAIL" || arrFilter1 == "RT_WARNING")
                regex = new Regex("^" + arrFilter1.ToLower() + "\\s{1,2}" + arrFilter2.ToLower() + "(\\W|$)");
            else if (arrFilter1 == "Script Failure")
                regex = new Regex("^" + arrFilter1.ToLower() + "\\s(\\W|$)" + arrFilter2.ToLower() + "(\\W|$){2}");
            //else if (arrFilter1 == "" && arrFilter2 == "")
            //    new Regex("[^\n]+");

            foreach (AlarmEventMessage message in ListsScriptsArg.TotalAlarmsAndEvents)
            {
                if (message.SeverityID == 13)
                {
                    string tekst = message.DisplayValue.ToLower();
                    if (regex.IsMatch(message.DisplayValue.ToLower()))
                            list.Add(message);
                }
            }

            return list;
        }

        public List<AlarmEventMessage> checkAlarmRecords(string arrFilter1, string arrFilter2, ListsScripts ListsScriptsArg)
        {
            List<AlarmEventMessage> list = new List<AlarmEventMessage>();

            foreach (AlarmEventMessage message in ListsScriptsArg.TotalAlarmsAndEvents)
            {
                if (message.SeverityID != 13)
                {
                    if (message.DisplayValue.Contains(arrFilter1))
                        if (message.DisplayValue.Contains(arrFilter2))
                            list.Add(message);
                }
            }

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
