using FluentAssertions;
using Joanneum.Robotics.Ros.MessageParser.Cli.CodeGeneration;
using Joanneum.Robotics.Ros.MessageParser.Cli.CodeGeneration.MessagePackage;
using Moq;
using Xunit;

namespace RobSharper.Ros.MessageParser.Cli.Tests.CodeGeneration
{
    public class PackageRegistryMessageParserAdapterTests : IRosMessagePackageParserTests
    {
        private static IBuildPackages CreateBuildPackages(params RosPackageInfo[] buildPackages)
        {
            var buildPackagesMock = new Mock<IBuildPackages>();
            buildPackagesMock
                .Setup(x => x.Packages)
                .Returns(buildPackages);

            return buildPackagesMock.Object;
        }
        
        protected override IRosMessagePackageParser CreateParser(RosPackageInfo package)
        {
            var buildPackages = CreateBuildPackages(package);

            var packageRegistry = new PackageRegistry(buildPackages);
            var innerParser = new RosMessagePackageParser(package, buildPackages);
            
            return CreateParser(innerParser, packageRegistry);
        }

        protected PackageRegistryMessageParserAdapter CreateParser(IRosMessagePackageParser innerParser, PackageRegistry packageRegistry)
        {
            var parser = new PackageRegistryMessageParserAdapter(packageRegistry, innerParser);
            return parser;
        }
        
        [Fact]
        public void Parse_messages_adds_dependencies_to_package_registry()
        {
            var package = TestUtils.CreatePackageInfo("common_msgs", "nav_msgs");
            var buildPackages = CreateBuildPackages(package);
            
            var innerParser = new RosMessagePackageParser(package, buildPackages);
            var packageRegistry = new PackageRegistry(buildPackages);
            
            var target = CreateParser(innerParser, packageRegistry);
            
            
            target.ParseMessages();

            packageRegistry.Items.Should().NotBeNull();
            packageRegistry.Items.Should().NotBeEmpty();
            
            packageRegistry.Items.Keys.Should().Contain("geometry_msgs");
            packageRegistry.Items.Keys.Should().Contain("std_msgs");
            packageRegistry.Items.Keys.Should().Contain("actionlib_msgs");
            packageRegistry.Items.Count.Should().Be(3);
        }
    }
}