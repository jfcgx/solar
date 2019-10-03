using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCD
{
    public class Meteo
    {

        System.IO.StreamWriter _fileLog;
        public void IniciaLectura()
        {
            string log = string.Format(@"log\log_meteo_{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
            _fileLog = System.IO.File.AppendText(log);
            //_fileLog.WriteLine("Fecha;Temperatura;Humedad");
            //_fileLog.Flush();

            var dsi = new Dispositivo("Termometro", 0, 0, string.Empty, 1, "192.168.0.49", ConnectionType.tasmota, ModuleType.TH, "1");
            dsi.MantieneEstado(true);
            dsi.Interval = 60000;
            dsi.InputData += Dsi_InputData;
        }

        private void Dsi_InputData(object sender, EventArgs e)
        {
            Console.WriteLine("Temperatura  :{0} \nHumedad      :{1}", ((Dispositivo)sender).Temperatura, ((Dispositivo)sender).Humedad);
            _fileLog.WriteLine("{0};{1};{2}", DateTime.Now.ToString(),((Dispositivo)sender).Temperatura, ((Dispositivo)sender).Humedad);
            _fileLog.Flush();
        }
    }
}
