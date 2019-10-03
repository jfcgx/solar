using System;

namespace ModBus
{
    public class InputDataEventArgs : EventArgs
    {
        private readonly byte[] _input;

        public InputDataEventArgs(byte[] input)
        {
            _input = input;
        }
        public byte[] BusinessObject
        {
            get { return _input; }
        }
    }
}
