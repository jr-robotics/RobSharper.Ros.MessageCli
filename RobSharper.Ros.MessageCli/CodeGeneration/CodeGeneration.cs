using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public static partial class CodeGeneration
    {
        public static void Execute(CodeGenerationOptions options, IRosPackageGeneratorFactory packageGeneratorFactory)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (packageGeneratorFactory == null) throw new ArgumentNullException(nameof(packageGeneratorFactory));
            
            CodeGenerationContext context;

            try
            {
                var packageFolders = RosPackageFolder
                    .Find(options.PackagePath, RosPackageFolder.BuildType.Mandatory)
                    .ConcatFolders(options.DependencyPackagePaths)
                    .ConcatRosPackagePathFolders(!options.IgnoreRosPackagePath)
                    .RemoveDuplicates()
                    .SetMandatoryPackages(options.Filter)
                    .ToList();
                
                context = CodeGenerationContext.Create(packageFolders);
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
            
            if (options.Filter != null && options.Filter.Any())
            {
                Colorful.Console.WriteLine($"Building {context.Packages.Count()} packages filtered with '{string.Join(' ', options.Filter)}'.");
            }
            else
            {
                Colorful.Console.WriteLine($"Building {context.Packages.Count()} packages");
            }
            
            using (var directories = new CodeGenerationDirectoryContext(options.OutputPath, options.PreserveGeneratedCode))
            {
                var buildOrder = new BuildOrderer(context);
                
                // Set build order depending on package dependencies
                try
                {
                    buildOrder.Sort();
                }
                catch (Exception e)
                {
                    Colorful.Console.WriteLine(e.Message, Color.Red);
                    Environment.ExitCode |= (int) ExitCodes.CouldNotDetermineBuildSequence;
                    
                    return;
                }
                
                foreach (var package in buildOrder.Packages)
                {
                    // Create Package
                    var packageDirectories = directories.GetPackageTempDir(package.PackageInfo);
                    var generator = packageGeneratorFactory.CreateMessagePackageGenerator(options, package, packageDirectories);

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