using System.IO;
using RobSharper.Ros.MessageCli.CodeGeneration;

namespace RobSharper.Ros.MessageCli.Tests.CodeGeneration
{
    public class TestUtils
    {
        public static string CreatePackagePath(params string[] nestedPackageNames)
        {
            return CreatePackagePath(true, nestedPackageNames);
        }
        
        public static string CreatePackagePath(bool isValidPackage, params string[] nestedPackageNames)
        {
            var packageFolder = Path.Combine("TestPackages", isValidPackage ? "valid" : "invalid", Path.Combine(nestedPackageNames));
            return packageFolder;
        }
        
        public static RosPackageInfo CreatePackageInfo(bool isValidPackage, params string[] packagePath)
        {
            var packageFolder = CreatePackagePath(isValidPackage, packagePath);
            var package = RosPackageInfo.Create(packageFolder);

            return package;
        }
    }
}