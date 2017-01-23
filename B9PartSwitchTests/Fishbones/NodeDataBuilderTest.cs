using System;
using System.Collections.Generic;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.FieldWrappers;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones
{
    public class NodeDataBuilderTest
    {
        private NodeData nodeData = new NodeData();
        private IValueParseMap parseMap = Substitute.For<IValueParseMap>();
        private IFieldWrapper fieldWrapper = Substitute.For<IFieldWrapper>();

        private NodeDataBuilder builder;

        public NodeDataBuilderTest()
        {
            builder = Substitute.ForPartsOf<NodeDataBuilder>(nodeData, fieldWrapper, parseMap);

            Assert.Same(nodeData, builder.nodeData);
            Assert.Same(fieldWrapper, builder.fieldWrapper);
            Assert.Same(parseMap, builder.valueParseMap);
        }

        #region Constructor

        [Fact]
        public void TestNew()
        {
            NodeDataBuilder builder2 = new NodeDataBuilder(nodeData, fieldWrapper, parseMap);
        }

        [Fact]
        public void TestNew__UseParser()
        {
            Type type1 = Substitute.For<Type>();
            Type type2 = Substitute.For<Type>();
            Type type3 = Substitute.For<Type>();
            Type type4 = Substitute.For<Type>();

            IValueParser parser1 = Substitute.For<IValueParser>();
            IValueParser parser2 = Substitute.For<IValueParser>();
            IValueParser parser3 = Substitute.For<IValueParser>();

            parser1.ParseType.Returns(type1);
            parser2.ParseType.Returns(type2);

            IUseParser useParser1 = Substitute.For<IUseParser>();
            IUseParser useParser2 = Substitute.For<IUseParser>();

            useParser1.CreateParser().Returns(parser1);
            useParser2.CreateParser().Returns(parser2);

            parseMap.CanParse(type3).Returns(true);
            parseMap.CanParse(Arg.Is<Type>(y => y != type3)).Returns(false);

            parseMap.GetParser(type3).Returns(parser3);
            parseMap.When(x => x.GetParser(Arg.Is<Type>(y => y != type3))).Throw<Exception>();

            fieldWrapper.MemberInfo.GetCustomAttributes(true).Returns(new[] { useParser1, useParser2 });
            NodeDataBuilder builder2 = new NodeDataBuilder(nodeData, fieldWrapper, parseMap);

            OverrideValueParseMap newParseMap = Assert.IsType<OverrideValueParseMap>(builder2.valueParseMap);

            Assert.True(newParseMap.CanParse(type1));
            Assert.True(newParseMap.CanParse(type2));
            Assert.True(newParseMap.CanParse(type3));
            Assert.False(newParseMap.CanParse(type4));

            Assert.Same(parser1, newParseMap.GetParser(type1));
            Assert.Same(parser2, newParseMap.GetParser(type2));
            Assert.Same(parser3, newParseMap.GetParser(type3));
            Assert.Throws<Exception>(() => newParseMap.GetParser(type4));
        }

        [Fact]
        public void TestNew__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeDataBuilder(null, fieldWrapper, parseMap));
            Assert.Throws<ArgumentNullException>(() => new NodeDataBuilder(nodeData, null, parseMap));
            Assert.Throws<ArgumentNullException>(() => new NodeDataBuilder(nodeData, fieldWrapper, null));
        }

        #endregion

        [Fact]
        public void TestCreateNodeDataField()
        {
            IOperaitonManager operationManager = Substitute.For<IOperaitonManager>();

            builder.When(x => x.CreateOperationManager()).DoNotCallBase();
            builder.CreateOperationManager().Returns(operationManager);

            NodeDataField nodeDataField = Assert.IsType<NodeDataField>(builder.CreateNodeDataField());
            Assert.NotNull(nodeDataField);

            Assert.Same(fieldWrapper, nodeDataField.field);
            Assert.Same(operationManager, nodeDataField.operationManager);
        }

        [Fact]
        public void TestCreateOperationManager()
        {
            INodeDataMapper mapper1 = Substitute.For<INodeDataMapper>();
            INodeDataMapper mapper2 = Substitute.For<INodeDataMapper>();
            INodeDataMapper mapper3 = Substitute.For<INodeDataMapper>();

            builder.When(x => x.CreateMapperWithParsePriority()).DoNotCallBase();
            builder.When(x => x.CreateLoadSaveMapper()).DoNotCallBase();
            builder.When(x => x.CreateMapperWithSerializePriority()).DoNotCallBase();

            builder.CreateMapperWithParsePriority().Returns(mapper1);
            builder.CreateLoadSaveMapper().Returns(mapper2);
            builder.CreateMapperWithSerializePriority().Returns(mapper3);

            OperationManager operationManager = (OperationManager)builder.CreateOperationManager();
            Assert.NotNull(operationManager);

            Assert.Same(mapper1, operationManager.parseMapper);
            Assert.Same(mapper2, operationManager.loadSaveMapper);
            Assert.Same(mapper3, operationManager.serializeMapper);
        }

        #region Mappers

        [Fact]
        public void TestCreateParseMapper()
        {
            INodeDataMapper mapper = Substitute.For<INodeDataMapper>();
            builder.When(x => x.CreateMapperWithParsePriority()).DoNotCallBase();
            builder.CreateMapperWithParsePriority().Returns(mapper);

            Assert.Same(mapper, builder.CreateParseMapper());

            builder.Received().CreateMapperWithParsePriority();
            builder.DidNotReceive().CreateMapperWithSerializePriority();
        }

        [Fact]
        public void TestCreateLoadSaveMapper()
        {
            nodeData.persistent = true;
            INodeDataMapper mapper = Substitute.For<INodeDataMapper>();
            builder.When(x => x.CreateMapperWithParsePriority()).DoNotCallBase();
            builder.CreateMapperWithParsePriority().Returns(mapper);

            Assert.Same(mapper, builder.CreateLoadSaveMapper());

            builder.Received().CreateMapperWithParsePriority();
            builder.DidNotReceive().CreateMapperWithSerializePriority();
        }

        [Fact]
        public void TestCreateLoadSaveMapper__NotPersistent()
        {
            nodeData.persistent = false;

            Assert.Null(builder.CreateLoadSaveMapper());

            builder.DidNotReceive().CreateMapperWithParsePriority();
            builder.DidNotReceive().CreateMapperWithSerializePriority();
        }

        [Fact]
        public void TestCreateSerializeMapper()
        {
            INodeDataMapper mapper = Substitute.For<INodeDataMapper>();
            builder.When(x => x.CreateMapperWithSerializePriority()).DoNotCallBase();
            builder.CreateMapperWithSerializePriority().Returns(mapper);

            Assert.Same(mapper, builder.CreateSerializeMapper());

            builder.DidNotReceive().CreateMapperWithParsePriority();
            builder.Received().CreateMapperWithSerializePriority();
        }

        [Fact]
        public void TestCreateSerializeMapper__UnityObject()
        {
            nodeData.alwaysSerialize = false;
            fieldWrapper.MemberInfo.ReflectedType.Returns(typeof(UnityEngine.MonoBehaviour));

            Assert.Null(builder.CreateSerializeMapper());

            builder.DidNotReceive().CreateMapperWithParsePriority();
            builder.DidNotReceive().CreateMapperWithSerializePriority();
        }

        [Fact]
        public void TestCreateSerializeMapper__UnityObject__AlwaysSerialize()
        {
            nodeData.alwaysSerialize = true;
        }

        [Fact]
        public void TestCreateMapperWithParsePriority()
        {
            INodeDataMapperBuilder valueScalarBuilder = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder valueListBuilder = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder nodeScalarBuilder = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder nodeListBuilder = Substitute.For<INodeDataMapperBuilder>();

            builder.When(x => x.CreateValueScalarMapperBuilder()).DoNotCallBase();
            builder.When(x => x.CreateValueListMapperBuilder()).DoNotCallBase();
            builder.When(x => x.CreateNodeScalarMapperBuilder()).DoNotCallBase();
            builder.When(x => x.CreateNodeListMapperBuilder()).DoNotCallBase();

            builder.CreateValueScalarMapperBuilder().Returns(valueScalarBuilder);
            builder.CreateValueListMapperBuilder().Returns(valueListBuilder);
            builder.CreateNodeScalarMapperBuilder().Returns(nodeScalarBuilder);
            builder.CreateNodeListMapperBuilder().Returns(nodeListBuilder);

            INodeDataMapperBuilder[] expected = { valueScalarBuilder, nodeScalarBuilder, valueListBuilder, nodeListBuilder };

            builder.WhenForAnyArgs(x => x.BuildFromPrioritizedList(null)).DoNotCallBase();

            INodeDataMapper result = Substitute.For<INodeDataMapper>();
            builder.BuildFromPrioritizedList(expected).Returns(result);

            Assert.Same(result, builder.CreateMapperWithParsePriority());

            builder.Received().BuildFromPrioritizedList(expected);
        }

        [Fact]
        public void TestCreateMapperWithSerializePriority()
        {
            INodeDataMapperBuilder valueScalarBuilder = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder valueListBuilder = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder nodeScalarBuilder = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder nodeListBuilder = Substitute.For<INodeDataMapperBuilder>();

            builder.When(x => x.CreateValueScalarMapperBuilder()).DoNotCallBase();
            builder.When(x => x.CreateValueListMapperBuilder()).DoNotCallBase();
            builder.When(x => x.CreateNodeScalarMapperBuilder()).DoNotCallBase();
            builder.When(x => x.CreateNodeListMapperBuilder()).DoNotCallBase();

            builder.CreateValueScalarMapperBuilder().Returns(valueScalarBuilder);
            builder.CreateValueListMapperBuilder().Returns(valueListBuilder);
            builder.CreateNodeScalarMapperBuilder().Returns(nodeScalarBuilder);
            builder.CreateNodeListMapperBuilder().Returns(nodeListBuilder);

            INodeDataMapperBuilder[] expected = { nodeListBuilder, nodeScalarBuilder, valueListBuilder, valueScalarBuilder };

            builder.WhenForAnyArgs(x => x.BuildFromPrioritizedList(null)).DoNotCallBase();

            INodeDataMapper result = Substitute.For<INodeDataMapper>();
            builder.BuildFromPrioritizedList(expected).Returns(result);

            Assert.Same(result, builder.CreateMapperWithSerializePriority());

            builder.Received().BuildFromPrioritizedList(expected);
        }

        #region BuildFromPrioritizedList

        [Fact]
        public void TestBuildFromPrioritizedList__First()
        {
            INodeDataMapperBuilder builder1 = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder builder2 = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder builder3 = Substitute.For<INodeDataMapperBuilder>();

            builder1.CanBuild.Returns(true);
            builder1.CanBuild.Returns(true);
            builder1.CanBuild.Returns(true);

            INodeDataMapper mapper = Substitute.For<INodeDataMapper>();

            builder1.BuildMapper().Returns(mapper);

            Assert.Same(mapper, builder.BuildFromPrioritizedList(builder1, builder2, builder3));

            builder1.Received().BuildMapper();
            builder2.DidNotReceive().BuildMapper();
            builder3.DidNotReceive().BuildMapper();
        }

        [Fact]
        public void TestBuildFromPrioritizedList__Middle()
        {
            INodeDataMapperBuilder builder1 = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder builder2 = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder builder3 = Substitute.For<INodeDataMapperBuilder>();

            builder1.CanBuild.Returns(false);
            builder2.CanBuild.Returns(true);
            builder3.CanBuild.Returns(true);

            INodeDataMapper mapper = Substitute.For<INodeDataMapper>();

            builder2.BuildMapper().Returns(mapper);

            Assert.Same(mapper, builder.BuildFromPrioritizedList(builder1, builder2, builder3));

            builder1.DidNotReceive().BuildMapper();
            builder2.Received().BuildMapper();
            builder3.DidNotReceive().BuildMapper();
        }

        [Fact]
        public void TestBuildFromPrioritizedList__Last()
        {
            INodeDataMapperBuilder builder1 = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder builder2 = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder builder3 = Substitute.For<INodeDataMapperBuilder>();

            builder1.CanBuild.Returns(false);
            builder2.CanBuild.Returns(false);
            builder3.CanBuild.Returns(true);

            INodeDataMapper mapper = Substitute.For<INodeDataMapper>();

            builder3.BuildMapper().Returns(mapper);

            Assert.Same(mapper, builder.BuildFromPrioritizedList(builder1, builder2, builder3));

            builder1.DidNotReceive().BuildMapper();
            builder2.DidNotReceive().BuildMapper();
            builder3.Received().BuildMapper();
        }

        [Fact]
        public void TestBuildFromPrioritizedList__Only()
        {
            INodeDataMapperBuilder builder1 = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapper mapper = Substitute.For<INodeDataMapper>();

            builder1.CanBuild.Returns(true);
            builder1.BuildMapper().Returns(mapper);

            Assert.Same(mapper, builder.BuildFromPrioritizedList(builder1));

            builder1.Received().BuildMapper();
        }

        [Fact]
        public void TestBuildFromPrioritizedList__None()
        {
            INodeDataMapperBuilder builder1 = Substitute.For<INodeDataMapperBuilder>();
            INodeDataMapperBuilder builder2 = Substitute.For<INodeDataMapperBuilder>();

            builder1.CanBuild.Returns(false);
            builder2.CanBuild.Returns(false);

            Assert.Throws<NotImplementedException>(() => builder.BuildFromPrioritizedList(builder1, builder2));

            builder1.DidNotReceive().BuildMapper();
            builder2.DidNotReceive().BuildMapper();
        }

        #endregion

        #endregion

        #region Mapper Builder Creation

        public void TestCreateValueScalarMapperBuilder()
        {
            builder.NodeDataName.Returns("foo");
            fieldWrapper.FieldType.Returns(typeof(DummyClass));

            ValueScalarMapperBuilder mapperBuilder = Assert.IsType<ValueScalarMapperBuilder>(builder.CreateValueScalarMapperBuilder());

            Assert.Equal("foo", mapperBuilder.nodeDataName);
            Assert.Same(typeof(DummyClass), mapperBuilder.fieldType);
            Assert.Same(parseMap, mapperBuilder.parseMap);
        }

        public void TestCreateValueListMapperBuilder()
        {
            builder.NodeDataName.Returns("foo");
            fieldWrapper.FieldType.Returns(typeof(List<DummyClass>));

            ValueListMapperBuilder mapperBuilder = Assert.IsType<ValueListMapperBuilder>(builder.CreateValueListMapperBuilder());

            Assert.Equal("foo", mapperBuilder.nodeDataName);
            Assert.Same(typeof(DummyClass), mapperBuilder.elementType);
            Assert.Same(parseMap, mapperBuilder.parseMap);
        }

        public void TestCreateNodeScalarMapperBuilder()
        {
            builder.NodeDataName.Returns("foo");
            fieldWrapper.FieldType.Returns(typeof(DummyIConfigNode));

            NodeScalarMapperBuilder mapperBuilder = Assert.IsType<NodeScalarMapperBuilder>(builder.CreateNodeScalarMapperBuilder());

            Assert.Equal("foo", mapperBuilder.nodeDataName);
            Assert.Same(typeof(DummyIConfigNode), mapperBuilder.fieldType);
        }

        public void TestCreateNodeListMapperBuilder()
        {
            builder.NodeDataName.Returns("foo");
            fieldWrapper.FieldType.Returns(typeof(List<DummyIConfigNode>));

            NodeListMapperBuilder mapperBuilder = Assert.IsType<NodeListMapperBuilder>(builder.CreateNodeListMapperBuilder());

            Assert.Equal("foo", mapperBuilder.nodeDataName);
            Assert.Same(typeof(DummyIConfigNode), mapperBuilder.elementType);
        }

        #endregion

        #region Utilities

        #region TestNodeDataName

        [Fact]
        public void TestNodeDataName__Attribute()
        {
            nodeData.name = "foo";
            fieldWrapper.MemberInfo.Name.Returns("bar");

            Assert.Equal("foo", builder.NodeDataName);
        }

        [Fact]
        public void TestNodeDataName__Field()
        {
            nodeData.name = null;
            fieldWrapper.MemberInfo.Name.Returns("bar");

            Assert.Equal("bar", builder.NodeDataName);
        }

        #endregion
        

        #endregion
    }
}
