namespace MagicMessagebus.Implementation.Test
{
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ninject;

    using Contract;

    [TestClass]
    public class MagicMessagebusTests
    {
        [TestMethod]
        public void AnyStaticMethodThatExpectsAnIMagicMessageWillBeCalledWhenCorrespondingMessageIsPublished()
        {
            // arrange
            var messagebus = new MagicMessagebus(null, null);
            var message = new MerryChristmas(8765);

            // act
            messagebus.Publish(message);
            Thread.Sleep(10); // todo: make it work with no sleep by collecting all the methods an invoke them without interruptions

            // assert
            Assert.AreEqual(8765, StaticKissReceiver.KissesReceived);
            Assert.AreEqual(0, InstanceKissReceiver.KissesReceived);
        }

        [TestMethod]
        public void SubscribeMethodsFoundInInterfacesUseNinjectToResolveImplementationAndAreCalled()
        {
            // arrange
            var kernel = new StandardKernel();
            kernel.Bind<IKissReceiver>().To<InstanceKissReceiver>();

            var messagebus = new MagicMessagebus(null, kernel);
            var message = new MerryChristmas(7654);

            // act
            messagebus.Publish(message);
            Thread.Sleep(10); // todo: make it work with no sleep by collecting all the methods an invoke them without interruptions

            // assert
            Assert.AreEqual(7654, InstanceKissReceiver.KissesReceived);
            Assert.AreEqual(0, SomeOtherInstanceKissReceiver.KissesReceived);
        }

        [TestMethod]
        public void BindingMagicMessagebusWithoutBindingForIErrorTrackerStillCreatesMessagebus()
        {
            // arrange
            var kernel = new StandardKernel();
            kernel.Bind<IMagicMessagebus>().To<MagicMessagebus>();

            // act
            var messagebus = kernel.Get<IMagicMessagebus>();

            // assert
            Assert.IsNotNull(messagebus);
            Assert.IsInstanceOfType(messagebus, typeof(IMagicMessagebus));
        }
    }

    public static class StaticKissReceiver
    {
        public static int KissesReceived { get; set; } = 0;

        public static void Subscribe(MerryChristmas wish)
        {
            KissesReceived += wish.Kisses;
        }
    }

    public class InstanceKissReceiver : IKissReceiver
    {
        public static int KissesReceived { get; set; } = 0;

        public void Subscribe(MerryChristmas wish)
        {
            KissesReceived += wish.Kisses;
        }
    }

    public class SomeOtherInstanceKissReceiver : IKissReceiver
    {
        public static int KissesReceived { get; set; } = 0;

        public void Subscribe(MerryChristmas wish)
        {
            KissesReceived = wish.Kisses;
        }
    }

    public interface IKissReceiver
    {
        void Subscribe(MerryChristmas wish);
    }

    public class MerryChristmas : IMagicMessage
    {
        public MerryChristmas(int kisses)
        {
            this.Kisses = kisses;
        }

        public int Kisses { get; set; }
    }
}
