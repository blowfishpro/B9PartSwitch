using System;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.Parsers
{
    public interface INodeObjectWrapper
    {
        void Load(ref object obj, ConfigNode node, OperationContext context);
        ConfigNode Save(object obj, OperationContext context);
    }
}
