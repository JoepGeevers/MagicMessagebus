namespace MagicMessagebusTest.SomeAssembly
{
    using System;

    using MagicMessagebus.Contract;

    public class SomeMessage : IMagicMessage
    {
        public Guid RandomId = Guid.NewGuid();
    }
}
