using Xunit;
using B9PartSwitch;

namespace B9PartSwitchTests.TestUtils
{
    public static class AssertUtil
    {
        public static void ConfigNodesEqual(ConfigNode node1, ConfigNode node2)
        {
            if (node1.IsNull() || node2.IsNull())
                Assert.Same(node1, node2);
            else
                Assert.Equal(node1.ToString(), node2.ToString());
        }
    }
}
