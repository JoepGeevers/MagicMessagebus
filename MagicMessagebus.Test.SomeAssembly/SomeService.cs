namespace MagicMessagebusTest.SomeAssembly
{
    using System;
    using System.Net;

    public class SomeService
    {
        public static Guid RandomId { get; private set; }

        public static HttpStatusCode Subscribe(SomeMessage message)
        {
            RandomId = message.RandomId;

            return HttpStatusCode.OK;
        }
    }
}
