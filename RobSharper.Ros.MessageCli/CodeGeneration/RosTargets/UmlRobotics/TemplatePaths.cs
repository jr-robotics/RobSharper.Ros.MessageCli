using System.IO;
using System.Reflection;

namespace RobSharper.Ros.MessageCli.CodeGeneration.RosTargets.UmlRobotics
{
    public static class TemplatePaths
    {
        public static readonly string TemplatesDirectory =
            Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "CodeGeneration", "RosTargets", "UmlRobotics",
                "TemplateFiles");

        public const string ProjectFile = "csproj.hbs";
        public const string NugetConfigFile = "nuget.config.hbs";
        public const string MessageFile = "Message.cs.hbs";
        public const string ServiceFile = "Service.cs.hbs";
    }
}