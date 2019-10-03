using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus
{
    public class SerialPortEventArgs : EventArgs
    {
        public SerialPort Serial { get; set; }
        public byte[] ReceivedData { get; private set; }

        public SerialPortEventArgs(SerialPort serial)
        {
            Serial = serial;
        }
        public SerialPortEventArgs(byte[] data)
        {
            ReceivedData = data;
        }
    }
}
