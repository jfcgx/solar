using Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace MonitorWeb
{
    public class Global : HttpApplication
    {
        public static GridService.MeasureControlClient measureControl = new GridService.MeasureControlClient();
        private static bool _estaOcupado;
        private static bool _cambio = false;
        private static Timer _aTimer = new Timer();
        public static TextFields estado = new TextFields();

        public static bool _estadoB1;
        public static bool _estadoB2;
        public static bool _estadoB3;
        public static bool _manual;


        //public static CCD.SocketClient socketClient;
        public static string logP;
        void Application_Start(object sender, EventArgs e)
        {
            measureControl.Open();


            // Código que se ejecuta al iniciar la aplicación
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            _aTimer.Interval = 3000; //Parametrizar
            _aTimer.Elapsed += new ElapsedEventHandler(aTimer_Elapsed);
            _aTimer.Enabled = true;


            //socketClient = new SocketClient("192.168.0.9", 1234);
            //socketClient.InputData += SocketClient_InputData;
            //socketClient.IniciarSocket();
        }
        private void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_estaOcupado)
            {
                _estaOcupado = true;

                try
                {
                    bool estadoTemp = false;
                    _cambio = false;

                    estado.Contenido = measureControl.GetLog() + Environment.NewLine + DateTime.Now.ToString("HHmmss");
                    //System.Threading.Thread.Sleep(100);
                    List<TextFields> SeedData = new List<TextFields>();
                    SeedData.Add(estado);
                    Application["BoardDatabase"] = SeedData;

                    estadoTemp = measureControl.GetEstado("LedRio");
                    if (_estadoB1 != estadoTemp)
                        _cambio = true;
                    _estadoB1 = estadoTemp;

                    estadoTemp = measureControl.GetEstado("Bomba02");
                    if (_estadoB2 != estadoTemp && !_cambio)
                        _cambio = true;
                    _estadoB2 = estadoTemp;

                    estadoTemp = measureControl.GetEstado("Filtro");
                    if (_estadoB3 != estadoTemp && !_cambio)
                        _cambio = true;
                    _estadoB3 = estadoTemp;

                }
                catch (Exception ex)
                {

                }
                _estaOcupado = false;
            }
        }
        //private void SocketClient_InputData(object sender, InputDataEventArgs e)
        //{
        //    string entrada = System.Text.Encoding.ASCII.GetString(e.BusinessObject).Trim();
        //    Console.WriteLine(entrada);
        //    logP += entrada;

        //    if (logP.Contains("Cargas Activas:"))
        //    {
        //        estado.Contenido = logP;
        //        logP = string.Empty;
        //    }
        //}

    }
}