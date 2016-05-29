using System;

namespace B9PartSwitch
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigNodeSerialized : Attribute { }

    public interface IConfigNodeSerializable : IConfigNode
    {
        void SerializeToNode(ConfigNode node);
    }
}
