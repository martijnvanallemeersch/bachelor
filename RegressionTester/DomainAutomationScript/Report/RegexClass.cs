using DevRegTest.domainVMApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DevRegTest.DomainAutomationScript.Report
{
    public class RegexClass
    {
        public CheckBox Check { get; set; }

        public string Regex { get; set; }

        public Dictionary<string, List<NameScriptsDirectories>> Collection { get; set; }

        public string Files { get; set; }

        public string Output { get; set; }
    }
}
