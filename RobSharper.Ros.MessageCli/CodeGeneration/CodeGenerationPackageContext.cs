using System;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public class CodeGenerationPackageContext
    {
        public RosPackageInfo PackageInfo { get; }
        public IRosMessagePackageParser Parser { get; }

        public CodeGenerationPackageContext(RosPackageInfo packageInfo, IRosMessagePackageParser parser)
        {
            PackageInfo = packageInfo ?? throw new ArgumentNullException(nameof(packageInfo));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }
    }
}