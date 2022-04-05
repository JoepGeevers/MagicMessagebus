namespace MagicMessagebus.Implementation.Test
{
    using Contract;
    using Implementation;
    using MagicMessagebusTest.SomeAssembly;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ninject;
    using NSubstitute;
    using System.Threading;

    [TestClass]
    public class AssemblyFilterTests
    {
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
                .Returns(true);

            var messagebus = new MagicMessagebus(filter);
            var message = new SomeMessage();

            // act
            MagicMessagebus.Publish(message);

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
                .Returns(false);

            var messagebus = (IMagicMessagebus)new MagicMessagebus(filter);
            var message = new SomeMessage();

            // act
            messagebus.Publish(message);

            Thread.Sleep(10);

            // assert
            Assert.AreNotEqual(message.RandomId, SomeService.RandomId);
        }

        // that the filter is called and the result is used
        // if you filter out magicmessabus itself, it still works

    }
}
