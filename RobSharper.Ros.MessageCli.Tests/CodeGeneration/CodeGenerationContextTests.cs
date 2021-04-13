using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using RobSharper.Ros.MessageCli.CodeGeneration;
using Xunit;

namespace RobSharper.Ros.MessageCli.Tests.CodeGeneration
{
    public class CodeGenerationContextTests
    {
        [Fact]
        public void Can_create_CodeGenerationContext_for_standard_package()
        {
            var context = CodeGenerationContext.Create(TestUtils.CreatePackagePath("std_msgs"));

            context.Should().NotBeNull();

            context.Packages.Should().NotContainNulls();
            context.Packages.Should().ContainSingle();

            var package = context.Packages.First().PackageInfo;

            package.Name.Should().Be("std_msgs");
            package.IsMetaPackage.Should().BeFalse();
            package.PackageDirectory.FullName.Should().Be(Path.GetFullPath(TestUtils.CreatePackagePath("std_msgs")));
        }
        
        [Fact]
        public void Can_create_CodeGenerationContext_for_nested_package()
        {
            var context = CodeGenerationContext.Create(TestUtils.CreatePackagePath("control_msgs"));

            context.Should().NotBeNull();

            context.Packages.Should().NotContainNulls();
            context.Packages.Should().ContainSingle();

            var package = context.Packages.First().PackageInfo;
            
            package.Name.Should().Be("control_msgs");
            package.IsMetaPackage.Should().BeFalse();
            package.PackageDirectory.FullName.Should().Be(Path.GetFullPath(TestUtils.CreatePackagePath("control_msgs", "control_msgs")));
        }
        
        [Fact]
        public void Can_create_CodeGenerationContext_for_meta_package()
        {
            var context = CodeGenerationContext.Create(TestUtils.CreatePackagePath("common_msgs"));

            context.Should().NotBeNull();

            context.Packages.Should().NotContainNulls();
            context.Packages.Count().Should().Be(10);

            context.Packages.First(x => x.PackageInfo.Name == "common_msgs").PackageInfo.IsMetaPackage.Should().BeTrue();
        }
    }
}