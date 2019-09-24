using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Semverify.ApiModel
{
    internal class MemberInfoDisplayComparer : IComparer<ApiMemberInfo>
    {
        public int Compare([AllowNull] ApiMemberInfo x, [AllowNull] ApiMemberInfo y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            var memberCompare = OrderForMemberType(x.MemberType).CompareTo(OrderForMemberType(y.MemberType));
            if (memberCompare != 0) return memberCompare;

            if (x is ApiEnumFieldInfo xEnumValue && y is ApiEnumFieldInfo yEnumValue)
            {
                var enumCompare = ((int)xEnumValue.GetRawConstantValue()).CompareTo((int)yEnumValue.GetRawConstantValue());
                if (enumCompare != 0) return enumCompare;
            }
          
            var nameCompare = string.CompareOrdinal(x.GetLocalName(), y.GetLocalName());
            if (nameCompare != 0) return nameCompare;

            if (x is ApiConstructorInfo xCtor && y is ApiConstructorInfo yCtor)
            {
                var xParams = xCtor.GetParameters();
                var yParams = yCtor.GetParameters();
                var paramCompare = xParams.Count.CompareTo(yParams.Count);
                if (paramCompare != 0) return paramCompare;
            }

            if (x is ApiMethodInfo xMethod && y is ApiMethodInfo yMethod)
            {
                var xParams = xMethod.GetParameters();
                var yParams = yMethod.GetParameters();
                var paramCompare = xParams.Count.CompareTo(yParams.Count);
                if (paramCompare != 0) return paramCompare;
            }

            return 0;
        }

        private int OrderForMemberType(MemberTypes type)
        {
            return type switch
            {             
                MemberTypes.Field => 0,
                MemberTypes.Constructor => 1,
                MemberTypes.Property => 2,
                MemberTypes.Method => 3,
                MemberTypes.TypeInfo => 4,
                MemberTypes.Event => 5,
                MemberTypes.Custom => 6,
                MemberTypes.NestedType => 5,
                _ => 0,
            };
        }
    }
}
