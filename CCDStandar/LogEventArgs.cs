using System;
using System.Collections.Generic;
using System.Text;

namespace CCDStandar
{
    public class LogEventArgs : EventArgs
    {
        private readonly string _input;

        public LogEventArgs(string input)
        {
            _input = input;
        }
        public string data
        {
            get { return _input; }
        }
    }
}
