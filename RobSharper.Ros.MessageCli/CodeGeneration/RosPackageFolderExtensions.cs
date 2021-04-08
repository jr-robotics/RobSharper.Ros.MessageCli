using System;
using System.Collections.Generic;
using System.Linq;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public static class RosPackageFolderExtensions
    {
        /// <summary>
        /// Removes packages with the same Name
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
                if (packageFolder.TryGetPackageInfo(out var packageInfo))
                {
                    if (names.Contains(packageInfo.Name))
                        continue;

                    names.Add(packageInfo.Name);
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
    }
}