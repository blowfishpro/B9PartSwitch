using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public interface INodeDataMapper
    {
        bool Load(ConfigNode node, ref object fieldValue, OperationContext context);
        bool Save(ConfigNode node, object fieldValue, OperationContext context);
    }
}
