using CCD.Properties;
using ModBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCD
{
    public class DDS238
    {
        //http://modbus.rapidscada.net/

        SerialPortInterface _serialPortInterface;//= new SerialPortInterface(Settings.Default.MeterPortName, Settings.Default.MeterBaudRate, Settings.Default.MeterDataBits, Settings.Default.MeterParity,Settings.Default.MeterHandShake,Settings.Default.MeterStopBits);
        SendFrameFormat _dsf;
        ReceiveFrameFormat _drf;
        DDSRegisterAddress _registerAddress;

        double _totalkWh;
        double _exportkWh;
        double _importkWh;
        double _voltage;
        double _current;
        int _activePower;
        double _powerFactor;
        double _frequency;
        bool _isBusy;
        bool _muestraDatosConsola;
        bool _esSocket;
        bool _escribeArchivoLog;
        int _cuentaTimeOut;
        int _maxTimeOut = 10;

        SocketServer _serverLog = new SocketServer();
        bool _status;
        SocketClient _clientRs;
        System.IO.StreamWriter _t;
        System.IO.StreamWriter _f;
        System.Timers.Timer _timer = new System.Timers.Timer();
        int _countTime;
        int _timeOut = 1000; // 1 segundo
        bool _dataRecived;

        public double TotalkWh
        {
            get
            {
                return _totalkWh;
            }

            set
            {
                _totalkWh = value;
            }
        }

        public double ExportkWh
        {
            get
            {
                return _exportkWh;
            }

            set
            {
                _exportkWh = value;
            }
        }

        public double ImportkWh
        {
            get
            {
                return _importkWh;
            }

            set
            {
                _importkWh = value;
            }
        }

        public double Voltage
        {
            get
            {
                return _voltage;
            }

            set
            {
                _voltage = value;
            }
        }

        public double Current
        {
            get
            {
                return _current;
            }

            set
            {
                _current = value;
            }
        }

        public int ActivePower
        {
            get
            {
                return _activePower;
            }

            set
            {
                _activePower = value;
            }
        }

        public double PowerFactor
        {
            get
            {
                return _powerFactor;
            }

            set
            {
                _powerFactor = value;
            }
        }

        public double Frequency
        {
            get
            {
                return _frequency;
            }

            set
            {
                _frequency = value;
            }
        }

        public bool Status
        {
            get
            {
                return _status;
            }
        }

        public SerialPortInterface SerialPortInterface
        {
            get
            {
                return _serialPortInterface;
            }

        }

        private static void _serverLog_InputData(object sender, InputDataEventArgs e)
        {
            Console.WriteLine(e.BusinessObject.ToString());
        }

        public DDS238(bool muestraDatosConsola, SerialPortInterface serial, string ip, int puerto, bool esSocket,bool escribeArchivoLog)
        {
            _esSocket = esSocket;
            _escribeArchivoLog = escribeArchivoLog;

            _dsf = new SendFrameFormat(0x01, FunctionCode.ReadHoldingRegisters);


            if (serial != null)
            {
                _serialPortInterface = serial;
            }
            if (!esSocket)
            {
                //string trm = string.Format("trm_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
                //_t = System.IO.File.AppendText(trm);
                _status = _serialPortInterface.Open();
                _serialPortInterface.DataReceived += Sp_DataReceived;
            }
            else
            {
                _clientRs = new SocketClient(ip, puerto);
                _clientRs.InputData += clientRs_InputData;
                _status = _clientRs.IniciarSocket();

            }

            _muestraDatosConsola = muestraDatosConsola;

            _timer.Interval = 1000;
            _timer.Elapsed += Tarea;
            _timer.Enabled = true;

            _serverLog.InputData += _serverLog_InputData;

            _serverLog.Init(123);
            if (_escribeArchivoLog)
            {
                string log = string.Format(@"log\log_meter_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
                _f = System.IO.File.AppendText(log);
            }
        }

        public static void TestConver(byte[] input)
        {
            var x = new ReceiveFrameFormat(input);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(input);
            double ep = (double)BitConverter.ToInt32(input, 0) / 10;
            Console.WriteLine(ep);
        }
      

        private void Tarea(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_isBusy)
            {
                _isBusy = true;
                if (DateTime.Now.Second == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                _registerAddress = DDSRegisterAddress.ActivePower;
                                break;
                            case 1:
                                _registerAddress = DDSRegisterAddress.Current;
                                break;
                            case 2:
                                _registerAddress = DDSRegisterAddress.Voltage;
                                break;
                            case 3:
                                _registerAddress = DDSRegisterAddress.ExportkWh;
                                break;
                            case 4:
                                _registerAddress = DDSRegisterAddress.ImportkWh;
                                break;
                            default:
                                break;
                        }
                        EnviaSolicitud();
                    }
                }
                else if (DateTime.Now.Second % 3 == 0)
                {

                    _registerAddress = DDSRegisterAddress.ActivePower;
                    EnviaSolicitud();
                }
                else if (DateTime.Now.Second % 15 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                _registerAddress = DDSRegisterAddress.ActivePower;
                                break;
                            case 1:
                                _registerAddress = DDSRegisterAddress.Current;
                                break;
                            case 2:
                                _registerAddress = DDSRegisterAddress.Voltage;
                                break;
                        }

                        EnviaSolicitud();
                    }
                }
                _isBusy = false;
            }
        }

        private void EnviaSolicitud()
        {
            _dataRecived = false;
            _countTime = 0;

            SetRegisterAddress();

            if (_esSocket)
            {
                _status = _clientRs.BinaryWriter(_dsf.ToBytes());

                if (_muestraDatosConsola)
                {
                    Console.WriteLine("BinaryWriter");
                    _dsf.ToBytes().ToList().ForEach(item => Console.Write(string.Format("{0:X} ", item)));
                    Console.WriteLine();
                }
            }
            else
            {
                _status = _serialPortInterface.Send(_dsf.ToBytes());
            }

            while (!_dataRecived && _countTime < _timeOut)
            {
                Thread.Sleep(1);
                _countTime++;
            }

            if (_countTime >= _timeOut)
            {
                Console.WriteLine("Medidor Timeout");
                _cuentaTimeOut++;
                if(_cuentaTimeOut >= _maxTimeOut)
                {
                    _clientRs.TerminaSocket();
                    _clientRs.IniciarSocket();
                }
            }
            else
            {
                _cuentaTimeOut = 0;
                PrintStatus(true);
            }

        }

        private void PrintStatus(bool escribeLog)
        {
            EscribeLog(string.Format("_______________________"));
            EscribeLog(string.Format(DateTime.Now.ToString("yyyyMMddhhmmss")));
            EscribeLog(string.Format("ImportkWh  : {0}", _importkWh));
            EscribeLog(string.Format("ExportkWh  : {0}", _exportkWh));
            EscribeLog(string.Format("Voltage    : {0}", _voltage));
            EscribeLog(string.Format("ActivePower: {0}", _activePower));
            EscribeLog(string.Format("Current    : {0}", _current));

            if (escribeLog)
            {
                try
                {
                    if (_escribeArchivoLog)
                    {
                        _f.WriteLine(string.Format("{0};{1};{2};{3:0.00};{4};{5:0.00}", DateTime.Now, _importkWh, _exportkWh, _voltage, _activePower, _current));
                        _f.Flush();
                    }
                }
                catch (Exception ex)
                {
                    EscribeLog(ex);
                }
            }
        }
        void EscribeLog(object obj)
        {
            var log = obj.ToString();
            _serverLog.Send(log + System.Environment.NewLine);
            if (_muestraDatosConsola)
                Console.WriteLine(log);
        }
        private void clientRs_InputData(object sender, InputDataEventArgs e)
        {
            InputData((byte[])(e.BusinessObject.Clone()));
        }

        private void Sp_DataReceived(object sender, SerialPortEventArgs arg)
        {
            InputData((byte[])(arg.ReceivedData.Clone()));
        }

        private void InputData(byte[] datagram)
        {
            try
            {
                _drf = new ReceiveFrameFormat(datagram);

                if (_muestraDatosConsola)
                {
                    Console.WriteLine("ReceivedData");
                    datagram.ToList().ForEach(item => Console.Write(string.Format("{0:X} ", item)));
                    Console.WriteLine();
                }

                if (_drf.CRCValido)
                {
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(_drf.DataArea);

                    switch (_registerAddress)
                    {
                        case DDSRegisterAddress.TotalkWh:
                            break;
                        case DDSRegisterAddress.ExportkWh:
                            _exportkWh = (double)BitConverter.ToInt32(_drf.DataArea, 0) / 100;
                            break;
                        case DDSRegisterAddress.ImportkWh:
                            _importkWh = (double)BitConverter.ToInt32(_drf.DataArea, 0) / 100;
                            break;
                        case DDSRegisterAddress.Voltage:
                            _voltage = (double)BitConverter.ToUInt16(_drf.DataArea, 0) / 10;
                            break;
                        case DDSRegisterAddress.Current:
                            _current = (double)BitConverter.ToUInt16(_drf.DataArea, 0) / 100;
                            break;
                        case DDSRegisterAddress.ActivePower:
                            _activePower = BitConverter.ToInt16(_drf.DataArea, 0);
                            break;
                        case DDSRegisterAddress.PowerFactor:
                            break;
                        case DDSRegisterAddress.Frequency:
                            break;
                        case DDSRegisterAddress.ID_BaudRate:
                            break;
                        default:
                            break;
                    }

                    _dataRecived = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void SetRegisterAddress()
        {
            _dsf.StartingAddress[0] = 0x00;
            _dsf.StartingAddress[1] = (byte)_registerAddress;

            switch (_registerAddress)
            {
                case DDSRegisterAddress.TotalkWh:
                    _dsf.Quantity = new byte[] { 0x00, 0x02 };
                    break;
                case DDSRegisterAddress.ExportkWh:
                    _dsf.Quantity = new byte[] { 0x00, 0x02 };
                    break;
                case DDSRegisterAddress.ImportkWh:
                    _dsf.Quantity = new byte[] { 0x00, 0x02 };
                    break;
                case DDSRegisterAddress.Voltage:
                    _dsf.Quantity = new byte[] { 0x00, 0x01 };
                    break;
                case DDSRegisterAddress.Current:
                    _dsf.Quantity = new byte[] { 0x00, 0x01 };
                    break;
                case DDSRegisterAddress.ActivePower:
                    _dsf.Quantity = new byte[] { 0x00, 0x01 };
                    break;
                case DDSRegisterAddress.PowerFactor:
                    _dsf.Quantity = new byte[] { 0x00, 0x01 };
                    break;
                case DDSRegisterAddress.Frequency:
                    _dsf.Quantity = new byte[] { 0x00, 0x01 };
                    break;
                case DDSRegisterAddress.ID_BaudRate:
                    _dsf.Quantity = new byte[] { 0x00, 0x01 };
                    break;
                default:
                    break;
            }
        }

    }
}
