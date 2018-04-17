using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevRegTest.domainVMApp
{
    public class MacAddress
    {
        //String for the address
        public String Address { get; set; }

        //Int for your own DMA ID
        public int DmaID { get; private set; }

        //String for the computer name
        public String ComputerName { get; private set; }

        #region Constructor
        public MacAddress(string address, string computerName, int dmaId, int pool)
        {
            this.Address = address;
            this.ComputerName = computerName;
            this.DmaID = dmaId;
        }
        #endregion Constructor
    }
}
