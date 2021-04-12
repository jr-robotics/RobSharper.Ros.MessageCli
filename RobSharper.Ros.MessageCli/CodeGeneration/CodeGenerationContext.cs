using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public class CodeGenerationContext : IBuildPackages
    {
        private readonly IList<RosPackageInfo> _availablePackages = new List<RosPackageInfo>();
        private readonly IList<CodeGenerationPackageContext> _mandatoryPackages = new List<CodeGenerationPackageContext>();
        
        public IEnumerable<RosPackageInfo> AvailablePackages => _availablePackages;

        public IEnumerable<CodeGenerationPackageContext> Packages => _mandatoryPackages;

        IEnumerable<RosPackageInfo> IBuildPackages.Packages
        {
            get
            {
               return _mandatoryPackages
                   .Select(x => x.PackageInfo)
                   .ToList();
            }
        }

        public CodeGenerationContext()
        {
        }

        public void AddPackage(RosPackageInfo packageInfo, bool isOptional = false)
        {
            if (packageInfo == null) throw new ArgumentNullException(nameof(packageInfo));

            if (!_availablePackages.Contains(packageInfo))
            {
                if (_availablePackages.Any(p => string.Equals(p.Name, packageInfo.Name)))
                    throw new InvalidOperationException("Package with the same name already exists.");
                
                _availablePackages.Add(packageInfo);
            }

            if (isOptional)
            {
                // Set mandatory if any other mandatory package
                // depends on this package
                var existingDependency = _mandatoryPackages
                    .SelectMany(p => p.Parser.PackageDependencies)
                    .Any(p => string.Equals(p, packageInfo.Name));

                isOptional = !existingDependency;
            }
            
            if (!isOptional)
            {
                SetMandatoryInternal(packageInfo);
            }
        }

        public void SetMandatory(RosPackageInfo packageInfo)
        {
            if (packageInfo == null) throw new ArgumentNullException(nameof(packageInfo));

            if (!_availablePackages.Contains(packageInfo))
                throw new InvalidOperationException("Package is not in the list of available packages. Call AddPackage before.");

            SetMandatoryInternal(packageInfo);
        }

        private void SetMandatoryInternal(RosPackageInfo packageInfo)
        {
            if (_mandatoryPackages.Any(x => x.PackageInfo == packageInfo))
                return;

            var messageParser = RosMessageParserFactory.Create(packageInfo, this);
            
            var context = new CodeGenerationPackageContext(packageInfo, messageParser);
            
            _mandatoryPackages.Add(context);
            
            // Add dependencies to build pipeline
            foreach (var dependentUponPackageName in context.Parser.PackageDependencies)
            {
                var dependentUponPackage =
                    _availablePackages.FirstOrDefault(p => string.Equals(p.Name, dependentUponPackageName));

                if (dependentUponPackage != null)
                    SetMandatoryInternal(dependentUponPackage);
            }
        }

        private static readonly ILogger Logger = LoggingHelper.Factory.CreateLogger<RosPackageInfo>();
        
        public static CodeGenerationContext Create(string packageFolder)
        {
            if (packageFolder == null) throw new ArgumentNullException(nameof(packageFolder));

            var packageFolders =
                RosPackageFolder.Find(packageFolder, RosPackageFolder.BuildType.Mandatory);
            
            return Create(packageFolders);
        }

        public static CodeGenerationContext Create(IEnumerable<RosPackageFolder> packageFolders)
        {
            // Ensure existing package info for mandatory packages
            // Skip faulty optional packages
            
            var context = new CodeGenerationContext();

            foreach (var package in packageFolders)
            {
                var isOptional = package.BuildStrategy == RosPackageFolder.BuildType.Optional;
                RosPackageInfo packageInfo;

                try
                {
                    packageInfo = package.PackageInfo;
                }
                catch (Exception e)
                {
                    if (isOptional)
                    {
                        Logger.LogWarning(e, e.Message);
                        continue;
                    }
                    
                    throw;
                }

                context.AddPackage(packageInfo, isOptional);
            }
            
            return context;
        }
    }
}