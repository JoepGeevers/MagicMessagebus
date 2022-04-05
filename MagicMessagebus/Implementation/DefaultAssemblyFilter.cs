namespace MagicMessagebus.Implementation
{
    using System.Linq;
    using System.Reflection;
 
    using Contract;

    public class DefaultAssemblyFilter : IMagicMessagebusAssemblyFilter
    {
        private readonly string[] blacklist = new [] {
            "System",
            "Microsoft",
            "netstandard",
            "Antlr3",
            "WebGrease",
            "DotNetOpenAuth",
        };

        public bool ScanForSubcriptions(Assembly assembly)
        {
            return false == this.blacklist
                .Where(assembly.FullName.StartsWith)
                .Any();
        }
    }
}