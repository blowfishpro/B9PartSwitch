namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public interface INodeDataMapper
    {
        bool Load(ConfigNode node, ref object result);
        bool Save(ConfigNode node, ref object result);
    }
}
