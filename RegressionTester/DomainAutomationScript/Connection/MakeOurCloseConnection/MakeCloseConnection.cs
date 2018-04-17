using DevRegTest.domainAutomationScript.Element.AbortAutomationScript;
using Skyline.DataMiner.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DevRegTest.domainAutomationScript.Connection.MakeOurCloseConnection
{
    public class MakeCloseConnection
    {
        public RemotingConnection connection { get; set; }

        public MakeCloseConnection()
        { }

        public bool MakeConnection(bool cmdOurUiArg)
        {
            bool error = true;

            RemotingConnection.RegisterChannel();

            try
            {
                // Creates a remote Connection and Authenticates itself.
                connection = new RemotingConnection("127.0.0.1")
                {
                    ClientApplicationName = "DevRegTest"
                };

                connection.Authenticate("Administrator", "Skyline321");
                connection.Subscribe();

                error = true;
            }
            catch (Exception ex)
            {
                if (cmdOurUiArg == true)
                {
                    MessageBox.Show(ex.Message,
                                    "Connection problem.",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);                  
                }
                else
                {
                    App.CommandLog(String.Format("{0}: {1} ({2})", DateTime.Now, "Connection problem", ex.Message));
                }

                error = false;
            }

            return error;
        }

        public void MakeConnectionTryCatch()
        {
            bool started = false;

            while (started == false)
            {
                try
                {
                    connection = new RemotingConnection("127.0.0.1")
                    {
                        ClientApplicationName = "DevRegTest"
                    };

                    connection.Authenticate("Administrator", "Skyline321");
                    connection.Subscribe();

                    started = true;
                }
                catch (Exception ex)
                {
                    ManualResetEventSlim wait = new ManualResetEventSlim(false);
                    wait.Wait(500);
                }
            }
        }

        public RemotingConnection GetConnection()
        {
            return connection;
        }

        public void CloseConnection()
        {
            try
            {
                connection.Close();
            }
            catch (Exception ex)
            {
                LoggingErrors.AddExceptionToList(String.Format("{0}: {1} ({2})", DateTime.Now, "Connection problem", "Could not close the connection with the server"));
            }
        }
    }
}
