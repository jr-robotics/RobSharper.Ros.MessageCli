using System;
using System.Collections.Generic;
using System.Linq;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public class BuildOrderer
    {
        private readonly CodeGenerationContext _context;

        private int _packagesHash;
        private IEnumerable<CodeGenerationPackageContext> _packages;

        public BuildOrderer(CodeGenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<CodeGenerationPackageContext> Packages
        {
            get
            {
                if (HasChanged())
                    Sort();
                
                return _packages;
            }
        }

        private bool HasChanged()
        {
            return _packagesHash != CalculateHash(_context.Packages);
        }

        private int CalculateHash(IEnumerable<CodeGenerationPackageContext> packages)
        {
            var hasher = new HashCode();

            foreach (var package in packages)
            {
                hasher.Add(package);
            }

            return hasher.ToHashCode();
        }

        /// <summary>
        /// Reorders the package list according to build dependencies.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if packages no build sequence without breaking dependencies can be found.</exception>
        public void Sort()
        {
            var packagesToBuild = _context.Packages.ToList();

            var hash = CalculateHash(packagesToBuild);
            var buildQueue = new List<CodeGenerationPackageContext>();

            while (true)
            {
                var remainingPackages = packagesToBuild
                    .Except(buildQueue)
                    .ToList();

                if (remainingPackages.Count == 0)
                    break;
                
                var packageEnqueued = false;
                foreach (var package in remainingPackages)
                {
                    var dependencies = package.Parser.PackageDependencies;

                    // Package can be built if every dependency is either
                    //    an external dependencies (not in the list of packages to build) OR
                    //    is already enqueued in the build queue
                    if (dependencies.All(dependency =>
                        !packagesToBuild.Any(p => string.Equals(p.PackageInfo.Name, dependency)) ||
                        buildQueue.Any(q => string.Equals(q.PackageInfo.Name, dependency))))
                    {
                        buildQueue.Add(package);
                        packageEnqueued = true;
                    }
                }

                // If no package was enqueued in one round, we cannot build
                if (!packageEnqueued)
                {
                    throw new CircularPackageDependencyException("Can not identify build sequence. Packages have a circular dependency.", remainingPackages);
                }
            }
            
            _packages = buildQueue;
            _packagesHash = hash;
        }
    }
}