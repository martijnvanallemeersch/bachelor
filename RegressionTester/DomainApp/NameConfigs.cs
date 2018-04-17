using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevRegTest.domainVMApp
{
    public class NameConfigs
    {
        //The string for the name of the Config
        public string Name { get; set; }

        #region Constructor
        public NameConfigs(string name)
        {
            Name = name;
        }
        #endregion Constructor 
    }
}
