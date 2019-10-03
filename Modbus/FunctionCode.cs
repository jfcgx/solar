using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus
{
    public enum FunctionCode
    {
        ReadCoil = 0x01,
        ReadDiscreteInput = 0x02,
        ReadHoldingRegisters = 0x03,
        ReadInputRegister = 0x04,
        WriteSingleCoil = 0x05,
        WriteSingleHoldingRegister = 0x06,
        WriteMultipleCoils = 15,
        WriteMultipleHoldingRegisters = 16
    }
}
