using MagicMessagebus.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MagicMessagebus.Test
{
    [TestClass]
    public class DefaultAssemblyFilterTests
    {
        [TestMethod]
        public void AssembliesThatStartWithMicrosoftShouldNotBeScanned()
        {
            // arrange
            var assembly = Assembly.GetAssembly(typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection));
            var filter = new DefaultAssemblyFilter();

            // act
            var result = filter.ScanForSubcriptions(assembly);

            // assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AssembliesThatStartWithMessagebusShouldBeScanned()
        {
            // arrange
            var assembly = Assembly.GetAssembly(typeof(DefaultAssemblyFilter));
            var filter = new DefaultAssemblyFilter();

            // act
            var result = filter.ScanForSubcriptions(assembly);

            // assert
            Assert.IsTrue(result);
        }
    }
}
