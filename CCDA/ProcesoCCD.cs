using CCD;
using System;
using System.Collections.Generic;
using System.Text;

namespace CCDA
{

    public class ProcesoCCD
    {
        public static ControladorConsumo process;

        public  ProcesoCCD()
        {
            process = ControladorConsumo.GetInstance();
            process.Inicia();
        }
    }
}
