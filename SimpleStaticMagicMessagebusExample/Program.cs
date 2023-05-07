namespace SimpleStaticMagicMessagebusExample
{
    using MagicMessagebus.Implementation;

    public class Program
    {
        static void Main()
        {
            Whatsub.Subscribe<WelcomeMessage>(DoWork);
        }

        static void DoWork(WelcomeMessage message)
        {
        }
    }


    public class WelcomeMessage
    {
        public string Body { get; set; }
    }
}