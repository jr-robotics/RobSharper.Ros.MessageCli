using System;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;

namespace RobSharper.Ros.MessageCli.CodeGeneration.RosTargets.UmlRobotics
{
    public class UmlRoboticsMessagePackageGeneratorFactory : IRosPackageGeneratorFactory
    {
        private readonly IKeyedTemplateFormatter _templateEngine;

        public UmlRoboticsMessagePackageGeneratorFactory(IKeyedTemplateFormatter templateEngine)
        {
            _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
        }
        
        public IRosPackageGenerator CreateMessagePackageGenerator(CodeGenerationOptions options,
            CodeGenerationPackageContext package,
            ProjectCodeGenerationDirectoryContext packageDirectories)
        {
            var generator = new UmlRoboticsMessagePackageGenerator(package, options, packageDirectories, _templateEngine);
            return generator;
        }
    }
}