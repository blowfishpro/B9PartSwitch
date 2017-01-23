using System;
using System.Reflection;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.FieldWrappers;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones
{
    public class NodeDataListBuilderTest
    {
        private class NodeDataClass
        {
            [NodeData]
            public DummyClass i;

            [NodeData]
            protected DummyClass j;

            public DummyClass k;

            [NodeData]
            public DummyClass x { get; protected set; }

            [NodeData]
            public DummyClass y { get { throw new NotImplementedException(); } set { } }

            public DummyClass z { get; set; }
        }

        #region Constructor

        [Fact]
        public void TestNew__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeDataListBuilder(null));
        }

        #endregion

        #region CreateList

        [Fact]
        public void TestCreateList()
        {
            NodeDataListBuilder builder = Substitute.ForPartsOf<NodeDataListBuilder>(typeof(NodeDataClass));

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            FieldInfo iField = typeof(NodeDataClass).GetField("i", flags);
            FieldInfo jField = typeof(NodeDataClass).GetField("j", flags);
            FieldInfo kField = typeof(NodeDataClass).GetField("k", flags);
            PropertyInfo xProperty = typeof(NodeDataClass).GetProperty("x", flags);
            PropertyInfo yProperty = typeof(NodeDataClass).GetProperty("y", flags);
            PropertyInfo zProperty = typeof(NodeDataClass).GetProperty("z", flags);

            Assert.NotNull(iField);
            Assert.NotNull(jField);
            Assert.NotNull(kField);
            Assert.NotNull(xProperty);
            Assert.NotNull(yProperty);
            Assert.NotNull(zProperty);

            INodeDataBuilder builder1 = Substitute.For<INodeDataBuilder>();
            INodeDataBuilder builder2 = Substitute.For<INodeDataBuilder>();
            INodeDataBuilder builder3 = Substitute.For<INodeDataBuilder>();
            INodeDataBuilder builder4 = Substitute.For<INodeDataBuilder>();

            builder.WhenForAnyArgs(x => x.CreateFieldBuilder(null, null, null)).DoNotCallBase();

            builder.CreateFieldBuilder(Arg.Any<NodeData>(), Arg.Is<FieldWrapper>(x => x.MemberInfo == iField), DefaultValueParseMap.Instance).Returns(builder1);
            builder.CreateFieldBuilder(Arg.Any<NodeData>(), Arg.Is<FieldWrapper>(x => x.MemberInfo == jField), DefaultValueParseMap.Instance).Returns(builder2);
            builder.CreateFieldBuilder(Arg.Any<NodeData>(), Arg.Is<PropertyWrapper>(x => x.MemberInfo == xProperty), DefaultValueParseMap.Instance).Returns(builder3);
            builder.CreateFieldBuilder(Arg.Any<NodeData>(), Arg.Is<PropertyWrapper>(x => x.MemberInfo == yProperty), DefaultValueParseMap.Instance).Returns(builder4);

            INodeDataField field1 = Substitute.For<INodeDataField>();
            INodeDataField field2 = Substitute.For<INodeDataField>();
            INodeDataField field3 = Substitute.For<INodeDataField>();
            INodeDataField field4 = Substitute.For<INodeDataField>();

            builder1.CreateNodeDataField().Returns(field1);
            builder2.CreateNodeDataField().Returns(field2);
            builder3.CreateNodeDataField().Returns(field3);
            builder4.CreateNodeDataField().Returns(field4);

            Assert.IsType<NodeDataList>(builder.CreateList());

            builder.Received().CreateFieldBuilder(Arg.Any<NodeData>(), Arg.Is<FieldWrapper>(x => x.MemberInfo == iField), DefaultValueParseMap.Instance);
            builder.Received().CreateFieldBuilder(Arg.Any<NodeData>(), Arg.Is<FieldWrapper>(x => x.MemberInfo == jField), DefaultValueParseMap.Instance);
            builder.DidNotReceive().CreateFieldBuilder(Arg.Any<NodeData>(), Arg.Is<FieldWrapper>(x => x.MemberInfo == kField), Arg.Any<IValueParseMap>());
            builder.Received().CreateFieldBuilder(Arg.Any<NodeData>(), Arg.Is<PropertyWrapper>(x => x.MemberInfo == xProperty), DefaultValueParseMap.Instance);
            builder.Received().CreateFieldBuilder(Arg.Any<NodeData>(), Arg.Is<PropertyWrapper>(x => x.MemberInfo == yProperty), DefaultValueParseMap.Instance);
            builder.DidNotReceive().CreateFieldBuilder(Arg.Any<NodeData>(), Arg.Is<PropertyWrapper>(x => x.MemberInfo == zProperty), Arg.Any<IValueParseMap>());

            builder1.Received().CreateNodeDataField();
            builder2.Received().CreateNodeDataField();
            builder3.Received().CreateNodeDataField();
            builder4.Received().CreateNodeDataField();
        }

        #endregion

        #region CreateFieldBuilder

        [Fact]
        public void TestCreateFieldBuilder()
        {
            NodeDataListBuilder builder = new NodeDataListBuilder(Substitute.For<Type>());

            NodeData nodeData = new NodeData { name = "fieldbro" };
            IFieldWrapper fieldWrapper = Substitute.For<IFieldWrapper>();
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();

            NodeDataBuilder fieldBuilder = Assert.IsType<NodeDataBuilder>(builder.CreateFieldBuilder(nodeData, fieldWrapper, parseMap));

            Assert.Same(nodeData, fieldBuilder.nodeData);
            Assert.Same(fieldWrapper, fieldBuilder.fieldWrapper);
            Assert.Same(parseMap, fieldBuilder.valueParseMap);
        }

        #endregion
    }
}
