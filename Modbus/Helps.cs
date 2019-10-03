using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace ModBus
{
    public static class Helps
    {
        private static void FinalizarInstanciaPrevia()
        {
            var lista = System.Diagnostics.Process.GetProcesses();
            Process currentProcess = Process.GetCurrentProcess();

            var q = (from l in lista
                     where (l.ProcessName.Contains(currentProcess.ProcessName))
                     select l).ToList();
            foreach (var its in q)
            {
                if (its.Id != currentProcess.Id)
                    its.Kill();
            }
        }
        public static bool IsBetween<T>(this T item, T start, T end)
        {
            return Comparer<T>.Default.Compare(item, start) >= 0
                && Comparer<T>.Default.Compare(item, end) <= 0;
        }
        public static string FeiRemCharSICE(string entrada)
        {
            string salida = string.Empty;

            salida = entrada.Replace(':', 'A');//58
            salida = salida.Replace(';', 'B');//59
            salida = salida.Replace('<', 'C');//60
            salida = salida.Replace('=', 'D');//61
            salida = salida.Replace('>', 'E');//62
            salida = salida.Replace('?', 'F');//63

            return salida;
        }
        public static string FeiCharSICE(string entrada)
        {
            //byte[] bt = new byte[1];
            //bt[0] = entrada;
            string salida = string.Empty;

            salida = entrada.Replace('A', ':');//58
            salida = salida.Replace('B', ';');//59
            salida = salida.Replace('C', '<');//60
            salida = salida.Replace('D', '=');//61
            salida = salida.Replace('E', '>');//62
            salida = salida.Replace('F', '?');//63

            return salida;
        }

        public static T DeserializarTo<T>(string xmlSerializado)
        {
            try
            {
                var xmlSerz = new XmlSerializer(typeof(T));

                using (var strReader = new StringReader(xmlSerializado))
                {
                    object obj = xmlSerz.Deserialize(strReader);
                    return (T)obj;
                }
            }
            catch { return default(T); }
        }
        //Serializar a XML (UTF-16) un objeto cualquiera
        public static string SerializarToXml(this object obj)
        {
            try
            {
                var strWriter = new StringWriter();
                var serializer = new XmlSerializer(obj.GetType());

                serializer.Serialize(strWriter, obj);
                string resultXml = strWriter.ToString();
                strWriter.Close();

                return resultXml;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static T DeserializarTo<T>(byte[] bXmlSerializado)
        {
            string xmlSerializado = Unzip(bXmlSerializado);
            try
            {
                var xmlSerz = new XmlSerializer(typeof(T));

                using (var strReader = new StringReader(xmlSerializado))
                {
                    object obj = xmlSerz.Deserialize(strReader);
                    return (T)obj;
                }
            }
            catch { return default(T); }
        }
        //Serializar a XML (UTF-16) un objeto cualquiera
        public static byte[] SerializarToXmlZip(this object obj)
        {
            try
            {
                var strWriter = new StringWriter();
                var serializer = new XmlSerializer(obj.GetType());

                serializer.Serialize(strWriter, obj);
                string resultXml = strWriter.ToString();
                strWriter.Close();

                return Zip(resultXml);
            }
            catch
            {
                return new byte[0];
            }
        }


        public static void CopyTo(Stream src, Stream dest)
        {
            var bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        #region HexToByte
        /// <summary>
        /// method to convert hex string into a byte array
        /// </summary>
        /// <param name="msg">string to convert</param>
        /// <returns>a byte array</returns>
        public static byte[] HexToByte(string msg)
        {
            byte[] comBuffer = new byte[msg.Length / 2];
            try
            {
                //remove any spaces from the string
                msg = msg.Replace(" ", "");
                //create a byte array the length of the
                //divided by 2 (Hex is 2 characters in length)
                //loop through the length of the provided string
                for (int i = 0; i < msg.Length; i += 2)
                    //convert each set of 2 characters to a byte
                    //and add to the array
                    comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
                //return the array
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return comBuffer;
        }
        #endregion
        #region ByteToHex
        /// <summary>
        /// method to convert a byte array into a hex string
        /// </summary>
        /// <param name="comByte">byte array to convert</param>
        /// <returns>a hex string</returns>
        public static string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            //return the converted value
            return builder.ToString().ToUpper();
        }
        #endregion

        public static int CrcCcittXModem(byte[] data)
        {
            unchecked
            {
                short crc = 0;

                for (int a = 0; a < data.Length; a++)
                {
                    crc ^= (short)(data[a] << 8);
                    for (int i = 0; i < 8; i++)
                    {
                        if ((crc & 0x8000) != 0)
                            crc = (short)((crc << 1) ^ 0x1021);
                        else
                            crc = (short)(crc << 1);
                    }
                }

                return crc & 0xffff;
            }
        }

        public static string ToHex(string data)
        {
            return data.Aggregate(string.Empty, (current, c) => current + ((int)c).ToString("X"));
        }

        /// <summary>
        /// Funcion que retorna la posicion minima de una matriz entregando el valor menor de la posicion.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static int GetPosMinimoArreglo(Collection<bool[]> bits)
        {
            var minimo = new int[bits.Count];
            for (var i = 0; i < bits.Count; i++)
            {
                for (var j = 0; j < bits[0].Length; j++)
                {
                    if (bits[i][j])
                    {
                        minimo[i] = j;
                        break;
                    }
                }
            }
            var min = int.MaxValue;
            foreach (int t in minimo)
            {
                if (t != 0 && t < min)
                {
                    min = t;
                }
            }
            return min;
        }

        /// <summary>
        ///  Funcion que retorna la posicion Maxima de una matriz entregando el valor mayor de la posicion.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static int GetMaximo(Collection<bool[]> bits)
        {
            var maximo = new int[bits.Count];
            for (var i = 0; i < bits.Count; i++)
            {
                for (var j = bits[0].Length - 1; j > 0; j--)
                {
                    if (bits[i][j])
                    {
                        maximo[i] = j;
                        break;
                    }
                }
            }
            return maximo.Concat(new[] { int.MinValue }).Max();
        }
        public static bool ValidateBinString(string binary)
        {
            return binary.All(t => t.Equals('0') || t.Equals('1'));
        }
        public static bool ValidDataIntegrity(string[] data)
        {
            int number;
            bool resul = false;
            foreach (var s in data)
            {
                resul = int.TryParse(s, out number);
                if (!resul)
                {
                    break;
                }
            }
            return resul;
        }
        public static bool[] StringBinToArrayBool(string strBin)
        {
            var bin = new bool[strBin.Length];
            for (var i = 0; i < strBin.Length; i++)
            {
                bin[i] = Convert.ToBoolean(Convert.ToInt32(strBin[i].ToString(CultureInfo.InvariantCulture)));
            }
            return bin;
        }
        public static string Hex2Bin(string value)
        {
            return Convert.ToString(Convert.ToInt32(value, 16), 2).PadLeft(value.Length * 4, '0');
        }
        public static string Dec2Bin(string value)
        {
            return Convert.ToString(Convert.ToInt32(value), 2).PadLeft(8, '0');
        }

        public static string DoubleToBinaryString(double d)
        {
            return Convert.ToString(System.BitConverter.DoubleToInt64Bits(d), 2);
        }
        public static double BinaryStringToDouble(string s)
        {
            return System.BitConverter.Int64BitsToDouble(Convert.ToInt64(s, 2));
        }
  
    }
}