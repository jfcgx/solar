using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CCDStandar
{
    public class Dispositivo
    {
        static Random random = new Random();
        HttpClient httpClient = new HttpClient();
        //private Conexion _conexion;
        private System.Timers.Timer _timer;
        private bool _estaOcupado;
        private int _timeOut =10;
        private int _cuentaEsperaRespuesta;
        private string _nombre;
        private int _powerConsumption;
        private string _ip;
        private string _trigger;
        private bool _estado;
        private bool _mantieneEstado;
        private ConnectionType _connection;
        private ModuleType  _moduleType;
        private string _key;
        public  string PowerOff ;
        public  string PowerOn ;
        private bool _manual;
        int _ponderacion;
        double _temperatura;
        double _humedad;
        private bool _bloqueo;
        private EnumTipoCarga _tipoCarga;

        public event EventHandler InputData;
        protected void OnInputChanged(object sender)
        {
            var handler = InputData;
            if (handler == null)
                return;

            handler(sender, new EventArgs());
        }
        public void MantieneEstado(bool value)
        {
            _mantieneEstado = value;
        }
        public string Nombre
        {
            get
            {
                return _nombre;
            }

            set
            {
                _nombre = value;
            }
        }

        public int PowerConsumption
        {
            get
            {
                return _powerConsumption;
            }

            set
            {
                _powerConsumption = value;
            }
        }

        public string Trigger
        {
            get
            {
                return _trigger;
            }

            set
            {
                _trigger = value;
            }
        }

        public bool Estado
        {
            get
            {
                return _estado;
            }
            set
            {
                _estado = value;
            }
        }

        public ConnectionType Connection
        {
            get
            {
                return _connection;
            }

            set
            {
                _connection = value;
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }

        public string Ip
        {
            get
            {
                return _ip;
            }

            set
            {
                _ip = value;
            }
        }

        public bool Manual { get => _manual; set => _manual = value; }
        public int Ponderacion { get => _ponderacion; set => _ponderacion = value; }
        public double Temperatura { get => _temperatura; set => _temperatura = value; }
        public double Humedad { get => _humedad; set => _humedad = value; }
        internal ModuleType ModuleType { get => _moduleType; set => _moduleType = value; }
        public bool Bloqueo { get => _bloqueo; set => _bloqueo = value; }
        public int Interval
        {
            set
            {
                Timer.Interval = value;
                Timer.Start();
            }
        }

        public EnumTipoCarga TipoCarga { get => _tipoCarga; set => _tipoCarga = value; }
        public System.Timers.Timer Timer { get => _timer; set => _timer = value; }

        public bool CambiaEstado(bool value)
        {
            bool valueSet = false;
            if (!_manual)
            {
                if (value != _estado)
                {
                    valueSet = EnviaEstado(value);
                }
            }
            return valueSet;
        }

        public void AnalizaRespuesta(string res)
        {
            if (Connection == ConnectionType.tasmota)
            {
                try
                {
                    res = WebUtility.HtmlDecode(res);

                    switch (ModuleType)
                    {
                        case ModuleType.Basic:
                            _estado = res.Contains("ON");
                            //Console.WriteLine("Edtado        :{0}", _estado);
                            break;
                        case ModuleType.Dual:
                            List<string> dual = Regex.Split(res, "{c}").ToList();
                            dual.RemoveAt(0);
                            int ind = 0;
                            int.TryParse(Key, out ind);
                            ind--;
                            _estado = dual[ind].Contains("ON");
                            //Console.WriteLine("Edtado        :{0}", _estado);
                            break;
                        case ModuleType.TH:
                            _estado = res.Contains("ON");
                            //Console.WriteLine("Edtado        :{0}", _estado);
                            var lec = res.Substring(res.IndexOf("{t}") + 3, res.LastIndexOf("{t}") - 3);

                            var nlec = Regex.Split(lec, "{s}");
                            foreach (var item in nlec)
                            {
                                if (item != string.Empty)
                                {
                                    var lecd = item.Substring(0, item.IndexOf("{e}"));
                                    var valorLectura = lecd.Substring(lecd.LastIndexOf("{m}"), lecd.Length - lecd.LastIndexOf("{m}"));
                                    var resultString = Regex.Match(valorLectura, @"\d+\.\d+").Value;

                                    if (lecd.Contains("Temperature"))
                                    {

                                        double.TryParse(resultString, NumberStyles.Any, CultureInfo.InvariantCulture, out _temperatura);
                                        //Console.WriteLine("Temperatura  :{0}", _temperatura);
                                    }
                                    else if (lec.Contains("Humidity"))
                                    {
                                        double.TryParse(resultString, NumberStyles.Any, CultureInfo.InvariantCulture, out _humedad);
                                        //Console.WriteLine("Humedad      :{0}", _humedad);
                                    }
                                }
                            }
                            break;
                    }                  
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        public bool EnviaEstado(bool value)
        {
            bool valueSet = false;
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Envia estado al dispositivo {0} con {1} valor {2}", _nombre, _connection.ToString(), value);
                Console.ResetColor();
                if (value)
                {
                    switch (Connection)
                    {
                        case ConnectionType.ifttt:
                            valueSet = CURLIFTTT(Trigger + "_on", Key);
                            _estado = valueSet;
                            break;
                        case ConnectionType.tasmota:
                            var result = CURLTASMOTA(_ip, PowerOn);
                            valueSet = result.Result;
                            _estado = valueSet;
                            break;
                        case ConnectionType.mqtt:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (Connection)
                    {
                        case ConnectionType.ifttt:
                            valueSet = CURLIFTTT(Trigger + "_off", Key);
                            _estado = !valueSet;
                            break;
                        case ConnectionType.tasmota:
                            var result = CURLTASMOTA(_ip, PowerOff);
                            valueSet = result.Result;
                            _estado = valueSet;
                            break;
                        case ConnectionType.mqtt:
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return valueSet;
        }

        public Dispositivo()
        {
        }
        public Dispositivo(string nombre, EnumTipoCarga tipoCarga, int powerConsumption, int ponderacion, string trigger, double interval, string ip, ConnectionType connection, ModuleType moduleType, string key, bool bloqueo)
        {
            _tipoCarga = tipoCarga;
            _moduleType = moduleType;
            Ponderacion = ponderacion;
            this.Ip = ip;
            this.Key = key;
            this.Connection = connection;
            this.Nombre = nombre;
            this.PowerConsumption = powerConsumption;
            this.Trigger = trigger;
            _bloqueo = bloqueo;

            if (connection != ConnectionType.ifttt)
            {
                PowerOn = string.Format("Power{0} On", key);
                PowerOff = string.Format("Power{0} Off", key);
            }
            if (interval > 0)
            {
                interval = interval * 60000 + random.Next(-59000, 59000);
                Timer = new System.Timers.Timer(interval);
                Timer.Elapsed += timer_Elapsed;
                Timer.Start();
                Console.WriteLine("Dispositivo {0} se actualizará con un intervalo de {1:0.00} minutos", _nombre, interval / 60000);
            }
        }
        public void Inicia()
        {
            if (Connection != ConnectionType.ifttt)
            {
                PowerOn = string.Format("Power{0} On", Key);
                PowerOff = string.Format("Power{0} Off", Key);
            }

            double interval = 5 * 60000 + random.Next(-60000, 60000);
            Timer = new System.Timers.Timer(interval);
            Timer.Elapsed += timer_Elapsed;
            Timer.Start();
            Console.WriteLine("Dispositivo {0} se actualizará con un intervalo de {1:0.00} minutos", _nombre, interval / 60000);
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_mantieneEstado)
            {
                if (!_estaOcupado)
                {
                    _estaOcupado = true;
                    //mantener estado cada 
                    //Console.ForegroundColor = ConsoleColor.Yellow;
                    ////Console.WriteLine("Mantiene estado {0} valor {1}", _nombre, _estado);
                    //Console.ResetColor();

                    switch (Connection)
                    {
                        case ConnectionType.ifttt:
                            EnviaEstado(_estado);
                            break;
                        case ConnectionType.tasmota:
                            var result = CURLTASMOTASTATUS(_ip, "1");
                            _cuentaEsperaRespuesta = 0;
                            while (!result.Result)
                            {
                                Thread.Sleep(10);
                                _cuentaEsperaRespuesta++;
                                if (_cuentaEsperaRespuesta > _timeOut)
                                {
                                    break;
                                }

                            }
                            if (_cuentaEsperaRespuesta > _timeOut)
                            {
                                Console.WriteLine("Dispositivo {0} no responde", _nombre);
                            }
                            else
                            {
                                OnInputChanged(this);
                            }
                            break;
                        case ConnectionType.mqtt:
                            break;
                        default:
                            break;
                    }
                    _estaOcupado = false;
                }
            }
        }

        public override string ToString()
        {
            return Nombre;
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
        public async Task<bool> CURLTASMOTA(string ip, string action)
        {
            bool estado = false;
            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["text"] = string.Empty;
                string url = string.Empty;

                url = string.Format("http://{0}/cm?cmnd={1}", ip, action);

                Console.WriteLine("Resultado para : {0}", url);
                var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(parameters));
                var contents = await response.Content.ReadAsStringAsync();
                Console.WriteLine("contents       : {0}", contents);
                estado = contents.Contains("ON");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ejecuta trigger: {0} {1}", ip, ex);
            }
            return estado;
        }
        public async Task<bool>  CURLTASMOTASTATUS(string ip, string action)
        {
            bool respuesta = false;
            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["text"] = string.Empty;
                string url = string.Format("http://{0}/?m={1}", ip, action);
                //Console.WriteLine("Resultado para : {0} {1}", url, _key);
                var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(parameters));
                var contents = await response.Content.ReadAsStringAsync();
                //Console.WriteLine("contents       : {0}", contents);

                AnalizaRespuesta(contents);
                respuesta = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ejecuta trigger: {0} {1}", ip, ex);
            }
            return respuesta;
        }
    }

    public enum ConnectionType
    {
        ifttt,
        tasmota,
        mqtt
    }
}
