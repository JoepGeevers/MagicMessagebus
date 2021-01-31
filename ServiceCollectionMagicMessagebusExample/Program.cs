namespace ServiceCollectionMagicMessagebusExample
{
    using MagicMessagebus.Contract;
    using MagicMessagebus.Implementation;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading;

    class Program
    {
        static void Main()
        {
            var provider = new ServiceCollection()
                .AddSingleton<IWakeUpSometimes, ComputerProgram>()
                .AddSingleton<IWriteToTheConsole, ConsoleWriter>()
                .AddSingleton<IMagicMessagebus, MagicMessagebus>()
                .BuildServiceProvider();

            var waker = provider.GetService<IWakeUpSometimes>();

            waker.WakeUp();

            Thread.Sleep(100); // It's fire and forget, so we need the message to get through before the application finishes
        }
    }

    public interface IWakeUpSometimes
    {
        void WakeUp();
    }

    public interface IWriteToTheConsole
    {
        void Subscribe(HelloWorld message);
    }

    public class ComputerProgram : IWakeUpSometimes
    {
        private readonly IMagicMessagebus messagebus;

        public ComputerProgram(IMagicMessagebus messagebus)
        {
            this.messagebus = messagebus;
        }

        public void WakeUp()
        {
            var message = new HelloWorld
            {
                Body = "Hello, World!",
            };

            this.messagebus.Publish(message);
        }
    }

    public class HelloWorld : IMagicMessage
    {
        public string Body { get; set; }
    }

    public class ConsoleWriter : IWriteToTheConsole
    {
        public void Subscribe(HelloWorld message)
        {
            Console.WriteLine(message.Body);
        }
    }
}
