using System.Reflection;

namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage.TemplateData
{
    public class MessageCliToolInfo
    {
        public static readonly MessageCliToolInfo Instance = Create();

        private static MessageCliToolInfo Create()
        {
            var assemblyName = Assembly
                .GetEntryAssembly()?
                .GetName();

            var name = assemblyName?.Name;
            var version = assemblyName?.Version.ToString();

            return new MessageCliToolInfo(name, version);
        }

        public string ToolName { get; }
        public string Version { get; }

        public MessageCliToolInfo(string toolName, string version)
        {
            ToolName = toolName;
            Version = version;
        }
    }
}