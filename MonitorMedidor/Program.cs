using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorMedidor
{
    class Program
    {
        static void Main(string[] args)
        {
            //49 DE 09 1F
            //01 03 04 00 00 0B FC FD 42

            //byte[] bb = new byte[9];
            //bb[0] = 0x01;
            //bb[1] = 0x03;
            //bb[2] = 0x04;
            //bb[3] = 0x00;
            //bb[4] = 0x00;
            //bb[5] = 0x0B;
            //bb[6] = 0xFC;
            //bb[7] = 0xFD;
            //bb[8] = 0x42;

            //CCD.DDS238.TestConver(bb);

            CCD.DDS238 d = new CCD.DDS238(true, null, "192.168.0.221", 8899, true,false);
           

            Console.ReadKey();
        }
    }
}
