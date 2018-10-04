using System;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitchTests.Fishbones
{
    public class NodeDataListTest
    {
        private INodeDataField field1 = Substitute.For<INodeDataField>();
        private INodeDataField field2 = Substitute.For<INodeDataField>();
        private NodeDataList list;

        public NodeDataListTest()
        {
            list = new NodeDataList(field1, field2);
        }

        #region Constructor

        [Fact]
        public void TestNew__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeDataList(fields: null));
        }

        [Fact]
        public void TestNew__NullInList()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeDataList(field1, null, field2));
        }

        #endregion

        #region Load

        [Fact]
        public void TestLoad()
        {
            ConfigNode node = new ConfigNode();

            object subject = new object();
            OperationContext context = new OperationContext(Operation.LoadInstance, subject);

            list.Load(node, context);

            field1.Received().Load(node, context);
            field2.Received().Load(node, context);

            field1.DidNotReceiveWithAnyArgs().Save(null, null);
            field2.DidNotReceiveWithAnyArgs().Save(null, null);
        }

        [Fact]
        public void TestLoad__NullArgument()
        {
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.LoadInstance, Substitute.For<object>());
            
            Assert.Throws<ArgumentNullException>(() => list.Load(null, context));
            Assert.Throws<ArgumentNullException>(() => list.Load(node, null));
        }

        [Fact]
        public void TestLoad__Exception()
        {
            ConfigNode node = new ConfigNode();

            object subject = new object();
            OperationContext context = new OperationContext(Operation.LoadInstance, subject);

            Exception ex = new Exception("some error");
            field1.When(f => f.Load(node, context)).Throw(ex);
            field1.Name.Returns("someField");

            Exception ex2 = Assert.Throws<Exception>(delegate
            {
                list.Load(node, context);
            });

            Assert.Equal("Exception while loading field someField on type System.Object", ex2.Message);
            
            field2.DidNotReceiveWithAnyArgs().Load(null, null);

            field1.DidNotReceiveWithAnyArgs().Save(null, null);
            field2.DidNotReceiveWithAnyArgs().Save(null, null);
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            ConfigNode node = new ConfigNode();

            object subject = new object();
            OperationContext context = new OperationContext(Operation.Save, subject);

            list.Save(node, context);

            field1.Received().Save(node, context);
            field2.Received().Save(node, context);

            field1.DidNotReceiveWithAnyArgs().Load(null, null);
            field2.DidNotReceiveWithAnyArgs().Load(null, null);
        }

        [Fact]
        public void TestSave__NullArgument()
        {
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.Save, Substitute.For<object>());
            
            Assert.Throws<ArgumentNullException>(() => list.Save(null, context));
            Assert.Throws<ArgumentNullException>(() => list.Save(node, null));
        }

        [Fact]
        public void TestSave__Exception()
        {
            ConfigNode node = new ConfigNode();

            object subject = new object();
            OperationContext context = new OperationContext(Operation.Save, subject);

            Exception ex = new Exception("some error");
            field1.When(f => f.Save(node, context)).Throw(ex);
            field1.Name.Returns("someField");

            Exception ex2 = Assert.Throws<Exception>(delegate
            {
                list.Save(node, context);
            });

            Assert.Equal("Exception while saving field someField on type System.Object", ex2.Message);
            
            field2.DidNotReceiveWithAnyArgs().Save(null, null);

            field1.DidNotReceiveWithAnyArgs().Load(null, null);
            field2.DidNotReceiveWithAnyArgs().Load(null, null);
        }

        #endregion
    }
}
