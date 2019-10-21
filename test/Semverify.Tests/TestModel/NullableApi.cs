#nullable enable
using Semverify.Tests.TestAttributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestModel
{
    public class NullableApi
    {
        public class ReturnType
        {
            public string? ReturnProperty { get; set; }
        }

        public class ReturnArgs : EventArgs
        {
            public string? Target { get; set; }
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public NullableApi(string? nullableLabel, string nope)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {

        }

        [ApiSignature("public event System.EventHandler<Semverify.Tests.TestModel.NullableApi.ReturnArgs?> Semverify.Tests.TestModel.NullableApi.ReturnEvent;")]
        public event EventHandler<ReturnArgs?> ReturnEvent { add { } remove { } }
        
        [ApiSignature("protected delegate string? Semverify.Tests.TestModel.NullableApi.OnNullableReturn(object sender, Semverify.Tests.TestModel.NullableApi.ReturnArgs? args);")]
        protected delegate string? OnNullableReturn(object sender, ReturnArgs? args);

        [ApiSignature("protected delegate string? Semverify.Tests.TestModel.NullableApi.OnNullableReturn<T>(object sender, Semverify.Tests.TestModel.NullableApi.ReturnArgs? args, T? gen) where T : class;")]
        protected delegate string? OnNullableReturn<T>(object sender, ReturnArgs? args, T? gen) where T : class;

        protected delegate string OnReturn(object sender, ReturnArgs? args);

        public ReturnType NonNullField = new ReturnType();

        [ApiSignature("public Semverify.Tests.TestModel.NullableApi.ReturnType? Semverify.Tests.TestModel.NullableApi.NullableField;")]
        public ReturnType? NullableField;

        public ReturnType NonNullProperty { get; protected set; } = new ReturnType();

        [ApiSignature("public Semverify.Tests.TestModel.NullableApi.ReturnType? Semverify.Tests.TestModel.NullableApi.NullableProperty { get; set; }")]
        public ReturnType? NullableProperty { get; set; }

        [ApiSignature("public Semverify.Tests.TestModel.ReturnType[]?[]? Semverify.Tests.TestModel.NullableApi.NullableMethod() { }")]
        public ReturnType[]?[]? NullableMethod() { return null; }

        [ApiSignature("public System.Collections.Generic.IEnumerable<Semverify.Tests.TestModel.NullableApi.ReturnType?>? Semverify.Tests.TestModel.NullableApi.NullableEnumerable() { }")]
        public IEnumerable<ReturnType?>? NullableEnumerable() { return null; }

        [ApiSignature("public Semverify.Tests.TestModel.NullableApi.ReturnType Semverify.Tests.TestModel.NullableApi.NullableParamMethod(string? nullableParam, string nonNullParam, int? nullableValueType) { }")]
        public ReturnType NullableParamMethod(string? nullableParam, string nonNullParam, int? nullableValueType) { return new ReturnType(); }

        [ApiSignature("public Semverify.Tests.TestModel.NullableApi.ReturnType? Semverify.Tests.TestModel.NullableApi.NullableParamAndReturnMethod(string? nullableParam, string nonNullParam, int? nullableValueType) { }")]
        public ReturnType? NullableParamAndReturnMethod(string? nullableParam, string nonNullParam, int? nullableValueType) { return null; }

        [ApiSignature("public Semverify.Tests.TestModel.NullableApi.ReturnType? Semverify.Tests.TestModel.NullableApi.NullableGenericMethod<T1, T2, T3>(T1? t1, T2 t2, T3? t3) where T1 : class where T2 : class where T3 : class { }")]
        public ReturnType? NullableGenericMethod<T1, T2, T3>(T1? t1, T2 t2, T3? t3) where T1 : class where T2 : class where T3 : class { return null; }

        [ApiSignature("public System.Collections.Generic.Dictionary<System.Collections.Generic.List<string?>, string[]?> Semverify.Tests.TestModel.NullableApi.SkeetExample;")]
        public Dictionary<List<string?>, string[]?> SkeetExample = new Dictionary<List<string?>, string[]?>();

        [ApiSignature("public System.Collections.Generic.Dictionary<System.Collections.Generic.List<string?>, string?[]> Semverify.Tests.TestModel.NullableApi.SkeetExample2;")]
        public Dictionary<List<string?>, string?[]> SkeetExample2 = new Dictionary<List<string?>, string?[]>();

        [ApiSignature("public System.Collections.Generic.Dictionary<System.Collections.Generic.List<string?>, string?[]?> Semverify.Tests.TestModel.NullableApi.SkeetExample3;")]
        public Dictionary<List<string?>, string?[]?> SkeetExample3 = new Dictionary<List<string?>, string?[]?>();

        [ApiSignature("public System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<int, int?>?>>? Semverify.Tests.TestModel.NullableApi.ComplicatedDictionary { get; set; }")]
        public Dictionary<string, Dictionary<string, Dictionary<int, int?>?>>? ComplicatedDictionary { get; set; }

        [ApiSignature("public System.Tuple<string, System.Collections.Generic.List<int?>, System.Collections.Generic.Dictionary<long, System.Collections.Generic.List<System.ValueTuple<string?, bool?>>>, string?, Semverify.Tests.TestModel.NullableApi.ReturnType, System.Collections.Concurrent.ConcurrentDictionary<string, Semverify.Tests.TestModel.NullableApi.ReturnType?>> Semverify.Tests.TestModel.NullableApi.WhyOnEarth { get; protected set; }")]
        public Tuple<string, List<int?>, Dictionary<long, List<(string?, bool?)>>, string?, ReturnType, ConcurrentDictionary<string, ReturnType?>> WhyOnEarth { get; protected set; }

        [ApiSignature("public bool Semverify.Tests.TestModel.NullableApi.ByRefNullableReferenceParam(Semverify.Tests.TestModel.NullableApi.ReturnType rt1, ref Semverify.Tests.TestModel.NullableApi.ReturnType? rt2, Semverify.Tests.TestModel.NullableApi.ReturnType rt3, Semverify.Tests.TestModel.NullableApi.ReturnType? rt4, out Semverify.Tests.TestModel.NullableApi.ReturnType? rt5, Semverify.Tests.TestModel.NullableApi.ReturnType rt6) { }")]
        public bool ByRefNullableReferenceParam(ReturnType rt1, ref ReturnType? rt2, ReturnType rt3, ReturnType? rt4, out ReturnType? rt5, ReturnType rt6) { rt5 = null; return false; }

        [ApiSignature("public override bool Semverify.Tests.TestModel.NullableApi.Equals(object? obj) { }")]
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();  
        }
    }
}
