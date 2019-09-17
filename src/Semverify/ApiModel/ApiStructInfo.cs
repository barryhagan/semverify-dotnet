using System.Collections.Generic;
using System.Reflection;

namespace Semverify.ApiModel
{
    internal class ApiStructInfo : ApiTypeInfo
    {
        public ApiStructInfo(MemberInfo memberInfo) : base(memberInfo)
        {
        }

        public override IList<string> GetModifiers()
        {
            var mods = GetTypeAccessModifiers();
            mods.Add("struct");
            return mods;
        }
    }
}
