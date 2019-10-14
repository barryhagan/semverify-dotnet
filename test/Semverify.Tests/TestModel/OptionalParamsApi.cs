using Semverify.Tests.TestAttributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestModel
{
    public class OptionalParamsApi
    {
        [ApiSignature("public void Semverify.Tests.TestModel.OptionalParamsApi.BoolTrue(bool arg = True) { }")]
        public void BoolTrue(bool arg = true)
        {
        }

        [ApiSignature("public void Semverify.Tests.TestModel.OptionalParamsApi.BoolFalse(bool arg = False) { }")]
        public void BoolFalse(bool arg = false)
        {
        }

        [ApiSignature("public void Semverify.Tests.TestModel.OptionalParamsApi.BoolDefault(bool arg = False) { }")]
        public void BoolDefault(bool arg = default)
        {
        }

        [ApiSignature("public void Semverify.Tests.TestModel.OptionalParamsApi.BoolDefaultExplicit(bool arg = False) { }")]
        public void BoolDefaultExplicit(bool arg = default(bool))
        {
        }

        [ApiSignature("public void Semverify.Tests.TestModel.OptionalParamsApi.GuidDefault(System.Guid arg = default) { }")]
        public void GuidDefault(Guid arg = default)
        {
        }

        [ApiSignature("public void Semverify.Tests.TestModel.OptionalParamsApi.GuidNew(System.Guid arg = default) { }")]
        public void GuidNew(Guid arg = new Guid())
        {
        }

        [ApiSignature("public void Semverify.Tests.TestModel.OptionalParamsApi.NullableGuidNew(System.Guid? arg = null) { }")]
        public void NullableGuidNew(Guid? arg = default)
        {
        }

        [ApiSignature("public void Semverify.Tests.TestModel.OptionalParamsApi.NullableGuidNull(System.Guid? arg = null) { }")]
        public void NullableGuidNull(Guid? arg = null)
        {
        }
    }
}
