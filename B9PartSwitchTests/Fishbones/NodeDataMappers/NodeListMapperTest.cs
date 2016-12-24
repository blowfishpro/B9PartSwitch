using System;
using System.Collections.Generic;
using Xunit;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitchTests.TestUtils;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class NodeListMapperTest
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

        private NodeListMapper mapper = new NodeListMapper("SOME_NODE", typeof(NodeDummy));

        #region Load

        [Fact]
        public void TestLoad()
        {
            List<NodeDummy> list = new List<NodeDummy>();
            object value = list;

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "thing1" },
                },
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "thing2" },
                },
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "thing2" },
                },
            };

            Assert.True(mapper.Load(node, ref value, Exemplars.LoadContext));
            Assert.Same(list, value);
            Assert.Equal(2, list.Count);

            AssertNodeDummy(list[0], "thing1");
            AssertNodeDummy(list[1], "thing2");
        }

        [Fact]
        public void TestLoad__Null()
        {
            object value = null;

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "thing1" },
                },
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "thing2" },
                },
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "thing2" },
                },
            };

            Assert.True(mapper.Load(node, ref value, Exemplars.LoadContext));
            Assert.IsType<List<NodeDummy>>(value);

            List<NodeDummy> list = (List<NodeDummy>)value;
            Assert.Equal(2, list.Count);

            AssertNodeDummy(list[0], "thing1");
            AssertNodeDummy(list[1], "thing2");
        }

        [Fact]
        public void TestLoad__ExistingValue()
        {
            List<NodeDummy> list = new List<NodeDummy>
            {
                new NodeDummy { value = "thing0" },
            };

            object value = list;

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "thing1" },
                },
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "thing2" },
                },
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "thing2" },
                },
            };

            Assert.True(mapper.Load(node, ref value, Exemplars.LoadContext));
            Assert.Same(list, value);
            Assert.Equal(3, list.Count);

            AssertNodeDummy(list[0], "thing0");
            AssertNodeDummy(list[1], "thing1");
            AssertNodeDummy(list[2], "thing2");
        }

        [Fact]
        public void TestLoad__NoNodes()
        {
            List<NodeDummy> list = new List<NodeDummy>
            {
                new NodeDummy { value = "thing0" },
            };

            object value = list;

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "thing2" },
                },
            };

            Assert.False(mapper.Load(node, ref value, Exemplars.LoadContext));
            Assert.Same(list, value);
            Assert.Equal(1, list.Count);

            AssertNodeDummy(list[0], "thing0");
        }

        [Fact]
        public void TestLoad__NullNode()
        {
            object dummy = new List<NodeDummy> { new NodeDummy { value = "blah" } };
            Assert.Throws<ArgumentNullException>(() => mapper.Load(null, ref dummy, Exemplars.LoadContext));
        }

        [Fact]
        public void TestLoad__WrongType()
        {
            object dummy = "why are you passing a string?";

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "thing2" },
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
            List<NodeDummy> list = new List<NodeDummy>
            {
                new NodeDummy { value = "blah1" },
                new NodeDummy { value = "blah2" },
            };

            ConfigNode node = new ConfigNode();

            Assert.True(mapper.Save(node, list, Exemplars.SaveContext));

            ConfigNode expected = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "blah1" },
                },
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "blah2" },
                }
            };

            AssertUtil.ConfigNodesEqual(expected, node);
        }

        [Fact]
        public void TestSave__Null()
        {
            List<NodeDummy> list = null;

            ConfigNode node = new ConfigNode();

            Assert.False(mapper.Save(node, list, Exemplars.SaveContext));
            Assert.False(node.HasNode("SOME_NODE"));
        }

        [Fact]
        public void TestSave__Empty()
        {
            List<NodeDummy> list = new List<NodeDummy>();

            ConfigNode node = new ConfigNode();

            Assert.False(mapper.Save(node, list, Exemplars.SaveContext));
            Assert.False(node.HasNode("SOME_NODE"));
            Assert.Equal(0, list.Count);
        }

        [Fact]
        public void TestSave__ExistingValue()
        {
            List<NodeDummy> list = new List<NodeDummy>
            {
                new NodeDummy { value = "blah1" },
                new NodeDummy { value = "blah2" },
            };

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "blah0" },
                },
            };

            Assert.True(mapper.Save(node, list, Exemplars.SaveContext));

            ConfigNode expected = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "blah0" },
                },
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "blah1" },
                },
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "blah2" },
                }
            };

            AssertUtil.ConfigNodesEqual(expected, node);
        }

        [Fact]
        public void TestSave__NullInList()
        {
            List<NodeDummy> list = new List<NodeDummy>
            {
                new NodeDummy { value = "blah1" },
                null,
                new NodeDummy { value = "blah2" },
            };

            ConfigNode node = new ConfigNode();

            Assert.True(mapper.Save(node, list, Exemplars.SaveContext));

            ConfigNode expected = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "blah1" },
                },
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "blah2" },
                }
            };

            AssertUtil.ConfigNodesEqual(expected, node);
        }

        [Fact]
        public void TestSave__NullNode()
        {
            object dummy = new List<NodeDummy> { new NodeDummy { value = "blah" } };
            Assert.Throws<ArgumentNullException>(() => mapper.Save(null, dummy, Exemplars.SaveContext));
        }

        [Fact]
        public void TestSave__WrongType()
        {
            object dummy = "why are you passing a string?";
            ConfigNode node = new ConfigNode();

            Assert.Throws<ArgumentException>(() => mapper.Save(node, dummy, Exemplars.SaveContext));
            Assert.False(node.HasNode("SOME_NODE"));
        }

        #endregion

        #region Private Methods

        private void AssertNodeDummy(NodeDummy dummy, string value)
        {
            Assert.NotNull(dummy);
            Assert.Equal(value, dummy.value);
        }

        #endregion
    }
}
