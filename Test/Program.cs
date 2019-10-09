using CCD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static List<Dispositivo> _dispositivos;
        private static string res;
        private static int _toleranciaPositiva = 70;
        private static int _toleranciaNegativa = -40;
        static void Main(string[] args)
        {
            //Inversor i = new Inversor(true, true, "192.168.0.222", 8899);
            //i.Inicia();
            //Console.Read();
            //Ini();
            var dsi = new Meteo();
            dsi.Inicia();
            //dsi.AnalizaRespuesta(res);
            ////res = System.Net.WebUtility.HtmlDecode(res);
            ////var lec = res.Substring(res.IndexOf("{t}") + 3, res.LastIndexOf("{t}")-3);

            ////var nlec = Regex.Split(lec, "{s}");
            ////foreach(var item in nlec )
            ////{
            ////    if (item != string.Empty)
            ////    {
            ////        var lecd = item.Substring(0, item.LastIndexOf("{e}"));
            ////    }
            ////}


            //var s1 = Stopwatch.StartNew();

            //var ls = BuscaDispositivos(987);


            //s1.Stop();

            //Console.WriteLine(s1.ElapsedMilliseconds);

            Console.Read();
        }

        static List<Dispositivo> BuscaDispositivos(int power)
        {
            var l = new List<Dispositivo>();

            for (int i = power + _toleranciaNegativa; i <= power + _toleranciaPositiva; i++)
            {
                var dis = GetDispositivos(i);
                if (dis != null)
                {
                    if (dis.Sum(s => s.Ponderacion) > l.Sum(s => s.Ponderacion))
                        l = dis;
                }
            }

            l.ForEach(p=> Console.Write("{0} ",            p));
            Console.WriteLine("suma aproximada {0}", l.Sum(s => s.PowerConsumption));
            return l;
        }
        static List<Dispositivo> GetDispositivos(int power)
        {
            var lista = new List<List<Dispositivo>>();

            foreach (var s in GetCombinations(_dispositivos, power, ""))
            {
                var l = new List<Dispositivo>();
                var sd = s.TrimEnd(';').Split(';');
                foreach (var item in sd)
                {

                    l.Add(_dispositivos.FindAll(p => p.Nombre.Equals(item)).FirstOrDefault());
                }

                lista.Add(l);
            }

            int id = 0;
            int sumaP = 0;
            for (int i=0; i < lista.Count; i++)
            {
                int suma = lista[i].Sum(s => s.Ponderacion);
                if (suma > sumaP)
                {
                    sumaP = suma;
                    id = i;
                }
            }


            return lista.Count>0  ? lista[id] : null;
        }
        public static IEnumerable<string> GetCombinations(List<Dispositivo> set, int sum, string values)
        {
            for (int i = 0; i < set.Count; i++)
            {
                int left = sum - set[i].PowerConsumption;
                string vals = set[i] + ";" + values;
                if (left == 0)
                {
                    yield return vals;
                }
                else
                {
                    List<Dispositivo> possible = set.Take(i).Where(n => n.PowerConsumption <= sum).ToList();
                    if (possible.Count > 0)
                    {
                        foreach (var s in GetCombinations(possible, left, vals))
                        {
                            yield return s;
                        }
                    }
                }
            }
        }



        private static void Ini()
        {
            //_dispositivos = new List<Dispositivo>();
            //Dispositivo AmbarTablero = new Dispositivo("AmbarTablero", 150, 1, "l03ambar", 3, string.Empty, ConnectionType.ifttt, "xxx");
            //Dispositivo AmbarPileta = new Dispositivo("AmbarPileta", 150, 1, "l02ambar", 3, string.Empty, ConnectionType.ifttt, "xxx");
            //Dispositivo LedPileta = new Dispositivo("LedPileta", 50, 1, "l01led", 5, string.Empty, ConnectionType.ifttt, "xxx");
            //Dispositivo LedTablero = new Dispositivo("LedTablero", 21, 1, "l02led", 5, string.Empty, ConnectionType.ifttt, "xxx");
            //Dispositivo Filtro = new Dispositivo("Filtro", 1000, 3, "filtro_piscina", 2, "192.168.0.52", ConnectionType.tasmota, "1");
            //Dispositivo Bomba01 = new Dispositivo("Bomba01", 400, 1, "bomba01", 2, "192.168.0.50", ConnectionType.tasmota, "1");
            //Dispositivo Bomba02 = new Dispositivo("Bomba02", 800, 2, "bomba02", 2, "192.168.0.50", ConnectionType.tasmota, "2");

            //_dispositivos.Add(AmbarTablero);
            //_dispositivos.Add(AmbarPileta);
            //_dispositivos.Add(LedPileta);
            //_dispositivos.Add(LedTablero);
            //_dispositivos.Add(Filtro);
            //_dispositivos.Add(Bomba01);
            //_dispositivos.Add(Bomba02);

            res = "{t}{s}SI7021 Temperature{m}21.9&deg;C{e}{s}SI7021 Humidity{m}49.4%{e}</table>{t}<tr><td style='width:100{c}normal;font-size:62px'>OFF</div></td></tr></table>";
        }
    }
}
