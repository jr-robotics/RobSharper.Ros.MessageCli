using System;
using System.Drawing;
using System.IO;
using System.Linq;
using RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public static partial class CodeGeneration
    {
        public static int Execute(CodeGenerationOptions options, IKeyedTemplateFormatter templateEngine)
        {
            CodeGenerationContext context;

            try
            {
                context = CodeGenerationContext.Create(options.PackagePath);
            }
            catch (DirectoryNotFoundException e)
            {
                Colorful.Console.WriteLine(e.Message, Color.Red);
                return 2;
            }

            if (!context.Packages.Any())
            {
                Colorful.Console.WriteLine("Package directory does not contain any packages.");
                return 0;
            }
            
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

                    try
                    {
                        generator.Execute();
                    }
                    catch (Exception e)
                    {
                        Colorful.Console.WriteLine();
                        Colorful.Console.WriteLine($"Could not process message package {package.PackageInfo.Name} [{package.PackageInfo.Version}]", Color.Red);
                        Colorful.Console.WriteLine(e.Message, Color.Red);
                        Colorful.Console.WriteLine();
                        
                        return 1;
                    }
                }
            }

            return 0;
        }
    }
}