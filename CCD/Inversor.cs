using CCD.Properties;
using ModBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCD
{
    public class Inversor
    {
        //https://github.com/egophil/PV-SysReader
        Dispositivo _swInversor = new Dispositivo("Inversor", -2000,0, "grid", 3, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY);

        HttpClient httpClient = new HttpClient();

        int _countTime;
        int _timeOut = 100; // 1 segundo
        int _maxTimeOutReinicioSocket = 1000;
        int _cuentaTimeOutParaReiniciarElSocket;
        bool _dataRecived;

        System.Timers.Timer _timer = new System.Timers.Timer();
        //Conexion _conexion = new Conexion();
        bool _isBusy;
        SerialPortInterface _serialPortInterface;

         inverter_elements _inverter_stat = new inverter_elements();

        SendFrameFormat _sf;
        bool _esSocket;
        bool _muestraDatosConsola;
        bool _status;
        SocketClient _clientRs;
        string _ip;
        int _puerto;

        public string Ip { get => _ip; set => _ip = value; }
        public int Puerto { get => _puerto; set => _puerto = value; }
        public bool EsSocket { get => _esSocket; set => _esSocket = value; }
        public bool MuestraDatosConsola { get => _muestraDatosConsola; set => _muestraDatosConsola = value; }
        public Dispositivo SwInversor { get => _swInversor; set => _swInversor = value; }
        public inverter_elements Inverter_stat { get => _inverter_stat; set => _inverter_stat = value; }

        public Inversor()
        {

        }
        public Inversor(bool esSocket, bool muestraDatosConsola, string ip, int puerto)
        {
            EsSocket = esSocket;
            MuestraDatosConsola = muestraDatosConsola;
            Ip = ip;
            Puerto = puerto;
        }
        public void Inicia()
        {
           
            _sf = new SendFrameFormat(0x01, FunctionCode.ReadInputRegister);

            _sf.StartingAddress[1] = 0x0;
            _sf.Quantity[1] = 0x14;


                if (!EsSocket)
                {
                    _serialPortInterface = new SerialPortInterface(Settings.Default.InverterPortName, Settings.Default.InverterBaudRate, Settings.Default.InverterDataBits, Settings.Default.InverterParity, Settings.Default.InverterHandShake, Settings.Default.InverterStopBits);
                    _serialPortInterface.Open();
                    _serialPortInterface.DataReceived += SerialPortInterface_DataReceived;
                }
                else
                {
                    _clientRs = new SocketClient(Ip, Puerto);
                    _status = _clientRs.IniciarSocket();
                    _clientRs.InputData += clientRs_InputData;

                }
                _timer.Interval = 3000;
                _timer.Elapsed += Timer_Elapsed;
                _timer.Enabled = true;
           
        }
        private void clientRs_InputData(object sender, InputDataEventArgs e)
        {
            AnalizaEntrada(e.BusinessObject);

        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_isBusy)
            {
                _isBusy = true;


                SolicitaEstado(_sf.ToBytes());
                _isBusy = false;
            }
        }


        public void EncenderGrid()
        {
            SwInversor.CambiaEstado(true);
            //Helps.CURL(Properties.Settings.Default.Grid_ON);
        }
        public void ApagarGrid()
        {
            SwInversor.CambiaEstado(false);
            //Helps.CURL(Properties.Settings.Default.Grid_OFF);
        }
        public void EncenderAviso()
        {
            CURLIFTTT(Properties.Settings.Default.Aviso_ON, Properties.Settings.Default.IFTTT_KEY);
        }
        public bool CURLIFTTT(string trigger, string key)
        {
            bool esExito = false;
            try
            {
                string url = string.Format("https://maker.ifttt.com/trigger/{0}/with/key/{1}", trigger, key);

                //using (var httpClient = new HttpClient())
                //{
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                {
                    var response = httpClient.SendAsync(request);
                    Console.WriteLine("Trigger {0} {1}", trigger, response.Result.StatusCode);
                }
                //}
                esExito = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ejecuta trigger: {0} {1}", trigger, ex);
                esExito = false;
            }
            return esExito;
        }
        public void ApagarAviso()
        {
            CURLIFTTT(Properties.Settings.Default.Aviso_OFF, Properties.Settings.Default.IFTTT_KEY);
        }
        public static void EjecutaProceso(string nombre)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = nombre;
            cmd.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            cmd.Start();
        }
        private void SerialPortInterface_DataReceived(object sender, SerialPortEventArgs arg)
        {
            AnalizaEntrada(arg.ReceivedData);
        }

        private void AnalizaEntrada(byte[] input)
        {
            if (MuestraDatosConsola)
            {
                Console.WriteLine("ReceivedData");
                input.ToList().ForEach(item => Console.Write(string.Format("{00:X} ", item)));
                Console.WriteLine();
            }

            ReceiveFrameFormat rf = new ReceiveFrameFormat(input);
           

            if (rf.CRCValido)
            {
                switch (rf.FunctionCode)
                {
                    case FunctionCode.ReadInputRegister:
                        //Reply02 = ID1, Size, DC_Voltage, Hex[10], AC_Voltage, Hex[4],
                        //AC_Current, Hex[4], AC_Freq, Hex[8], PV_Watt, Hex[2],
                        //KW_Day, Checksum ;
                        Inverter_stat.pv_dc_voltage = LittleEndianVal(rf.DataArea, 0, 2);
                        Inverter_stat.iso_samples = LittleEndianVal(rf.DataArea, 2, 2);
                        Inverter_stat.ac_voltage = LittleEndianVal(rf.DataArea, 12, 2);
                        Inverter_stat.ac_current = LittleEndianVal(rf.DataArea, 18, 2);
                        Inverter_stat.frequency_ac = LittleEndianVal(rf.DataArea, 24, 2);
                        Inverter_stat.pv_watt = LittleEndianVal(rf.DataArea, 34, 2);
                        Inverter_stat.kw_day = LittleEndianVal(rf.DataArea, 38, 2);
                        if (MuestraDatosConsola)
                        {
                            Console.WriteLine(Inverter_print_stats(Inverter_stat));
                        }
                        break;

                    default:
                        break;
                }
                _dataRecived = true;
            }


        }
        public void SolicitaEstado(byte[] msg)
        {
            _dataRecived = false;
            _countTime = 0;
            if (_esSocket)
            {
                _clientRs.BinaryWriter(msg);
            }
            else
            {
                _serialPortInterface.Send(msg);
            }

            while (!_dataRecived && _countTime < _timeOut)
            {
                Thread.Sleep(1);
                _countTime++;
            }

            if (_countTime >= _timeOut)
            {
                Console.WriteLine("Inversor Timeout");
                _cuentaTimeOutParaReiniciarElSocket++;
                if (_cuentaTimeOutParaReiniciarElSocket >= _maxTimeOutReinicioSocket)
                {
                    _clientRs.TerminaSocket();
                    _clientRs.IniciarSocket();
                }
            }
            else
            {
                _cuentaTimeOutParaReiniciarElSocket = 0;
            }
        }
       
        public int LittleEndianVal(byte[] buffer, int index, int largo)
        {
            int vr = 0;
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer, index, largo);
            switch (largo)
            {
                case 2:
                    vr = BitConverter.ToInt16(buffer, index);
                    break;
                case 4:
                    vr = BitConverter.ToInt32(buffer, index);
                    break;
            }

            return vr;
        }
        public void TestTrama(string val)
        {
            //val = val.Replace(" ", string.Empty);
            //byte[] buffer = Helps.HexToByte(val);

            //var bytes_read = buffer.Count();

            //var msg_frame_id = LittleEndianVal(buffer, 0, 2);

            //var msg_frame_size = (int)buffer[2];

            //if (msg_frame_id == DefineConstants.MSG2_FRAME_ID && bytes_read >= msg_frame_size)
            //{
            //    Inverter_stat.pv_dc_voltage = LittleEndianVal(buffer, 3, 2);
            //    Inverter_stat.iso_samples = LittleEndianVal(buffer, 5, 2);
            //    Inverter_stat.ac_voltage = LittleEndianVal(buffer, 15, 2);
            //    Inverter_stat.ac_current = LittleEndianVal(buffer, 21, 2);
            //    Inverter_stat.frequency_ac = LittleEndianVal(buffer, 27, 2);
            //    Inverter_stat.pv_watt = LittleEndianVal(buffer, 37, 2);
            //    Inverter_stat.kw_day = LittleEndianVal(buffer, 41, 2);

            //}

            ////bs_inverter_print_stats(_inverter_stat);
            //Thread.Sleep(1000);
        }
        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", string.Empty);
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public string bs_inverter_print_stats()
        {
            return Inverter_print_stats(Inverter_stat);
        }
        public string Inverter_print_stats(inverter_elements inverter_stat)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Today generated energy in KW : {0}{1}", (double)inverter_stat.kw_day / 10, System.Environment.NewLine));
            sb.Append(string.Format("Photovoltaic DC voltage in V : {0}{1}", (double)inverter_stat.pv_dc_voltage / 10, System.Environment.NewLine));
            sb.Append(string.Format("AC Voltage in V              : {0}{1}", (double)inverter_stat.ac_voltage / 10, System.Environment.NewLine));
            sb.Append(string.Format("Photovoltaic power in W      : {0}{1}", inverter_stat.pv_watt, System.Environment.NewLine));
            sb.Append(string.Format("AC current in A1             : {0}{1}", (double)inverter_stat.ac_current / 100, System.Environment.NewLine));
            return sb.ToString();
        }
    }
    public class inverter_elements
    {
        public int kw_total;
        public int runtime_min;
        public int total_runtime_h;
        public int temperature;
        public int a_cpu_bus_u;
        public int iso_samples;
        public int pv_dc_voltage;
        public int ac_voltage;
        public int ac_current;
        //public double ac_current2;
        public int frequency_ac;
        public int pv_watt;
        public int kw_day;

        public int b_cpu_ac_u;
        public int voltage_pv_start;
        public int t_starts;
        public int code;

        public int v_ac_low_voltage;
        public int v_ac_low_voltage_time;
        public int v_ac_ultra_low_voltage;
        public int v_ac_ultra_low_voltage_time;

        public int v_ac_high_voltage;
        public int v_ac_high_voltage_time;
        public int v_ac_ultra_high_voltage;
        public int v_ac_ultra_high_voltage_time;

        public int low_frequency;
        public int low_frequency_time;
        public int ultra_low_frequency;
        public int ultra_low_frequency_time;

        public int high_frequency;
        public int high_frequency_time;
        public int ultra_high_frequency;
        public int ultra_high_frequency_time;

        public int dci1_in_ma;
        public int dci_limit;
        public int dci_shift;
        public int full_power_limit;
        public int active_power_reduction_rate;
        public int kpv_gain;
        public int kac_gain;
        public int ka_bus_gain;
        public int kb_cpu_ac_gain;

        public int fac_high_spi;
        public int fac_high_spi_time;
        public int fac_low_spi;
        public int fac_low_spi_time;

        public int connecting_fac_high;
        public int connecting_fac_low;

        public int[] serial_number = new int[13];
        public int[] firmware = new int[5];
        public int modbus_addr;

    }
}
