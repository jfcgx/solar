using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorWeb
{
    public class InputDataEventArgs : EventArgs
    {
        private readonly byte[] _input;

        public InputDataEventArgs(byte[] input)
        {
            _input = input;
        }
        public byte[] BusinessObject
        {
            get { return _input; }
        }
    }
    public class SuperSocketClient
    {
        #region eventos
        public event EventHandler<InputDataEventArgs> InputData;
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler Disconnect;
        public event ChangedEventHandler Connect;

        protected void OnInputChanged(byte[] e)
        {
            try
            {
                if (e.Length > 0)
                {
                    if (e != null && InputData != null)
                    {
                        var handler = InputData;
                        if (handler == null)
                            return;

                        handler(this, new InputDataEventArgs(e));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("<< {0}:{1} Error : " + e, _ip, _puerto);
            }
        }

        protected virtual void OnDisconnect(EventArgs e)
        {
            Disconnect?.Invoke(this, e);
        }
        #endregion
        #region socket
        int _puerto;
        string _ip;
        #endregion
        #region atributos
        private bool _esActivo;
        private bool _intentaConectar;
        byte[] _data;
        #endregion
        #region propiedades

        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }
        public bool EsActivo
        {
            get { return _esActivo; }

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
        
        public int Puerto
        {
            get
            {
                return _puerto;
            }

            set
            {
                _puerto = value;
            }
        }

        public bool MuestraConsola { get => _muestraConsola; set => _muestraConsola = value; }
        #endregion

        private bool _muestraConsola = false;
        private TcpClient _clientSocket;
        private System.Timers.Timer _timer;
        public SuperSocketClient(string ip, int puerto)
        {
            Puerto = puerto;
            Ip = ip;
        }
        public bool IniciarSocket()
        {
            //_intentaReconectar = false; // no reconectar
            bool esExito = false;
            if (!_intentaConectar)
            {
                _intentaConectar = true;

                if (!_esActivo)
                {
                    try
                    {
                        _clientSocket = new TcpClient();

                        _clientSocket.Connect(_ip, _puerto);
                        //_clientSocket.Connected += clientSocket_Connected;
                        //_clientSocket.Closed += clientSocket_Closed;
                        //_clientSocket.Error += clientSocket_Error;
                        //_clientSocket.DataReceived += BinaryRead;

                        esExito = true;
                    }
                    catch (Exception ex)
                    {
                        
                        ReiniciaSocket();
                        esExito = false;
                    }
                }
                else
                {
                    ReiniciaSocket();
                }
                
                if (_timer == null)
                {
                    _timer = new System.Timers.Timer(10000);
                    _timer.Elapsed += RevisarConexion;
                    _timer.Start();
                }
                _intentaConectar = false;
            }
            return esExito;
        }
        private void RevisarConexion(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_clientSocket.Connected || !_esActivo)
            {
                Console.WriteLine("socket no conectado reiniciando....{0} {1}", _esActivo, _intentaConectar);
                IniciarSocket();
            }
        }
        public void BinaryRead()
        {
            try
            {
                while (_esActivo)
                {
                    NetworkStream netStream = _clientSocket.GetStream();
                    Thread.Sleep(50);
                    byte[] buffer = new byte[_clientSocket.Available];
                    int k = 0;
                    if (netStream.CanRead)
                    {
                        k = netStream.Read(buffer, 0, (int)_clientSocket.Available);

                        if (k > 0)
                        {
                            var bufferTemp = new byte[k];

                            for (int i = 0; i < bufferTemp.Length; i++)
                            {
                                bufferTemp[i] = buffer[i];
                            }
                            try
                            {
                                if (_muestraConsola) Console.WriteLine("{0}:{1} Socket BinaryRead   <-" + ByteArrayToString(bufferTemp), _ip, _puerto);

                                OnInputChanged(bufferTemp);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("<< {0}:{1} Error : " , _ip, _puerto);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _esActivo = false;
                Console.WriteLine("<< {0}:{1} Error : ", _ip, _puerto);
                Thread.Sleep(10);
            }
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public bool StringWriter(string str)
        {
            bool esExito = false;

            try
            {
                if (_esActivo)
                {
                    var arByte = Encoding.ASCII.GetBytes(str);
                    Stream stm = _clientSocket.GetStream();
                    stm.Write(arByte, 0, arByte.Length);
                    esExito = true;
                }
            }
            //catch (Exception)
            catch (Exception ex)
            {
                _esActivo = false;
            }
            return esExito;
        }
        public bool BinaryWriter(byte[] arByte)
        {
            bool esExito = false;

            try
            {
                if (_esActivo)
                {
                    if (_clientSocket.Connected)
                    {
                        Stream stm = _clientSocket.GetStream();
                        stm.Write(arByte, 0, arByte.Length);

                        esExito = true;
                    }
                }
            }
            catch (Exception)
            {
                _esActivo = false;
            }
            return esExito;
        }
        public void TerminaSocket()
        {
            try
            {
                _esActivo = false;
                _timer?.Stop();
                _timer = null;

                _clientSocket?.Close();
                Thread.Sleep(1000);
                _clientSocket = null;
                OnDisconnect(EventArgs.Empty);

                GC.Collect();

                _intentaConectar = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void ReiniciaSocket()
        {
            try
            {
                _esActivo = false;
                _timer?.Stop();
                _clientSocket.Close();
                Thread.Sleep(1000);
                _clientSocket = null;
                OnDisconnect(EventArgs.Empty);
                GC.Collect();

                if (_timer != null)
                    _timer.Start();
                else
                {
                    _timer = new System.Timers.Timer(10000);
                    _timer.Elapsed += RevisarConexion;
                    _timer.Start();
                }

                _intentaConectar = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        //void clientSocket_Error(object sender, ErrorEventArgs e)
        //{
        //    Console.WriteLine("<<Error {0}:{1} : " + e.Exception.Message, _ip, _puerto);
        //    _esActivo = false;
        //}

        //void clientSocket_Closed(object sender, EventArgs e)
        //{
        //    Console.WriteLine("<<Desconectado del servidor {0}:{1}>>", _ip, _puerto);
        //    _esActivo = false;
        //}

        //void clientSocket_Connected(object sender, EventArgs e)
        //{
        //    Console.WriteLine("<<Conectado al servidor {0}:{1}>>", _ip, _puerto);
        //    _esActivo = true; 
        //    Connect?.Invoke(this, null);
        //}
    }
}
