using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCDStandar
{
    public class EmuMeter
    {
        private System.Timers.Timer timer = new System.Timers.Timer();
        private Random random = new Random();
        private int power;

        public int Power
        {
            get
            {
                return power;
            }

            set
            {
                power = value;
            }
        }

        public EmuMeter()
        {
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1000;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Power = random.Next(-2000, 2000);
        }
    }
}
