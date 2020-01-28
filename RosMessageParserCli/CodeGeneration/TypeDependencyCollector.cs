using System;
using System.Collections.Generic;
using System.Linq;

namespace Joanneum.Robotics.Ros.MessageParser.Cli.CodeGeneration
{
    public class TypeDependencyCollector : DefaultRosMessageVisitorListener
    {
        private readonly IEnumerable<string> _ignoredPackages;
        private readonly ISet<RosTypeInfo> _dependencies = new HashSet<RosTypeInfo>();

        public TypeDependencyCollector(IEnumerable<string> ignoredPackages = null)
        {
            _ignoredPackages = ignoredPackages ?? Enumerable.Empty<string>();
        }

        public ISet<RosTypeInfo>  Dependencies => _dependencies;
        
        public override void OnVisitRosType(RosTypeInfo typeInfo)
        {
            if (typeInfo.HasPackage && !_ignoredPackages.Contains(typeInfo.PackageName))
            {
                _dependencies.Add(typeInfo);
            }
        }
    }
}