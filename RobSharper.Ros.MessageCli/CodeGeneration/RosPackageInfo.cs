using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using RobSharper.Ros.PackageXml;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public class RosPackageInfo
    {
        public string Name { get; }
        public string Version { get; }
        public bool IsMetaPackage { get; }
        public string Description { get; }
        public string ProjectUrl { get; }
        public string RepositoryUrl { get; }
        public IEnumerable<string> Authors { get; }

        private readonly List<string> _packageDependencies;

        private IEnumerable<FileInfo> _messages;

        public DirectoryInfo PackageDirectory { get; }

        public IList<string> PackageDependencies => _packageDependencies;

        public bool HasMessages => Messages.Any();

        public IEnumerable<FileInfo> Messages
        {
            get
            {
                if (_messages != null)
                {
                    return _messages;
                }

                _messages = PackageDirectory
                    .GetDirectories()
                    .Where(d => new[] {"msg", "srv", "action"}.Contains(d.Name.ToLowerInvariant()))
                    .SelectMany(d => d.GetFiles())
                    .Where(f => f.GetRosMessageType() != RosMessageType.None)
                    .ToList();

                return _messages;
            }
        }

        public bool IsOptional { get; private set; }

        public RosPackageInfo(DirectoryInfo packageDirectory, string name, string version,
            IEnumerable<string> packageDependencies, bool isMetaPackage, string description = null,
            IEnumerable<string> authors = null, string projectUrl = null, string repositoryUrl = null, 
            bool isOptional = false)
        {
            PackageDirectory = packageDirectory ?? throw new ArgumentNullException(nameof(packageDirectory));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            IsMetaPackage = isMetaPackage;
            Description = description;
            ProjectUrl = projectUrl;
            RepositoryUrl = repositoryUrl;
            Authors = authors ?? Enumerable.Empty<string>();
            IsOptional = isOptional;

            _packageDependencies = new List<string>();
            
            if (packageDependencies != null)
                _packageDependencies.AddRange(packageDependencies);
        }


        public static bool IsPackageFolder(string packageFolder)
        {
            // folder is package folder if it contains a package.xml file
            return File.Exists(Path.Combine(packageFolder, "package.xml"));
        }

        public static RosPackageInfo Create(string packageRootPath)
        {
            if (packageRootPath == null) throw new ArgumentNullException(nameof(packageRootPath));
            
            return Create(new RosPackageFolder(packageRootPath, RosPackageFolder.BuildType.Mandatory));
        }

        public static RosPackageInfo Create(RosPackageFolder packageFolder)
        {
            var logger = LoggingHelper.Factory.CreateLogger<RosPackageInfo>();
            
            var packageXmlPath = Path.Combine(packageFolder.Path, "package.xml");

            using (logger.BeginScope($"package.xml ({packageXmlPath})"))
            {
                if (!File.Exists(packageXmlPath))
                {
                    logger.LogError("package.xml not found");
                    throw new FileNotFoundException("package.xml not found");
                }

                try
                {
                    var package = PackageXmlReader.ReadPackageXml(packageXmlPath);

                    var authors = package.Maintainers
                        .Union(package.Authors)
                        .Distinct()
                        .Select(x => x.ToString())
                        .ToList();

                    var projectUrl = package.Urls?.FirstOrDefault(x => x.Type == PackageUrlType.Website)?.Url;
                    var repositoryUrl = package.Urls?.FirstOrDefault(x => x.Type == PackageUrlType.Repository)?.Url;
                    
                    var packageDirectory = new DirectoryInfo(packageFolder.Path);
                    return new RosPackageInfo(packageDirectory,
                        package.Name, 
                        package.Version, 
                        package.PackageDependencies, 
                        package.IsMetaPackage,
                        package.Description,
                        authors,
                        projectUrl,
                        repositoryUrl,
                        packageFolder.BuildStrategy == RosPackageFolder.BuildType.Optional);
                }
                catch (Exception e)
                {
                    logger.LogError("Could not deserialize package.xml", e);
                    throw;
                }
            }
        }
    }
}