using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public interface IRosPackageGeneratorFactory
    {
        IRosPackageGenerator CreateMessagePackageGenerator(CodeGenerationOptions options,
            IKeyedTemplateFormatter templateEngine, CodeGenerationPackageContext package,
            ProjectCodeGenerationDirectoryContext packageDirectories);
    }
}