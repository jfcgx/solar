using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus
{

    //MODBUS RTU Memory Map
    //Modbus RTU
    //Data Type         Common name                 Starting address
    //Modbus Coils      Bits, binary values, flags  00001
    //Digital Inputs    Binary inputs               10001
    //Analog Inputs     Binary inputs               30001
    //Modbus Registers  Analog values, variables    40001

    // http://modbus.rapidscada.net/
    public class SendFrameFormat
    {
        byte _id;
        FunctionCode _functionCode;
        byte[] _startingAddress = new byte[2];
        byte[] _quantity = new byte[2];
        byte[] _crc = new byte[2];
        public byte Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public FunctionCode FunctionCode
        {
            get
            {
                return _functionCode;
            }

            set
            {
                _functionCode = value;
            }
        }

        public byte[] Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                _quantity = value;
            }

        }

        public byte[] Crc
        {
            get
            {
                return _crc;
            }

            set
            {
                _crc = value;
            }
        }

        public byte[] StartingAddress
        {
            get
            {
                return _startingAddress;
            }
            set
            {
                _startingAddress = value;
            }
        }

        public SendFrameFormat(byte id, FunctionCode functionCode)
        {
            this.Id = id;
            this.FunctionCode = functionCode; //0x03
        }
        public SendFrameFormat(byte id, FunctionCode functionCode, byte[] startingAddress, byte[] dateNumbre)
        {
            this.Id = id;
            this.FunctionCode = functionCode; //0x03
            this.StartingAddress = startingAddress;
            this.Quantity = dateNumbre;
        }

        public byte[] ToBytes()
        {
            byte[] salida = new byte[6];

            salida[0] = Id;                             //Slave address
            salida[1] = (byte)FunctionCode;             //Function code
            salida[2] = StartingAddress[0];             //Starting address Byte 1
            salida[3] = StartingAddress[1];             //Starting address Byte 2
            salida[4] = Quantity[0];                    //Quantity Byte 1
            salida[5] = Quantity[1];                    //Quantity Byte 2

            Crc = BitConverter.GetBytes(Crc16.ComputeCrc(salida));

            Array.Resize(ref salida, 8);

            salida[6] = Crc[0];
            salida[7] = Crc[1];

            return salida;
        }
    }
}
