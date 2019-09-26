using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestAttributes
{
    internal class ApiSignatureAttribute : Attribute
    {
        public string Value { get; private set; }
        public bool Isolate { get; set; }

        public ApiSignatureAttribute(string value)
        {
            Value = value;
        }
    }
}
