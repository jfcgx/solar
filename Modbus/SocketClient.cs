using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ModBus
{
    public class SocketClient
    {
        #region const
        int readTimeOut = 2000;
        #endregion

        #region eventos
        public event EventHandler<InputDataEventArgs> InputData;       
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler Disconnect;
        public event ChangedEventHandler Connect;      
        #endregion

        #region socket
        int _puerto;
        string _ip;
        #endregion

        #region Thread
        private  TcpClient _tcpClient;
        private System.Timers.Timer _timer;

        private ThreadStart _ts;
        private Thread _thread;

        private bool _esActivo;
        private bool _intentaConectar;

        #endregion

        #region atributos
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
        #endregion
        public SocketClient(string ip, int puerto)
        {
            Puerto = puerto;
            Ip = ip;
        }
        protected virtual void OnDisconnect(EventArgs e)
        {
            Disconnect?.Invoke(this, e);
        }
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

            }
        }
        public bool IsConnected
        {
            get
            {
                try
                {
                    if (_tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.Connected)
                    {
                        /* pear to the documentation on Poll:
                         * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                         * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                         * -or- true if data is available for reading; 
                         * -or- true if the connection has been closed, reset, or terminated; 
                         * otherwise, returns false
                         */

                        // Detect if client disconnected
                        if (_tcpClient.Client.Poll(1, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (_tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool IniciarSocket()
        {
            bool esExito = false;
            if (!_intentaConectar)
            {
                _intentaConectar = true;
              
                if (!_esActivo)
                {
                    try
                    {
                        _tcpClient = new TcpClient();
                        _ts = new ThreadStart(BinaryRead);
                        _thread = new Thread(_ts);
                        _thread.Priority = ThreadPriority.Normal;

                        Console.WriteLine(DateTime.Now + " Connecting:{0}:{1}", Ip, Puerto);
                        _tcpClient.Connect(Ip, Puerto); // use the ipaddress as in the server program
                        Console.WriteLine(DateTime.Now + " Connected");

                        esExito = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now} No Connected:{Ip}");
                        TerminaSocket();
                        esExito = false;
                    }
                }
                else
                {
                    TerminaSocket();
                }
                if (esExito)
                {
                    if (_tcpClient.Connected)
                    {
                        _esActivo = true;
                        esExito = true;
                        _thread.Start();
                        //if (_timer == null)
                        //{
                            _timer = new System.Timers.Timer(3000);
                            _timer.Elapsed += RevisarConexion;
                            _timer.Start();
                        //}
                        Connect?.Invoke(this, null);
                    }
                }
                _intentaConectar = false;
            }
            return esExito;
        }

        private void RevisarConexion(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_esActivo)
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
                    NetworkStream netStream = _tcpClient.GetStream();
                    Thread.Sleep(50);
                    byte[] buffer = new byte[_tcpClient.Available];
                    int k = 0;
                    if (netStream.CanRead)
                    {
                        k = netStream.Read(buffer, 0, (int)_tcpClient.Available);

                        if (k > 0)
                        {
                            var bufferTemp = new byte[k];

                            for (int i = 0; i < bufferTemp.Length; i++)
                            {
                                bufferTemp[i] = buffer[i];
                            }
                            try
                            {
                                OnInputChanged(bufferTemp);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _esActivo = false;
                Thread.Sleep(10);
            }
        }
        public string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:X2} ", b);
            return hex.ToString().TrimEnd();
        }
        public bool BinaryWriter(byte[] arByte)
        {
            bool esExito = false;

            try
            {
                if (_esActivo)
                {
                    if (_tcpClient.Connected)
                    {
                        Stream stm = _tcpClient.GetStream();
                        stm.Write(arByte, 0, arByte.Length);

                        //Console.WriteLine("SocketClient BinaryWriter->" + ByteArrayToString(arByte));
                        esExito = true;
                    }
                }
            }
            catch (Exception)
            {
                TerminaSocket();
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
                    Stream stm = _tcpClient.GetStream();
                    stm.Write(arByte, offset, count);
                    esExito = true;
                }
            }
            catch (Exception)
            {
                _esActivo = false;
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
                    Stream stm = _tcpClient.GetStream();
                    stm.Write(arByte, 0, arByte.Length);
                    esExito = true;
                }
            }
            //catch (Exception)
            catch (Exception ex)
            {
                TerminaSocket();
            }
            return esExito;
        }
        public void TerminaSocket()
        {
            try
            {
                _esActivo = false;

                OnDisconnect(EventArgs.Empty);

                if(_timer!=null) _timer.Close();

                if (_tcpClient != null)
                {
                    _tcpClient.Client.Shutdown(SocketShutdown.Both);
                    _tcpClient.Client.Close();
                    _tcpClient.Close();
                }

                _tcpClient = null;
                _thread = null;
                _timer = null;

                if (_thread != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("El hilo permanece activo.....................");
                }

                GC.Collect();
                _intentaConectar = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
