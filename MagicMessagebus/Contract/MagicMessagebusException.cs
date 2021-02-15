namespace MagicMessagebus.Contract
{
    using System;

    public class MagicMessagebusException : Exception
    {
        public MagicMessagebusException(string message) : base(message) { }
    }
}