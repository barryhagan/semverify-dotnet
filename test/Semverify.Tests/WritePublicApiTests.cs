using Semverify.ApiModel;
using Semverify.Tests.TestAttributes;
using Semverify.Tests.TestModel;
using Shouldly;
using System.IO;
using System.Linq;
using Xunit;

namespace Semverify.Tests
{
    public class WriteApiTests
    {
        [Fact]
        public void write_assembly_api()
        {
            var assembly = typeof(InheritanceApi).Assembly;
            var module = new ApiModuleInfo(assembly.Modules.First());
            File.WriteAllText(Path.Combine(Path.GetTempPath(), module.Name + ".api.txt"), module.FormatForApiOutput());
        }

        [Fact]
        public void write_assembly_signatures()
        {
            var assembly = typeof(InheritanceApi).Assembly;
            var module = new ApiModuleInfo(assembly.Modules.First());
            File.WriteAllLines(Path.Combine(Path.GetTempPath(), module.Name + ".signatures.txt"), module.EnumerateAllMembers().Select(m => m.GetSignature()));
        }

        [Fact]
        public void signatures_match_expected_values()
        {
            var api = new ApiTypeInfo(typeof(EventsApi));
            var apiMembers = api.EnumerateMembers();

            foreach (var apiMember in apiMembers)
            {
                if (apiMember.MemberInfo.GetCustomAttributes(typeof(SignatureAttribute), false).FirstOrDefault() is SignatureAttribute expectedSignature)
                {
                    apiMember.GetSignature().ShouldBe(expectedSignature.Value);
                }
            }
        }

        [Fact]
        public void local_names_match_expected_values()
        {
            var api = new ApiTypeInfo(typeof(EventsApi));
            var apiMembers = api.EnumerateMembers();

            foreach (var apiMember in apiMembers)
            {
                if (apiMember.MemberInfo.GetCustomAttributes(typeof(LocalNameAttribute), false).FirstOrDefault() is LocalNameAttribute expectedName)
                {
                    apiMember.GetLocalName().ShouldBe(expectedName.Value);
                }
            }
        }
    }
}
