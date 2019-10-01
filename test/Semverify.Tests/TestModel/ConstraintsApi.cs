#nullable enable
using Semverify.Tests.TestAttributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestModel
{
    public abstract class ConstraintsApi
    {
        [ApiSignature("public abstract TEnum Semverify.Tests.TestModel.ConstraintsApi.MapEnum<TEnum>(string? pgName = null) where TEnum : struct, Enum;")]
        public abstract TEnum MapEnum<TEnum>(string? pgName = null) where TEnum : struct, Enum;

        [ApiSignature("public abstract T Semverify.Tests.TestModel.ConstraintsApi.NewConstraint<T>(string? pgName = null) where T : System.IDisposable, new();", Isolate = true)]
        public abstract T NewConstraint<T>(string? pgName = null) where T : IDisposable, new();

        [ApiSignature("public abstract T Semverify.Tests.TestModel.ConstraintsApi.NewTupleConstraint<T, T1>(string? pgName = null) where T : System.Tuple<T1>, new();", Isolate = true)]
        public abstract T NewTupleConstraint<T, T1>(string? pgName = null) where T : Tuple<T1>, new();

        [ApiSignature("public abstract T Semverify.Tests.TestModel.ConstraintsApi.NewClassConstraint<T>(string? pgName = null) where T : class, new();", Isolate = true)]
        public abstract T NewClassConstraint<T>(string? pgName = null) where T : class, new();

    }
}
