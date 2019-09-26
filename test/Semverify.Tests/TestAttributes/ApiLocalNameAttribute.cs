using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestAttributes
{
    internal class ApiLocalNameAttribute : Attribute
    {
        public string Value { get; private set; }

        public ApiLocalNameAttribute(string value)
        {
            Value = value;
        }
    }
}
