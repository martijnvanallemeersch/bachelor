using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevRegTest.domainVMApp;

namespace DevRegTest
{
    static class LoggingErrors
    {
        
        static List<String> LoggingErrorsList = new List<string>();

        public static void AddExceptionToList(string Exception)
        {
            LoggingErrorsList.Add(Exception);
            
            if (File.Exists(@"C:\RegressionTester_Skyline\Current_Test\LoggingTool\Log.txt"))
            {
                try
                {
                    using (StreamWriter sw = File.AppendText(@"C:\RegressionTester_Skyline\Current_Test\LoggingTool\Log.txt"))
                    {
                        sw.WriteLine(Exception);
                    }
                }
                catch(Exception ex)
                {

                }
                
            }
        }
    }
}
