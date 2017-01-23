namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public interface INodeDataMapperBuilder
    {
        bool CanBuild { get; }
        INodeDataMapper BuildMapper();
    }
}
