#nullable enable
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

        public NullableApi(string? nullableLabel, string nope)
        {

        }

        public event EventHandler<ReturnArgs?>? ReturnEvent { add { } remove { } }
        protected delegate string? OnNullableReturn(object sender, ReturnArgs? args);
        protected delegate string OnReturn(object sender, ReturnArgs? args);

        public ReturnType NonNullField = new ReturnType();
        public ReturnType? NullableField;

        public ReturnType NonNullProperty { get; protected set; } = new ReturnType();
        public ReturnType? NullableProperty { get; set; }

        public ReturnType? NullableMethod() { return null; }
        public ReturnType NullableParamMethod(string? nullableParam, string nonNullParam, int? nullableValueType) { return new ReturnType(); }
        public ReturnType? NullableParamAndReturnMethod(string? nullableParam, string nonNullParam, int? nullableValueType) { return null; }

        public ReturnType? NullableGenericMethod<T1, T2, T3>(T1? t1, T2 t2, T3? t3) where T1 : class where T2 : class where T3 : class { return null; }

        public Dictionary<List<string?>, string[]?> SkeetExample = new Dictionary<List<string?>, string[]?>();
        public Dictionary<List<string?>, string?[]> SkeetExample2 = new Dictionary<List<string?>, string?[]>();
        public Dictionary<List<string?>, string?[]?> SkeetExample3 = new Dictionary<List<string?>, string?[]?>();
        public Dictionary<string, Dictionary<string, Dictionary<int, int?>?>>? ComplicatedDictionary { get; set; }
        public Tuple<string, List<int?>, Dictionary<long, List<(string?, bool?)>>, string?, ReturnType, ConcurrentDictionary<string, ReturnType?>> WhyOnEarth { get; protected set; }


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
