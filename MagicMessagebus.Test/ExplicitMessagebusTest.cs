namespace MagicMessagebus.Implementation.Test
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Implementation;
    using NSubstitute;
    using System.ComponentModel.DataAnnotations;
    using System;

    [TestClass]
    public class ExplicitMessagebusTest
    {
        [TestMethod]
        public void MyTestMethod()
        {
            var collection = new ServiceCollection();

            var fakeWriteToTheConsole = Substitute.For<IWriteToTheConsole>();

            collection
                .AddSingleton(x => fakeWriteToTheConsole)
                .AddMagicMessagebus()
                .WithSubscription<IWriteToTheConsole, HelloWorld>((s, m) => s.WriteLine(m.Message));

            var provider = collection.BuildServiceProvider();

            var messagebus = provider.GetService<IWhatsub>();

            var hello = new HelloWorld
            {
                Message = "hello",
            };

            messagebus.Publish(hello);

            fakeWriteToTheConsole
                .Received(1)
                .WriteLine(hello.Message);
        }
    }

    public class HelloWorld : Contract.IMagicMessage
    {
        public string Message { get; set; }
    }
    
    public class WriteToTheConsole : IWriteToTheConsole
    {
        public void WriteLine(string line) => Console.WriteLine(line);
    }

    public interface IWriteToTheConsole
    {
        void WriteLine(string message);
    }
}