﻿using Semverify.Tests.TestAttributes;
using System;

namespace Semverify.Tests.TestModel
{
    public interface IEvent
    {
        string Descrition { get; set; }
        bool Equals(IEvent other);
    }

    public interface IEvent<in T>
    {
        bool Handle(T e);
    }
      
    public abstract class EventBase : IEvent
    {
        public string Description { get; set; }
        public bool Equals(IEvent other) { return false; }
        public abstract string Test();
        protected virtual string DescriptionNew { get; set; }
        public string Descrition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public abstract class EventBaseToo : EventBase
    {
        protected override string DescriptionNew { get; set; }
    }

    public class EventsApi
    {
        public class ClickEventArgs : EventArgs
        {
            public string Target { get; set; }
        }

        [LocalName("OnClickPublic")]
        [Signature("public virtual void Semverify.Tests.TestModel.EventsApi.OnClickPublic(Semverify.Tests.TestModel.EventsApi.ClickEventArgs args) { }")]
        public virtual void OnClickPublic(ClickEventArgs args)
        {
            ClickPublic?.Invoke(this, args);
        }

        [LocalName("ClickPublic")]
        [Signature("public event System.EventHandler<Semverify.Tests.TestModel.EventsApi.ClickEventArgs> Semverify.Tests.TestModel.EventsApi.ClickPublic;")]
        public event EventHandler<ClickEventArgs> ClickPublic;

        [LocalName("OnClickProtected")]
        [Signature("protected delegate bool Semverify.Tests.TestModel.EventsApi.OnClickProtected(object sender, Semverify.Tests.TestModel.EventsApi.ClickEventArgs args);")]
        protected delegate bool OnClickProtected(object sender, ClickEventArgs args);

        [LocalName("ClickProtected")]
        [Signature("protected event Semverify.Tests.TestModel.EventsApi.OnClickProtected Semverify.Tests.TestModel.EventsApi.ClickProtected;")]
        protected event OnClickProtected ClickProtected;

    }
}
