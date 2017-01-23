using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones
{
    public interface IContextualNode
    {
        void Load(ConfigNode node, OperationContext context);
        void Save(ConfigNode node, OperationContext context);
    }
}
