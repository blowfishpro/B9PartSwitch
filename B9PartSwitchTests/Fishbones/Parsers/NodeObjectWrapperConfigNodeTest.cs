using System;
using Xunit;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitchTests.TestUtils;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class NodeObjectWrapperConfigNodeTest
    {
        private readonly NodeObjectWrapperConfigNode wrapper = new NodeObjectWrapperConfigNode();

        #region Load

        [Fact]
        public void TestLoad()
        {
            ConfigNode origValue = new ConfigNode();
            object value = origValue;

            ConfigNode node = new TestConfigNode()
            {
                { "key", "value" },
                new TestConfigNode("A_NODE")
                {
                    { "some", "stuff" },
                },
            };

            wrapper.Load(ref value, node, Exemplars.LoadPrefabContext);

            ConfigNode newNode = Assert.IsType<ConfigNode>(value);
            Assert.NotSame(origValue, newNode);
            Assert.NotSame(node, newNode);

            AssertUtil.ConfigNodesEqual(node, newNode);
        }

        [Fact]
        public void TestLoad__ObjNull()
        {
            object value = null;

            ConfigNode node = new TestConfigNode()
            {
                { "key", "value" },
                new TestConfigNode("A_NODE")
                {
                    { "some", "stuff" },
                },
            };

            wrapper.Load(ref value, node, Exemplars.LoadPrefabContext);

            ConfigNode newNode = Assert.IsType<ConfigNode>(value);
            Assert.NotSame(node, newNode);

            AssertUtil.ConfigNodesEqual(node, newNode);
        }

        [Fact]
        public void TestLoad__NodeNull()
        {
            object obj = null;
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(delegate
            {
                wrapper.Load(ref obj, null, Exemplars.LoadPrefabContext);
            });

            Assert.Equal("node", ex.ParamName);
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            ConfigNode node = new TestConfigNode()
            {
                { "key", "value" },
                new TestConfigNode("A_NODE")
                {
                    { "some", "stuff" },
                },
            };

            ConfigNode newNode = wrapper.Save(node, Exemplars.SaveContext);

            Assert.NotSame(node, newNode);
            AssertUtil.ConfigNodesEqual(node, newNode);
        }

        [Fact]
        public void TestSave__NullObj()
        {
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(delegate
            {
                wrapper.Save(null, Exemplars.SaveContext);
            });

            Assert.Equal("obj", ex.ParamName);
        }

        [Fact]
        public void TestSave__NotConfigNode()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(delegate
            {
                wrapper.Save("a string", Exemplars.SaveContext);
            });

            Assert.Contains("Expected parameter of type ConfigNode but got System.String", ex.Message);
            Assert.Equal("obj", ex.ParamName);
        }

        #endregion
    }
}
