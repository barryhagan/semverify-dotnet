using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Semverify.ApiModel
{
    internal class ApiInterfaceInfo : ApiTypeInfo
    {
        public ApiInterfaceInfo(MemberInfo memberInfo) : base(memberInfo)
        {
        }

        public override IList<string> GetModifiers()
        {
            var mods = GetTypeAccessModifiers();
            mods.Add("interface");
            return mods;
        }

        protected override IList<string> GetInterfaces()
        {
            var implList = new List<string>();
            var interfaces = TypeInfo.GetInterfaces();
            var inheritedInterfaces = interfaces.SelectMany(i => i.GetInterfaces());
            foreach (var intface in interfaces.Except(inheritedInterfaces))
            {
                implList.Add(intface.ResolveQualifiedName());
            }
            return implList;
        }

    }
}
