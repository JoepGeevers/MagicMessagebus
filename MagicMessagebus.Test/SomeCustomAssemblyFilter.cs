namespace MagicMessagebus.Implementation.Test
{
    using System.Reflection;

    using Contract;

    public class SomeCustomAssemblyFilter : IMagicMessagebusAssemblyFilter
    {
        public bool ScanForSubcriptions(Assembly assembly)
        {
            return false;
        }
    }
}
