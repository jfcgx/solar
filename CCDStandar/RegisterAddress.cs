using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCDStandar
{
    public enum DDSRegisterAddress
    {
        TotalkWh = 0x01,  //0x0001
        ExportkWh = 0x08, //0x0009
        ImportkWh = 0x0A, //0x000B
        Voltage = 0x0C,   //
        Current = 0x0D,   //
        ActivePower = 0x0E,
        PowerFactor = 0x10,
        Frequency = 0x11,
        ID_BaudRate = 0x15
    }
}
