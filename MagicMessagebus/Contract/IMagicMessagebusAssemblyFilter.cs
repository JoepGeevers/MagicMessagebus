namespace MagicMessagebus.Contract
{
    using System.Reflection;

    public interface IMagicMessagebusAssemblyFilter
    {
        bool ScanForSubcriptions(Assembly assembly);
    }
}