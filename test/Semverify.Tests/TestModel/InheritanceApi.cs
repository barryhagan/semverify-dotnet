using System;
using System.Collections.Generic;
using System.Text;

namespace Semverify.Tests.TestModel
{
    public class InheritanceApi
    {
        public InheritanceApi(int intParam)
        {
        }

        public int BaseIntField;
        public int BaseIntProperty { get; set; }
        public virtual void VirtualMethod() { }
        public void BaseMethod() { }

        protected enum BaseEnum
        {
            Zero = 0
        }

        protected struct BaseStruct
        {
            public int StructIntProperty { get; set; }
        }
    }

    public class InheritanceApiChild : InheritanceApi
    {
        public InheritanceApiChild(int intParam) : base(intParam) { }
        public new int BaseIntField { get; set; }
        public new int BaseIntProperty;

        public override void VirtualMethod()
        {
            base.VirtualMethod();
        }
    }

    public class InheritanceApiGrandChild : InheritanceApiChild
    {
        public InheritanceApiGrandChild(int intParam) : base(intParam) { }
        public InheritanceApiGrandChild this[int index] { get { return null; } }
        public void ByRefMethod<T>(ref T[] param1) { }
        public new static int BaseMethod;
        public new void VirtualMethod()
        {
        }
    }
}
