using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Field
{
    public class TextFields
    {
        private string _contenido;
        public string Contenido { get => _contenido; set => _contenido = value; }

        public TextFields()
        {
            Contenido = string.Empty;
        }

    }
}
