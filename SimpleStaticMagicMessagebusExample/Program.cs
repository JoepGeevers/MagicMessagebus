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
            var messagebus = MagicMessagebus.Create();

            var message = new WelcomeMessage
            {
                Body = "Hello, World!",
            };

            messagebus.Publish(message);

            Thread.Sleep(100); // It's fire and forget, so we need the message to get through before the application finishes
        }
    }

    public static class Audience
    {
        public static void Subscribe(WelcomeMessage message)
        {
            Console.WriteLine(message.Body);
        }
    }

    public class WelcomeMessage : IMagicMessage
    {
        public string Body { get; set; }
    }
}