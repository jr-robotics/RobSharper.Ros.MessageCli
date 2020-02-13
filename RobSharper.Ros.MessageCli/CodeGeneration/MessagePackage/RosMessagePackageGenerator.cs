using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;
using RobSharper.Ros.MessageCli.CodeGeneration.UmlRobotics;
using RobSharper.Ros.MessageParser;

namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage
{
    public class RosMessagePackageGenerator : IRosPackageGenerator
    {
        private readonly CodeGenerationOptions _options;
        private readonly ProjectCodeGenerationDirectoryContext _directories;
        private readonly IKeyedTemplateFormatter _templateEngine;

        private readonly dynamic _data;
        
        private string _projectFilePath;

        private readonly NameMapper _nameMapper;

        public CodeGenerationPackageContext Package { get; }

        public RosMessagePackageGenerator(CodeGenerationPackageContext package, CodeGenerationOptions options,
            ProjectCodeGenerationDirectoryContext directories, IKeyedTemplateFormatter templateEngine)
        {
            Package = package ?? throw new ArgumentNullException(nameof(package));
            
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _directories = directories ?? throw new ArgumentNullException(nameof(directories));
            _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));

            var namespaceTemplate = (options.RootNamespace + ".{{Name}}").TrimStart('.');
            _nameMapper = new UmlRoboticsNameMapper(Package.PackageInfo.Name, new StaticHandlebarsTemplateFormatter(namespaceTemplate));
            
            _data = new ExpandoObject();
        }
        
        private void EnsurePackageData()
        {
            if (((IDictionary<string, object>)_data).ContainsKey("Package"))
                return;
            
            _data.Package = new ExpandoObject();
            _data.Package.RosName = Package.PackageInfo.Name;
            _data.Package.Version = Package.PackageInfo.Version;
            _data.Package.Name = Package.PackageInfo.Name.ToPascalCase();
            _data.Package.Namespace = _nameMapper.GetNamespace(Package.PackageInfo.Name);
            _data.Package.Description = Package.PackageInfo.Description;
            _data.Package.HasDescription = !string.IsNullOrEmpty(Package.PackageInfo.Description);
            _data.Package.ProjectUrl = Package.PackageInfo.ProjectUrl;
            _data.Package.HasProjectUrl = !string.IsNullOrEmpty(Package.PackageInfo.ProjectUrl);
            _data.Package.RepositoryUrl = Package.PackageInfo.RepositoryUrl;
            _data.Package.HasRepositoryUrl = !string.IsNullOrEmpty(Package.PackageInfo.RepositoryUrl);
            _data.Package.Authors = Package.PackageInfo.Authors;
            _data.Package.HasAuthors = Package.PackageInfo.Authors != null && Package.PackageInfo.Authors.Any();
            _data.Package.AuthorsString = string.Join(";", Package.PackageInfo.Authors ?? Enumerable.Empty<string>());
        }

        public void Execute()
        {
            Colorful.Console.WriteLine($"Processing message package {Package.PackageInfo.Name} [{Package.PackageInfo.Version}]");
            CreateProjectFile();
            AddNugetDependencies();
        
            if (!Package.PackageInfo.IsMetaPackage)
            {
                CreateMessages();
                CreateServices();
                CreateActions();
            }

            BuildProject();
            CopyOutput();
            
            Colorful.Console.WriteLine($"Package {Package.PackageInfo.Name} [{Package.PackageInfo.Version}] created", Color.Lime);
            Colorful.Console.WriteLine();
        }

        private void CreateProjectFile()
        {
            EnsurePackageData();

            var projectFilePath = _directories.TempDirectory.GetFilePath($"{_data.Package.Namespace}.csproj");
            var projectFileContent = _templateEngine.Format(TemplatePaths.ProjectFile, _data.Package);
            WriteFile(projectFilePath, projectFileContent);

            
            var nugetConfig = new
            {
                TempNugetFolder = _directories.NugetTempDirectory.FullName,
                NugetSources = _options.NugetFeedXmlSources
            };

            var nugetConfigFilePath = _directories.TempDirectory.GetFilePath("nuget.config");
            var nugetConfigFile = _templateEngine.Format(TemplatePaths.NugetConfigFile, nugetConfig);
            WriteFile(nugetConfigFilePath, nugetConfigFile);

            _projectFilePath = projectFilePath;
        }

        private void BuildProject()
        {
            Colorful.Console.WriteLine();
            Colorful.Console.WriteLine($"Building package {Package.PackageInfo.Name} [{Package.PackageInfo.Version}]");
            Colorful.Console.WriteLine();
            
            DotNetProcess.Build(_projectFilePath);
        }

        private void AddNugetDependencies()
        {
            EnsurePackageData();

            IList<string> messageNugetPackages;
            
            // If package is a meta package use dependencies
            // form package info (parsed package.xml)
            // else use real dependencies retrieved form
            // message files.
            if (Package.PackageInfo.IsMetaPackage)
            {
                messageNugetPackages = Package.Parser
                    .PackageDependencies
                    .Select(x => _nameMapper.ResolveNugetPackageName(x))
                    .Distinct()
                    .ToList();
            }
            else
            {
                messageNugetPackages = Package.Parser
                    .ExternalTypeDependencies
                    .Select(x => _nameMapper.ResolveNugetPackageName(x))
                    .Distinct()
                    .ToList();
            }

            if (!messageNugetPackages.Any()) 
                return;
            
            Colorful.Console.WriteLine($"Restoring package dependencies");
            foreach (var dependency in messageNugetPackages)
            {
                Colorful.Console.WriteLine($"  {dependency}");
            }
            Colorful.Console.WriteLine();
            
            foreach (var dependency in messageNugetPackages)
            {
                var command = $"add \"{_projectFilePath}\" package {dependency}";

                try
                {
                    DotNetProcess.Execute(command);
                }
                catch (ProcessFailedException e)
                {
                    throw new DependencyNotFoundException(dependency,
                        $"Could not add dependency {dependency}.", e);
                }
            }
        }

        private void CopyOutput()
        {
            var nupkgFileName = $"{_data.Package.Namespace}.{_data.Package.Version}.nupkg";
            var nupkgSourceFile = new FileInfo(Path.Combine(_directories.TempDirectory.FullName, "bin", "Release", nupkgFileName));
            
            // Copy nuget package to temp package source, so it can be consumed by other projects in the current build pipeline
            var nugetTempDestination = new FileInfo(Path.Combine(_directories.NugetTempDirectory.FullName, nupkgFileName));
            ReplaceFiles(nupkgSourceFile, nugetTempDestination);
            
            
            // Copy nuget package to output directory if requested
            if (_options.CreateNugetPackage)
            {
                var nupkgDestinationFile = new FileInfo(Path.Combine(_directories.OutputDirectory.FullName, nupkgFileName));
                
                ReplaceFiles(nupkgSourceFile, nupkgDestinationFile);
            }

            
            // Copy dll to output directory if requested
            if (_options.CreateDll)
            {
                var dllFileName = $"{_data.Package.Namespace}.dll";
                
                var dllSourceFile = new FileInfo(Path.Combine(_directories.TempDirectory.FullName, "bin", "Release", "netstandard2.0", dllFileName));
                var dllDestinationFile = new FileInfo(Path.Combine(_directories.OutputDirectory.FullName, dllFileName));
                
                ReplaceFiles(dllSourceFile, dllDestinationFile);
            }
        }

        private static void ReplaceFiles(FileInfo sourceFile, FileInfo destinationFile)
        {
            if (destinationFile.Exists)
            {
                destinationFile.Delete();
            }
            else
            {
                var destinationDir = destinationFile.Directory;
                if (destinationDir != null && !destinationDir.Exists)
                {
                    destinationDir.Create();
                }
            }

            if (!sourceFile.Exists)
            {
                throw new InvalidOperationException($"Source file does not exist: {sourceFile}");
            }
            
            sourceFile.CopyTo(destinationFile.FullName);
        }

        private void CreateMessages()
        {
            foreach (var message in Package.Parser.Messages)
            {
                CreateMessage(message.Key, message.Value);
            }
        }
        
        private void CreateMessage(RosTypeInfo rosType, MessageDescriptor message)
        {
            WriteMessageInternal(rosType, DetailedRosMessageType.Message, message);
        }

        private void CreateServices()
        {
            foreach (var service in Package.Parser.Services)
            {
                CreateService(service.Key, service.Value);
            }
        }
        
        private void CreateService(RosTypeInfo rosType, ServiceDescriptor service)
        {
            if (_nameMapper.IsBuiltInType(rosType))
                return;

            WriteServiceInternal(rosType);
            
            WriteMessageInternal(rosType, DetailedRosMessageType.ServiceRequest, service.Request);
            WriteMessageInternal(rosType, DetailedRosMessageType.ServiceResponse, service.Response);
        }


        private void CreateActions()
        {
            foreach (var action in Package.Parser.Actions)
            {
                CreateAction(action.Key, action.Value);
            }
        }
        
        private void CreateAction(RosTypeInfo rosType, ActionDescriptor action)
        {
            WriteMessageInternal(rosType, DetailedRosMessageType.ActionGoal, action.Goal);
            WriteMessageInternal(rosType, DetailedRosMessageType.ActionResult, action.Result);
            WriteMessageInternal(rosType, DetailedRosMessageType.ActionFeedback, action.Feedback);
        }

        private void WriteMessageInternal(RosTypeInfo rosType, DetailedRosMessageType messageType, MessageDescriptor message)
        {
            if (messageType == DetailedRosMessageType.None || 
                messageType == DetailedRosMessageType.Action ||
                messageType == DetailedRosMessageType.Service)
            {
                throw new ArgumentException($"message type is not detailed enough", nameof(messageType));
            }
            
            if (_nameMapper.IsBuiltInType(rosType))
                return;

            var fields = message.Fields
                .Select(x => new
                {
                    Index = message.Items
                                .Select((item, index) => new {Item = item, Index = index})
                                .First(f => f.Item == x)
                                .Index + 1, // Index of this field in serialized message (starting at 1)
                    RosType = x.TypeInfo,
                    RosIdentifier = x.Identifier,
                    Type = new
                    {
                        InterfaceName = _nameMapper.ResolveFullQualifiedInterfaceName(x.TypeInfo),
                        ConcreteName = _nameMapper.ResolveFullQualifiedTypeName(x.TypeInfo),
                        IsBuiltInType = x.TypeInfo.IsBuiltInType,
                        IsArray = x.TypeInfo.IsArray,
                        IsValueType = x.TypeInfo.IsValueType(),
                        SupportsEqualityComparer = x.TypeInfo.SupportsEqualityComparer()
                    },
                    Identifier = x.Identifier
                })
                .ToList();

            var constants = message.Constants
                .Select(c => new
                {
                    Index = message.Items
                                .Select((item, index) => new {item, index})
                                .First(x => x.item == c)
                                .index + 1,
                    RosType = c.TypeInfo,
                    RosIdentifier = c.Identifier,
                    TypeName = _nameMapper.ResolveFullQualifiedTypeName(c.TypeInfo),
                    Identifier = c.Identifier,
                    Value = c.Value
                })
                .ToList();

            var typeName = _nameMapper.GetTypeName(rosType.TypeName, messageType);
            var data = new
            {
                Package = _data.Package,
                RosTypeName = _nameMapper.GetRosTypeName(rosType.TypeName, messageType),
                RosAbstractTypeName = rosType.TypeName,
                TypeName = typeName,
                AbstractTypeName = _nameMapper.GetTypeName(rosType.TypeName, DetailedRosMessageType.None),
                Fields = fields,
                Constants = constants,
                MessageType = new
                {
                    MessageType = (int) messageType,
                    IsMessage = messageType.HasFlag(DetailedRosMessageType.Message),
                    IsAction = messageType.HasFlag(DetailedRosMessageType.Action),
                    IsActionGoal = messageType.HasFlag(DetailedRosMessageType.ActionGoal),
                    IsActionResult = messageType.HasFlag(DetailedRosMessageType.ActionResult),
                    IsActionFeedback = messageType.HasFlag(DetailedRosMessageType.ActionFeedback),
                    IsService = messageType.HasFlag(DetailedRosMessageType.Service),
                    IsServiceRequest =  messageType.HasFlag(DetailedRosMessageType.ServiceRequest),
                    IsServiceResponse = messageType.HasFlag(DetailedRosMessageType.ServiceResponse)
                }
            };

            var filePath = _directories.TempDirectory.GetFilePath($"{typeName}.cs");
            var content = _templateEngine.Format(TemplatePaths.MessageFile, data);

            WriteFile(filePath, content);
        }

        private void WriteServiceInternal(RosTypeInfo serviceType)
        {
            var typeName = _nameMapper.GetTypeName(serviceType.TypeName, DetailedRosMessageType.Service);
            
            var data = new
            {
                Package = _data.Package,
                ServiceType = new
                {
                    RosTypeName = _nameMapper.GetRosTypeName(serviceType.TypeName, DetailedRosMessageType.Service),
                    TypeName = typeName
                },
                RequestType = new
                {
                    RosTypeName = _nameMapper.GetRosTypeName(serviceType.TypeName, DetailedRosMessageType.ServiceRequest),
                    TypeName = _nameMapper.GetTypeName(serviceType.TypeName, DetailedRosMessageType.ServiceRequest)
                },
                ResponseType = new
                {
                    RosTypeName = _nameMapper.GetRosTypeName(serviceType.TypeName, DetailedRosMessageType.ServiceResponse),
                    TypeName = _nameMapper.GetTypeName(serviceType.TypeName, DetailedRosMessageType.ServiceResponse)
                }
            };
            

            var filePath = _directories.TempDirectory.GetFilePath($"{typeName}.cs");
            var content = _templateEngine.Format(TemplatePaths.ServiceFile, data);

            WriteFile(filePath, content);
        }
        
        private void WriteFile(string filePath, string content)
        {
            if (!Path.IsPathFullyQualified(filePath))
                throw new ArgumentException("File path must be fully qualified", nameof(filePath));
            
            File.WriteAllText(filePath, content);
        }

        
    }
}