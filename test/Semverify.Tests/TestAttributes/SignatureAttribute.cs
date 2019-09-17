using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestAttributes
{
    internal class SignatureAttribute : Attribute
    {
        public string Value { get; private set; }

        public SignatureAttribute(string value)
        {
            Value = value;
        }
    }
}
