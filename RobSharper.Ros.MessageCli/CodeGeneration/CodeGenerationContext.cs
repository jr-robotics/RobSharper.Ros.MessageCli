using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public class CodeGenerationContext : IBuildPackages
    {
        public IEnumerable<CodeGenerationPackageContext> Packages { get; private set; }

        private PackageRegistry _packageRegistry;

        public PackageRegistry PackageRegistry
        {
            get
            {
                if (_packageRegistry == null)
                {
                    _packageRegistry = new PackageRegistry(this);
                }

                return _packageRegistry;
            }
        }

        IEnumerable<RosPackageInfo> IBuildPackages.Packages
        {
            get { return Packages?.Select(x => x.PackageInfo); }
        }

        private CodeGenerationContext(IEnumerable<RosPackageInfo> packageInfos)
        {
            if (packageInfos == null) throw new ArgumentNullException(nameof(packageInfos));

            var context = this;
            var factory = new RosMessageParserFactory();
            
            Packages = packageInfos
                .Select(p => new CodeGenerationPackageContext(context, p, factory.Create(p, context)))
                .ToList();
        }

        /// <summary>
        /// Parses the message files of all packages
        /// </summary>
        public void ParseMessages()
        {
            foreach (var package in Packages)
            {
                package.Parser.ParseMessages();
            }
        }

        /// <summary>
        /// Filters the package list based on the given filter parameter.
        /// 
        /// Filter is a list of package names which will be used to filter the package list.
        /// Only packages matching the filter expression and dependent packages will be included in
        /// the final packages list.
        /// The filter may also contain the asterisk (*) as first or last character to specify
        /// an ends with or starts with expression.
        ///
        /// If the filter is null or empty, no filtering is applied.
        /// </summary>
        /// <param name="filters">Filter to apply</param>
        public void FilterPackages(IEnumerable<string> filters)
        {
            if (filters == null || !filters.Any())
                return;
            
            var innerFilters = new List<Func<string, bool>>();

            foreach (var filter in filters)
            {
                var f = filter;
                Func<string, bool> innerFilter;

                if (f.StartsWith("*"))
                {
                    innerFilter = s => s.EndsWith(f.Substring(1));
                }
                else if (f.EndsWith("*"))
                {
                    innerFilter = s => s.StartsWith(f.Substring(0, f.Length - 1));
                }
                else
                {
                    innerFilter = s => s.Equals(f);
                }

                innerFilters.Add(innerFilter);
            }
            
            if (innerFilters.Count == 0)
                return;

            Func<string, bool> filterExpr;

            if (innerFilters.Count == 1)
            {
                filterExpr = innerFilters.First();
            }
            else
            {
                filterExpr = s => innerFilters.Any(f => f(s));
            }
            
            FilterPackages(filterExpr);
        }

        public void FilterPackages(Func<string, bool> filterExpr)
        {
            if (filterExpr == null) throw new ArgumentNullException(nameof(filterExpr));
            
            ParseMessages();

            var filteredPackages = Packages
                .Where(p => filterExpr(p.PackageInfo.Name))
                .ToList();


            bool hasMissingPackages;
            do
            {
                var missingPackageNames = filteredPackages
                    .SelectMany(p => p.Parser.PackageDependencies)
                    .Distinct()
                    .Except(filteredPackages
                        .Select(p => p.PackageInfo.Name)
                    )
                    .ToList();

                hasMissingPackages = missingPackageNames.Any();

                var missingPackages = Packages
                    .Where(p => missingPackageNames.Contains(p.PackageInfo.Name))
                    .ToList();

                filteredPackages.AddRange(missingPackages);
            } while (hasMissingPackages);

            Packages = filteredPackages;
        }

        /// <summary>
        /// Reorders the package list according to build dependencies.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if packages no build sequence without breaking dependencies can be found.</exception>
        public void ReorderPackagesForBuilding()
        {
            ParseMessages();

            var buildQueue = new List<CodeGenerationPackageContext>();
            List<CodeGenerationPackageContext> packagesToEnqueue;

            while(true)
            {
                packagesToEnqueue = Packages.Except(buildQueue).ToList();

                if (packagesToEnqueue.Count == 0)
                    break;

                var packageEnqueued = false;
                foreach (var package in packagesToEnqueue)
                {
                    var dependencies = package.Parser.PackageDependencies;

                    // Package can be built if all dependencies are
                    //    external dependencies (not in build pipeline) OR
                    //    have to be built but are already enqueued
                    if (dependencies.All(x =>
                        !PackageRegistry.Items[x].IsInBuildPipeline || buildQueue.Any(q => q.PackageInfo.Name == x)))
                    {
                        buildQueue.Add(package);
                        packageEnqueued = true;
                    }
                }

                // If no package was enqueued in one round, we cannot build
                if (!packageEnqueued)
                {
                    throw new CircularPackageDependencyException("Can not identify build sequence. Packages have a circular dependency.", packagesToEnqueue);
                }
            }

            Packages = buildQueue;
        }

        public static CodeGenerationContext Create(string packageFolder)
        {
            if (packageFolder == null) throw new ArgumentNullException(nameof(packageFolder));

            packageFolder = Path.GetFullPath(packageFolder);
            
            if (!Directory.Exists(packageFolder))
            {
                throw new DirectoryNotFoundException($"Directory {packageFolder} does not exit.");
            }

            var packageFolders = FindPackageFolders(packageFolder);
            var packages = packageFolders
                .Select(RosPackageInfo.Create)
                .Where(p => p.IsMetaPackage || p.HasMessages);
            
            var context = new CodeGenerationContext(packages);

            return context;
        }
        
        private static IEnumerable<string> FindPackageFolders(string packageFolder)
        {
            var packageFolders = new List<string>();
            
            if (RosPackageInfo.IsPackageFolder(packageFolder))
            {
                packageFolders.Add(packageFolder);
            }
            else
            {
                foreach (var directory in Directory.GetDirectories(packageFolder))
                {
                    packageFolders.AddRange(FindPackageFolders(directory));
                }
            }

            return packageFolders;
        }
    }
}