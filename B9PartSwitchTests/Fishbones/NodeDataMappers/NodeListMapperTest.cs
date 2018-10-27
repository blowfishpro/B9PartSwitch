using System;
using System.Collections.Generic;
using Xunit;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitchTests.TestUtils;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class NodeListMapperTest
    {
        private NodeListMapper mapper = new NodeListMapper("SOME_NODE", typeof(DummyIConfigNode));

        #region Constructor

        [Fact]
        public void TestNew__IConfigNode()
        {
            NodeListMapper mapper2 = new NodeListMapper("blah", typeof(DummyIConfigNode));

            Assert.Equal("blah", mapper2.name);
            Assert.Same(typeof(DummyIConfigNode), mapper2.elementType);
            Assert.Same(typeof(List<DummyIConfigNode>), mapper2.listType);
        }

        [Fact]
        public void TestNew__IContextualNode()
        {
            NodeListMapper mapper2 = new NodeListMapper("blah", typeof(DummyIContextualNode));

            Assert.Equal("blah", mapper2.name);
            Assert.Same(typeof(DummyIContextualNode), mapper2.elementType);
            Assert.Same(typeof(List<DummyIContextualNode>), mapper2.listType);
        }

        [Fact]
        public void TestNew__BadType()
        {
            Assert.Throws<ArgumentException>(() => new NodeListMapper("blah", typeof(DummyClass)));
        }

        [Fact]
        public void TestNew__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeListMapper("blah", null));
            Assert.Throws<ArgumentNullException>(() => new NodeListMapper(null, typeof(DummyIConfigNode)));
            Assert.Throws<ArgumentNullException>(() => new NodeListMapper(null, null));
        }

        #endregion

        #region Load

        [Fact]
        public void TestLoad()
        {
            List<DummyIConfigNode> list = new List<DummyIConfigNode>();
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

            Assert.True(mapper.Load(ref value, node, Exemplars.LoadContext));
            Assert.Same(list, value);
            Assert.Equal(2, list.Count);

            AssertDummyIConfigNode(list[0], "thing1");
            AssertDummyIConfigNode(list[1], "thing2");
        }

        [Fact]
        public void TestLoad__IContextualNode()
        {
            NodeListMapper mapper2 = new NodeListMapper("SOME_NODE", typeof(DummyIContextualNode));
            List<DummyIContextualNode> list = new List<DummyIContextualNode>();
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

            OperationContext context = Exemplars.LoadContext;
            Assert.True(mapper2.Load(ref value, node, context));
            Assert.Same(list, value);
            Assert.Equal(2, list.Count);

            Assert.Equal("thing1", list[0].value);
            Assert.Equal("thing2", list[1].value);

            Assert.Same(context, list[0].lastContext);
            Assert.Same(context, list[1].lastContext);
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

            Assert.True(mapper.Load(ref value, node, Exemplars.LoadContext));
            Assert.IsType<List<DummyIConfigNode>>(value);

            List<DummyIConfigNode> list = (List<DummyIConfigNode>)value;
            Assert.Equal(2, list.Count);

            AssertDummyIConfigNode(list[0], "thing1");
            AssertDummyIConfigNode(list[1], "thing2");
        }

        [Fact]
        public void TestLoad__ExistingValue()
        {
            List<DummyIConfigNode> list = new List<DummyIConfigNode>
            {
                new DummyIConfigNode { value = "thing0" },
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

            Assert.True(mapper.Load(ref value, node, Exemplars.LoadContext));
            Assert.Same(list, value);
            Assert.Equal(3, list.Count);

            AssertDummyIConfigNode(list[0], "thing0");
            AssertDummyIConfigNode(list[1], "thing1");
            AssertDummyIConfigNode(list[2], "thing2");
        }

        [Fact]
        public void TestLoad__ExistingValue__Deserialize()
        {
            List<DummyIConfigNode> list = new List<DummyIConfigNode>
            {
                new DummyIConfigNode { value = "thing0" },
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

            OperationContext context = new OperationContext(Operation.Deserialize, new object());
            Assert.True(mapper.Load(ref value, node, context));
            Assert.Same(list, value);
            Assert.Equal(2, list.Count);
            
            AssertDummyIConfigNode(list[0], "thing1");
            AssertDummyIConfigNode(list[1], "thing2");
        }

        [Fact]
        public void TestLoad__NoNodes()
        {
            List<DummyIConfigNode> list = new List<DummyIConfigNode>
            {
                new DummyIConfigNode { value = "thing0" },
            };

            object value = list;

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "thing2" },
                },
            };

            Assert.False(mapper.Load(ref value, node, Exemplars.LoadContext));
            Assert.Same(list, value);
            Assert.Single(list);

            AssertDummyIConfigNode(list[0], "thing0");
        }

        [Fact]
        public void TestLoad__NullNode()
        {
            object dummy = new List<DummyIConfigNode> { new DummyIConfigNode { value = "blah" } };
            Assert.Throws<ArgumentNullException>(() => mapper.Load(ref dummy, null, Exemplars.LoadContext));
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

            Assert.Throws<ArgumentException>(() => mapper.Load(ref dummy, new ConfigNode(), Exemplars.LoadContext));
            Assert.Throws<ArgumentException>(() => mapper.Load(ref dummy, node, Exemplars.LoadContext));
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            List<DummyIConfigNode> list = new List<DummyIConfigNode>
            {
                new DummyIConfigNode { value = "blah1" },
                new DummyIConfigNode { value = "blah2" },
            };

            ConfigNode node = new ConfigNode();

            Assert.True(mapper.Save(list, node, Exemplars.SaveContext));

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
        public void TestSave__IContextualNode()
        {
            NodeListMapper mapper2 = new NodeListMapper("SOME_NODE", typeof(DummyIContextualNode));
            List<DummyIContextualNode> list = new List<DummyIContextualNode>
            {
                new DummyIContextualNode { value = "blah1" },
                new DummyIContextualNode { value = "blah2" },
            };

            ConfigNode node = new ConfigNode();
            OperationContext context = Exemplars.SaveContext;
            Assert.True(mapper2.Save(list, node, Exemplars.SaveContext));

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

            Assert.Same(context, list[0].lastContext);
            Assert.Same(context, list[1].lastContext);
        }

        [Fact]
        public void TestSave__Null()
        {
            List<DummyIConfigNode> list = null;

            ConfigNode node = new ConfigNode();

            Assert.False(mapper.Save(list, node, Exemplars.SaveContext));
            Assert.False(node.HasNode("SOME_NODE"));
        }

        [Fact]
        public void TestSave__Empty()
        {
            List<DummyIConfigNode> list = new List<DummyIConfigNode>();

            ConfigNode node = new ConfigNode();

            Assert.False(mapper.Save(list, node, Exemplars.SaveContext));
            Assert.False(node.HasNode("SOME_NODE"));
            Assert.Empty(list);
        }

        [Fact]
        public void TestSave__ExistingValue()
        {
            List<DummyIConfigNode> list = new List<DummyIConfigNode>
            {
                new DummyIConfigNode { value = "blah1" },
                new DummyIConfigNode { value = "blah2" },
            };

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_NODE")
                {
                    { "value", "blah0" },
                },
            };

            Assert.True(mapper.Save(list, node, Exemplars.SaveContext));

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
            List<DummyIConfigNode> list = new List<DummyIConfigNode>
            {
                new DummyIConfigNode { value = "blah1" },
                null,
                new DummyIConfigNode { value = "blah2" },
            };

            ConfigNode node = new ConfigNode();

            Assert.True(mapper.Save(list, node, Exemplars.SaveContext));

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
            object dummy = new List<DummyIConfigNode> { new DummyIConfigNode { value = "blah" } };
            Assert.Throws<ArgumentNullException>(() => mapper.Save(dummy, null, Exemplars.SaveContext));
        }

        [Fact]
        public void TestSave__WrongType()
        {
            object dummy = "why are you passing a string?";
            ConfigNode node = new ConfigNode();

            Assert.Throws<ArgumentException>(() => mapper.Save(dummy, node, Exemplars.SaveContext));
            Assert.False(node.HasNode("SOME_NODE"));
        }

        #endregion

        #region Private Methods

        private void AssertDummyIConfigNode(DummyIConfigNode dummy, string value)
        {
            Assert.NotNull(dummy);
            Assert.Equal(value, dummy.value);
        }

        #endregion
    }
}
