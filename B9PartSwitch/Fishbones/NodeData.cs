using System;

namespace B9PartSwitch.Fishbones
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NodeData : Attribute
    {
        public string name = null;
        public bool persistent = false;
        public bool alwaysSerialize = false;
    }
}
