﻿namespace NinjectMagicMessagebusExample
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using MagicMessagebus.Contract;
    using MagicMessagebus.Implementation;
    using Ninject;

    class Program
    {
        static void Main()
        {
            var kernel = new StandardKernel();

            kernel.Bind<IWakeUpSometimes>().To<ComputerProgram>().InSingletonScope();
            kernel.Bind<IWriteToTheConsole>().To<ConsoleWriter>().InSingletonScope();
            kernel.Bind<IMagicMessagebus>().To<MagicMessagebus>().InSingletonScope();

            kernel.Get<IMagicMessagebus>().Subscribe<IWriteToTheConsole, HelloWorldMessage>();

            var subscription1 = new Subscription<IWriteToTheConsole, HelloWorldMessage>();

            var list = new List<Subscription<ISubscriber<IMagicMessage>, IMagicMessage>>(); // new List<Subscription<ISubscriber<IMagicMessage>, IMagicMessage>();

            // waarom kan dit niet? pak dit eens uit en maak het eens simpeler?
            //Subscription<ISubscriber<IMagicMessage>, IMagicMessage> foo = new Subscription<IWriteToTheConsole, HelloWorldMessage>();

            // this worked with the out keyword!!!!!
            List<ISubscriber<IMagicMessage>> lijstje = new List<ISubscriber<IMagicMessage>>
            {
                //new ConsoleWriter(null)
            };

            lijstje.First().Subscribe(new HelloWorldMessage());

            IMagicMessage a = new HelloWorldMessage();

            //list.Add(subscription1);

            var waker = kernel.Get<IWakeUpSometimes>();

            waker.WakeUp();

            Thread.Sleep(100); // It's fire and forget, so we need the message to get through before the application finishes
        }
    }

    public interface IWakeUpSometimes
    {
        void WakeUp();
    }

    public interface IWriteToTheConsole : ISubscriber<HelloWorldMessage>
    {
    }

    public class ComputerProgram : IWakeUpSometimes
    {
        private readonly IMagicMessagebus messagebus;

        public ComputerProgram(IMagicMessagebus messagebus)
        {
            this.messagebus = messagebus;
        }

        public void WakeUp()
        {
            var message = new HelloWorldMessage
            {
                Body = "Hello, World!",
            };

            this.messagebus.Publish(message);
        }
    }

    public class HelloWorldMessage : IMagicMessage
    {
        public string Body { get; set; }
    }

    public class ConsoleWriter : IWriteToTheConsole
    {
        public ConsoleWriter(IMagicMessagebus messagebus)
        {
            //this.Subscribe(this.Foo);
            //this.Subscribe(this.Bar);
        }

        private void Subscribe(Action<HelloWorldMessage> function)
        {
        }

        public void Foo(IMagicMessage message)
        {
        }

        public void Bar(HelloWorldMessage message)
        {
        }

        public HttpStatusCode Subscribe(HelloWorldMessage message)
        {
            throw new NotImplementedException();
        }
    }
}