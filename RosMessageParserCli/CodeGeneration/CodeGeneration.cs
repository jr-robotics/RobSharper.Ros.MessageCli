using System;
using System.Collections.Generic;
using System.Linq;

namespace Joanneum.Robotics.Ros.MessageParser.Cli.CodeGeneration
{
    public static class CodeGeneration
    {
        public static int Execute(CodeGenerationOptions options)
        {
            var context = CodeGenerationContext.Create(options.PackagePath);

            using (var directories = new CodeGenerationDirectoryContext(options.OutputPath, options.PreserveGeneratedCode))
            {
                // Build package dependencyGraph
                foreach (var package in context.Packages)
                {
                    package.Parser.ParseMessages();
                }
                
                // Set build order
                context.ReorderPackagesForBuilding();

                // Check external dependencies
                CheckExternalPackagerDependencies(context);
                
                foreach (var package in context.Packages)
                {
                    // Create Package
                    var packageDirectories = directories.GetPackageTempDir(package.PackageInfo);
                    var generator = new RosMessagePackageGenerator(package, options, packageDirectories);

                    generator.CreateProjectFile();
                    generator.BuildMessages();

                    generator.BuildProject();
                    generator.CopyResultsToOutput();
                }
            }

            return 0;
        }

        private static void CheckExternalPackagerDependencies(CodeGenerationContext context)
        {
            foreach (var package in context.PackageRegistry.Items.Values)
            {
                if (package.IsAvailable)
                    continue;
                
                // Check nuget repo!
            }
        }
    }
}