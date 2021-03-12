using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;
using RobSharper.Ros.MessageParser;

namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage
{
    public abstract class RosMessagePackageGenerator : IRosPackageGenerator
    {
        private readonly ProjectCodeGenerationDirectoryContext _directories;
        private readonly IKeyedTemplateFormatter _templateEngine;
        
        private string _projectFilePath;

        private NameMapper _nameMapper;
        private PackageTemplateData _packageTemplateData;

        protected PackageTemplateData PackageTemplateData
        {
            get
            {
                EnsurePackageData();
                return _packageTemplateData;
            }
        }
        
        
        protected CodeGenerationOptions Options { get; }
        
        protected CodeGenerationPackageContext Package { get; }

        protected NameMapper NameMapper
        {
            get
            {
                if (_nameMapper == null)
                    _nameMapper = GetNameMapper();

                return _nameMapper;
            }
        }

        protected abstract string ProjectTemplateFilePath { get; }
        protected abstract string NugetConfigTemplateFilePath { get; }
        protected abstract string MessageTemplateFilePath { get; }
        protected abstract string ServiceTemplateFilePath { get; }
        protected abstract string ActionTemplateFilePath { get; }

        protected virtual bool GenerateActionFile => ActionTemplateFilePath != null;
        protected virtual bool GenerateServiceFile => ServiceTemplateFilePath != null;

        protected RosMessagePackageGenerator(CodeGenerationPackageContext package, CodeGenerationOptions options,
            ProjectCodeGenerationDirectoryContext directories, IKeyedTemplateFormatter templateEngine)
        {
            Package = package ?? throw new ArgumentNullException(nameof(package));
            
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _directories = directories ?? throw new ArgumentNullException(nameof(directories));
            _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
        }
        
        private void EnsurePackageData()
        {
            if (_packageTemplateData != null)
                return;

            _packageTemplateData = CreatePackageData();
        }

        protected virtual PackageTemplateData CreatePackageData()
        {
            var data = new PackageTemplateData
            {
                RosName = Package.PackageInfo.Name,
                Version = Package.PackageInfo.Version,
                Name = Package.PackageInfo.Name.ToPascalCase(),
                Namespace = _nameMapper.GetNamespace(Package.PackageInfo.Name),
                Description = Package.PackageInfo.Description,
                ProjectUrl = Package.PackageInfo.ProjectUrl,
                RepositoryUrl = Package.PackageInfo.RepositoryUrl,
                Authors = Package.PackageInfo.Authors,
            };

            return data;
        }

        public void Execute()
        {
            Colorful.Console.WriteLine($"Processing message package {Package.PackageInfo.Name} [{Package.PackageInfo.Version}]");
            Colorful.Console.WriteLine(Package.PackageInfo.PackageDirectory.FullName);
            
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

        protected abstract NameMapper GetNameMapper();

        private void CreateProjectFile()
        {
            EnsurePackageData();

            var projectFilePath = _directories.TempDirectory.GetFilePath($"{PackageTemplateData.Namespace}.csproj");
            var projectFileContent = _templateEngine.Format(ProjectTemplateFilePath, PackageTemplateData);
            WriteFile(projectFilePath, projectFileContent);

            
            var nugetConfig = new
            {
                TempNugetFolder = _directories.NugetTempDirectory.FullName,
                NugetSources = Options.NugetFeedXmlSources
            };

            var nugetConfigFilePath = _directories.TempDirectory.GetFilePath("nuget.config");
            var nugetConfigFile = _templateEngine.Format(NugetConfigTemplateFilePath, nugetConfig);
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
                    Environment.ExitCode |= (int) ExitCodes.CouldNotAddDependency;
                    throw new DependencyNotFoundException(dependency,
                        $"Could not add dependency {dependency}.", e);
                }
            }
        }

        private void CopyOutput()
        {
            var nugetFileName = $"{PackageTemplateData.Namespace}.{PackageTemplateData.Version}";
            
            var nupkgFileName = $"{nugetFileName}.nupkg";
            var snupkgFileName = $"{nugetFileName}.snupkg";
            
            CopyNugetFile(nupkgFileName);
            CopyNugetFile(snupkgFileName);

            // Copy dll to output directory if requested
            if (Options.CreateDll)
            {
                var dllFileName = $"{PackageTemplateData.Namespace}.dll";
                
                var dllSourceFile = new FileInfo(Path.Combine(_directories.TempDirectory.FullName, "bin", "Release", "netstandard2.0", dllFileName));
                var dllDestinationFile = new FileInfo(Path.Combine(_directories.OutputDirectory.FullName, dllFileName));
                
                ReplaceFiles(dllSourceFile, dllDestinationFile);
            }
        }

        private void CopyNugetFile(string fileName)
        {
            var sourceFile = new FileInfo(Path.Combine(_directories.TempDirectory.FullName, "bin", "Release", fileName));

            // Copy nuget package to temp package source, so it can be consumed by other projects in the current build pipeline
            var tempDestinationFile = new FileInfo(Path.Combine(_directories.NugetTempDirectory.FullName, fileName));
            ReplaceFiles(sourceFile, tempDestinationFile);

            // Copy nuget package to output directory if requested
            if (Options.CreateNugetPackage)
            {
                var destinationFile = new FileInfo(Path.Combine(_directories.OutputDirectory.FullName, fileName));
                ReplaceFiles(sourceFile, destinationFile);
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
            WriteActionInternal(rosType);
            
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

            var data = new MessageTemplateData
            {
                Package = PackageTemplateData,
                RosTypeName = _nameMapper.GetRosTypeName(rosType.TypeName, messageType),
                RosAbstractTypeName = rosType.TypeName,
                TypeName = _nameMapper.GetTypeName(rosType.TypeName, messageType),
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
                    IsServiceRequest = messageType.HasFlag(DetailedRosMessageType.ServiceRequest),
                    IsServiceResponse = messageType.HasFlag(DetailedRosMessageType.ServiceResponse)
                }
            };

            var typeName = data.TypeName;
            var filePath = _directories.TempDirectory.GetFilePath($"{typeName}.cs");
            var content = _templateEngine.Format(MessageTemplateFilePath, data);
            
            WriteFile(filePath, content);
        }

        public class MessageTemplateData
        {
            public PackageTemplateData Package { get; set; }
            
            public string RosTypeName { get; set; }
            public string RosAbstractTypeName { get; set; }
            
            public string TypeName { get; set; }
            public string AbstractTypeName { get; set; }
            
            public IEnumerable<object> Fields { get; set; }
            
            public IEnumerable<object> Constants { get; set; }
            
            public object MessageType { get; set; }
        }
        
        private void WriteServiceInternal(RosTypeInfo serviceType)
        {
            if (!GenerateServiceFile)
                return;
            
            var data = GetServiceTemplateData(serviceType);
            var filePath = _directories.TempDirectory.GetFilePath($"{data.ServiceType.TypeName}.cs");
            var content = _templateEngine.Format(ServiceTemplateFilePath, data);

            WriteFile(filePath, content);
        }

        protected virtual ServiceTemplateData GetServiceTemplateData(RosTypeInfo serviceType)
        {
            var data = new ServiceTemplateData
            {
                Package = PackageTemplateData,
                ServiceType = new ConcreteTypeTemplateData
                {
                    RosTypeName = NameMapper.GetRosTypeName(serviceType.TypeName, DetailedRosMessageType.Service),
                    TypeName = NameMapper.GetTypeName(serviceType.TypeName, DetailedRosMessageType.Service)
                },
                RequestType = new ConcreteTypeTemplateData
                {
                    RosTypeName = NameMapper.GetRosTypeName(serviceType.TypeName, DetailedRosMessageType.ServiceRequest),
                    TypeName = NameMapper.GetTypeName(serviceType.TypeName, DetailedRosMessageType.ServiceRequest)
                },
                ResponseType = new ConcreteTypeTemplateData
                {
                    RosTypeName = NameMapper.GetRosTypeName(serviceType.TypeName, DetailedRosMessageType.ServiceResponse),
                    TypeName = NameMapper.GetTypeName(serviceType.TypeName, DetailedRosMessageType.ServiceResponse)
                }
            };
            return data;
        }

        private void WriteActionInternal(RosTypeInfo actionType)
        {
            if (!GenerateActionFile)
                return;

            throw new NotSupportedException("No need for this until now");
        }
        
        private void WriteFile(string filePath, string content)
        {
            if (!Path.IsPathFullyQualified(filePath))
                throw new ArgumentException("File path must be fully qualified", nameof(filePath));
            
            File.WriteAllText(filePath, content);
        }

        
    }
}