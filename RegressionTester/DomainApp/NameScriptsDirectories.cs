using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevRegTest.domainVMApp
{
    public class NameScriptsDirectories
    {
        //The string for the name of the Script
        public string Name { get; set; }

        public string Path { get; set; }

        #region constuctor
        public NameScriptsDirectories(string name, string path)
        {
            Name = name;
            Path = path;
        }
        #endregion constuctor
    }
}
