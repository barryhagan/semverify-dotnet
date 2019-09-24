using Semverify.ApiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shouldly;
using Xunit;

namespace Semverify.Tests
{
    public class MemberInfoDisplayComparerTests
    {
        public enum DisplayTestEnum
        {
            None = 0,
            Two = 2,
            One = 1,
            Same = 100
        }

        public enum DisplayTestLongEnum : long
        {
            A = long.MaxValue - 1,
            B = long.MaxValue - 2,
            Same = 10L
        }

        [Fact]
        public void can_compare_enum_fields_by_value()
        {
            var enumApi = new ApiEnumInfo(typeof(DisplayTestEnum));

            var sortedEnums = enumApi.EnumerateMembers().OrderBy(e => e, new MemberInfoDisplayComparer()).Select(e => e.GetLocalName()).ToArray();
            sortedEnums.ShouldBeEquivalentTo(new[] { nameof(DisplayTestEnum.None), nameof(DisplayTestEnum.One), nameof(DisplayTestEnum.Two), nameof(DisplayTestEnum.Same) });
        }

        [Fact]
        public void can_compare_enum_fields_by_long_underlying_value()
        {
            var enumApi = new ApiEnumInfo(typeof(DisplayTestLongEnum));

            var sortedEnums = enumApi.EnumerateMembers().OrderBy(e => e, new MemberInfoDisplayComparer()).Select(e => e.GetLocalName()).ToArray();
            sortedEnums.ShouldBeEquivalentTo(new[] { nameof(DisplayTestLongEnum.Same), nameof(DisplayTestLongEnum.B), nameof(DisplayTestLongEnum.A) });
        }

        [Fact]
        public void comparing_different_underlying_enum_types_returns_zero()
        {
            var intEnum = new ApiEnumInfo(typeof(DisplayTestEnum)).EnumerateMembers().First(m => m.GetLocalName() == "Same");
            var longEnum = new ApiEnumInfo(typeof(DisplayTestLongEnum)).EnumerateMembers().First(m => m.GetLocalName() == "Same");

            var comparer = new MemberInfoDisplayComparer();
            comparer.Compare(intEnum, longEnum).ShouldBe(0);

        }
    }
}
