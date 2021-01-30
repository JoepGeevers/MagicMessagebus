
# MagicMessagebus
***Add publish/subscribe power to your .NET solution. Like magic.***

## The simplest example that could possibly work
*(Ridiculously simple, but probably not the best solution for larger applications)*

  1. Create a message
   
         public class HelloWorld : IMagicMessage
         {
             /* your stuff goes here */
         }

2. Publish that message anywhere in your code

       var messagebus = new MagicMessagebus();
       messagebus.Publish(new HelloWorld());

3. Subcribe to that message anywhere in your code (static though)

       public static void Subscribe(HelloWorld message)
       {
           /* anything you need done goes here */
       }

4. 💰

## The simplest example with Dependency Injection
*(Using Ninject as the Dependency Injector. Others will follow)*

1. Bind the MagicMessagebus

       kernel.Bind<IMagicMessagebus>().To<MagicMessagebus>().InSingletonScope();

2. Inject into your service

       private readonly IMagicMessagebus messagebus;

       public FooService(IMagicMessagebus messagebus)
       {
           this.messagebus = messagebus;
       }

3. Publish your message

       this.messagebus.Publish(new HelloWorld());

4. Subcribe to the  message in any interface that's bound in Ninject

       void Subscribe(HelloWorld message);
       
5. Implement that service

       public void Subscribe(HelloWorld message)
       {
           /* anything you need done goes here */
       }

6. 💰💰
