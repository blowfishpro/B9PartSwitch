using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitch.Fishbones.TypeConfigs;

namespace B9PartSwitchTests.Fishbones.TypeConfigs
{
    public class OperationManagerTest
    {
        private INodeDataMapper mapper1 = Substitute.For<INodeDataMapper>();
        private INodeDataMapper mapper2 = Substitute.For<INodeDataMapper>();
        private INodeDataMapper mapper3 = Substitute.For<INodeDataMapper>();

        private OperationManager manager;

        public OperationManagerTest()
        {
            manager = new OperationManager(mapper1, mapper2, mapper3);
        }

        [Fact]
        public void TestMapperFor__LoadPrefab()
        {
            Assert.Same(mapper1, manager.MapperFor(Operation.LoadPrefab));
        }

        [Fact]
        public void TestMapperFor__LoadInstance()
        {
            Assert.Same(mapper2, manager.MapperFor(Operation.LoadInstance));
        }

        [Fact]
        public void TestMapperFor__Save()
        {
            Assert.Same(mapper2, manager.MapperFor(Operation.Save));
        }

        [Fact]
        public void TestMapperFor__Deserialize()
        {
            Assert.Same(mapper3, manager.MapperFor(Operation.Deserialize));
        }

        [Fact]
        public void TestMapperFor__Serialize()
        {
            Assert.Same(mapper3, manager.MapperFor(Operation.Serialize));
        }

        [Fact]
        public void TestMapperFor__ReturnsNull()
        {
            OperationManager manager2 = new OperationManager(mapper1, null, mapper3);
            Assert.Null(manager2.MapperFor(Operation.LoadInstance));
        }
    }
}
