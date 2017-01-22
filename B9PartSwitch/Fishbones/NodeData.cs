using System;

namespace B9PartSwitch.Fishbones
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NodeData : Attribute
    {
        public string name = null;
        public bool persistent = false;
    }
}
