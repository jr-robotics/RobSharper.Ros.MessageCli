using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;
using RobSharper.Ros.MessageCli.ColorfulConsoleLogging;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public static partial class CodeGeneration
    {
        private static readonly ILogger Logger = LoggingHelper.Factory.CreateLogger(typeof(CodeGeneration));
        
        public static void Execute(CodeGenerationOptions options, IRosPackageGeneratorFactory packageGeneratorFactory)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
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
            catch (Exception e)
            {
                Logger.LogError(e, "Could not find all mandatory packages.");
                
                Environment.ExitCode |= (int) ExitCodes.RosPackagePathNotFound;
                return;
            }

            if (!context.Packages.Any())
            {
                Logger.LogInformation("Package directory does not contain any packages.");
                Environment.ExitCode |= (int) ExitCodes.Success;
                return;
            }
            
            if (options.Filter != null && options.Filter.Any())
            {
                Logger.LogInformation($"Building {context.Packages.Count()} packages filtered with '{string.Join(' ', options.Filter)}'.");
            }
            else
            {
                Logger.LogInformation($"Building {context.Packages.Count()} packages");
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
                    Logger.LogError(e, "Could not determine build sequence.");
                    Environment.ExitCode |= (int) ExitCodes.CouldNotDetermineBuildSequence;
                    return;
                }
                
                var packageStopwatch = new Stopwatch();
                
                foreach (var package in buildOrder.Packages)
                {
                    packageStopwatch.Restart();
                    
                    // Create Package
                    var packageDirectories = directories.GetPackageTempDir(package.PackageInfo);
                    var generator = packageGeneratorFactory.CreateMessagePackageGenerator(options, package, packageDirectories);

                    try
                    {
                        generator.Execute();
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e,
                            $"Could not process message package {package.PackageInfo.Name} [{package.PackageInfo.Version}]");
                        Environment.ExitCode |= (int) ExitCodes.CouldNotProcessPackage;

                        return;
                    }
                    finally
                    {
                        packageStopwatch.Stop();
                        Logger.LogInformation($"Time Elapsed {packageStopwatch.Elapsed:hh\\:mm\\:ss\\.ff}");
                        Logger.LogInformation(string.Empty);
                    }
                }
            }
            
            stopwatch.Stop();
            Logger.LogInformation($"Total Time Elapsed {stopwatch.Elapsed:hh\\:mm\\:ss\\.ff}");
        }
    }
}