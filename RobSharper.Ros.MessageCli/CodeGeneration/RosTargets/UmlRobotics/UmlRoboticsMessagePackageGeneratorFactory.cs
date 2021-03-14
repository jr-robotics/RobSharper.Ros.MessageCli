using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;

namespace RobSharper.Ros.MessageCli.CodeGeneration.RosTargets.UmlRobotics
{
    public class UmlRoboticsMessagePackageGeneratorFactory : IRosPackageGeneratorFactory
    {
        public IRosPackageGenerator CreateMessagePackageGenerator(CodeGenerationOptions options,
            IKeyedTemplateFormatter templateEngine, CodeGenerationPackageContext package,
            ProjectCodeGenerationDirectoryContext packageDirectories)
        {
            var generator = new UmlRoboticsMessagePackageGenerator(package, options, packageDirectories, templateEngine);
            return generator;
        }
    }
}