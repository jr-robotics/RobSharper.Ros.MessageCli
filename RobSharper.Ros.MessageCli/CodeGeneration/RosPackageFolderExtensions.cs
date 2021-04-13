using System;
using System.Collections.Generic;
using System.Linq;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public static class RosPackageFolderExtensions
    {
        /// <summary>
        /// Removes packages with the same name or path
        /// </summary>
        /// <param name="packageFolders"></param>
        /// <returns></returns>
        public static IEnumerable<RosPackageFolder> RemoveDuplicates(this IEnumerable<RosPackageFolder> packageFolders)
        {
            if (packageFolders == null)
                return null;
            
            var names = new HashSet<string>();
            var result = new List<RosPackageFolder>();
            
            foreach (var packageFolder in packageFolders)
            {
                string name;
                
                if (packageFolder.TryGetPackageInfo(out var packageInfo))
                {
                    name = packageInfo.Name;
                }
                else
                {
                    name = "PATH::" + packageFolder.Path;
                }

                if (!names.Contains(name))
                {
                    names.Add(name);
                    result.Add(packageFolder);
                }
            }

            return result;
        }
        
        /// <summary>
        /// Sets the BuildStrategy property of all package folders.
        /// </summary>
        /// <param name="mandatoryPackages">
        /// List of mandatory packages. If null or empty, the packageFolders are not modified.
        /// The mandatoryPackages may also contain the asterisk (*) as first or last character to specify
        /// an "ends with" or "starts with" expression.
        /// </param>
        public static IEnumerable<RosPackageFolder> SetMandatoryPackages(
            this IEnumerable<RosPackageFolder> packageFolders, IEnumerable<string> mandatoryPackages)
        {
            if (packageFolders == null)
                return null;

            if (mandatoryPackages == null || !mandatoryPackages.Any())
                return packageFolders;

            var innerFilters = new List<Func<string, bool>>();

            foreach (var filter in mandatoryPackages)
            {
                var f = filter;
                Func<string, bool> innerFilter;

                if (f.StartsWith("*"))
                {
                    innerFilter = s => s.EndsWith(f.Substring(1));
                }
                else if (f.EndsWith("*"))
                {
                    innerFilter = s => s.StartsWith(f.Substring(0, f.Length - 1));
                }
                else
                {
                    innerFilter = s => s.Equals(f);
                }

                innerFilters.Add(innerFilter);
            }

            if (innerFilters.Count == 0)
                return packageFolders;

            Func<string, bool> filterExpr;

            if (innerFilters.Count == 1)
            {
                filterExpr = innerFilters.First();
            }
            else
            {
                filterExpr = s => innerFilters.Any(f => f(s));
            }

            return SetMandatoryPackages(packageFolders, filterExpr);
        }

        public static IEnumerable<RosPackageFolder> SetMandatoryPackages(
            this IEnumerable<RosPackageFolder> packageFolders, Func<string, bool> filterExpr)
        {
            if (filterExpr == null) throw new ArgumentNullException(nameof(filterExpr));

            foreach (var packageFolder in packageFolders)
            {
                var isMandatory = false;
                
                if (packageFolder.TryGetPackageInfo(out var packageInfo))
                {
                    isMandatory = filterExpr(packageInfo.Name);
                }

                packageFolder.BuildStrategy =
                    isMandatory ? RosPackageFolder.BuildType.Mandatory : RosPackageFolder.BuildType.Optional;
            }

            return packageFolders;
        }

        public static IEnumerable<RosPackageFolder> ConcatFolder(this IEnumerable<RosPackageFolder> packageFolders,
            string folder,
            RosPackageFolder.BuildType type = RosPackageFolder.BuildType.Optional)
        {
            if (string.IsNullOrEmpty(folder))
                return packageFolders;

            var newItems = CreateRosPackageFolders(folder, type);
            return packageFolders.Concat(newItems);
        }

        public static IEnumerable<RosPackageFolder> ConcatFolders(this IEnumerable<RosPackageFolder> packageFolders,
            IEnumerable<string> folders,
            RosPackageFolder.BuildType type = RosPackageFolder.BuildType.Optional)
        {
            if (folders == null)
                return packageFolders;

            var newItems = folders
                .SelectMany(f => CreateRosPackageFolders(f, type));

            return packageFolders.Concat(newItems);
        }
        
        /// <summary>
        /// Add paths defined in $ROS_PACKAGE_PATH.
        /// </summary>
        /// <param name="packageFolders"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<RosPackageFolder> ConcatRosPackagePathFolders(this IEnumerable<RosPackageFolder> packageFolders, RosPackageFolder.BuildType type = RosPackageFolder.BuildType.Optional)
        {
            var rosPackagePath = Environment.GetEnvironmentVariable("ROS_PACKAGE_PATH");
            return ConcatFolder(packageFolders, rosPackagePath, type);
        }

        /// <summary>
        /// Add paths defined in $ROS_PACKAGE_PATH if condition is true.
        /// </summary>
        /// <param name="packagefolders"></param>
        /// <param name="condition">Folders in $ROS_PACKAGE_PATH are only add if this parameter is true</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<RosPackageFolder> ConcatRosPackagePathFolders(this IEnumerable<RosPackageFolder> packagefolders, bool condition, RosPackageFolder.BuildType type = RosPackageFolder.BuildType.Optional)
        {
            if (!condition)
                return packagefolders;

            return ConcatRosPackagePathFolders(packagefolders, type);
        }

        private static IEnumerable<RosPackageFolder> CreateRosPackageFolders(string folder, RosPackageFolder.BuildType type)
        {
            if (folder == null)
                return Enumerable.Empty<RosPackageFolder>();

            var result = folder
                .Split(':')
                .SelectMany(x => RosPackageFolder.Find(x, type))
                .ToList();

            return result;
        }
    }
}