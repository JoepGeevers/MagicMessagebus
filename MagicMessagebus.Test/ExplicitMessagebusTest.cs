namespace MagicMessagebus.Implementation.Test
{
    using System;
	using System.Diagnostics;

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
                .WithSubscription<ITicketService, TicketCreated>((s, m) => s.BookTicket(m.TicketId));

            collection
                .AddWhatsub()
                .Subscribe<TicketCreated>()
                    .To<ITicketService>((s, m) => s.BookTicket(m.TicketId));

			var provider = collection.BuildServiceProvider();
            
            var whatsub = provider.GetService<Whatsub>();

            var hello = new TicketCreated
            {
                TicketId = "hello",
            };

            whatsub.Publish(hello);

            //fakeWriteToTheConsole
            //    .Received(1)
            //    .BookTicket(hello.TicketId);

            Whatsub.Subscribe<TicketCreated>(m => Debugger.Break());
            whatsub.Publish(new TicketCreated());
        }
    }

    public class TicketCreated : Contract.IMagicMessage
    {
        public string TicketId { get; set; }
    }
    
    public class WriteToTheConsole : ITicketService
    {
        public void BookTicket(string line)
        {
            Console.WriteLine(line);
        }
	}

    public interface ITicketService
    {
        void BookTicket(string message);
	}
}