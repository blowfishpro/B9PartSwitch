﻿using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.Fishbones.NodeDataMappers;

namespace B9PartSwitch.Fishbones.TypeConfigs
{
    public interface IOperaitonManager
    {
        INodeDataMapper MapperFor(Operation operation);
    }
}
