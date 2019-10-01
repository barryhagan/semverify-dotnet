#nullable enable
using Semverify.Tests.TestAttributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestModel
{
    public abstract class ConstraintsApi
    {
        [ApiSignature("public abstract TEnum Semverify.Tests.TestModel.ConstraintsApi.MapEnum<TEnum>(string? pgName = null) where TEnum : struct, Enum;", Isolate = true)]
        public abstract TEnum MapEnum<TEnum>(string? pgName = null) where TEnum : struct, Enum;
    }
}
