using System;
using System.Drawing;
using System.IO;
using System.Linq;
using RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage;
using RobSharper.Ros.MessageCli.CodeGeneration.RosTargets.UmlRobotics;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;
using Console = Colorful.Console;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public static partial class CodeGeneration
    {
        public static void Execute(CodeGenerationOptions options, IKeyedTemplateFormatter templateEngine)
        {
            CodeGenerationContext context;

            try
            {
                context = CodeGenerationContext.Create(options.PackagePath);
            }
            catch (DirectoryNotFoundException e)
            {
                Colorful.Console.WriteLine(e.Message, Color.Red);
                Environment.ExitCode |= (int) ExitCodes.RosPackagePathNotFound;
                return;
            }

            if (!context.Packages.Any())
            {
                Colorful.Console.WriteLine("Package directory does not contain any packages.");
                Environment.ExitCode |= (int) ExitCodes.Success;
                return;
            }
            
            using (var directories = new CodeGenerationDirectoryContext(options.OutputPath, options.PreserveGeneratedCode))
            {
                // Parse message files and build package dependency graph
                context.ParseMessages();

                if (options.Filter != null && options.Filter.Any())
                {
                    context.FilterPackages(options.Filter);
                    Colorful.Console.WriteLine($"Building {context.Packages.Count()} packages filtered with '{string.Join(' ', options.Filter)}'.");
                }
                else
                {
                    Colorful.Console.WriteLine($"Building {context.Packages.Count()} packages");
                }


                // Set build order depending on package dependencies
                try
                {
                    context.ReorderPackagesForBuilding();
                }
                catch (Exception e)
                {
                    Colorful.Console.WriteLine(e.Message, Color.Red);
                    Environment.ExitCode |= (int) ExitCodes.CouldNotDetermineBuildSequence;
                        
                    return;
                }
                
                foreach (var package in context.Packages)
                {
                    // Create Package
                    var packageDirectories = directories.GetPackageTempDir(package.PackageInfo);
                    var generator = new UmlRoboticsMessagePackageGenerator(package, options, packageDirectories, templateEngine);

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

                        Environment.ExitCode |= (int) ExitCodes.CouldNotProcessPackage;
                        
                        return;
                    }
                }
            }
        }
    }
}