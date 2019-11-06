using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModBus
{
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
        private TcpClientSession _clientSocket;
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
                        _clientSocket = new AsyncTcpSession();

                        _clientSocket.Connect(new DnsEndPoint(_ip, _puerto));
                        _clientSocket.Connected += clientSocket_Connected;
                        _clientSocket.Closed += clientSocket_Closed;
                        _clientSocket.Error += clientSocket_Error;
                        _clientSocket.DataReceived += BinaryRead;

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
            if (!_clientSocket.IsConnected || !_esActivo)
            {
                Console.WriteLine("socket no conectado reiniciando....{0} {1}", _esActivo, _intentaConectar);
                IniciarSocket();
            }
        }
        public void BinaryRead(object sender, DataEventArgs e)
        {
            try
            {
                if (_clientSocket.IsConnected)
                {
                    try
                    {

                        byte[] data= new byte[e.Length];
                        Array.Copy((byte[])e.Data, 0, data, 0, e.Length);
                        OnInputChanged(data);

                        if (_muestraConsola) Console.WriteLine("{0}:{1} Socket BinaryRead   <-" + ByteArrayToString(data), _ip,_puerto);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("<< {0}:{1} Error : " + e, _ip, _puerto);
                    }
                }
            }
            catch (Exception ex)
            {
                _esActivo = false;

                Console.WriteLine("<< {0}:{1} Error : " + e, _ip, _puerto);
            }
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public bool BinaryWriter(byte[] arByte)
        {
            bool esExito = false;

            try
            {
                if (_esActivo)
                {
                    _clientSocket.Send(new ArraySegment<byte>(arByte, 0, arByte.Length));
                    if (_muestraConsola) Console.WriteLine("{0}:{1} Socket BinaryWriter ->" + ByteArrayToString(arByte),Ip,Puerto);
                    esExito = true;
                }
            }
            catch (Exception ex)
            {
                ReiniciaSocket();
            }
            return esExito;
        }
        public bool BinaryWriter(byte[] arByte, int offset, int count)
        {
            bool esExito = false;

            try
            {
                if (_esActivo)
                {
                    _clientSocket.Send(new ArraySegment<byte>(arByte, 0, arByte.Length));
                    esExito = true;
                }
            }
            catch (Exception)
            {
                ReiniciaSocket();
            }
            return esExito;
        }
        public bool StringWriter(string str)
        {
            bool esExito = false;

            try
            {
                if (_esActivo)
                {
                    var arByte = Encoding.ASCII.GetBytes(str);
                    _clientSocket.Send(new ArraySegment<byte>(arByte, 0, arByte.Length));
                    esExito = true;
                }
            }
            catch (Exception ex)
            {
                ReiniciaSocket();
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
        void clientSocket_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("<<Error {0}:{1} : " + e.Exception.Message, _ip, _puerto);
            _esActivo = false;
        }

        void clientSocket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("<<Desconectado del servidor {0}:{1}>>", _ip, _puerto);
            _esActivo = false;
        }

        void clientSocket_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("<<Conectado al servidor {0}:{1}>>", _ip, _puerto);
            _esActivo = true; 
            Connect?.Invoke(this, null);
        }
    }
}
