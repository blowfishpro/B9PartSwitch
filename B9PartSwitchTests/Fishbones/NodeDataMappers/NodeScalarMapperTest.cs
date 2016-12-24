using System;
using Xunit;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitchTests.TestUtils;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class NodeScalarMapperTest
    {
        private class NodeDummy : IConfigNode
        {
            public string value;

            public void Load(ConfigNode node)
            {
                value = node.GetValue("value");
            }

            public void Save(ConfigNode node)
            {
                node.SetValue("value", value, true);
            }
        }

        private NodeScalarMapper mapper = new NodeScalarMapper("SOME_NODE", typeof(NodeDummy));

        [Fact]
        public void TestNew__ThrowsIfNull()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeScalarMapper(null, typeof(NodeDummy)));
            Assert.Throws<ArgumentNullException>(() => new NodeScalarMapper("SOME_NODE", null));
        }

        #region Load

        [Fact]
        public void TestLoad()
        {
            object dummy = new NodeDummy();
            object dummyRef = dummy;

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "something" },
                },
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "something else" },
                },
            };

            Assert.True(mapper.Load(node, ref dummy, Exemplars.LoadContext));
            Assert.Same(dummyRef, dummy);
            Assert.Equal("something", ((NodeDummy)dummy).value);
        }

        [Fact]
        public void TestLoad__Null()
        {
            object dummy = null;

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "something" },
                },
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "something else" },
                },
            };

            Assert.True(mapper.Load(node, ref dummy, Exemplars.LoadContext));
            Assert.NotNull(dummy);
            Assert.Equal("something", ((NodeDummy)dummy).value);
        }

        [Fact]
        public void TestLoad__NoNode()
        {
            object dummy = new NodeDummy { value = "abc" };
            object dummyRef = dummy;

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("NOT_THE_NODE_YOURE_LOOKING_FOR")
                {
                    { "value", "something" },
                },
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "something else" },
                },
            };

            Assert.False(mapper.Load(node, ref dummy, Exemplars.LoadContext));
            Assert.Same(dummyRef, dummy);
            Assert.Equal("abc", ((NodeDummy)dummy).value);
        }

        [Fact]
        public void TestLoad__NullNode()
        {
            object dummy = new NodeDummy() { value = "abc" };
            Assert.Throws<ArgumentNullException>(() => mapper.Load(null, ref dummy, Exemplars.LoadContext));
        }

        [Fact]
        public void TestLoad__WrongType()
        {
            object dummy = "why are you passing a string?";

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "something" },
                },
            };

            Assert.Throws<ArgumentException>(() => mapper.Load(new ConfigNode(), ref dummy, Exemplars.LoadContext));
            Assert.Throws<ArgumentException>(() => mapper.Load(node, ref dummy, Exemplars.LoadContext));
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            ConfigNode node = new ConfigNode();
            NodeDummy dummy = new NodeDummy { value = "foo" };

            Assert.True(mapper.Save(node, dummy, Exemplars.SaveContext));

            ConfigNode innerNode = node.GetNode("SOME_NODE");
            Assert.NotNull(innerNode);
            Assert.Equal("foo", innerNode.GetValue("value"));

            Assert.Equal("foo", dummy.value);
        }

        [Fact]
        public void TestSave__NullValue()
        {
            ConfigNode node = new ConfigNode();
            object dummy = null;

            Assert.False(mapper.Save(node, dummy, Exemplars.SaveContext));

            Assert.Null(node.GetNode("SOME_NODE"));
        }

        [Fact]
        public void TestSave__NullNode()
        {
            NodeDummy dummy = new NodeDummy { value = "abc" };
            Assert.Throws<ArgumentNullException>(() => mapper.Save(null, dummy, Exemplars.SaveContext));
        }

        [Fact]
        public void TestSave__WrongType()
        {
            string dummy = "why are you passing a string?";

            Assert.Throws<ArgumentException>(() => mapper.Save(new ConfigNode(), dummy, Exemplars.SaveContext));
        }

        #endregion
    }
}
