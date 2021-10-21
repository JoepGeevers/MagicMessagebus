namespace GenericsProblem
{
    public static class Program
    {
        static void Main()
        {
            IReceiver<IMessage> foo = new ConsoleWriter();

            var type = foo.GetType();
        }
    }

    public class ConsoleWriter : IWriter
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