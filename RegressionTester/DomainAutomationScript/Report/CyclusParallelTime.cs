using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevRegTest.domainAutomationScript.Report
{
    public class CyclusParallelTime
    {
        public DateTime StartCyclus { get; set; }

        public DateTime StopCyclus { get; set; }

        public CyclusParallelTime()
        {
            StartCyclus = DateTime.Now.AddHours(1);
            StopCyclus = DateTime.Now.AddHours(-1);
        }

        public void changeStartCyclus(DateTime time)
        {
            if(time < StartCyclus)
            {
                StartCyclus = time;
            }
        }

        public void changeStopCyclus(DateTime time)
        {
            if (time > StopCyclus)
            {
                StopCyclus = time;
            }
        }
    }
}
