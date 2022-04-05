namespace MagicMessagebus.Implementation.Test
{
    using System.Threading;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ninject;
    using NSubstitute;

    using Contract;
    using Implementation;
    using MagicMessagebusTest.SomeAssembly;

    [TestClass]
    public class AssemblyFilterTests
    {
        [TestCleanup()]
        public void Cleanup()
        {
            MagicMessagebus.Map = null;
        }

        [TestMethod]
        public void WhenNinjectConstructsTheMagicMessagbus_WithoutBindingForAssemblyFilter_DefaultAssemblyFilterIsUsed()
        {
            // arrange
            var kernel = new StandardKernel();
            kernel.Bind<IMagicMessagebus>().To<MagicMessagebus>().InSingletonScope();

            // act
            var messagebus = kernel.Get<IMagicMessagebus>();

            // assert
            Assert.IsNotNull(messagebus);
            Assert.IsInstanceOfType(messagebus, typeof(IMagicMessagebus));

            var implementation = messagebus as MagicMessagebus;

            Assert.IsNotNull(implementation);
            Assert.IsTrue(implementation.assemblyFilter is DefaultAssemblyFilter);
        }

        [TestMethod]
        public void WhenServiceProviderConstructsTheMagicMessagbus_WithoutBindingForAssemblyFilter_DefaultAssemblyFilterIsUsed()
        {
            // arrange
            var collection = new ServiceCollection();
            collection.AddTransient<IMagicMessagebus, MagicMessagebus>();
            var provider = collection.BuildServiceProvider();

            // act
            var messagebus = provider.GetService<IMagicMessagebus>();

            // assert
            Assert.IsNotNull(messagebus);
            Assert.IsInstanceOfType(messagebus, typeof(IMagicMessagebus));

            var implementation = messagebus as MagicMessagebus;

            Assert.IsNotNull(implementation);
            Assert.IsTrue(implementation.assemblyFilter is DefaultAssemblyFilter);
        }

        [TestMethod]
        public void WhenMagicMessagbusIsConstructedWithoutDependencyInjectionFramework_WithoutOptionalArgumentForAssemblyFilter_DefaultAssemblyFilterIsUsed()
        {
            // arrange

            // act
            var implementation = new MagicMessagebus();

            // assert
            Assert.IsTrue(implementation.assemblyFilter is DefaultAssemblyFilter);
        }

        [TestMethod]
        public void WhenNinjectConstructsTheMagicMessagbus_WithBindingForCustomAssemblyFilter_CustomAssemblyFilterIsUsed()
        {
            // arrange
            var kernel = new StandardKernel();
            kernel.Bind<IMagicMessagebus>().To<MagicMessagebus>().InSingletonScope();
            kernel.Bind<IMagicMessagebusAssemblyFilter>().To<SomeCustomAssemblyFilter>().InSingletonScope();

            // act
            var messagebus = kernel.Get<IMagicMessagebus>();

            // assert
            Assert.IsNotNull(messagebus);
            Assert.IsInstanceOfType(messagebus, typeof(IMagicMessagebus));

            var implementation = messagebus as MagicMessagebus;

            Assert.IsNotNull(implementation);
            Assert.IsTrue(implementation.assemblyFilter is SomeCustomAssemblyFilter);
        }

        [TestMethod]
        public void WhenServiceProviderConstructsTheMagicMessagbus_WithBindingForCustomAssemblyFilter_CustomAssemblyFilterIsUsed()
        {
            // arrange
            var collection = new ServiceCollection();
            collection.AddTransient<IMagicMessagebus, MagicMessagebus>();
            collection.AddTransient<IMagicMessagebusAssemblyFilter, SomeCustomAssemblyFilter>();
            var provider = collection.BuildServiceProvider();

            // act
            var messagebus = provider.GetService<IMagicMessagebus>();

            // assert
            Assert.IsNotNull(messagebus);
            Assert.IsInstanceOfType(messagebus, typeof(IMagicMessagebus));

            var implementation = messagebus as MagicMessagebus;

            Assert.IsNotNull(implementation);
            Assert.IsTrue(implementation.assemblyFilter is SomeCustomAssemblyFilter);
        }

        [TestMethod]
        public void WhenMagicMessagbusIsConstructedWithoutDependencyInjectionFramework_WithOptionalArgumentForCustomAssemblyFilter_CustomAssemblyFilterIsUsed()
        {
            // arrange

            // act
            var implementation = new MagicMessagebus(new SomeCustomAssemblyFilter());

            // assert
            Assert.IsTrue(implementation.assemblyFilter is SomeCustomAssemblyFilter);
        }

        [TestMethod]
        public void WhenAssemblyFilterSaysToScanAssembly_SubscribeMethodIsCalled()
        {
            // arrange
            var filter = Substitute.For<IMagicMessagebusAssemblyFilter>();
            filter
                .ScanForSubcriptions(null)
                .ReturnsForAnyArgs(true);

            IMagicMessagebus messagebus = new MagicMessagebus(filter);
            var message = new SomeMessage();

            // act
            messagebus.Publish(message);

            Thread.Sleep(10);

            // assert
            Assert.AreEqual(message.RandomId, SomeService.RandomId);
        }

        [TestMethod]
        public void WhenAssemblyFilterSaysNotToScanAssembly_SubscribeMethodIsNotCalled()
        {
            // arrange
            var filter = Substitute.For<IMagicMessagebusAssemblyFilter>();
            filter
                .ScanForSubcriptions(null)
                .ReturnsForAnyArgs(false);

            IMagicMessagebus messagebus = new MagicMessagebus(filter);
            var message = new SomeMessage();

            // act
            messagebus.Publish(message);

            Thread.Sleep(10);

            // assert
            Assert.AreNotEqual(message.RandomId, SomeService.RandomId);
        }
        // if you filter out magicmessabus itself, it still works
    }
}
