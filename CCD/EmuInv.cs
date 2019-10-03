using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCD
{
    public class EmuInv     
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

        public EmuInv()
        {
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1000;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Power = random.Next(0, 2000);

        }
    }
}
