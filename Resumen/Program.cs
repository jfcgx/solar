using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Resumen
{
    class Program
    {
        //kwh importado diario
        //kwh exportado diario
        //kwh acumulado
        //kwh me anterior

        static void Main(string[] args)
        {
            if (args.Count() > 0)
            {
                Dato dato = new Dato();
                var fw = dato.Acumulado(int.Parse(args[0]), int.Parse(args[1]));

                System.IO.StreamWriter _re = new System.IO.StreamWriter("resumen.csv");
                _re.WriteLine("Promedio;Max;Produccion;Exportado;Importado;TotalFactura;Total;Promedio Actual;Max Actual;Produccion Actual;Exportado Actual;Importado Actual;TotalFactura Actual;Total Actual");
                _re.Write(fw);
                _re.Flush();

                return;
            }

            List<Dato> lista = new List<Resumen.Dato>();
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Environment.CurrentDirectory);

            List<System.IO.FileInfo> archivos = new List<System.IO.FileInfo>();

            archivos = di.GetFiles(@"log\log_meter_????????.txt").ToList();

            List<System.IO.FileInfo> archivosFV = new List<System.IO.FileInfo>();

            archivosFV = di.GetFiles(@"log\log_????????.txt").ToList();

            try
            {
                var dd = new System.IO.FileInfo("Lectura.csv");
                if (dd.Exists)
                    dd.Delete();
                Thread.Sleep(500);

                System.IO.StreamWriter _tu = new System.IO.StreamWriter("Lectura.csv");



                _tu.WriteLine("Dia;ImportkWh;ExportkWh;InitImportkWh;EndtImportkWh;InitExportkWh;EndtExportkWh;GeneratedEnergy;Registros");
                foreach (var item in archivos)
                {
                    DateTime today;//= DateTime.ParseExact(item.Name.Substring(item.Name.LastIndexOf('_')+1,8), "yyyyMMdd", null);
                    double initExportkWh;
                    double initImportkWh;
                    double endtExportkWh;
                    double endtImportkWh;
                    double generatedEnergy;


                    //
                    //var archivoLectura = new System.IO.FileInfo(meter);

                    if (item.Exists)
                    {
                        try
                        {
                            string meter = item.FullName;

                            string fv = string.Format(@"log\log_{0}", meter.Split('_')[2]);

                            var ffv = System.IO.File.ReadLines(fv);

                            var f = System.IO.File.ReadLines(meter);
                            string lineaLectura = f.First(p => p.Split(';')[1] != "0");

                            var estados = lineaLectura.Split(';');
                            today = (DateTime.Parse(estados[0]).Date);
                            Console.WriteLine("Dia          :{0}", today);
                            initExportkWh = (double.Parse(estados[2]));
                            Console.WriteLine("initExportkWh:{0}", initExportkWh);
                            initImportkWh = (double.Parse(estados[1]));
                            Console.WriteLine("initImportkWh:{0}", initImportkWh);


                            lineaLectura = f.Last(p => p.Split(';')[1] != "0");

                            estados = lineaLectura.Split(';');
                            endtExportkWh = (double.Parse(estados[2]));
                            Console.WriteLine("endtExportkWh:{0}", endtExportkWh);
                            endtImportkWh = (double.Parse(estados[1]));
                            Console.WriteLine("endtImportkWh:{0}", endtImportkWh);


                            lineaLectura = ffv.Last(p => p.Split(';')[7] != "0");
                            estados = lineaLectura.Split(';');
                            generatedEnergy = double.Parse(estados[7]);
                            Console.WriteLine("generatedEnergy:{0}", generatedEnergy);

                            Dato d = new Resumen.Dato(today, initExportkWh, initImportkWh, endtExportkWh, endtImportkWh, generatedEnergy);
                            d.Registros = f.Count();


                            _tu.WriteLine("{0};{1:0.00};{2:0.00};{3:0.00};{4:0.00};{5:0.00};{6:0.00};{7};{8}", today, d.DayImportkWh, d.DayExportkWh,d.InitImportkWh ,d.EndtImportkWh,d.InitExportkWh, d.EndtExportkWh, d.GeneratedEnergy, d.Registros);

                            lista.Add(d);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("no se puede leer el archivo {0} {1}",item.Name,ex);
                        }
                    }
                }
                _tu.Flush();
                _tu.Close();
                Console.WriteLine("Archivo generado");
               


                _tu.WriteLine("Dia;ImportkWh;ExportkWh;InitImportkWh;EndtImportkWh;InitExportkWh;EndtExportkWh;GeneratedEnergy;Registros");


            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }

        }

        
    }

    public class Dato
    {
        DateTime _today;//= DateTime.ParseExact(item.Name.Substring(item.Name.LastIndexOf('_')+1,8), "yyyyMMdd", null);
        double _initExportkWh;
        double _initImportkWh;
        double _endtExportkWh;
        double _endtImportkWh;
        double _generatedEnergy;
        int _registros;

        public string Acumulado(int diaIni, int diaFin)
        {
            string salida = string.Empty;
            try
            {
                DateTime inicioCicloAnterior;
                DateTime finCicloAnterior;
                if (DateTime.Now.Day < diaIni)
                {

                    inicioCicloAnterior = DateTime.Now;
                    finCicloAnterior = DateTime.Parse(inicioCicloAnterior.ToString("yyyy") + '-' + inicioCicloAnterior.ToString("MM") + '-' + diaFin).AddMonths(-1);
                    inicioCicloAnterior = DateTime.Parse(inicioCicloAnterior.ToString("yyyy") + '-' + inicioCicloAnterior.AddMonths(-2).ToString("MM") + '-' + diaIni);
                }
                else
                {
                    inicioCicloAnterior = DateTime.Now;
                    finCicloAnterior = DateTime.Parse(inicioCicloAnterior.ToString("yyyy") + '-' + inicioCicloAnterior.ToString("MM") + '-' + diaFin);
                    inicioCicloAnterior = DateTime.Parse(inicioCicloAnterior.ToString("yyyy") + '-' + inicioCicloAnterior.AddMonths(-1).ToString("MM") + '-' + diaIni);
                }
                double Exportado = 0;
                double Importado = 0;
                double TotalFactura = 0;
                double Producción = 0;
                double Max = 0;
                double Promedio = 0;
                double Total = 0;
                string[] primera = null;
                string[] ultima = null;
                int cuentaDias = 0;

                DateTime inicioCicloActual = finCicloAnterior.AddDays(1);
                DateTime finCicloActual = DateTime.Now.Date;

                double ExportadoActual = 0;
                double ImportadoActual = 0;
                double ProducciónActual = 0;
                double TotalFacturaActual = 0;
                double MaxActual = 0;
                double PromedioActual = 0;
                double TotalActual = 0;
                string[] primeraActual = null;
                string[] ultimaActual = null;
                int cuentaDiasActual = 0;

                using (FileStream stream = File.Open("Lectura.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        reader.ReadLine(); // salta primera linea
                        while (!reader.EndOfStream)
                        {
                            var re = reader.ReadLine();
                            var rel = re.Split(';');
                            if (rel[0] != string.Empty)
                            {
                                DateTime dateTime = DateTime.Parse(rel[0]);

                                if (dateTime.Ticks >= inicioCicloAnterior.Ticks && dateTime.Ticks <= finCicloAnterior.Ticks)
                                {
                                    cuentaDias++;
                                    if (primera == null)
                                        primera = rel;
                                    ultima = rel;
                                    var GeneratedEnergy = double.Parse(rel[7]);
                                    Producción += GeneratedEnergy;
                                    if (Max < GeneratedEnergy)
                                        Max = GeneratedEnergy;
                                }

                                if (dateTime.Ticks >= inicioCicloActual.Ticks && dateTime.Ticks <= finCicloActual.Ticks)
                                {
                                    cuentaDiasActual++;
                                    if (primeraActual == null)
                                        primeraActual = rel;
                                    ultimaActual = rel;
                                    var GeneratedEnergy = double.Parse(rel[7]);
                                    ProducciónActual += GeneratedEnergy;
                                    if (MaxActual < GeneratedEnergy)
                                        MaxActual = GeneratedEnergy;
                                }

                            }
                        }
                        Promedio = Producción / cuentaDias;
                        Exportado = double.Parse(ultima[6]) - double.Parse(primera[5]);
                        Importado = double.Parse(ultima[4]) - double.Parse(primera[3]);
                        TotalFactura = Importado + Exportado;
                        Total = TotalFactura + Producción;

                        PromedioActual = ProducciónActual / cuentaDiasActual;
                        ExportadoActual = double.Parse(ultimaActual[6]) - double.Parse(primeraActual[5]);
                        ImportadoActual = double.Parse(ultimaActual[4]) - double.Parse(primeraActual[3]);
                        TotalFacturaActual = ImportadoActual + ExportadoActual;
                        TotalActual = TotalFacturaActual + ProducciónActual;

                    }
                    salida = string.Format("{0:N2};{1:N2};{2:N2};{3:N2};{4:N2};{5:N2};{6:N2};{7:N2};{8:N2};{9:N2};{10:N2};{11:N2};{12:N2};{13:N2}", Promedio,Max, Producción, Exportado, Importado, TotalFactura, Total, PromedioActual,MaxActual, ProducciónActual, ExportadoActual, ImportadoActual, TotalFacturaActual, TotalActual);
                    Console.WriteLine(salida);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return salida;
        }
        public DateTime Today
        {
            get
            {
                return _today;
            }

            set
            {
                _today = value;
            }
        }

        public double InitExportkWh
        {
            get
            {
                return _initExportkWh;
            }

            set
            {
                _initExportkWh = value;
            }
        }

        public double InitImportkWh
        {
            get
            {
                return _initImportkWh;
            }

            set
            {
                _initImportkWh = value;
            }
        }

        public double EndtExportkWh
        {
            get
            {
                return _endtExportkWh;
            }

            set
            {
                _endtExportkWh = value;
            }
        }

        public double EndtImportkWh
        {
            get
            {
                return _endtImportkWh;
            }

            set
            {
                _endtImportkWh = value;
            }
        }
        public double DayExportkWh
        {
            get
            {
                return _endtExportkWh - _initExportkWh;
            }
        }
        public double DayImportkWh
        {
            get
            {
                return _endtImportkWh - _initImportkWh;
            }
        }

        public int Registros
        {
            get
            {
                return _registros;
            }

            set
            {
                _registros = value;
            }
        }

        public double GeneratedEnergy { get => _generatedEnergy; set => _generatedEnergy = value; }

        public Dato(DateTime today, double initExportkWh, double initImportkWh, double endtExportkWh, double endtImportkWh, double generatedEnergy)
        {
            this.Today = today;
            this.InitExportkWh = initExportkWh;
            this.InitImportkWh = initImportkWh;
            this.EndtExportkWh = endtExportkWh;
            this.EndtImportkWh = endtImportkWh;
            this.GeneratedEnergy = generatedEnergy;
        }

        public Dato()
        {
        }
    }
}
