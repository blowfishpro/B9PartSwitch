using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B9PartSwitch
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigNodeSerialized : Attribute { }

    public interface IConfigNodeSerializable : IConfigNode
    {
        void SerializeToNode(ConfigNode node);
    }
}
