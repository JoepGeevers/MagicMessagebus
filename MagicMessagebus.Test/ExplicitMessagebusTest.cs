namespace MagicMessagebus.Implementation.Test
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Implementation;
    using NSubstitute;
    using System.ComponentModel.DataAnnotations;

    [TestClass]
    public class ExplicitMessagebusTest
    {
        [TestMethod]
        public void MyTestMethod()
        {
            var collection = new ServiceCollection();

            var fakeWriteToTheConsole = Substitute.For<IWriteToTheConsole>();

            collection
                .AddSingleton<IWriteToTheConsole>(x => fakeWriteToTheConsole)
                .AddMagicMessagebus()
                .WithSubscription<IWriteToTheConsole, HelloWorld>((s, m) => s.Subscribe(m));

            var provider = collection.BuildServiceProvider();

            var messagebus = provider.GetService<MagicMessagebus>();

            var hello = new HelloWorld();

            messagebus.PublishV2 (hello);

            fakeWriteToTheConsole
                .Received(1)
                .Subscribe(hello);
        }
    }

    public class HelloWorld : Contract.IMagicMessage
    {
    }
    
    public class WriteToTheConsole : IWriteToTheConsole
    {
        public void Subscribe(HelloWorld m)
        {
            throw new System.NotImplementedException();
        }
    }

    public interface IWriteToTheConsole
    {
        void Subscribe(HelloWorld m);
    }
}