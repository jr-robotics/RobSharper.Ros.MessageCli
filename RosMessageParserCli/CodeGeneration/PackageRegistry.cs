using System;
using System.Collections.Generic;
using System.Linq;

namespace Joanneum.Robotics.Ros.MessageParser.Cli.CodeGeneration
{
    public class PackageRegistry : IPackageRegistry
    {
        private readonly IBuildPackages _context;
        public IDictionary<string, PackageRegistryItem> Items { get; } = new Dictionary<string, PackageRegistryItem>();

        public PackageRegistry(IBuildPackages context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PackageRegistryItem AddDependency(string packageName)
        {
            if (packageName == null) throw new ArgumentNullException(nameof(packageName));

            if (Items.TryGetValue(packageName, out var dependency))
                return dependency;

            dependency = new PackageRegistryItem(packageName);
            
            if (_context.Packages.Select(x => x.Name).Contains(dependency.PackageName))
                dependency.SetIsInBuildPipline();
            
            Items.Add(dependency.PackageName, dependency);
            return dependency;
        }
    }
}