using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevRegTest.domainVMApp
{
    public class VersionDMA
    {
        //Where is original script located
        public string PathVersion { get; set; }

        //Where has to script to be placed
        public string EndPath { get; set; }

        //The name of the script
        public string Name { get; set; }

        //name of executable that needs to run
        public string Execute { get; set; }

        //Checks if dir needs to be copied or just a script file
        public bool Directory { get; set; }

        //Boolean to check if its an installer
        public bool DMAInstaller { get; set; }

        // Check whatever its an upgrade so a specific batch file kan be made
        public bool DMAUpgrade { get; set; }

        //Checks whether the DMA needs to be restarted or not
        public bool DMARestart { get; set; }

        public bool BatchFile { get; set; }

        #region Constructors
        public VersionDMA(string name)
        {
            this.Name = name;
        }

        public VersionDMA(string name, string pathVersion, string endPath, string execute, bool directory, bool dma)
        {
            this.PathVersion = pathVersion;
            this.EndPath = endPath;
            this.Name = name;
            this.Execute = execute;
            this.Directory = directory;
            this.DMAInstaller = dma;
            this.DMARestart = false;
        }
        #endregion

        //Here we are generating a version
        public String GenerateVersion()
        {
            string pattern = "([A-Za-z\\ ]*)([0-9\\.-]*)([\\ A-Za-z()]*)";
            string resultText = Regex.Replace(Name, pattern, "$2");
            return resultText;
        }
    }
}
