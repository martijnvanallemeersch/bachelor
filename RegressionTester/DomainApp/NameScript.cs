using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevRegTest.domainVMApp
{
    public class NameScriptOrDelt
    {
        //The string for the name of the Script
        public string Name { get; set; }

        #region Constructor
        public NameScriptOrDelt(string name)
        {
            Name = name;
        }
        #endregion Constructor
    }
}
