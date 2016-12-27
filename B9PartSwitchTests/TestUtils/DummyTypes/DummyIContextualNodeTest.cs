using Xunit;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitchTests.TestUtils.DummyTypes
{
    public class DummyIContextualNodeTest
    {
        [Fact]
        public void TestLoad()
        {
            DummyIContextualNode dummy = new DummyIContextualNode();

            ConfigNode node = new TestConfigNode
            {
                { "value", "blah1234" },
                { "otherValue", "blah6789" },
            };

            OperationContext context = Exemplars.LoadContext;

            dummy.Load(node, context);
            Assert.Equal("blah1234", dummy.value);
            Assert.Same(context, dummy.lastContext);
        }

        [Fact]
        public void TestSave()
        {
            DummyIContextualNode dummy = new DummyIContextualNode();
            dummy.value = "blah1234";
            ConfigNode node = new ConfigNode();
            OperationContext context = Exemplars.SaveContext;

            dummy.Save(node, context);
            Assert.Equal("blah1234", node.GetValue("value"));
            Assert.Same(context, dummy.lastContext);
        }
    }
}
