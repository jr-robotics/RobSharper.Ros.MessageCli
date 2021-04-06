using System;
using System.Collections.Generic;
using System.IO;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public readonly struct RosPackageFolder
    {
        public enum BuildType
        {

            Mandatory,
            Optional
        }
        
        public string Path { get; }
        
        public BuildType BuildStrategy { get; }

        public RosPackageFolder(string path, BuildType type)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            path = System.IO.Path.GetFullPath(path);
            
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory {path} does not exit.");
            }

            if (!RosPackageInfo.IsPackageFolder(path))
            {
                throw new InvalidOperationException($"Directory {path} is not a ROS package folder (package.xml file is missing).");
            }
            
            Path = path;
            BuildStrategy = type;
        }

        public static IEnumerable<RosPackageFolder> Find(string basePath, BuildType type)
        {
            if (basePath == null) throw new ArgumentNullException(nameof(basePath));
            
            basePath = System.IO.Path.GetFullPath(basePath);
            
            if (!Directory.Exists(basePath))
            {
                throw new DirectoryNotFoundException($"Directory {basePath} does not exit.");
            }

            return FindInternal(basePath, type);
        }
        
        private static IEnumerable<RosPackageFolder> FindInternal(string basePath, BuildType type)
        {
            var packageFolders = new List<RosPackageFolder>();
            
            if (RosPackageInfo.IsPackageFolder(basePath))
            {
                packageFolders.Add(new RosPackageFolder(basePath, type));
            }
            else
            {
                foreach (var directory in Directory.GetDirectories(basePath))
                {
                    packageFolders.AddRange(FindInternal(directory, type));
                }
            }

            return packageFolders;
        }
    }
}