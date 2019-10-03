﻿
using CCD.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CCD
{
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
        private bool _bomba01ComoCarga;
        public bool BombaManual
        {
            get => _cargaManual;
            set
            {
                _cargaManual = value;
                _dispositivos.FirstOrDefault(p => p.Nombre.Equals("Bomba01")).Manual = value;
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

        public void Inicia()
        {
            _mantieneEstado = true;
            _dispositivos = new List<Dispositivo>();
            Dispositivo AmbarTablero = new Dispositivo("AmbarTablero", 100, 1, "l03ambar", 3, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY);
            Dispositivo AmbarPileta = new Dispositivo("AmbarPileta", 100, 1, "l02ambar", 3, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY);
            Dispositivo LedPileta = new Dispositivo("LedPileta", 30, 1, "l01led", 5, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY);
            Dispositivo LedTablero = new Dispositivo("LedTablero", 90, 1, "l02led", 5, string.Empty, (ConnectionType)Settings.Default.ConnectionType, ModuleType.Basic, Settings.Default.IFTTT_KEY); //porque tenia 100?
            Dispositivo Filtro = new Dispositivo("Filtro", 900, 1, "filtro_piscina", 2, "192.168.0.52", ConnectionType.tasmota, ModuleType.Basic, "1");
            Dispositivo Bomba01 = new Dispositivo("Bomba01", 400, 1, "bomba01", 2, "192.168.0.50", ConnectionType.tasmota, ModuleType.Dual, "1");
            Dispositivo Bomba02 = new Dispositivo("Bomba02", 800, 2, "bomba02", 2, "192.168.0.50", ConnectionType.tasmota, ModuleType.Dual, "2");

            _dispositivos.Add(AmbarTablero);
            _dispositivos.Add(AmbarPileta);
            _dispositivos.Add(LedPileta);
            _dispositivos.Add(LedTablero);
            _dispositivos.Add(Filtro);
            _dispositivos.Add(Bomba01);
            _dispositivos.Add(Bomba02);

            _dispositivos.First(p => p.Nombre.Equals("Bomba02")).Bloqueo = true;
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
                     where cust.Estado && !(cust.Nombre.Equals("Bomba01") || cust.Nombre.Equals("Bomba02"))
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
            foreach (var d in _dispositivos)
            {
                if (!d.Nombre.Equals("Bomba01"))
                    d.CambiaEstado(false);
                else
                    d.CambiaEstado(true);
            }
        }
        public  void EnPausa(bool valor)
        {
            _enPausa = valor;
        }
        public void CheckAlMenosUnaBombaActiva()
        {

            if (!_dispositivos.First(p => p.Nombre.Equals("Bomba01")).Estado && !_dispositivos.First(p => p.Nombre.Equals("Bomba02")).Estado)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ninguna bomba activa conmuta a bomba 1");
                Console.ResetColor();

                _bomba01ComoCarga = false;

                _dispositivos.First(p => p.Nombre.Equals("Bomba01")).Bloqueo = _bomba01ComoCarga ? false : true;

                _dispositivos.First(p => p.Nombre.Equals("Bomba01")).CambiaEstado(true);
                Thread.Sleep(2000);
                _dispositivos.First(p => p.Nombre.Equals("Bomba02")).CambiaEstado(false);
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

                        if (_countEsperaIncremento == _esperaIncremento)
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
                            Console.WriteLine("_potenciaExcedente  {0} = _cargaTotal {1} - potenciaActiva {2}", _potenciaExcedente, _cargaTotal, potenciaActiva);

                            _potenciaExcedente = _potenciaExcedente < 0 ? 0 : _potenciaExcedente;

                            _countEsperaIncremento = 0;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Potencia Excedente  :{0} actualizada", _potenciaExcedente);
                            Console.WriteLine("dispositivos.Count  :{0}", _dispositivos.Count());

                            Console.ResetColor();

                            // aqui controlare ademas las bombas de riego
                            if (!_cargaManual)
                            {
                                if (potenciaInversor < 550) //conmuta bombas prendido el termo// p
                                {
                                    if (!_dispositivos.First(p => p.Nombre.Equals("Bomba01")).Estado)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Potencia Inversor Baja   :{0}  conmuta a bomba 1", potenciaInversor);
                                        Console.ResetColor();
                                        _bomba01ComoCarga = false;
                                        _dispositivos.First(p => p.Nombre.Equals("Bomba01")).CambiaEstado(true);
                                    }
                                    if (_dispositivos.First(p => p.Nombre.Equals("Bomba02")).Estado)
                                    {
                                        Thread.Sleep(2000);
                                        _dispositivos.First(p => p.Nombre.Equals("Bomba02")).CambiaEstado(false);
                                    }
                                }
                                else if (_consumoActual > 1500 && _cargaTotal < 30) //conmuta bombas prendido el termo
                                {
                                    if (_dispositivos.First(p => p.Nombre.Equals("Bomba02")).Estado)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Potencia Inversor Baja   :{0}  conmuta a bomba 1", potenciaInversor);
                                        Console.ResetColor();
                                        _bomba01ComoCarga = false;
                                        _dispositivos.First(p => p.Nombre.Equals("Bomba01")).CambiaEstado(true);
                                        Thread.Sleep(2000);
                                        _dispositivos.First(p => p.Nombre.Equals("Bomba02")).CambiaEstado(false);
                                    }
                                }
                                else if (potenciaActiva < -200)
                                {
                                    _bomba01ComoCarga = true;
                                    _dispositivos.First(p => p.Nombre.Equals("Bomba02")).CambiaEstado(true);
                                }
                                else if (_dispositivos.First(p => p.Nombre.Equals("Filtro")).Estado)
                                {
                                    _bomba01ComoCarga = true;
                                }

                                _dispositivos.First(p => p.Nombre.Equals("Bomba01")).Bloqueo = _bomba01ComoCarga ? false : true;
                            }

                            if (_potenciaExcedente > 0)
                            {
                                //_dispositivos.First(p => p.Nombre.Equals("Bomba01")).Bloqueo = _bomba01ComoCarga ? false : true;

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
                        }
                    }
                    else
                    {
                        _countEsperaIncremento = 0;
                    }

                    _cargaTotal = 0;
                    //if (_bomba01ComoCarga && !(d.Nombre.Equals("Bomba02")))
                    //    _cargaTotal += d.Estado ? d.PowerConsumption : 0;
                    //else
                    //{
                    //    if (!(d.Nombre.Equals("Bomba01")))
                    //    {
                    //        _cargaTotal += d.Estado ? d.PowerConsumption : 0;
                    //    }
                    //}
                    _cargaTotal += _dispositivos.First(p => p.Nombre.Equals("LedPileta")).Estado ? _dispositivos.First(p => p.Nombre.Equals("LedPileta")).PowerConsumption : 0;
                    _cargaTotal += _dispositivos.First(p => p.Nombre.Equals("LedTablero")).Estado ? _dispositivos.First(p => p.Nombre.Equals("LedTablero")).PowerConsumption : 0;
                    _cargaTotal += _dispositivos.First(p => p.Nombre.Equals("AmbarTablero")).Estado ? _dispositivos.First(p => p.Nombre.Equals("AmbarTablero")).PowerConsumption : 0;
                    _cargaTotal += _dispositivos.First(p => p.Nombre.Equals("AmbarPileta")).Estado ? _dispositivos.First(p => p.Nombre.Equals("AmbarPileta")).PowerConsumption : 0;
                    _cargaTotal += _dispositivos.First(p => p.Nombre.Equals("Filtro")).Estado ? _dispositivos.First(p => p.Nombre.Equals("Filtro")).PowerConsumption : 0;
                    if (_bomba01ComoCarga) _cargaTotal += _dispositivos.First(p => p.Nombre.Equals("Bomba01")).Estado ? _dispositivos.First(p => p.Nombre.Equals("Bomba01")).PowerConsumption : 0;
                    //_cargaTotal += Bomba2.Estado ? Bomba2.PowerConsumption : 0;
                    foreach (var d in _dispositivos)
                    {
                        sb.AppendFormat(string.Format("{0}:{1}| ", d.Nombre, d.Estado ? "ON" : "OFF"));


                    }

                    sb.Append(Environment.NewLine);
                    sb.AppendFormat("Bomba 01 como carga :{0}{1}", _bomba01ComoCarga, Environment.NewLine);
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
                l.AddRange(_dispositivos);
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