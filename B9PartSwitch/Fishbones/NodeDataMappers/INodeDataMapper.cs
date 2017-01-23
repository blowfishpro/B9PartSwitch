using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public interface INodeDataMapper
    {
        bool Load(ref object fieldValue, ConfigNode node, OperationContext context);
        bool Save(object fieldValue, ConfigNode node, OperationContext context);
    }
}
