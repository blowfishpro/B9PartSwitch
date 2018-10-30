using System;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitchTests.TestUtils;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class NodeScalarMapperTest
    {
        private readonly INodeObjectWrapper wrapper;
        private readonly NodeScalarMapper mapper;

        public NodeScalarMapperTest()
        {
            wrapper = Substitute.For<INodeObjectWrapper>();
            mapper = new NodeScalarMapper("SOME_NODE", wrapper);
        }

        #region Constructor

        [Fact]
        public void TestNew()
        {
            Assert.Equal("SOME_NODE", mapper.name);
            Assert.Same(wrapper, mapper.nodeObjectWrapper);
        }

        [Fact]
        public void TestNew__ThrowsIfNull()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeScalarMapper(null, Substitute.For<INodeObjectWrapper>()));
            Assert.Throws<ArgumentNullException>(() => new NodeScalarMapper("SOME_NODE", null));
        }

        #endregion

        #region Load

        [Fact]
        public void TestLoad()
        {
            object dummy = new object();
            object dummyRef = dummy;

            ConfigNode innerNode = new TestConfigNode("SOME_NODE")
            {
                { "value", "something" },
            };

            ConfigNode outerNode = new TestConfigNode
            {
                innerNode,
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "something else" },
                },
            };

            OperationContext context = Exemplars.LoadPrefabContext;
            Assert.True(mapper.Load(ref dummyRef, outerNode, context));
            Assert.Same(dummyRef, dummy);

            wrapper.Received().Load(ref dummyRef, innerNode, context);
        }

        [Fact]
        public void TestLoad__Null()
        {
            object dummy = null;

            ConfigNode innerNode = new TestConfigNode("SOME_NODE")
            {
                { "value", "something" },
            };

            ConfigNode outerNode = new TestConfigNode
            {
                innerNode,
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "something else" },
                },
            };

            OperationContext context = Exemplars.LoadPrefabContext;
            object newDummy = new object();

            wrapper.When(x => x.Load(ref dummy, innerNode, context)).Do(x => x[0] = newDummy);
            Assert.True(mapper.Load(ref dummy, outerNode, context));

            Assert.Same(dummy, newDummy);
        }

        [Fact]
        public void TestLoad__NoNode()
        {
            object dummy = new object();
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

            Assert.False(mapper.Load(ref dummyRef, node, Exemplars.LoadPrefabContext));
            wrapper.DidNotReceiveWithAnyArgs().Load(ref dummyRef, null, null);
            Assert.Same(dummyRef, dummy);
        }

        [Fact]
        public void TestLoad__NullNode()
        {
            object dummy = new object();
            Assert.Throws<ArgumentNullException>(() => mapper.Load(ref dummy, null, Exemplars.LoadPrefabContext));
        }

        [Fact]
        public void TestLoad__NullContext()
        {
            object dummy = new object();
            Assert.Throws<ArgumentNullException>(() => mapper.Load(ref dummy, new ConfigNode(), null));
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            object dummy = new object();
            OperationContext context = Exemplars.SaveContext;
            ConfigNode innerNode = new ConfigNode();
            ConfigNode outerNode = new ConfigNode();

            wrapper.Save(dummy, context).Returns(innerNode);
            Assert.True(mapper.Save(dummy, outerNode, context));
            wrapper.Received().Save(dummy, context);

            Assert.Same(innerNode, outerNode.GetNode("SOME_NODE"));
        }

        [Fact]
        public void TestSave__NullValue()
        {
            ConfigNode node = new ConfigNode();
            object dummy = null;

            Assert.False(mapper.Save(dummy, node, Exemplars.SaveContext));
            wrapper.DidNotReceiveWithAnyArgs().Save(null, null);

            Assert.Null(node.GetNode("SOME_NODE"));
        }

        [Fact]
        public void TestSave__NullNode()
        {
            Assert.Throws<ArgumentNullException>(() => mapper.Save(new object(), null, Exemplars.SaveContext));
        }

        [Fact]
        public void TestSave__NullContext()
        {
            Assert.Throws<ArgumentNullException>(() => mapper.Save(new object(), new ConfigNode(), null));
        }

        #endregion
    }
}
