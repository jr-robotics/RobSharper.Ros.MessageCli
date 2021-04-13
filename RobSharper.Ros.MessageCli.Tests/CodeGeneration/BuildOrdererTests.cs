using System.Linq;
using FluentAssertions;
using RobSharper.Ros.MessageCli.CodeGeneration;
using Xunit;

namespace RobSharper.Ros.MessageCli.Tests.CodeGeneration
{
    public class BuildOrdererTests
    {
        [Fact]
        public void Can_reorder_packages_for_building_if_only_one_package_should_be_built()
        {
            var context = CodeGenerationContext.Create(TestUtils.CreatePackagePath("std_msgs"));
            var orderer = new BuildOrderer(context);
            
            orderer.Sort();
            orderer.Packages.Count().Should().Be(1);
        }

        [Fact]
        public void Can_reorder_packages_for_building_dependent_packages()
        {
            var context = CodeGenerationContext.Create(TestUtils.CreatePackagePath("common_msgs"));
            var orderer = new BuildOrderer(context);
            
            orderer.Sort();
            orderer.Packages.Count().Should().Be(10);
            orderer.Packages.Last().PackageInfo.Name.Should().Be("common_msgs", "the meta package should be built as last package");
        }

        [Fact]
        public void Reorder_packages_for_building_throws_exception_if_circular_dependencies_detected()
        {
            var context = CodeGenerationContext.Create(TestUtils.CreatePackagePath(false, "circular_msgs"));
            var orderer = new BuildOrderer(context);

            orderer.Invoking(x => x.Sort())
                .Should().Throw<CircularPackageDependencyException>();
            
        }
    }
}