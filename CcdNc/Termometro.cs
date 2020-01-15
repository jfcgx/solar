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
        Dispositivo _dispositivo;
        double _temperatura;
        double _humedad;

        bool _aumentoHumedad;

        List<double> _lecturaHumedad;
        List<double> _lecturaTemperatura;
        int _nMuestra = 30;
        public double Temperatura { get => _temperatura; set => _temperatura = value; }
        public double Humedad { get => _humedad; set => _humedad = value; }
        public bool AumentoHumedad { get => _aumentoHumedad; set => _aumentoHumedad = value; }
        public Dispositivo Dispositivo { get => _dispositivo; set => _dispositivo = value; }

        public Termometro(int numeroMuestas)
        {
            _nMuestra = numeroMuestas;
        }

        public void IniciaLectura()
        {
            Dispositivo = new Dispositivo("Termometro", EnumTipoCarga.Termo, 0, 0, string.Empty, 1, "192.168.0.49", ConnectionType.tasmota, ModuleType.TH, "1");

            _lecturaHumedad = new List<double>();
            _lecturaTemperatura = new List<double>();

            Dispositivo.MantieneEstado(true);

            Dispositivo.Timer.Stop();

            while (DateTime.Now.Second != 0)
            {
                Thread.Sleep(1);
            }

            Dispositivo.Interval = 10000;
            Dispositivo.InputData += Dsi_InputData;
        }

        private void Dsi_InputData(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            double temperatura = ((Dispositivo)sender).Temperatura;
            double humedad = ((Dispositivo)sender).Humedad;

            _lecturaHumedad.Insert(0, humedad); // mantiene idea lista filo
            if (_lecturaHumedad.Count > _nMuestra) _lecturaHumedad.RemoveAt(_nMuestra);

            _lecturaTemperatura.Insert(0, temperatura);
            if (_lecturaTemperatura.Count > _nMuestra) _lecturaTemperatura.RemoveAt(_nMuestra);

            AumentoHumedad = false;

            double promedioHumedad = 0;
            double promedioTemperatura = 0;

            promedioHumedad = _lecturaHumedad.Sum() / _lecturaHumedad.Count;
            promedioTemperatura = _lecturaTemperatura.Sum() / _lecturaTemperatura.Count;

            if (promedioHumedad < humedad) AumentoHumedad = true;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Temperatura promedio:{0:0.00} " +
                            "\nTemperatura actual__:{1:0.00}" +
                            "\nHumedad Promedio____:{2:0.00}", promedioTemperatura, temperatura, promedioHumedad);
            Console.WriteLine("Humedad actual______:{0:0.00}", humedad);
            Console.WriteLine("Muestras humedad____:{0}", _lecturaHumedad.Count);
            Console.WriteLine("Humedad aumento_____:{0}", AumentoHumedad);
            Console.ResetColor();
            _temperatura = promedioTemperatura;
            _humedad = promedioHumedad;
        }
    }
}
