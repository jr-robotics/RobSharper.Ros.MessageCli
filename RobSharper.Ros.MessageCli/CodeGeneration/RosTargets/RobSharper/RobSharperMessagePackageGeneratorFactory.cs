using System;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;

namespace RobSharper.Ros.MessageCli.CodeGeneration.RosTargets.RobSharper
{
    public class RobSharperMessagePackageGeneratorFactory : IRosPackageGeneratorFactory
    {
        private readonly IKeyedTemplateFormatter _templateEngine;

        public RobSharperMessagePackageGeneratorFactory(IKeyedTemplateFormatter templateEngine)
        {
            _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
        }
        
        public IRosPackageGenerator CreateMessagePackageGenerator(CodeGenerationOptions options,
            CodeGenerationPackageContext package,
            ProjectCodeGenerationDirectoryContext packageDirectories)
        {
            var generator = new RobSharperMessagePackageGenerator(package, options, packageDirectories, _templateEngine);
            return generator;
        }
    }
}