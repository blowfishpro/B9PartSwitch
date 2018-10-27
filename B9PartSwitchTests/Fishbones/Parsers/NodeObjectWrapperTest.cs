using System;
using Xunit;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitchTests.TestUtils;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class NodeObjectWrapperTest
    {
        #region IsNodeType

        [Fact]
        public void TestIsNodeType()
        {
            Assert.True(NodeObjectWrapper.IsNodeType(typeof(DummyIConfigNode)));
            Assert.True(NodeObjectWrapper.IsNodeType(typeof(DummyIContextualNode)));
            Assert.False(NodeObjectWrapper.IsNodeType(typeof(DummyClass)));
        }

        #endregion

        #region Load

        [Fact]
        public void TestLoad__IConfigNode()
        {
            ConfigNode node = new TestConfigNode
            {
                { "value", "blah1234" }
            };

            DummyIConfigNode dummy = new DummyIConfigNode();
            OperationContext context = new OperationContext(Operation.LoadInstance, "some object");

            NodeObjectWrapper.Load(dummy, node, context);

            Assert.Equal("blah1234", dummy.value);
        }

        [Fact]
        public void TestLoad__IContextualNode()
        {
            ConfigNode node = new TestConfigNode
            {
                { "value", "blah1234" }
            };

            DummyIContextualNode dummy = new DummyIContextualNode();
            OperationContext context = new OperationContext(Operation.LoadInstance, "some object");

            NodeObjectWrapper.Load(dummy, node, context);

            Assert.Equal("blah1234", dummy.value);
            Assert.Same(context, dummy.lastContext);
        }

        [Fact]
        public void TestLoad__UnrecognizedType()
        {
            object dummy = "this is a string";
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.LoadInstance, "some object");

            Assert.Throws<ArgumentException>(() => NodeObjectWrapper.Load(dummy, node, context));
        }

        [Fact]
        public void TestLoad__NotLoadingContext()
        {
            object dummy = new DummyIConfigNode();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.Save, "some object");

            Assert.Throws<ArgumentException>(() => NodeObjectWrapper.Load(dummy, node, context));
        }

        [Fact]
        public void TestLoad__Null()
        {
            object dummy = new DummyIConfigNode();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.LoadInstance, "some object");

            Assert.Throws<ArgumentNullException>(() => NodeObjectWrapper.Load(null, node, context));
            Assert.Throws<ArgumentNullException>(() => NodeObjectWrapper.Load(dummy, null, context));
            Assert.Throws<ArgumentNullException>(() => NodeObjectWrapper.Load(dummy, node, null));
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave__IConfigNode()
        {
            ConfigNode node = new ConfigNode();

            DummyIConfigNode dummy = new DummyIConfigNode
            {
                value = "test5678"
            };
            OperationContext context = new OperationContext(Operation.Save, "some object");

            NodeObjectWrapper.Save(dummy, node, context);

            Assert.Equal("test5678", node.GetValue("value"));
        }

        [Fact]
        public void TesSave__IContextualNode()
        {
            ConfigNode node = new ConfigNode();

            DummyIContextualNode dummy = new DummyIContextualNode
            {
                value = "test5678"
            };
            OperationContext context = new OperationContext(Operation.Save, "some object");

            NodeObjectWrapper.Save(dummy, node, context);

            Assert.Equal("test5678", node.GetValue("value"));
            Assert.Same(context, dummy.lastContext);
        }

        [Fact]
        public void TesSave__UnrecognizedType()
        {
            object dummy = "this is a string";
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.Save, "some object");

            Assert.Throws<ArgumentException>(() => NodeObjectWrapper.Save(dummy, node, context));
        }

        [Fact]
        public void TestSave__NotSavingContext()
        {
            object dummy = new DummyIConfigNode();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.LoadInstance, "some object");

            Assert.Throws<ArgumentException>(() => NodeObjectWrapper.Save(dummy, node, context));
        }

        [Fact]
        public void TestSave__Null()
        {
            object dummy = new DummyIConfigNode();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.Save, "some object");

            Assert.Throws<ArgumentNullException>(() => NodeObjectWrapper.Save(null, node, context));
            Assert.Throws<ArgumentNullException>(() => NodeObjectWrapper.Save(dummy, null, context));
            Assert.Throws<ArgumentNullException>(() => NodeObjectWrapper.Save(dummy, node, null));
        }

        #endregion
    }
}
