using ModBus;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace CCD
{
    public class HandleClinet
    {
        public delegate void ChangedEventHandler(object sender, EventArgs e);

        #region eventos
        public event EventHandler<InputDataEventArgs> InputData;
        protected void OnInputChanged(byte[] e)
        {
            var handler = InputData;
            if (handler == null)
                return;

            handler(this, new InputDataEventArgs(e));
        }
        #endregion

        #region soket
        TcpClient _clientSocket;
        List<byte[]> _listaRespuesta = new List<byte[]>();
        byte[] _dataRead;
        string _clNo;
        bool _esActiva;
        bool _isClosing;
        Thread _ctThreadIn;
        Thread _ctThreadOut;
        #endregion

        #region Propiedades

        public byte[] DataRead
        {
            get { return _dataRead; }
            set { _dataRead = value; }
        }
        public bool EsActiva
        {
            get { return _esActiva; }
            set { _esActiva = value; }
        }

        public string ClNo { get => _clNo; set => _clNo = value; }

        public void EnviaRespuesta(byte[] respuesta)
        {
            _listaRespuesta.Add(respuesta);
        }
        #endregion

        public void StartClient(TcpClient inClientSocket, string clineNo)
        {
            _clientSocket = inClientSocket;
            ClNo = clineNo;
            _esActiva = true;


            _ctThreadIn = new Thread(doIn);
            _ctThreadIn.Priority = ThreadPriority.Normal;
            _ctThreadIn.Start();

            _ctThreadOut = new Thread(doOut);
            _ctThreadOut.Priority = ThreadPriority.Normal;
            _ctThreadOut.Start();

            Thread.Sleep(10);

        }

        private void doIn()
        {
            byte[] bytesFrom;
            while (_esActiva)
            {
                try
                {
                    if (_clientSocket != null)
                    {
                        NetworkStream networkStream = _clientSocket.GetStream();
                        if (_clientSocket.ReceiveBufferSize > 0)
                        {
                            bytesFrom = new byte[_clientSocket.Available];
                            int count = 0;
                            try
                            {
                                count = networkStream.Read(bytesFrom, 0, (int)_clientSocket.Available);
                            }
                            catch (SocketException ex)
                            {
                                if ((ex.SocketErrorCode == SocketError.Interrupted)) // a blocking listen has been cancelled
                                {
                                    Console.WriteLine(" >> HandleClinet.SocketError.Interrupted");
                                }
                            }
                            ReadCallback(count, bytesFrom);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                    _esActiva = false;
                }
                Thread.Sleep(1);
            }
        }

        private void doOut()
        {
            while (_esActiva)
            {
                try
                {
                    NetworkStream networkStream = _clientSocket.GetStream();
                    while (_listaRespuesta.Count == 0 && _esActiva)
                    {
                        Thread.Sleep(10);
                    }
                    if (_listaRespuesta.Count > 0)
                    {
                        if (_listaRespuesta[0].Length > 0 && _esActiva)
                        {
                            networkStream.WriteTimeout = 20;
                            networkStream.Write(_listaRespuesta[0], 0, _listaRespuesta[0].Length);
                            networkStream.Flush();
                        }
                        _listaRespuesta.RemoveAt(0);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                    _esActiva = false;
                }
            }
        }

        private void ReadCallback(int bytesRead, byte[] state)
        {
            _dataRead = state;
            OnInputChanged(_dataRead);
        }

        public void Close() // helper finalize function
        {
            if (!_isClosing)
            {
                _isClosing = true;
                _esActiva = false;
                if (_clientSocket != null)
                {
                    _clientSocket.Close();
                }

                Thread.Sleep(100);

                _ctThreadIn.Abort();
                _ctThreadOut.Abort();

                _clientSocket = null;
                _isClosing = false;
            }
            // here you can free the resources you allocated explicitly
            //System.GC.SuppressFinalize(this);
        }
    }
}
