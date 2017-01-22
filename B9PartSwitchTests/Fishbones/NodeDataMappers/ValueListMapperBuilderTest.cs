using System;
using System.Collections.Generic;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class ValueListMapperBuilderTest
    {
        #region Constructor

        [Fact]
        public void TestNew()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueListMapperBuilder builder = new ValueListMapperBuilder("foo", typeof(List<DummyClass>), parseMap);

            Assert.Equal("foo", builder.nodeDataName);
            Assert.Same(typeof(DummyClass), builder.elementType);
            Assert.Same(parseMap, builder.parseMap);
        }

        [Fact]
        public void TestNew__NullArgument()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();

            Assert.Throws<ArgumentNullException>(() => new ValueListMapperBuilder(null, typeof(List<DummyClass>), parseMap));
            Assert.Throws<ArgumentNullException>(() => new ValueListMapperBuilder("foo", null, parseMap));
            Assert.Throws<ArgumentNullException>(() => new ValueListMapperBuilder("foo", typeof(List<DummyClass>), null));
        }

        #endregion

        #region CanBuild

        [Fact]
        public void TestCanBuild__True()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueListMapperBuilder builder = new ValueListMapperBuilder("foo", typeof(List<DummyClass>), parseMap);

            parseMap.CanParse(typeof(DummyClass)).Returns(true);

            Assert.True(builder.CanBuild);
        }

        [Fact]
        public void TestCanBuild__False()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueListMapperBuilder builder = new ValueListMapperBuilder("foo", typeof(List<DummyClass>), parseMap);

            parseMap.CanParse(typeof(DummyClass)).Returns(false);

            Assert.False(builder.CanBuild);
        }

        [Fact]
        public void TestCanBuild__NotList()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueListMapperBuilder builder = new ValueListMapperBuilder("foo", typeof(DummyClass), parseMap);

            parseMap.CanParse(typeof(DummyClass)).Returns(true);

            Assert.False(builder.CanBuild);
        }

        #endregion

        #region Build

        [Fact]
        public void TestBuildMapper()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueListMapperBuilder builder = new ValueListMapperBuilder("foo", typeof(List<DummyClass>), parseMap);

            IValueParser parser = Substitute.For<IValueParser>();
            parser.ParseType.Returns(typeof(DummyClass));
            parseMap.CanParse(typeof(DummyClass)).Returns(true);
            parseMap.GetParser(typeof(DummyClass)).Returns(parser);

            ValueListMapper mapper = Assert.IsType<ValueListMapper>(builder.BuildMapper());

            parseMap.Received().GetParser(typeof(DummyClass));

            Assert.Equal("foo", mapper.name);
            Assert.Same(parser, mapper.parser);

            Assert.Same(typeof(DummyClass), mapper.elementType);
            Assert.Same(typeof(List<DummyClass>), mapper.listType);
        }

        [Fact]
        public void TestBuildMapper__CantBuild()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueListMapperBuilder builder = new ValueListMapperBuilder("foo", typeof(List<DummyClass>), parseMap);

            parseMap.CanParse(typeof(DummyClass)).Returns(false);

            Assert.Throws<InvalidOperationException>(() => builder.BuildMapper());
        }

        #endregion
    }
}
