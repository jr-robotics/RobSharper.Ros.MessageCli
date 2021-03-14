using System.IO;
using System.Reflection;
using RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;

namespace RobSharper.Ros.MessageCli.CodeGeneration.RosTargets.RobSharper
{
    public class RobSharperMessagePackageGenerator : RosMessagePackageGenerator
    {
        public static readonly string TemplatesDirectory =
            Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "CodeGeneration", "RosTargets", "RobSharper",
                "TemplateFiles");
        
        public RobSharperMessagePackageGenerator(CodeGenerationPackageContext package, CodeGenerationOptions options,
            ProjectCodeGenerationDirectoryContext directories, IKeyedTemplateFormatter templateEngine) : base(package,
            options, directories, templateEngine)
        {
        }

        protected override string ProjectTemplateFile => "csproj.hbs";
        protected override string NugetConfigTemplateFile => "nuget.config.hbs";
        protected override string MessageTemplateFile => "Message.cs.hbs";
        protected override string ServiceTemplateFile => null;
        protected override string ActionTemplateFile => null;
    }
}