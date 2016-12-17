namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public interface INodeDataMapper
    {
        bool Load(ConfigNode node, ref object fieldValue);
        bool Save(ConfigNode node, object fieldValue);
    }
}
