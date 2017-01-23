using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitchTests.TestUtils.DummyTypes
{
    public class DummyIContextualNode : IContextualNode
    {
        public string value;
        public OperationContext lastContext;

        public void Load(ConfigNode node, OperationContext context)
        {
            value = node.GetValue("value");
            lastContext = context;
        }

        public void Save(ConfigNode node, OperationContext context)
        {
            node.SetValue("value", value, true);
            lastContext = context;
        }
    }
}
