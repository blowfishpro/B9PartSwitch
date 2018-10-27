using System;
using Xunit;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitchTests.TestUtils;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class NodeScalarMapperTest
    {
        private NodeScalarMapper mapper = new NodeScalarMapper("SOME_NODE", typeof(DummyIConfigNode));

        #region Constructor

        [Fact]
        public void TestNew()
        {
            NodeScalarMapper mapper2 = new NodeScalarMapper("blah", typeof(DummyIConfigNode));

            Assert.Equal("blah", mapper2.name);
            Assert.Same(typeof(DummyIConfigNode), mapper2.fieldType);
        }

        [Fact]
        public void TestNew__IContextualNode()
        {
            NodeScalarMapper mapper2 = new NodeScalarMapper("blah", typeof(DummyIContextualNode));

            Assert.Equal("blah", mapper2.name);
            Assert.Same(typeof(DummyIContextualNode), mapper2.fieldType);
        }

        [Fact]
        public void TestNew__BadType()
        {
            Assert.Throws<ArgumentException>(() => new NodeScalarMapper("blah", typeof(DummyClass)));
        }

        [Fact]
        public void TestNew__ThrowsIfNull()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeScalarMapper(null, typeof(DummyIConfigNode)));
            Assert.Throws<ArgumentNullException>(() => new NodeScalarMapper("SOME_NODE", null));
        }

        #endregion

        #region Load

        [Fact]
        public void TestLoad()
        {
            DummyIConfigNode dummy = new DummyIConfigNode();
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

            Assert.True(mapper.Load(ref dummyRef, node, Exemplars.LoadContext));
            Assert.Same(dummyRef, dummy);
            Assert.Equal("something", ((DummyIConfigNode)dummy).value);
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

            Assert.True(mapper.Load(ref dummy, node, Exemplars.LoadContext));
            Assert.IsType<DummyIConfigNode>(dummy);
            Assert.Equal("something", ((DummyIConfigNode)dummy).value);
        }

        [Fact]
        public void TestLoad__IContextualNode()
        {
            NodeScalarMapper mapper2 = new NodeScalarMapper("SOME_NODE", typeof(DummyIContextualNode));
            DummyIContextualNode dummy = new DummyIContextualNode();
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

            OperationContext context = Exemplars.LoadContext;

            Assert.True(mapper2.Load(ref dummyRef, node, context));
            Assert.Same(dummyRef, dummy);
            Assert.Equal("something", dummy.value);
            Assert.Same(context, dummy.lastContext);
        }

        [Fact]
        public void TestLoad__IContextualNode__Null()
        {
            NodeScalarMapper mapper2 = new NodeScalarMapper("SOME_NODE", typeof(DummyIContextualNode));
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

            OperationContext context = Exemplars.LoadContext;

            Assert.True(mapper2.Load(ref dummy, node, context));
            Assert.IsType<DummyIContextualNode>(dummy);

            DummyIContextualNode dummy2 = (DummyIContextualNode)dummy;

            Assert.Equal("something", dummy2.value);
            Assert.Same(context, dummy2.lastContext);
        }

        [Fact]
        public void TestLoad__NoNode()
        {
            object dummy = new DummyIConfigNode { value = "abc" };
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

            Assert.False(mapper.Load(ref dummy, node, Exemplars.LoadContext));
            Assert.Same(dummyRef, dummy);
            Assert.Equal("abc", ((DummyIConfigNode)dummy).value);
        }

        [Fact]
        public void TestLoad__NullNode()
        {
            object dummy = new DummyIConfigNode() { value = "abc" };
            Assert.Throws<ArgumentNullException>(() => mapper.Load(ref dummy, null, Exemplars.LoadContext));
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

            Assert.Throws<ArgumentException>(() => mapper.Load(ref dummy, new ConfigNode(), Exemplars.LoadContext));
            Assert.Throws<ArgumentException>(() => mapper.Load(ref dummy, node, Exemplars.LoadContext));
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            ConfigNode node = new ConfigNode();
            DummyIConfigNode dummy = new DummyIConfigNode { value = "foo" };

            Assert.True(mapper.Save(dummy, node, Exemplars.SaveContext));

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

            Assert.False(mapper.Save(dummy, node, Exemplars.SaveContext));

            Assert.Null(node.GetNode("SOME_NODE"));
        }

        [Fact]
        public void TestSave__IContextualNode()
        {
            NodeScalarMapper mapper2 = new NodeScalarMapper("SOME_NODE", typeof(DummyIContextualNode));
            ConfigNode node = new ConfigNode();
            DummyIContextualNode dummy = new DummyIContextualNode { value = "foo" };
            OperationContext context = Exemplars.SaveContext;

            Assert.True(mapper2.Save(dummy, node, context));

            ConfigNode innerNode = node.GetNode("SOME_NODE");
            Assert.NotNull(innerNode);
            Assert.Equal("foo", innerNode.GetValue("value"));

            Assert.Equal("foo", dummy.value);
            Assert.Same(context, dummy.lastContext);
        }

        [Fact]
        public void TestSave__NullNode()
        {
            DummyIConfigNode dummy = new DummyIConfigNode { value = "abc" };
            Assert.Throws<ArgumentNullException>(() => mapper.Save(dummy, null, Exemplars.SaveContext));
        }

        [Fact]
        public void TestSave__WrongType()
        {
            string dummy = "why are you passing a string?";

            Assert.Throws<ArgumentException>(() => mapper.Save(dummy, new ConfigNode(), Exemplars.SaveContext));
        }

        #endregion
    }
}
