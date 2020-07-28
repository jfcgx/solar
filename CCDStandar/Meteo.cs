using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCDStandar
{
    public class Meteo
    {
        bool _nubulizador;      
     
        System.IO.StreamWriter _fileLog;
        Termometro _termometro;
        static bool _nebulizadorActivo;
        bool _primeraLectura = true;
        string _path;

        System.Timers.Timer _timer;
        public System.Timers.Timer Timer { get => _timer; set => _timer = value; }
        public static bool NebulizadorActivo { get => _nebulizadorActivo; set => _nebulizadorActivo = value; }

        public Meteo(string path)
        {
            _path = path;
         
        }
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.Now.Second == 0 && DateTime.Now.Minute % 5 == 0 || _primeraLectura)
            {
                _fileLog.WriteLine("{0};{1:0.00};{2:0.00}", DateTime.Now.ToString(), _termometro.Temperatura, _termometro.Humedad);
                _fileLog.Flush();

                _termometro.Dispositivo.Manual = false;
                if (_termometro.Humedad < 50 && _termometro.Temperatura > 27)
                {
                    _termometro.Dispositivo.CambiaEstado(true);
                    _nebulizadorActivo = true;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Enciende Nebulizador");
                    Console.ResetColor();
                }
                else
                {
                    _termometro.Dispositivo.CambiaEstado(false);
                    _nebulizadorActivo = false;
                }

            }
            _primeraLectura = false;
        }

        public void Inicia()
        {
            int numeroMuestras = 30;

            string log = string.Format(@"log_meteo_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
            _fileLog = System.IO.File.AppendText(System.IO.Path.Combine(_path, log));

            _termometro = new Termometro(numeroMuestras);
            _termometro.IniciaLectura();

            Timer = new System.Timers.Timer();
            Timer.Interval = 1000;
            Timer.Elapsed += timer_Elapsed;
            Timer.Start();

        }

    }
}
