

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModBus
{

    public delegate void DataReceived(object sender, SerialPortEventArgs arg);

    public class SerialPortInterface
    {
        private const string Bin = "00000000000000000000000000000000000000";

        private bool _emular = false;
        private string _emularMsg = string.Empty;

        public event DataReceived DataReceived;
        public bool Active { get; set; }
        public string TerminoTrama { get; set; }
        public char SeparadorTrama { get; set; }
        public int TamanoTrama { get; set; }


        /// <summary>
        ///     End of transmition byte in this case EOT (ASCII 4).
        /// </summary>
        public char Initial { get; set; }

        /// <summary>
        /// Serial port class
        /// </summary>
        private SerialPort _serialPort = new SerialPort();

        /// <summary>
        /// BaudRate set to default for Serial Port Class
        /// </summary>
        private int _baudRate = 9600;

        /// <summary>
        /// DataBits set to default for Serial Port Class
        /// </summary>
        private int _dataBits = 8;

        public SerialPort SerialPort
        {
            get { return _serialPort; }
        }
        /// <summary>
        /// Handshake set to default for Serial Port Class
        /// </summary>
        private Handshake _handshake = Handshake.None;

        /// <summary>
        /// Parity set to default for Serial Port Class
        /// </summary>
        private Parity _parity = Parity.None;
        /// <summary>
        /// Communication Port name, not default in SerialPort. Defaulted to COM1
        /// </summary>
        private string _portName = "COM1";

        /// <summary>
        /// StopBits set to default for Serial Port Class
        /// </summary>
        private StopBits _stopBits = StopBits.One;

        /// <summary>
        /// Holds data received until we get a terminator.
        /// </summary>
        //private readonly StringBuilder _tString = new StringBuilder();
        private List<byte> readByte = new List<byte>();
        //private Protocolo _mppt;

        private static bool _isBusy;

        /// <summary>
        /// Gets or sets BaudRate (Default: 9600)
        /// </summary>
        public int BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        /// <summary>
        /// Gets or sets DataBits (Default: 8)
        /// </summary>
        public int DataBits
        {
            get { return _dataBits; }
            set { _dataBits = value; }
        }

        /// <summary>
        /// Gets or sets Handshake (Default: None)
        /// </summary>
        public Handshake Handshake
        {
            get { return _handshake; }
            set { _handshake = value; }
        }

        /// <summary>
        /// Gets or sets Parity (Default: None)
        /// </summary>
        public Parity Parity
        {
            get { return _parity; }
            set { _parity = value; }
        }

        /// <summary>
        /// Gets or sets PortName (Default: COM1)
        /// </summary>
        public string PortName
        {
            get { return _portName; }
            set { _portName = value; }
        }

        /// <summary>
        /// Gets or sets StopBits (Default: One}
        /// </summary>
        public StopBits StopBits
        {
            get { return _stopBits; }
            set { _stopBits = value; }
        }

       

        //public Protocolo Mppt
        //{
        //    get
        //    {
        //        return _mppt;
        //    }

        //    set
        //    {
        //        _mppt = value;
        //    }
        //}



        /// <summary>
        /// Sets the current settings for the Comport and tries to open it.
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public SerialPortInterface(string serialPortName, int baudRate, int databits, System.IO.Ports.Parity parity, System.IO.Ports.Handshake handshake, System.IO.Ports.StopBits stopbits)
        {
            //_serialPort = new SerialPort();
            _serialPort.PortName = serialPortName;
            Console.WriteLine(serialPortName);

            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = databits;
            _serialPort.Handshake = handshake;
            _serialPort.Parity = parity;
            _serialPort.StopBits = stopbits;
            //_serialPort.ReadTimeout = Settings.Default.ReadTimeout;
            //_serialPort.WriteTimeout = Settings.Default.WriteTimeout;
        }

        public bool Open()
        {
            try
            {
                if (_serialPort == null) return false;

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        public bool Send(byte[] data)
        {
            try
            {
                _serialPort.Write(data, 0, data.Length);
            }
            catch { return false; }
            return true;
        }
        public bool Send(string data)
        {
            try
            {
                byte[] newMsg = Helps.HexToByte(data);
                _serialPort.Write(newMsg, 0, newMsg.Length);
            }
            catch { return false; }
            return true;
        }

        public bool SendString(string str)
        {
            try
            {
                //var data = Encoding.ASCII.GetBytes(str);
                //_serialPort.Write(data, 0, data.Length);

                _serialPort.Write(str);

                //_serialPort.WriteLine(str);
            }
            catch { return false; }
            return true;

        }

        /// <summary>
        /// Handles DataReceived Event from SerialPort
        /// </summary>
        /// <param name="sender">Serial Port</param>
        /// <param name="e">Event arguments</param>
        /// 
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _isBusy = true;
            try
            {
                if (DataReceived != null)
                {
                    Thread.Sleep(300);
                    int bIN = _serialPort.BytesToRead;
                    byte[] datagram = new byte[bIN];
                    _serialPort.Read(datagram, 0, bIN);
                    DataReceived(this, new SerialPortEventArgs(datagram));
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(this + " " + ex.Message);
            }
            _isBusy = false;
        }

        public string SetData(string strData)
        {
            string strBinary;
            try
            {
                var data = strData.Split(SeparadorTrama);
                //if (data.Count() == 5)
                if (data.Count() <= 5)
                {
                    if (Helps.ValidDataIntegrity(data))
                    {
                        var temp = data.Aggregate(string.Empty, (current, s) => current + Helps.Dec2Bin(s));
                        //temp = temp.Substring(0, TamanoTrama);
                        temp = data.Length == 5 ? temp.Substring(0, TamanoTrama) : temp.PadRight(39, '0');
                        strBinary = Helps.ValidateBinString(temp) ? temp : Bin;
                    }
                    else { strBinary = Bin; }
                }
                else { strBinary = Bin; }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                strBinary = Bin;
            }
            return strBinary;
        }

        public void Close()
        {
            _serialPort.DataReceived -= SerialPort_DataReceived;
            _serialPort.Close();
        }

        public void Emular(string data)
        {
            _emular = true;
            _emularMsg = data;
            SerialPort_DataReceived(_serialPort, null);
            _emular = false;
        }
    }
}
