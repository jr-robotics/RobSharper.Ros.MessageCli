using System.Collections.Generic;
using System.Linq;
using RobSharper.Ros.MessageParser;

namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage
{
    public class PackageDependencyCollector : DefaultRosMessageVisitorListener
    {
        private readonly IEnumerable<string> _ignoredPackages;
        private readonly ISet<string> _dependencies = new HashSet<string>();

        public ISet<string> Dependencies => _dependencies;

        public PackageDependencyCollector(IEnumerable<string> ignoredPackages = null)
        {
            _ignoredPackages = ignoredPackages ?? Enumerable.Empty<string>();
        }
        
        public override void OnVisitRosType(RosTypeInfo typeInfo)
        {
            if (typeInfo.HasPackage && !_ignoredPackages.Contains(typeInfo.PackageName))
            {
                _dependencies.Add(typeInfo.PackageName);
            }
        }
    }
}