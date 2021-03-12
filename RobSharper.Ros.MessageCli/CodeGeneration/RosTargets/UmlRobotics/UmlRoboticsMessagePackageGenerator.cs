using RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;

namespace RobSharper.Ros.MessageCli.CodeGeneration.RosTargets.UmlRobotics
{
    public class UmlRoboticsMessagePackageGenerator : RosMessagePackageGenerator
    {
        public UmlRoboticsMessagePackageGenerator(CodeGenerationPackageContext package, CodeGenerationOptions options,
            ProjectCodeGenerationDirectoryContext directories, IKeyedTemplateFormatter templateEngine) : base(package,
            options, directories, templateEngine)
        {
        }

        protected override string ProjectTemplateFilePath => TemplatePaths.ProjectFile;
        protected override string NugetConfigTemplateFilePath => TemplatePaths.NugetConfigFile;
        protected override string MessageTemplateFilePath => TemplatePaths.MessageFile;
        protected override string ServiceTemplateFilePath => TemplatePaths.ServiceFile;
        protected override string ActionTemplateFilePath => null;
        protected override NameMapper GetNameMapper()
        {
            var namespaceTemplate = (Options.RootNamespace + ".{{Name}}").TrimStart('.');
            var nameMapper = new UmlRoboticsNameMapper(Package.PackageInfo.Name,
                new StaticHandlebarsTemplateFormatter(namespaceTemplate));

            return nameMapper;
        }
    }
}