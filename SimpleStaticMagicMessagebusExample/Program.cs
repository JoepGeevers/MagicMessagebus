namespace SimpleStaticMagicMessagebusExample
{
    using System;
    using System.Threading;

    using MagicMessagebus.Contract;
    using MagicMessagebus.Implementation;

    public class Program
    {
        static void Main()
        {
            Whatsub.Subscribe<WelcomeMessage>((m) => { });

            var message = new WelcomeMessage
            {
                Body = "Hello, World!",
            };

            Thread.Sleep(100); // It's fire and forget, so we need the message to get through before the application finishes
        }
    }


    public class WelcomeMessage
    {
        public string Body { get; set; }
    }
}