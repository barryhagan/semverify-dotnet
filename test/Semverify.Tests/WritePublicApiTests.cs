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
        public void signatures_match_expected_values()
        {
            var api = new ApiModuleInfo(typeof(WriteApiTests).Assembly.Modules.First());
            var apiMembers = api.EnumerateAllMembers();

            foreach (var apiMember in apiMembers)
            {
                if (apiMember.MemberInfo.GetCustomAttributes(typeof(ApiSignatureAttribute), false).FirstOrDefault() is ApiSignatureAttribute expectedSignature)
                {
                    apiMember.GetSignature().ShouldBe(expectedSignature.Value);
                }
            }
        }

        // Runs only the signature attributes with Isolate = true
        // useful when setting up and debugging new tests.
        [Fact]       
        public void isolated_signatures_match_expected_values()
        {
            var api = new ApiModuleInfo(typeof(WriteApiTests).Assembly.Modules.First());
            var apiMembers = api.EnumerateAllMembers();

            foreach (var apiMember in apiMembers)
            {
                if (apiMember.MemberInfo.GetCustomAttributes(typeof(ApiSignatureAttribute), false).FirstOrDefault() is ApiSignatureAttribute expectedSignature && expectedSignature.Isolate)
                {
                    apiMember.GetSignature().ShouldBe(expectedSignature.Value);
                }
            }
        }

        [Fact]
        public void local_names_match_expected_values()
        {
            var api = new ApiModuleInfo(typeof(WriteApiTests).Assembly.Modules.First());
            var apiMembers = api.EnumerateAllMembers();

            foreach (var apiMember in apiMembers)
            {
                if (apiMember.MemberInfo.GetCustomAttributes(typeof(ApiLocalNameAttribute), false).FirstOrDefault() is ApiLocalNameAttribute expectedName)
                {
                    apiMember.GetLocalName().ShouldBe(expectedName.Value);
                }
            }
        }
    }
}
