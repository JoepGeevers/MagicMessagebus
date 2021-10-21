using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            IReceiver<IMessage> foo = new ConsoleWriter();

            var type = foo.GetType();

            var types = type.GenericTypeArguments;
        }
    }

    public class ConsoleWriter
    {
        //public void Receive(HelloWorldMessage message) { }
    }

    public interface IWriter : IReceiver<HelloWorldMessage> { }

    public class HelloWorldMessage : IMessage { }

    public interface IReceiver<out TMessage> where TMessage : IMessage
    {
        //void Receive(TMessage message);
    }

    public interface IMessage { }
}
