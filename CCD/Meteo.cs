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
            _fileLog.WriteLine("{0};{1};{2}", DateTime.Now.ToString(), _termometro.Temperatura, _termometro.Humedad);
            _fileLog.Flush();

            if (_termometro.Humedad <= 70 && !_termometro.AumentoHumedad)
            {
                ((Dispositivo)sender).CambiaEstado(true);
            }
            else if (_termometro.Humedad > 60 && _termometro.AumentoHumedad)
            {
                ((Dispositivo)sender).CambiaEstado(false);
            }
        }

        public void Inicia()
        {
            string log = string.Format(@"log\log_meteo_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
            _fileLog = System.IO.File.AppendText(log);

            _termometro = new Termometro();
            _termometro.IniciaLectura();

            while (DateTime.Now.Second != 0)
            {
                Thread.Sleep(1);
            }
            Timer = new System.Timers.Timer();
            Timer.Interval = 60000;
            Timer.Elapsed += timer_Elapsed;
            Timer.Start();

        }

    }
}
