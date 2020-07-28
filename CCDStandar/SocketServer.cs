using ModBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CCDStandar
{
    public class SocketServer
    {
        Thread _thread;
        ThreadStart _threadStart;

        private System.Timers.Timer _aTimer;
        private bool _isActive;

        #region eventos
        public event EventHandler<InputDataEventArgs> InputData;
        protected void OnInputChanged(object sender, byte[] e)
        {
            var handler = InputData;
            if (handler == null)
                return;

            handler(sender, new InputDataEventArgs(e));
        }
        #endregion

        #region socket
        private TcpListener _serverSocket;
        private TcpClient _clientSocket;
        public int _localPort;
        IPAddress _ipAddress = IPAddress.Any;
        List<HandleClinet> _clientes = new List<HandleClinet>();
        bool _isBusy;
        #endregion

        public bool IsActive
        {
            get
            {
                return _isActive;
            }

            set
            {
                _isActive = value;
            }
        }

        public bool Init(int puerto)
        {
            bool esExito = false;
            _localPort = puerto;
            _threadStart = new ThreadStart(ProcesoSocket);
            _thread = new Thread(_threadStart);

            _isActive = true;
            _aTimer = new System.Timers.Timer();
            _aTimer.Enabled = true;
            _aTimer.Interval = 1000;
            _aTimer.Elapsed += new System.Timers.ElapsedEventHandler(Elapsed);
            GC.KeepAlive(_aTimer);


            _serverSocket = new TcpListener(_ipAddress, _localPort);
            _serverSocket.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            _clientSocket = default(TcpClient);

            try
            {
                _serverSocket.Start();
                Console.WriteLine(" >> " + "Server Started Socket {0}:{1}", _ipAddress, _localPort);

                _thread.Start();
                esExito = true;
            }
            catch (Exception)
            {
                Closed();
            }
            return esExito;

        }

        internal void BinaryWriter(object sender, byte[] businessObject)
        {
            var cli = (HandleClinet)sender;

            _clientes.FindAll(p => !p.ClNo.Equals(cli.ClNo)).ForEach(x => x.EnviaRespuesta(businessObject));
        }

        private void ProcesoSocket()
        {
            while (_isActive)
            {

                try
                {
                    _clientSocket = _serverSocket.AcceptTcpClient();
                }
                catch (SocketException e)
                {
                    if ((e.SocketErrorCode == SocketError.Interrupted)) // a blocking listen has been cancelled
                    {
                        Console.WriteLine(" >> Interrupted ");
                    }
                }
                if (_clientSocket.Connected)
                {
                    Console.WriteLine(" >> " + "Client No: " + Convert.ToString(_clientes.Count) + " started!");
                    HandleClinet client = new HandleClinet();
                    client.InputData += new EventHandler<InputDataEventArgs>(Program_InputChanged);
                    client.StartClient(_clientSocket, Convert.ToString(_clientes.Count));
                    _clientes.Add(client);
                }
                Thread.Sleep(100);
            }
        }

        public void Closed()
        {
            try
            {
                _isActive = false;

                if (_aTimer != null)
                {
                    _aTimer.Stop();
                    _aTimer.Dispose();
                }

                if (_clientes != null)
                {
                    foreach (var c in _clientes)
                    {
                        c.InputData -= Program_InputChanged;
                        c.Close();
                    }
                    _clientes.Clear();
                }
                _clientes = null;

                if (_clientSocket != null)
                {
                    _clientSocket.Close();
                }

                if (_serverSocket != null)
                {
                    _serverSocket.Stop();
                }

                if (_thread != null)
                {
                    if (_thread.IsAlive)
                    {
                        _thread.Abort();
                        _thread.Join();
                    }
                }

                _clientSocket = null;
                _serverSocket = null;
                _aTimer = null;

                _thread = null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Elapsed(object sender, EventArgs e)
        {
            #region Mantiene Conexiones
            if (!(_isBusy))
            {
                _isBusy = true;
                if (_clientes != null)
                {
                    if (_clientes.Count > 0)
                    {
                        if (!_clientes.TrueForAll(a => a.EsActiva))
                        {
                            _clientes.FindAll(p => !p.EsActiva).ForEach(x => x.InputData -= Program_InputChanged);
                            _clientes.FindAll(p => !p.EsActiva).ForEach(x => x.Close());
                            _clientes.RemoveAll(p => !p.EsActiva);
                        }
                    }
                }
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();

                _isBusy = false;
            }
            #endregion
        }

        private void Program_InputChanged(object sender, InputDataEventArgs e)
        {
            OnInputChanged(sender, e.BusinessObject);
        }

        public void Send(byte[] serverResp)
        {
            _clientes.ForEach(x => x.EnviaRespuesta(serverResp));
        }
       
        public void Send(string serverResp)
        {
            byte[] bytesToSend = System.Text.Encoding.ASCII.GetBytes(serverResp);
            _clientes.ForEach(x => x.EnviaRespuesta(bytesToSend));
        }

        public bool MatarProceso(string nomProceso)
        {
            bool esExito = true;

            try
            {
                Process[] procesos = Process.GetProcessesByName(nomProceso);
                if (procesos.Length > 0)
                {
                    for (int counter = 0; counter < procesos.Length; counter++)
                    {
                        procesos[counter].CloseMainWindow();
                        if (procesos[counter].HasExited == false)
                        {
                            procesos[counter].Kill();
                            procesos[counter].Close();
                            Console.WriteLine("Proceso detenido: " + nomProceso);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al detener el proceso: " + nomProceso + " " + ex.Message);
                esExito = false;
            }

            return esExito;
        }
    }
}