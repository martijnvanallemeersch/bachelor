using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DevRegTest.domainVMApp
{
    public class Protocol
    {
        #region properties
        //The protocol name
        public String NameProtocol { get; set; }

        //The element name
        public String NameElement { get; set; }

        //The string for the version
        public String Version { get; set; }
        #endregion properties

        #region constructors
        public Protocol(String nameProtocol, String nameElement, String nameTrend, String version)
        {
            NameProtocol = nameProtocol;
            NameElement = nameElement;
          
            Version = version;
        }
        #endregion constructors
    }
}
