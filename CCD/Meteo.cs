using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCD
{
    public class Meteo
    {
        bool _nubulizador;      
     
        System.IO.StreamWriter _fileLog;
        Termometro _termometro;

        System.Timers.Timer _timer;
        public System.Timers.Timer Timer { get => _timer; set => _timer = value; }
        public Meteo()
        {        

         
        }
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.Now.Second == 0 && DateTime.Now.Minute % 5 == 0)
            {
                _fileLog.WriteLine("{0};{1:0.00};{2:0.00}", DateTime.Now.ToString(), _termometro.Temperatura, _termometro.Humedad);
                _fileLog.Flush();

                _termometro.Dispositivo.Manual = false;
                if (_termometro.Humedad < 65 && _termometro.Temperatura > 27)
                {
                    _termometro.Dispositivo.CambiaEstado(true);

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Enciende Nebulizador");
                    Console.ResetColor();
                }
                else
                {
                    _termometro.Dispositivo.CambiaEstado(false);
                }

            }
        }

        public void Inicia()
        {
            int numeroMuestras = 30;

            string log = string.Format(@"log\log_meteo_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
            _fileLog = System.IO.File.AppendText(log);

            _termometro = new Termometro(numeroMuestras);
            _termometro.IniciaLectura();

            Timer = new System.Timers.Timer();
            Timer.Interval = 1000;
            Timer.Elapsed += timer_Elapsed;
            Timer.Start();

        }

    }
}
