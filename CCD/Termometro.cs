using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCD
{
    public class Termometro
    {
        double _temperatura;
        double _humedad;

        bool _aumentoHumedad;

        List<double> _lecturaHumedad;
        int _nMuestra = 60;
        int _nParaDecirAumento = 30;
        public double Temperatura { get => _temperatura; set => _temperatura = value; }
        public double Humedad { get => _humedad; set => _humedad = value; }
        public bool AumentoHumedad { get => _aumentoHumedad; set => _aumentoHumedad = value; }

        public Termometro()
        {
          
        }

        public void IniciaLectura()
        {
            _lecturaHumedad = new List<double>();

            var dsi = new Dispositivo("Termometro", EnumTipoCarga.Termo, 0, 0, string.Empty, 1, "192.168.0.49", ConnectionType.tasmota, ModuleType.TH, "1");

            dsi.MantieneEstado(true);

            dsi.Timer.Stop();

            while (DateTime.Now.Second != 0)
            {
                Thread.Sleep(1);
            }

            dsi.Interval = 10000;
            dsi.InputData += Dsi_InputData;
        }

        private void Dsi_InputData(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            double temperatura = ((Dispositivo)sender).Temperatura;
            double humedad = ((Dispositivo)sender).Humedad;

            _lecturaHumedad.Insert(0, humedad); // mantiene idea lista filo
            if (_lecturaHumedad.Count > _nMuestra) _lecturaHumedad.RemoveAt(_nMuestra);

            AumentoHumedad = false;

            double promedioHumedad = 0;

            promedioHumedad = _lecturaHumedad.Sum() / _lecturaHumedad.Count;

            if (_lecturaHumedad.Last() < humedad) AumentoHumedad = true;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Temperatura     :{0} " +
                            "\nHumedad Promedio:{1}", temperatura, promedioHumedad);
            Console.WriteLine("Humedad actual  :{0}", humedad);
            Console.WriteLine("Muestras humedad:{0}", _lecturaHumedad.Count);
            Console.WriteLine("Humedad aumento :{0}", AumentoHumedad);
            Console.ResetColor();
            _temperatura = temperatura;
            _humedad = promedioHumedad;
        }
    }
}
