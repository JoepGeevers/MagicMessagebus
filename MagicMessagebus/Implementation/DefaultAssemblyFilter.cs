namespace MagicMessagebus.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
 
    using Contract;

    public class DefaultAssemblyFilter : IMagicMessagebusAssemblyFilter
    {
        private readonly List<string> blacklist = new[]
            {
                "System",
                "Microsoft",
                "netstandard",
                "Antlr3",
                "WebGrease",
                "DotNetOpenAuth",
                "MailJetClient",
                "Facebook",
            }
            .SelectMany((l) => new[] { ',', '.' }, (l, r) => l + r)
            .ToList();

        public bool ScanForSubcriptions(Assembly assembly)
        {
            return false == this.blacklist
                .Where(assembly.FullName.StartsWith)
                .Any();
        }
    }
}