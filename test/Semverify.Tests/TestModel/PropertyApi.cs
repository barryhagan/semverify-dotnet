using Semverify.Tests.TestAttributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestModel
{
    public class PropertyApi
    {
        [ApiSignature("public string Semverify.Tests.TestModel.PropertyApi.Internal_Public { set; }")]
        public string Internal_Public { internal get; set; }

        [ApiSignature("protected string Semverify.Tests.TestModel.PropertyApi.PrivateProtected_Protected { set; }")]
        protected string PrivateProtected_Protected { private protected get; set; }

        [ApiSignature("public string Semverify.Tests.TestModel.PropertyApi.PrivateProtected_Public { set; }")]
        public string PrivateProtected_Public { private protected get; set; }

        [ApiSignature("protected string Semverify.Tests.TestModel.PropertyApi.Private_Protected { set; }")]
        protected string Private_Protected { private get; set; }

        [ApiSignature("public string Semverify.Tests.TestModel.PropertyApi.Protected_Public { protected get; set; }")]
        public string Protected_Public { protected get; set; }

        [ApiSignature("public string Semverify.Tests.TestModel.PropertyApi.PublicGet { get; }")]
        public string PublicGet { get; }

        [ApiSignature("protected string Semverify.Tests.TestModel.PropertyApi.PublicSet { set; }")]
        protected string PublicSet { set { } }

        [ApiSignature("public string Semverify.Tests.TestModel.PropertyApi.Public_Internal { get; }")]
        public string Public_Internal { get; internal set; }

        [ApiSignature("public string Semverify.Tests.TestModel.PropertyApi.Public_Private { get; }")]
        public string Public_Private { get; private set; }

        [ApiSignature("public string Semverify.Tests.TestModel.PropertyApi.Public_Protected { get; protected set; }")]
        public string Public_Protected { get; protected set; }

        [ApiSignature("public string Semverify.Tests.TestModel.PropertyApi.Public_ProtectedInternal { get; protected internal set; }")]
        public string Public_ProtectedInternal { get; protected internal set; }

        [ApiSignature("protected internal string Semverify.Tests.TestModel.PropertyApi.Public_Returns_Protected_Internal { get; protected set; }")]
        protected internal string Public_Returns_Protected_Internal { get; protected set; }

    }
}
