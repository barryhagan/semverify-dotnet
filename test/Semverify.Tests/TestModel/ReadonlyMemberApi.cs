using Semverify.Tests.TestAttributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestModel
{
    public struct Point
    {
        [ApiSignature("public readonly string Semverify.Tests.TestModel.Point.Name;", Isolate = true)]
        public readonly string Name;

        public double X { get; set; }
        public double Y { get; set; }

        [ApiSignature("public readonly double Semverify.Tests.TestModel.Point.Distance { get; }")]
        public readonly double Distance => Math.Sqrt(X * X + Y * Y);

        [ApiSignature("public readonly override string Semverify.Tests.TestModel.Point.ToString()")]
        public readonly override string ToString() =>
            $"({X}, {Y}) is {Distance} from the origin";
    }
}
