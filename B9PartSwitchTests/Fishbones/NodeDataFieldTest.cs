using System;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.Fishbones.FieldWrappers;
using B9PartSwitch.Fishbones.NodeDataMappers;

namespace B9PartSwitchTests.Fishbones
{
    public class NodeDataFieldTest
    {
        private IFieldWrapper fieldWrapper = Substitute.For<IFieldWrapper>();
        private IOperaitonManager operationManager = Substitute.For<IOperaitonManager>();
        private INodeDataMapper mapper = Substitute.For<INodeDataMapper>();

        private NodeDataField field;

        private object dummy = null;

        public NodeDataFieldTest()
        {
            field = new NodeDataField(fieldWrapper, operationManager);
        }

        [Fact]
        public void TestNew()
        {
            Assert.Same(fieldWrapper, field.field);
            Assert.Same(operationManager, field.operationManager);
        }

        [Fact]
        public void TestNew__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeDataField(null, operationManager));
            Assert.Throws<ArgumentNullException>(() => new NodeDataField(fieldWrapper, null));
            Assert.Throws<ArgumentNullException>(() => new NodeDataField(null, null));
        }

        #region Load

        [Fact]
        public void TestLoad()
        {
            object subject = new object();
            object value = new object();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.LoadPrefab, subject);

            fieldWrapper.GetValue(subject).Returns(value);
            operationManager.MapperFor(Operation.LoadPrefab).Returns(mapper);
            mapper.Load(ref value, node, context).Returns(true);

            field.Load(node, context);

            fieldWrapper.Received().GetValue(subject);
            mapper.Received().Load(ref value, node, context);
            fieldWrapper.Received().SetValue(subject, value);

            mapper.DidNotReceiveWithAnyArgs().Save(null, null, null);
        }

        [Fact]
        public void TestLoad__Null()
        {
            object subject = new object();
            object value = new object();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.LoadPrefab, subject);

            fieldWrapper.GetValue(subject).Returns(null);
            operationManager.MapperFor(Operation.LoadPrefab).Returns(mapper);
            mapper.Load(ref dummy, node, context).Returns(x =>
            {
                x[0] = value;
                return true;
            });

            field.Load(node, context);

            fieldWrapper.Received().GetValue(subject);
            mapper.Received().Load(ref value, node, context);
            fieldWrapper.Received().SetValue(subject, value);

            mapper.DidNotReceiveWithAnyArgs().Save(null, null, null);
        }

        [Fact]
        public void TestLoad__False()
        {
            object subject = new object();
            object value = new object();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.LoadPrefab, subject);

            fieldWrapper.GetValue(subject).Returns(value);
            operationManager.MapperFor(Operation.LoadPrefab).Returns(mapper);
            mapper.Load(ref dummy, node, context).Returns(false);

            field.Load(node, context);

            fieldWrapper.Received().GetValue(subject);
            mapper.Received().Load(ref value, node, context);

            fieldWrapper.DidNotReceiveWithAnyArgs().SetValue(null, null);
            mapper.DidNotReceiveWithAnyArgs().Save(null, null, null);
        }

        [Fact]
        public void TestLoad__NoMapper()
        {
            object subject = new object();
            object value = new object();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.LoadPrefab, subject);

            fieldWrapper.GetValue(subject).Returns(value);
            operationManager.MapperFor(Operation.LoadPrefab).Returns((INodeDataMapper)null);

            field.Load(node, context);
            
            fieldWrapper.DidNotReceiveWithAnyArgs().GetValue(null);
            fieldWrapper.DidNotReceiveWithAnyArgs().SetValue(null, null);
        }

        [Fact]
        public void TestLoad__NullArgument()
        {
            object subject = new object();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.LoadPrefab, subject);
            
            Assert.Throws<ArgumentNullException>(() => field.Load(null, context));
            Assert.Throws<ArgumentNullException>(() => field.Load(node, null));
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            object subject = new object();
            object value = new object();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.Save, subject);

            fieldWrapper.GetValue(subject).Returns(value);
            operationManager.MapperFor(Operation.Save).Returns(mapper);

            field.Save(node, context);

            fieldWrapper.Received().GetValue(subject);
            mapper.Received().Save(value, node, context);
            fieldWrapper.DidNotReceiveWithAnyArgs().SetValue(null, null);

            mapper.DidNotReceiveWithAnyArgs().Load(ref dummy, null, null);
        }

        [Fact]
        public void TestSave__Null()
        {
            object subject = new object();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.Save, subject);

            fieldWrapper.GetValue(subject).Returns(null);
            operationManager.MapperFor(Operation.Save).Returns(mapper);

            field.Save(node, context);

            fieldWrapper.Received().GetValue(subject);
            mapper.Received().Save(null, node, context);
            fieldWrapper.DidNotReceiveWithAnyArgs().SetValue(null, null);

            mapper.DidNotReceiveWithAnyArgs().Load(ref dummy, null, null);
        }

        [Fact]
        public void TestSave__NoMapper()
        {
            object subject = new object();
            object value = new object();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.Save, subject);

            fieldWrapper.GetValue(subject).Returns(value);
            operationManager.MapperFor(Operation.Save).Returns((INodeDataMapper)null);

            field.Save(node, context);

            fieldWrapper.DidNotReceiveWithAnyArgs().GetValue(null);
            mapper.DidNotReceiveWithAnyArgs().Save(null, null, null);
            fieldWrapper.DidNotReceiveWithAnyArgs().SetValue(null, null);

            mapper.DidNotReceiveWithAnyArgs().Load(ref dummy, null, null);
        }

        [Fact]
        public void TestSave__NullArgument()
        {
            object subject = new object();
            ConfigNode node = new ConfigNode();
            OperationContext context = new OperationContext(Operation.LoadPrefab, subject);
            
            Assert.Throws<ArgumentNullException>(() => field.Save(null, context));
            Assert.Throws<ArgumentNullException>(() => field.Save(node, null));
        }

        #endregion
    }
}
