# MagicMessagebus
*Add publish/subscribe power to your .NET solution. Like magic.*
 
  1. Create a message
   
    public class HelloWorld : IMagicMessage
    {
	    /* your stuff goes here */
	}

2. Publish that message

	    messagebus.Publish(new HelloWorld());

3. Subcribe to that message

	    public static void Subscribe(FooMessage message)
	    {
		    /* anything you need done goes here */
	    }

4. 💰
