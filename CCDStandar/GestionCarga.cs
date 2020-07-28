
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCDStandar
{
    public enum EnumTipoCarga
    {
        Ninguno,
        Led,
        Luz,
        BombaRiego,
        BombaAux,
        Filtro,
        Termo,
        Inversor,
        Medidor
    }
    public class GestionCarga
    {
        private bool _cargaManual;
        private bool _enPausa;

        private List<Dispositivo> _dispositivos;
        private int _cargaTotal;
        private int _toleranciaPositiva = 70;
        private int _toleranciaNegativa = -50;
        private int _toleranciaBusquedaPositiva = 3000;
        private int _toleranciaBusquedaNegativa = -40;
        private int _esperaIncremento = 6;
        private int _potenciaExcedente;
        private int _consumoActual;
        private int _countEsperaIncremento;
        private bool _mantieneEstado;
        public static string _path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public bool BombaManual
        {
            get => _cargaManual;
            set
            {
                _cargaManual = value;

                _dispositivos.FirstOrDefault(p => p.Nombre.Equals("Bomba02")).Manual = value;
                _dispositivos.FirstOrDefault(p => p.Nombre.Equals("Filtro")).Manual = value;
            }
        }
    public List<Dispositivo> Dispositivos { get => _dispositivos; }

        private static GestionCarga instance = null;
        public static GestionCarga GetInstance()
        {
            if (instance == null)
                instance = new GestionCarga();

            return instance;
        }
        private GestionCarga()
        {
        }

        public void Inicia(List<string> dispositivos)
        {

            _mantieneEstado = true;
            _dispositivos = new List<Dispositivo>();

            
            //dispositivos.Add("LedTablero,Led,24,1,LedTablero,2,192.168.0.44,tasmota,Dual,1,false");
            //dispositivos.Add("AmbarTablero,Luz,100,1,AmbarTablero,2,192.168.0.44,tasmota,Dual,2,false");
            //dispositivos.Add("LedPiscina,Led,7,1,LedPiscina,2,192.168.0.44,tasmota,Dual,3,false");
            //dispositivos.Add("LedPileta,Led,30,1,LedPileta,2,192.168.0.45,tasmota,Dual,2,false");
            //dispositivos.Add("AmbarPileta,Luz,100,1,AmbarPileta,2,192.168.0.45,tasmota,Dual,1,false");
            //dispositivos.Add("Filtro,Filtro,1000,1,filtro_piscina,2,192.168.0.52,tasmota,Basic,1,false");
            //dispositivos.Add("LedRio,Led,84,1,LedRio,2,192.168.0.50,tasmota,Dual,1,false");
            //dispositivos.Add("AmbarFrambuesa,Luz,100,1,AmbarFrambuesa,2,192.168.0.47,tasmota,Basic,1,false");
            //dispositivos.Add("AmbarInvernadero,Luz,150,1,AmbarInvernadero,2,192.168.0.46,tasmota,Basic,1,false");
            //dispositivos.Add("Bomba02,BombaRiego,1500,2,bomba02,2,192.168.0.50,tasmota,Dual,2,true");
            //dispositivos.Add("medidor,Filtro,1000,1,medidor,2,192.168.0.51,tasmota,Basic,1,true");



            foreach (var item in dispositivos)
            {
                var v = item.Split(',').ToList();

                Dispositivo dispositivo = new Dispositivo(
                    v[0],
                    (EnumTipoCarga)Enum.Parse(typeof(EnumTipoCarga), v[1]),
                    int.Parse(v[2]),
                    int.Parse(v[3]),
                    v[4],
                    double.Parse(v[5]),
                    v[6],
                    (ConnectionType)Enum.Parse(typeof(ConnectionType), v[7]),
                     (ModuleType)Enum.Parse(typeof(ModuleType), v[8]),
                    v[9],
                    bool.Parse( v[10])
                    );

                _dispositivos.Add(dispositivo);
            }


            //Dispositivo Termo = new Dispositivo("Termo", EnumTipoCarga.Luz, 1500, 2, "Termo", 0, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY);
            //Dispositivo AmbarInvernadero = new Dispositivo("AmbarInvernadero", EnumTipoCarga.Luz, 150, 1, "AmbarInvernadero", 3, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY);
            //Dispositivo AmbarTablero = new Dispositivo("AmbarTablero", EnumTipoCarga.Luz, 100, 1, "l03ambar", 3, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY);
            //Dispositivo AmbarPileta = new Dispositivo("AmbarPileta", EnumTipoCarga.Luz, 100, 1, "l02ambar", 3, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY);
            //Dispositivo LedPileta = new Dispositivo("LedPileta", EnumTipoCarga.Luz, 30, 1, "l01led", 5, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY);
            //Dispositivo LedTablero = new Dispositivo("LedTablero", EnumTipoCarga.Luz, 100, 1, "l02led", 5, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY); //porque tenia 100?
            //Dispositivo Filtro = new Dispositivo("Filtro", EnumTipoCarga.Filtro, 1000, 1, "filtro_piscina", 2, "192.168.0.52", ConnectionType.tasmota, ModuleType.Basic, "1");
            //Dispositivo LedRio = new Dispositivo("LedRio", EnumTipoCarga.Led, 70, 1, "LedRio", 2, "192.168.0.50", ConnectionType.tasmota, ModuleType.Dual, "1");//0,37kw segun mANUL
            //Dispositivo Bomba02 = new Dispositivo("Bomba02", EnumTipoCarga.BombaRiego, 1500, 2, "bomba02", 2, "192.168.0.50", ConnectionType.tasmota, ModuleType.Dual, "2");
            //Dispositivo medidor = new Dispositivo("medidor", EnumTipoCarga.Filtro, 1000, 1, "medidor", 2, "192.168.0.51", ConnectionType.tasmota, ModuleType.Basic, "1");


            //_dispositivos.Add(Termo);
            //_dispositivos.Add(AmbarInvernadero);
            //_dispositivos.Add(AmbarTablero);
            //_dispositivos.Add(AmbarPileta);
            //_dispositivos.Add(LedPileta);
            //_dispositivos.Add(LedTablero);
            //_dispositivos.Add(Filtro);
            //_dispositivos.Add(LedRio);
            //_dispositivos.Add(Bomba02);
            //_dispositivos.Add(medidor);

            //_dispositivos.First(p => p.Nombre.Equals("Bomba02")).Bloqueo = true;

            //_dispositivos.First(p => p.Nombre.Equals("medidor")).Bloqueo = true;

            _dispositivos.First(p => p.Nombre.Equals("medidor")).CambiaEstado(true);
            Thread.Sleep(3000);
        }
        
        public bool SetEstado(string nombre, bool valor)
        {
            bool esExito = false;

            _dispositivos.First(p => p.Nombre.Equals(nombre)).EnviaEstado(valor);            

            return esExito;
        }
        public bool GetEstado(string nombre)
        {
            bool estado = false;
            try
            {
                estado = _dispositivos.First(p => p.Nombre.Equals(nombre)).Estado;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            return estado;
        }
        public  void MantieneEstado(bool valor)
        {
            if (_mantieneEstado != valor)
            {
                _mantieneEstado = valor;
                _dispositivos.ForEach(a => a.MantieneEstado(valor));
            }
        }
        public  bool CargaActiva()
        {
            bool activo = false;
            var dd = from cust in _dispositivos
                     where cust.Estado && !(cust.Nombre.Equals("Bomba02"))
                     select cust;

            activo = dd.Count() > 0;

            var ff = from cust in _dispositivos
                     where cust.Manual
                     select cust;

            _cargaManual = ff.Count() > 0;

            return activo;
        }
        public  int CargaTotal
        {
            get
            {
                return _cargaTotal;
            }

            set
            {
                _cargaTotal = value;
            }
        }
        public  void ApagaConsumos()
        {
            _dispositivos.Where(p=>!p.Bloqueo).ToList().ForEach(a => a.CambiaEstado(false));
        }
        public  void EnPausa(bool valor)
        {
            _enPausa = valor;
        }
        public void CheckAlMenosUnaBombaActiva()
        {

            if (!_dispositivos.First(p => p.Nombre.Equals("Bomba02")).Estado)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("!!!!!!!!!!! Ninguna bomba activa!!!!!!!!!!!!!!!!!!!");
                Console.ResetColor();
                _dispositivos.First(p => p.Nombre.Equals("Bomba02")).CambiaEstado(true);
            }
        }
        public string Consume(int potenciaActiva, int potenciaInversor)
        {
            StringBuilder sb = new StringBuilder();// string.Empty;
            try
            {
                if (!_enPausa)
                {
                    if (potenciaActiva > _toleranciaPositiva || potenciaActiva < _toleranciaNegativa)
                    {

                        _consumoActual = potenciaActiva + potenciaInversor;
                        Console.WriteLine("_consumoActual {0} = potenciaActiva {1} + potenciaInversor {2}", _consumoActual, potenciaActiva, potenciaInversor);

                        if (_countEsperaIncremento == _esperaIncremento )
                        {
                            MantieneEstado(false); // durante el ciclo de cambio no se mantengan los estados de forma automática

                            #region comentario
                            //al la carga total le resto la carga voluntaria (luces filtro etc) 
                            //asi obtengo el valor de la carga no controlada (riegos, refri etc) 
                            //el resultado se lo resto a la potencia del inversor, esto me da el 
                            //valor aproximado del excedente para aplicar el criterio de consumo nuevamente

                            //potenciaExcedente = (potenciaInversor - consumoActual) + _cargaTotal;

                            //simplificando: 
                            #endregion

                            _potenciaExcedente = _cargaTotal - potenciaActiva;

                            //if (_dispositivos.First(p => p.Nombre.Equals("Filtro")).Estado && Meteo.NebulizadorActivo && _consumoActual >= 400 && _consumoActual <= 550)
                            //{
                            //    _potenciaExcedente = _potenciaExcedente + 400;
                            //}

                            Console.WriteLine("_potenciaExcedente  {0} = _cargaTotal {1} - potenciaActiva {2}", _potenciaExcedente, _cargaTotal, potenciaActiva);

                            _potenciaExcedente = _potenciaExcedente < 0 ? 0 : _potenciaExcedente;

                            _countEsperaIncremento = 0;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Potencia Excedente  :{0} actualizada", _potenciaExcedente);
                            Console.WriteLine("dispositivos.Count  :{0}", _dispositivos.Count());

                            Console.ResetColor();

                            // aqui controlare ademas las bombas de riego
                            #region inestable aún para maneter los estaods
                            if (false)
                            {
                                if (!_cargaManual)
                                {
                                    if (potenciaInversor < 550) //conmuta bombas potencia baja
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Potencia Inversor Baja   :{0}  conmuta a bomba 1", potenciaInversor);
                                        Console.ResetColor();
                                        Thread.Sleep(2000);
                                        _dispositivos.First(p => p.Nombre.Equals("Bomba02")).CambiaEstado(false);
                                    }
                                    else if (_consumoActual > 2000 && _cargaTotal < 30) //conmuta bombas prendido el termo mod: _consumoActual > 1500
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Potencia Inversor Baja   :{0}  conmuta a bomba 1", potenciaInversor);
                                        Console.ResetColor();
                                        Thread.Sleep(2000);
                                        _dispositivos.First(p => p.Nombre.Equals("Bomba02")).CambiaEstado(false);
                                    }
                                    else if (Meteo.NebulizadorActivo)
                                    {
                                        _dispositivos.First(p => p.Nombre.Equals("Bomba02")).CambiaEstado(true);
                                    }
                                    else if (potenciaActiva < -200)
                                    {
                                        _dispositivos.First(p => p.Nombre.Equals("Bomba02")).CambiaEstado(true);
                                    }
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine("omite bombas", potenciaInversor);
                                Console.ResetColor();
                            }
                            #endregion



                            if (_potenciaExcedente > 0)
                            {
                                List<Dispositivo> listaON = BuscaDispositivos(_potenciaExcedente).ToList();
                                List<Dispositivo> ListaOFF = _dispositivos.Except(listaON).Where(p => !p.Bloqueo).ToList();

                                listaON.ForEach(a => a.CambiaEstado(true));
                                ListaOFF.ForEach(a => a.CambiaEstado(false));
                            }
                            else
                            {
                                _dispositivos.Where(p => !p.Bloqueo).ToList().ForEach(a => a.CambiaEstado(false));
                            }
                        }
                        else
                        {
                            _countEsperaIncremento++;

                            //if (Math.Abs( potenciaActiva ) > 400)
                            //{
                            //    _countEsperaIncremento++;
                            //}
                        }
                    }
                    else
                    {
                        _countEsperaIncremento = 0;
                    }

                    _cargaTotal = 0;
                    
                    //_cargaTotal += _dispositivos.First(p => p.Nombre.Equals("Termo")).Estado ? _dispositivos.First(p => p.Nombre.Equals("Termo")).PowerConsumption : 0;
                    //_cargaTotal += _dispositivos.First(p => p.Nombre.Equals("AmbarInvernadero")).Estado ? _dispositivos.First(p => p.Nombre.Equals("AmbarInvernadero")).PowerConsumption : 0;
                    //_cargaTotal += _dispositivos.First(p => p.Nombre.Equals("LedRio")).Estado ? _dispositivos.First(p => p.Nombre.Equals("LedRio")).PowerConsumption : 0;
                    //_cargaTotal += _dispositivos.First(p => p.Nombre.Equals("LedPileta")).Estado ? _dispositivos.First(p => p.Nombre.Equals("LedPileta")).PowerConsumption : 0;
                    //_cargaTotal += _dispositivos.First(p => p.Nombre.Equals("LedTablero")).Estado ? _dispositivos.First(p => p.Nombre.Equals("LedTablero")).PowerConsumption : 0;
                    //_cargaTotal += _dispositivos.First(p => p.Nombre.Equals("AmbarTablero")).Estado ? _dispositivos.First(p => p.Nombre.Equals("AmbarTablero")).PowerConsumption : 0;
                    //_cargaTotal += _dispositivos.First(p => p.Nombre.Equals("AmbarPileta")).Estado ? _dispositivos.First(p => p.Nombre.Equals("AmbarPileta")).PowerConsumption : 0;
                    //_cargaTotal += _dispositivos.First(p => p.Nombre.Equals("Filtro")).Estado ? _dispositivos.First(p => p.Nombre.Equals("Filtro")).PowerConsumption : 0;


                    _cargaTotal = _dispositivos.Where(a => !a.Bloqueo).Where(b=>b.Estado).Sum(p => p.PowerConsumption);

                    foreach (var d in _dispositivos)
                    {
                        sb.AppendFormat(string.Format("{0}:{1}| ", d.Nombre, d.Estado ? "ON" : "OFF"));
                    }

                    sb.Append(Environment.NewLine);
                    sb.AppendFormat("Carga Manual        :{0}{1}", _cargaManual, Environment.NewLine);
                    sb.AppendFormat("Potencia Inversor   :{0}{1}", potenciaInversor, Environment.NewLine);
                    sb.AppendFormat("Consumo Actual      :{0}{1}", _consumoActual, Environment.NewLine);
                    sb.AppendFormat("Potencia Excedente  :{0}{1}", _potenciaExcedente, Environment.NewLine);
                    sb.AppendFormat("Potencia por cargas :{0}{1}", _cargaTotal, Environment.NewLine);

                    MantieneEstado(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException);
            }
            return sb.ToString();
        }
        public bool Entre( int num, int lower, int upper )
        {
            bool valor = false;
            if (num >= 0) 
                valor= lower <= num && num <= upper;
            else
                valor = lower >= num && num >= upper;

            return valor;
        }

        List<Dispositivo> BuscaDispositivos(int power)
        {
            var l = new List<Dispositivo>();
            var ll = new List<List<Dispositivo>>();

            for (int i = power + _toleranciaBusquedaNegativa; i <= power + _toleranciaBusquedaPositiva; i++)
            {
                var dis = GetDispositivos(i);

                if (dis != null)
                {
                    if (dis.Count > 0)
                    {
                        ll.Add(dis);
                        if (ll.Count > 0) break;
                    }
                }
            }

            if (ll.Count > 0)
            {
                var ordenanda = ll.OrderBy(p => p.Sum(aa => aa.PowerConsumption));

                if (ordenanda != null)
                {
                    l = ordenanda.FirstOrDefault();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    l.ForEach(p => Console.Write("{0} ", p));
                    Console.WriteLine("suma aproximada {0}", l.Sum(s => s.PowerConsumption));
                    Console.ResetColor();
                }
            }
            else
            {
                l.AddRange(_dispositivos.Where(p=>!p.Bloqueo));
            }

            return l;
        }
        List<Dispositivo> GetDispositivos(int power)
        {
            var lista = new List<List<Dispositivo>>();

            var participantes = _dispositivos.Where(p => !p.Bloqueo).ToList();

            foreach (var s in GetCombinations(participantes, power, ""))
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
            for (int i = 0; i < lista.Count; i++)
            {
                int suma = lista[i].Sum(s => s.Ponderacion);
                if (suma > sumaP)
                {
                    sumaP = suma;
                    id = i;
                }
            }


            return lista.Count > 0 ? lista[id] : null;
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


    }
}
