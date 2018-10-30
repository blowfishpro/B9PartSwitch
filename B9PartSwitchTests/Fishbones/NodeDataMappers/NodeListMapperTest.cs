using System;
using System.Collections.Generic;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitchTests.TestUtils;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class NodeListMapperTest
    {
        private readonly INodeObjectWrapper wrapper;
        private readonly NodeListMapper mapper;

        public NodeListMapperTest()
        {
            wrapper = Substitute.For<INodeObjectWrapper>();
            mapper = new NodeListMapper("SOME_NODE", typeof(object), wrapper);
        }

        #region Constructor

        [Fact]
        public void TestNew()
        {
            Assert.Equal("SOME_NODE", mapper.name);
            Assert.Same(typeof(List<object>), mapper.listType);
            Assert.Same(wrapper, mapper.nodeObjectWrapper);
        }

        [Fact]
        public void TestNew__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeListMapper(null, typeof(object), wrapper));
            Assert.Throws<ArgumentNullException>(() => new NodeListMapper("blah", null, wrapper));
            Assert.Throws<ArgumentNullException>(() => new NodeListMapper("blah", typeof(object), null));
        }

        #endregion

        #region Load

        [Fact]
        public void TestLoad()
        {
            List<object> list = new List<object>();
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

            OperationContext context = Exemplars.LoadPrefabContext;

            object obj1 = new object();
            object obj2 = new object();

            object dummyNull = null;

            wrapper.When(x => x.Load(ref dummyNull, node.nodes[0], context)).Do(x => x[0] = obj1);
            wrapper.When(x => x.Load(ref dummyNull, node.nodes[1], context)).Do(x => x[0] = obj2);

            Assert.True(mapper.Load(ref value, node, context));
            Assert.Same(list, value);
            Assert.Equal(2, list.Count);

            Assert.Same(obj1, list[0]);
            Assert.Same(obj2, list[1]);
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

            OperationContext context = Exemplars.LoadPrefabContext;

            object obj1 = new object();
            object obj2 = new object();

            object dummyNull = null;

            wrapper.When(x => x.Load(ref dummyNull, node.nodes[0], context)).Do(x => x[0] = obj1);
            wrapper.When(x => x.Load(ref dummyNull, node.nodes[1], context)).Do(x => x[0] = obj2);

            Assert.True(mapper.Load(ref value, node, context));
            List<object> list = Assert.IsType<List<object>>(value);
            
            Assert.Equal(2, list.Count);

            Assert.Same(obj1, list[0]);
            Assert.Same(obj2, list[1]);
        }

        [Fact]
        public void TestLoad__ExistingValue()
        {
            object obj0 = new object();

            List<object> list = new List<object>
            {
                obj0,
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

            OperationContext context = Exemplars.LoadPrefabContext;

            object obj1 = new object();
            object obj2 = new object();

            object dummyNull = null;

            wrapper.When(x => x.Load(ref dummyNull, node.nodes[0], context)).Do(x => x[0] = obj1);
            wrapper.When(x => x.Load(ref dummyNull, node.nodes[1], context)).Do(x => x[0] = obj2);

            Assert.True(mapper.Load(ref value, node, context));
            Assert.Same(list, value);
            Assert.Equal(3, list.Count);

            Assert.Same(obj0, list[0]);
            Assert.Same(obj1, list[1]);
            Assert.Same(obj2, list[2]);
        }

        [Fact]
        public void TestLoad__ExistingValue__Deserialize()
        {
            object obj0 = new object();

            List<object> list = new List<object>
            {
                obj0,
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

            object obj1 = new object();
            object obj2 = new object();

            object dummyNull = null;

            wrapper.When(x => x.Load(ref dummyNull, node.nodes[0], context)).Do(x => x[0] = obj1);
            wrapper.When(x => x.Load(ref dummyNull, node.nodes[1], context)).Do(x => x[0] = obj2);

            Assert.True(mapper.Load(ref value, node, context));
            Assert.Same(list, value);
            Assert.Equal(2, list.Count);
            
            Assert.Same(obj1, list[0]);
            Assert.Same(obj2, list[1]);
        }

        [Fact]
        public void TestLoad__ExistingValue__LoadInstance()
        {
            object obj0 = new object();

            List<object> list = new List<object>
            {
                obj0,
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
            OperationContext context = new OperationContext(Operation.LoadInstance, new object());

            object obj1 = new object();
            object obj2 = new object();

            object dummyNull = null;

            wrapper.When(x => x.Load(ref dummyNull, node.nodes[0], context)).Do(x => x[0] = obj1);
            wrapper.When(x => x.Load(ref dummyNull, node.nodes[1], context)).Do(x => x[0] = obj2);

            Assert.True(mapper.Load(ref value, node, context));
            Assert.Same(list, value);
            Assert.Equal(2, list.Count);

            Assert.Same(obj1, list[0]);
            Assert.Same(obj2, list[1]);
        }

        [Fact]
        public void TestLoad__NoNodes()
        {
            object obj0 = new object();

            List<object> list = new List<object>
            {
                obj0,
            };

            object value = list;

            ConfigNode node = new TestConfigNode
            {
                new TestConfigNode("SOME_OTHER_NODE")
                {
                    { "value", "thing2" },
                },
            };

            Assert.False(mapper.Load(ref value, node, Exemplars.LoadPrefabContext));
            Assert.Same(list, value);
            Assert.Single(list);

            Assert.Same(obj0, list[0]);

            wrapper.DidNotReceiveWithAnyArgs().Load(ref obj0, null, null);
        }

        [Fact]
        public void TestLoad__NullNode()
        {
            object dummy = null;
            Assert.Throws<ArgumentNullException>(() => mapper.Load(ref dummy, null, Exemplars.LoadPrefabContext));
        }

        [Fact]
        public void TestLoad__NullContext()
        {
            object dummy = null;
            Assert.Throws<ArgumentNullException>(() => mapper.Load(ref dummy, new ConfigNode(), null));
        }

        [Fact]
        public void TestLoad__WrongType()
        {
            object dummy = "why are you passing a string?";
            Assert.Throws<ArgumentException>(() => mapper.Load(ref dummy, new ConfigNode(), Exemplars.LoadPrefabContext));
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            object obj1 = new object();
            object obj2 = new object();

            List<object> list = new List<object>
            {
                obj1,
                null,
                obj2,
            };

            OperationContext context = Exemplars.SaveContext;

            wrapper.Save(obj1, context).Returns(new TestConfigNode("SOME_NODE")
            {
                { "value", "blah1" },
            });
            wrapper.Save(obj2, context).Returns(new TestConfigNode("SOME_NODE")
            {
                { "value", "blah2" },
            });

            ConfigNode node = new TestConfigNode
            {
                { "value", "something" },
                new ConfigNode("SOME_OTHER_NODE"),
            };

            Assert.True(mapper.Save(list, node, context));

            ConfigNode expected = new TestConfigNode
            {
                { "value", "something" },
                new ConfigNode("SOME_OTHER_NODE"),
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
            List<DummyIConfigNode> list = null;

            ConfigNode node = new ConfigNode();

            Assert.False(mapper.Save(list, node, Exemplars.SaveContext));
            Assert.False(node.HasNode("SOME_NODE"));

            wrapper.DidNotReceiveWithAnyArgs().Save(null, null);
        }

        [Fact]
        public void TestSave__Empty()
        {
            List<object> list = new List<object>();

            ConfigNode node = new ConfigNode();

            Assert.False(mapper.Save(list, node, Exemplars.SaveContext));
            Assert.False(node.HasNode("SOME_NODE"));
            Assert.Empty(list);

            wrapper.DidNotReceiveWithAnyArgs().Save(null, null);
        }

        [Fact]
        public void TestSave__NullNode()
        {
            Assert.Throws<ArgumentNullException>(() => mapper.Save(new List<object>(), null, Exemplars.SaveContext));
        }

        [Fact]
        public void TestSave__NullContext()
        {
            Assert.Throws<ArgumentNullException>(() => mapper.Save(new List<object>(), new ConfigNode(), null));
        }

        [Fact]
        public void TestSave__WrongType()
        {
            ConfigNode node = new ConfigNode();

            Assert.Throws<ArgumentException>(() => mapper.Save("a string", node, Exemplars.SaveContext));
            Assert.False(node.HasNode("SOME_NODE"));
        }

        #endregion
    }
}
