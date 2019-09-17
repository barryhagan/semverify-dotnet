using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestAttributes
{
    internal class LocalNameAttribute : Attribute
    {
        public string Value { get; private set; }

        public LocalNameAttribute(string value)
        {
            Value = value;
        }
    }
}
