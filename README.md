
# MagicMessagebus
*Add publish/subscribe power to your .NET solution. Like magic.*

The simplest example that could possibly work (but for the love of god, please don't)

  1. Create a message
   
         public class HelloWorld : IMagicMessage
         {
             /* your stuff goes here */
         }

2. Publish that message anywhere in your code

       var messagebus = new MagicMessagebus();
       messagebus.Publish(new HelloWorld());

3. Subcribe to that message anywhere in your code

       public static void Subscribe(HelloWorld message)
       {
           /* anything you need done goes here */
       }

4. 💰
