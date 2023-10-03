namespace MagicMessagebus.Implementation.Test
{
    using System;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSubstitute;

    using Whatsub;

    [TestClass]
    public class ExplicitMessagebusTest
    {
        [TestMethod]
        public void MyTestMethod()
        {
            var collection = new ServiceCollection();

            var fakeWriteToTheConsole = Substitute.For<ITicketService>();

            collection.AddSingleton(x => fakeWriteToTheConsole);
            collection.AddSingleton(x => fakeWriteToTheConsole);

            collection
                .AddWhatsub()
                .WithSubscription<ITicketService, TicketCreated>((s, m) => s.BookTicket(m.Message));

            var provider = collection.BuildServiceProvider();
            
            var whatsub = provider.GetService<Whatsub>();

            var hello = new TicketCreated
            {
                Message = "hello",
            };

            whatsub.Publish(hello);

            fakeWriteToTheConsole
                .Received(1)
                .BookTicket(hello.Message);
        }
    }

    public class TicketCreated : Contract.IMagicMessage
    {
        public string Message { get; set; }
    }
    
    public class WriteToTheConsole : ITicketService
    {
        public Status BookTicket(string line)
        {
            Console.WriteLine(line);

            return Status.NoContent;
        }
    }

    public interface ITicketService
    {
        Status BookTicket(string message);
    }
}