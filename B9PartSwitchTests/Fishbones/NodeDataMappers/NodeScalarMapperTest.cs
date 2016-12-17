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

            Assert.True(mapper.Load(node, ref dummy));
            Assert.ReferenceEquals(dummyRef, dummy);
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

            Assert.True(mapper.Load(node, ref dummy));
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

            Assert.False(mapper.Load(node, ref dummy));
            Assert.ReferenceEquals(dummyRef, dummy);
            Assert.Equal("abc", ((NodeDummy)dummy).value);
        }

        [Fact]
        public void TestLoad__NullNode()
        {
            object dummy = new NodeDummy() { value = "abc" };
            Assert.Throws<ArgumentNullException>(() => mapper.Load(null, ref dummy));
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

            Assert.Throws<ArgumentException>(() => mapper.Load(new ConfigNode(), ref dummy));
            Assert.Throws<ArgumentException>(() => mapper.Load(node, ref dummy));
        }

        [Fact]
        public void TestSave()
        {
            ConfigNode node = new ConfigNode();
            object dummy = new NodeDummy { value = "foo" };

            Assert.True(mapper.Save(node, ref dummy));

            ConfigNode innerNode = node.GetNode("SOME_NODE");
            Assert.NotNull(innerNode);
            Assert.Equal("foo", innerNode.GetValue("value"));

            Assert.Equal("foo", ((NodeDummy)dummy).value);
        }

        [Fact]
        public void TestSave__NullValue()
        {
            ConfigNode node = new ConfigNode();
            object dummy = null;

            Assert.False(mapper.Save(node, ref dummy));

            Assert.Null(node.GetNode("SOME_NODE"));
        }

        [Fact]
        public void TestSave__NullNode()
        {
            object dummy = new NodeDummy { value = "abc" };
            Assert.Throws<ArgumentNullException>(() => mapper.Save(null, ref dummy));
        }

        [Fact]
        public void TestSave__WrongType()
        {
            object dummy = "why are you passing a string?";

            Assert.Throws<ArgumentException>(() => mapper.Save(new ConfigNode(), ref dummy));
        }
    }
}
