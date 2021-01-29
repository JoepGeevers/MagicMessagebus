namespace SimpleStaticMagicMessagebusExample
{
    using System;

    using MagicMessagebus.Contract;
    using MagicMessagebus.Implementation;

    public class Program
    {
        static void Main()
        {
            var messagebus = new MagicMessagebus();

            var message = new WelcomeMessage
            {
                Body = "Hello, World!",
            };

            messagebus.Publish(message);
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