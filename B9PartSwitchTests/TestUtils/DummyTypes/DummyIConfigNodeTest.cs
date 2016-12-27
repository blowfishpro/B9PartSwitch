using Xunit;

namespace B9PartSwitchTests.TestUtils.DummyTypes
{
    public class DummyIConfigNodeTest
    {
        [Fact]
        public void TestLoad()
        {
            DummyIConfigNode dummy = new DummyIConfigNode();

            ConfigNode node = new TestConfigNode
            {
                { "value", "blah1234" },
                { "otherValue", "blah6789" },
            };

            dummy.Load(node);
            Assert.Equal("blah1234", dummy.value);
        }

        [Fact]
        public void TestSave()
        {
            DummyIConfigNode dummy = new DummyIConfigNode();
            dummy.value = "blah1234";
            ConfigNode node = new ConfigNode();

            dummy.Save(node);
            Assert.Equal("blah1234", node.GetValue("value"));
        }
    }
}
