using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CCD.Properties;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using ModBus;

namespace CCD
{
    [ServiceContract]
    public interface IMeasureControl
    {
        [OperationContract]
        bool SetEstado(string nombre, bool valor);
        [OperationContract]
        bool GetEstado(string nombre);
        [OperationContract]
        string GetLog();
        [OperationContract]
        string Resumen();
        [OperationContract]
        bool Manual(bool value);
    }
    public class MeasureControl : IMeasureControl
    {

        Meteo _meteo;
        string _logTemp;
        DateTime _today;

        static double _todayImportkWh;
        static double _todayExportkWh;
        static Inversor _inverter;

        double _initImportkWh;
        double _initExportkWh;

        SocketServer _serverLog;
        System.IO.StreamWriter _fileLog;
        System.Timers.Timer _timer;
        System.Timers.Timer _timerMonitor;
        DDS238 _meter;
        bool _isBusy = false;
        bool primeraVuelta = true;

        int _ultimaPotenciaInv;
        bool _envioOrdenApagadoGrid;
        bool _envioAviso;
        int _potenciaActiva;

        int _contadorEsperaCargaAntesDeApagar;

        int _duracionPausa;
        int _contadorPausa;
        string _nombreProceso;

        public static string Log;
        private static GestionCarga _gc;

        private static MeasureControl instance = null;
        private int _cuentaMedidorSinConexion = 0;
        private int _medidorTimeOut = 120;
        private bool _bloqueoCierrePorTimeOut;

        public static MeasureControl GetInstance()
        {
            if (instance == null)
                instance = new MeasureControl();

            return instance;
        }
        private MeasureControl()
        {

        }
        public void Inicia()
        {
            Process currentProcess = Process.GetCurrentProcess();
            _nombreProceso = currentProcess.ProcessName;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(_nombreProceso);
            Console.ResetColor();

            FinalizarInstanciaPrevia();
            Thread.Sleep(3000);

            _serverLog = new SocketServer();
            _logTemp = string.Empty;

            _timerMonitor = new System.Timers.Timer();
            _timer = new System.Timers.Timer();
            _ultimaPotenciaInv = 0;
            _gc = GestionCarga.GetInstance();
            _gc.Inicia();
            //GridInverter.ApagarAviso();

            _serverLog.InputData += _serverLog_InputData;
            _serverLog.Init(1234);


            _timer.Interval = 1000;
            _timer.Elapsed += Timer_Elapsed;
            _timer.Enabled = true;

            //GridInverter.EncenderGrid();


            string log = string.Format(@"log\log_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
            _fileLog = System.IO.File.AppendText(log);

            var fi = new System.IO.FileInfo("est.txt");
            _today = DateTime.Now.Date;

            #region Estado
            if (fi.Exists)
            {
                try
                {
                    string ultimoEstado = System.IO.File.ReadAllText("est.txt");
                    var estados = ultimoEstado.Split(';');

                    _ultimaPotenciaInv = int.Parse(estados[0]);
                    _envioOrdenApagadoGrid = bool.Parse(estados[1]);
                    _envioAviso = bool.Parse(estados[2]);
                    _potenciaActiva = int.Parse(estados[3]);
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("AmbarTablero")).Estado = (bool.Parse(estados[4]));
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("AmbarPileta")).Estado = (bool.Parse(estados[5]));
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("LedPileta")).Estado = (bool.Parse(estados[6]));
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("LedTablero")).Estado = (bool.Parse(estados[7]));
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("Filtro")).Estado = (bool.Parse(estados[8]));
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("Bomba01")).Estado = (bool.Parse(estados[9]));
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("Bomba02")).Estado = (bool.Parse(estados[10]));
                    // (DateTime.Parse(estados[11]));
                    _initExportkWh = (double.Parse(estados[12]));
                    Console.WriteLine(_initExportkWh);
                    _initImportkWh = (double.Parse(estados[13]));
                    Console.WriteLine(_initImportkWh);
                }
                catch (Exception ex)
                {
                    EscribeLog(ex.Message);
                }
            }
            #endregion

            #region Busca la ultima lectura del medidor (dia anterior)
            try
            {
                if (_initExportkWh == 0 || _initImportkWh == 0)
                {

                    string meter = string.Format(@"log\log_meter_{0}.txt", DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
                    var archivoLectura = new System.IO.FileInfo(meter);
                    if (archivoLectura.Exists)
                    {
                        var f = System.IO.File.ReadLines(meter);
                        string lineaUltimaLectura = f.Last();

                        var estados = lineaUltimaLectura.Split(';');

                        //_today = (DateTime.Parse(estados[0]).Date.AddDays(1));
                        _initExportkWh = (double.Parse(estados[2]));
                        Console.WriteLine("initExportkWh:{0}", _initExportkWh);
                        _initImportkWh = (double.Parse(estados[1]));
                        Console.WriteLine("initImportkWh:{0}", _initImportkWh);

                    }
                }
            }
            catch (Exception ex)
            {
                EscribeLog(ex.Message);
            }
            #endregion

            EscribeLog(log);

            Console.Beep();

            _inverter = new Inversor(true, false, "192.168.0.222", 8899);
            _inverter.Inicia();

            if (Properties.Settings.Default.MeterSocket)
            {
                var ip = Properties.Settings.Default.MeterIP.Split(':')[0];
                var port = Properties.Settings.Default.MeterIP.Split(':')[1];
                _meter = new DDS238(false, null, ip, int.Parse(port), true, true);
            }
            else
            {
                _meter = new DDS238(false, new SerialPortInterface(Properties.Settings.Default.MeterPortName, Properties.Settings.Default.MeterBaudRate,
                    Properties.Settings.Default.MeterDataBits, Properties.Settings.Default.MeterParity,
                    Properties.Settings.Default.MeterHandShake, Properties.Settings.Default.MeterStopBits), string.Empty,
                    0, false, true);
            }

            Thread.Sleep(2000);

            if (_meter.Status)
            {
                _inverter.EncenderGrid();
                _timerMonitor.Interval = 1000;
                _timerMonitor.Elapsed += Monitor;
                _timerMonitor.Start();

            }
            else
            {
                if (!_bloqueoCierrePorTimeOut)
                {
                    _bloqueoCierrePorTimeOut = true;
                    _inverter.ApagarGrid();
                    Thread.Sleep(1000);
                    _gc.ApagaConsumos();
                    Thread.Sleep(4000);
                    ReiniciaApp();
                }

            }
            EscribeLog(string.Format("Estado monitor {0}", _meter.Status));

            _meteo = new Meteo();
            _meteo.IniciaLectura();
        }
        public void EstadoApp()
        {

            #region Reinicia App cada 1 hora
            if (DateTime.Now.Minute == 0 && DateTime.Now.Second == 0) // cada 1 hora
            {
                _isBusy = true;

                if (DateTime.Now.Hour == 0) // escribir los datos de la última lectura del medidor para el día en curso
                {
                    _initExportkWh = _meter.ExportkWh;
                    _initImportkWh = _meter.ImportkWh;
                    _today = DateTime.Today;
                    _ultimaPotenciaInv = 0;

                    EscribeEstado();
                    Thread.Sleep(2000);
                }

                ReiniciaApp();

                _isBusy = false;

            }
            #endregion

        }
        public void ReiniciaApp()
        {

            Process cmd = new Process();
            cmd.StartInfo.FileName = _nombreProceso + @".exe";
            cmd.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            cmd.Start();

        }
        public void Monitor(object sender, System.Timers.ElapsedEventArgs e)
        {
            _gc.CheckAlMenosUnaBombaActiva();

            if (!_meter.Status)
            {
                if (_cuentaMedidorSinConexion >= _medidorTimeOut)
                {
                    if (!_bloqueoCierrePorTimeOut)
                    {
                        _bloqueoCierrePorTimeOut = true;
                        EscribeLog("Apagando todas las cargas");
                        _inverter.ApagarGrid();
                        Thread.Sleep(1000);
                        _gc.ApagaConsumos();
                        Thread.Sleep(4000);

                        ReiniciaApp();
                    }
                }
                else
                {
                    _cuentaMedidorSinConexion++;
                    Console.WriteLine("medidor sin conexión pasada {0}", _cuentaMedidorSinConexion);
                }
            }
            else
            {
                _cuentaMedidorSinConexion = 0;
                if (!_isBusy && !(DateTime.Now.Minute == 0 && DateTime.Now.Second == 0))
                {
                    _isBusy = true;

                    if (_meter.Status)
                    {
                        #region ciclo repetitivo de analisis de consumos
                        if (DateTime.Now.Second % Properties.Settings.Default.CadaXSegundos == 0)
                        {
                            //EscribeLog("--------------------------------------");
                            //cc.Mantiene(_meter.ActivePower);
                            //EscribeLog("--------------------------------------");
                            //EscribeLog(DateTime.Now);

                            double corrienteInv = ((double)_inverter.Inverter_stat.ac_current / 100);
                            int potenciaInv = _inverter.Inverter_stat.pv_watt;
                            _potenciaActiva = _meter.ActivePower;

                            if (_meter.ImportkWh != 0)
                            {
                                _todayExportkWh = _meter.ExportkWh - _initExportkWh;
                                _todayImportkWh = _meter.ImportkWh - _initImportkWh;

                                EscribeLog(string.Format("Export kWh :{0:0.0}", _todayExportkWh));
                                Thread.Sleep(10);
                                EscribeLog(string.Format("Import kWh :{0:0.0}", _todayImportkWh));
                            }
                            EscribeLog(string.Format("Inversor   :{0}", _inverter.SwInversor.Estado));
                            Console.ForegroundColor = ConsoleColor.Green;
                            EscribeLog(string.Format("Potencia W :{0}{1}", _potenciaActiva, Environment.NewLine));
                            Console.ResetColor();

                            int potenciaConsumida = _potenciaActiva + potenciaInv;
                            if (potenciaConsumida == 0) potenciaConsumida = 1;
                            double factorConsumo = ((double)Math.Abs(potenciaInv) / potenciaConsumida) * 100;
                            EscribeLog(string.Format("Cobertura  :{0:0.0}%{1}", factorConsumo, Environment.NewLine));

                            if (!primeraVuelta)
                            {

                                EscribeLog(_inverter.bs_inverter_print_stats());
                                if (_inverter.Inverter_stat.pv_dc_voltage > 100)
                                {
                                    _gc.MantieneEstado(true);

                                    EscribeLog(_gc.Consume(_potenciaActiva, potenciaInv));
                                }
                                else
                                {
                                    _gc.MantieneEstado(false);
                                }

                                #region Consumo Positivo
                                if (_potenciaActiva >= 20) //esta importando
                                {
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    EscribeLog("Importando");
                                    Console.ResetColor();
                                    if (_envioAviso)    //apaga el aviso si estaba encendido
                                    {
                                        _envioAviso = false;
                                        _inverter.ApagarAviso();
                                        Thread.Sleep(1000);
                                    }
                                    EscribeLog(string.Format("Cargas Activas: {0}", _gc.CargaActiva()));
                                }
                                #endregion

                                #region Consumo Negativo
                                else
                                {
                                    #region Aviso 0 tolerancia
                                    if (_potenciaActiva < 0) //tolerancia
                                    {
                                        if (!_envioAviso)
                                        {
                                            _envioAviso = true;
                                            _inverter.EncenderAviso();
                                        }
                                        if (Properties.Settings.Default.AvisoSonoro)
                                            Console.Beep();
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        EscribeLog("Exportando");
                                        Console.ResetColor();
                                    }
                                    #endregion

                                    if (factorConsumo > 200)//el excedente es mayor al consumo actual voy as apagar el inversor
                                    {
                                        if (!_envioOrdenApagadoGrid)
                                        {
                                            // tambien apagar todos los consumos!!
                                            if (_contadorEsperaCargaAntesDeApagar > 10)
                                            {
                                                if (!_bloqueoCierrePorTimeOut)
                                                {
                                                    _bloqueoCierrePorTimeOut = true;
                                                    _envioOrdenApagadoGrid = true;
                                                    _ultimaPotenciaInv = potenciaInv; //aqui porque si es fuera volvera a cero
                                                    EscribeLog(string.Format("el excedente es mayor a {0}% del consumo, voy as apagar el inversor: {1}W", factorConsumo, _potenciaActiva));
                                                    _inverter.ApagarGrid();
                                                }
                                            }
                                            else
                                            {
                                                _contadorEsperaCargaAntesDeApagar++;
                                            }
                                        }
                                    }
                                }
                                #endregion
                                try
                                {
                                    _fileLog.WriteLine(string.Format("{0};{1};{2:0.00};{3};{4};{5:0.00};{6};{7}",
                                        DateTime.Now,
                                        _meter.Voltage,
                                        _meter.Current,
                                        _meter.ActivePower,
                                        (double)(_inverter.Inverter_stat.ac_voltage / 10),
                                        _inverter.Inverter_stat.ac_current / 100,
                                        _inverter.Inverter_stat.pv_watt,
                                        (double)_inverter.Inverter_stat.kw_day / 10));
                                    _fileLog.Flush();
                                }
                                catch (Exception ex)
                                {
                                    EscribeLog(ex);
                                }
                                EscribeEstado();
                            }
                            else
                            {
                                EscribeLog("Omite la primera pasada");

                                primeraVuelta = false;
                            }
                            EscribeLog("#FIN#");
                        }
                        #endregion
                    }
                    else
                    {
                        if (_inverter.SwInversor.Estado)
                        {
                            if (!_bloqueoCierrePorTimeOut)
                            {
                                _bloqueoCierrePorTimeOut = true;
                                _inverter.ApagarGrid();
                                EscribeLog("Apagando todas las cargas");
                                _gc.ApagaConsumos();
                                Thread.Sleep(1000 * 60 * 10); // espera 10 min
                                _gc.MantieneEstado(false);
                            }
                        }

                    }
                    _isBusy = false;
                }
            }
        }
        private void EscribeEstado()
        {

            try
            {
                var dd = new System.IO.FileInfo("est.txt");
                if (dd.Exists)
                    dd.Delete();
                Thread.Sleep(500);

                System.IO.StreamWriter _tu = new System.IO.StreamWriter("est.txt");
                _tu.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13}",
                    _ultimaPotenciaInv,
                    _envioOrdenApagadoGrid,
                    _envioAviso,
                    _potenciaActiva,
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("AmbarTablero")).Estado,
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("AmbarPileta")).Estado,
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("LedPileta")).Estado,
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("LedTablero")).Estado,
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("Filtro")).Estado,
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("Bomba01")).Estado,
                    _gc.Dispositivos.Find(p => p.Nombre.Equals("Bomba02")).Estado,
                    _today,
                    _initExportkWh,
                    _initImportkWh
                    );
                _tu.Flush();
                _tu.Close();
            }
            catch (Exception ex)
            {

                EscribeLog(ex);
            }
        }
        private void EscribeLog(object obj)
        {
            var log = obj.ToString();
            if (log.Contains("#FIN#"))
            {

                Log = _logTemp;
                _logTemp = string.Empty;
            }
            else
            {
                _serverLog.Send(log + System.Environment.NewLine);
                _logTemp += log + System.Environment.NewLine;
                Console.WriteLine(log);
            }
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            EstadoApp();

            if (_envioOrdenApagadoGrid)
            {
                if ((_ultimaPotenciaInv / 2) < _meter.ActivePower)
                {
                    _envioOrdenApagadoGrid = false;
                    _inverter.EncenderGrid();
                    _contadorEsperaCargaAntesDeApagar = 0;
                }
            }
        }
        private void DetenerControl(bool detener)
        {
            //_duracionPausa = min * 60;
            //_contadorPausa = 0;

            //if (min > 0)
            //{
            _timer.Enabled = false;
            if (detener)
            {
                if (!_bloqueoCierrePorTimeOut)
                {
                    _bloqueoCierrePorTimeOut = true;
                    EscribeLog(string.Format("Apagando todas las cargas .... "));// {0} minutos", _duracionPausa / 60));
                    _inverter.ApagarGrid();
                    _gc.EnPausa(true);
                    _gc.MantieneEstado(false);
                    _gc.ApagaConsumos();
                    Thread.Sleep(5000);
                    _gc.ApagaConsumos();
                }
            }
            else
            {
                _inverter.EncenderGrid();
                _gc.EnPausa(false);
                EscribeLog(string.Format("Enciende inversor .... "));
                Thread.Sleep(5000);
            }
            _timer.Enabled = true;

            //}
        }
        private void _serverLog_InputData(object sender, InputDataEventArgs e)
        {
            try
            {
                //string entrada = System.Text.Encoding.ASCII.GetString(e.BusinessObject).Trim();
                //entrada = System.Text.RegularExpressions.Regex.Replace(entrada, @"[^\w\s.!@$%^&*()\-\/]+", "");
                //Console.WriteLine("Entrada {0}", entrada);
                //bool ac;
                //if (entrada.Contains("q1"))
                //{
                //    ac = _gc.Dispositivos.Find(p => p.Nombre.Equals("Bomba02")).Estado;
                //    _gc.Dispositivos.Find(p => p.Nombre.Equals("Bomba02")).CambiaEstado(!ac);
                //    Console.WriteLine("Cambia estado Bomba02 {0}", ac);
                //}
                //if (entrada.Contains("q2"))
                //{
                //    ac = _gc.Dispositivos.Find(p => p.Nombre.Equals("Bomba01")).Estado;
                //    _gc.Dispositivos.Find(p => p.Nombre.Equals("Bomba01")).CambiaEstado(!ac);
                //    Console.WriteLine("Cambia estado Bomba01 {0}", ac);
                //}
                //if (entrada.Contains("q3"))
                //{
                //    ac = _gc.Dispositivos.Find(p => p.Nombre.Equals("Filtro")).Estado;
                //    _gc.Dispositivos.Find(p => p.Nombre.Equals("Filtro")).CambiaEstado(!ac);
                //    Console.WriteLine("Cambia estado Filtro {0}", ac);
                //}
                //if (entrada.Contains("qD"))
                //{
                //    DetenerControl(true);
                //    Console.WriteLine("Deteniendo control de carga -> Entrada:|{0}|", entrada);
                //}

                //if (entrada.Contains("qR"))
                //{
                //    DetenerControl(false);
                //    Console.WriteLine("Reanudando control de carga ->Entrada:|{0}|", entrada);
                //}
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }

        }

        private void FinalizarInstanciaPrevia()
        {
            var lista = System.Diagnostics.Process.GetProcesses();
            Process currentProcess = Process.GetCurrentProcess();

            var q = (from l in lista
                     where (l.ProcessName.Contains(currentProcess.ProcessName))
                     select l).ToList();
            foreach (var its in q)
            {
                if (its.Id != currentProcess.Id)
                    its.Kill();
            }
        }

        public bool SetEstado(string nombre, bool valor)
        {
            return _gc.SetEstado(nombre, valor);
        }

        public bool GetEstado(string nombre)
        {
            return _gc.GetEstado(nombre);
        }

        public string GetLog()
        {
            return Log;
        }

        public string Resumen()
        {
            using (FileStream stream = File.Open("resumen.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    reader.ReadLine(); // salta primera linea
                    while (!reader.EndOfStream)
                    {
                        var re = string.Format("{0};{1};{2};{3}", reader.ReadLine(), _todayImportkWh, _todayExportkWh, _inverter.Inverter_stat.kw_day);
                        return re;
                    }
                }
            }
            return string.Empty;
        }

        public bool Manual(bool value)
        {
            _gc.BombaManual = value;

            return _gc.BombaManual;

        }
    }
}
