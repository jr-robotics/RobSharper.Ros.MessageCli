using RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public static class CodeGeneration
    {
        public static int Execute(CodeGenerationOptions options, IKeyedTemplateFormatter templateEngine)
        {
            var context = CodeGenerationContext.Create(options.PackagePath);

            using (var directories = new CodeGenerationDirectoryContext(options.OutputPath, options.PreserveGeneratedCode))
            {
                // Parse message files and build package dependency graph
                context.ParseMessages();
                
                // Set build order depending on package dependencies
                context.ReorderPackagesForBuilding();
                
                foreach (var package in context.Packages)
                {
                    // Create Package
                    var packageDirectories = directories.GetPackageTempDir(package.PackageInfo);
                    var generator = new RosMessagePackageGenerator(package, options, packageDirectories, templateEngine);

                    generator.Execute();
                }
            }

            return 0;
        }
    }
}