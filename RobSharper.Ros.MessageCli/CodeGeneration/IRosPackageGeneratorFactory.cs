namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public interface IRosPackageGeneratorFactory
    {
        IRosPackageGenerator CreateMessagePackageGenerator(CodeGenerationOptions options,
            CodeGenerationPackageContext package,
            ProjectCodeGenerationDirectoryContext packageDirectories);
    }
}