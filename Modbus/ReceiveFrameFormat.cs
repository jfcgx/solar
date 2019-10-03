using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus
{
    public class ReceiveFrameFormat
    {

        byte _id;
        FunctionCode _functionCode;
        byte _dataLength;
        byte[] _dataArea;
        byte[] _crc = new byte[2];

        bool _cRCValido;

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

        public byte DataLength
        {
            get
            {
                return _dataLength;
            }

            set
            {
                _dataLength = value;
            }
        }

        public byte[] DataArea
        {
            get
            {
                return _dataArea;
            }

            set
            {
                _dataArea = value;
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

        public bool CRCValido { get => _cRCValido; }

        public ReceiveFrameFormat(byte[] datagram)
        {
            if (datagram != null)
            {
                if (datagram.Length > 0)
                {
                    try
                    {
                        this.Id = datagram[0];
                        this.FunctionCode = (FunctionCode)datagram[1];
                        this.DataLength = datagram[2];
                        this.DataArea = datagram.ToList().GetRange(3, DataLength).ToArray();
                        this.Crc = datagram.ToList().GetRange(3 + DataLength, 2).ToArray();

                        _cRCValido = ValidaCRC(datagram);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }
        public ReceiveFrameFormat(byte _idMeter, byte _functionCode, byte _dataLength, byte[] _dataArea, byte[] _crc)
        {
            this.Id = _idMeter;
            this.FunctionCode = (FunctionCode)_functionCode;
            this.DataLength = _dataLength;
            this.DataArea = _dataArea;
            this.Crc = _crc;
        }
        public byte[] ToBytes()
        {
            byte[] salida = new byte[3 + DataLength];

            try
            {
                salida[0] = Id;
                salida[1] = (byte)FunctionCode;
                salida[2] = DataLength;

                for (int i = 0; i < DataLength; i++)
                {
                    salida[3 + i] = DataArea[i];
                }

                Crc = BitConverter.GetBytes(Crc16.ComputeCrc(salida));

                Array.Resize(ref salida, salida.Length + 2);

                salida[salida.Length - 2] = Crc[0];
                salida[salida.Length - 1] = Crc[1];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return salida;
        }
        public bool ValidaCRC(byte[] datagrama)
        {
            var d = datagrama.ToList().GetRange(0,datagrama.Length - 2).ToArray();
            var crc = BitConverter.GetBytes(Crc16.ComputeCrc(d));

            return (crc[0] == Crc[0]) && (crc[1] == Crc[1]);
        }
    }
}
