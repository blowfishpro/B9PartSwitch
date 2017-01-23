using System;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class ValueScalarMapperBuilderTest
    {
        #region Constructor

        [Fact]
        public void TestNew()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueScalarMapperBuilder builder = new ValueScalarMapperBuilder("foo", typeof(DummyClass), parseMap);

            Assert.Equal("foo", builder.nodeDataName);
            Assert.Same(typeof(DummyClass), builder.fieldType);
            Assert.Same(parseMap, builder.parseMap);
        }

        [Fact]
        public void TestNew__NullArgument()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();

            Assert.Throws<ArgumentNullException>(() => new ValueScalarMapperBuilder(null, typeof(DummyClass), parseMap));
            Assert.Throws<ArgumentNullException>(() => new ValueScalarMapperBuilder("foo", null, parseMap));
            Assert.Throws<ArgumentNullException>(() => new ValueScalarMapperBuilder("foo", typeof(DummyClass), null));
        }

        #endregion

        #region CanBuild

        [Fact]
        public void TestCanBuild__True()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueScalarMapperBuilder builder = new ValueScalarMapperBuilder("foo", typeof(DummyClass), parseMap);

            parseMap.CanParse(typeof(DummyClass)).Returns(true);

            Assert.True(builder.CanBuild);
        }

        [Fact]
        public void TestCanBuild__False()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueScalarMapperBuilder builder = new ValueScalarMapperBuilder("foo", typeof(DummyClass), parseMap);

            parseMap.CanParse(typeof(DummyClass)).Returns(false);

            Assert.False(builder.CanBuild);
        }

        #endregion

        #region BuildMapper

        [Fact]
        public void TestBuildMapper()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueScalarMapperBuilder builder = new ValueScalarMapperBuilder("foo", typeof(DummyClass), parseMap);
            
            IValueParser parser = Substitute.For<IValueParser>();
            parseMap.CanParse(typeof(DummyClass)).Returns(true);
            parseMap.GetParser(typeof(DummyClass)).Returns(parser);

            ValueScalarMapper mapper = Assert.IsType<ValueScalarMapper>(builder.BuildMapper());

            parseMap.Received().GetParser(typeof(DummyClass));

            Assert.Equal("foo", mapper.name);
            Assert.Same(parser, mapper.parser);
        }

        [Fact]
        public void TestBuildMapper__CantBuild()
        {
            IValueParseMap parseMap = Substitute.For<IValueParseMap>();
            ValueScalarMapperBuilder builder = new ValueScalarMapperBuilder("foo", typeof(DummyClass), parseMap);

            parseMap.CanParse(typeof(DummyClass)).Returns(false);

            Assert.Throws<InvalidOperationException>(() => builder.BuildMapper());
        }

        #endregion
    }
}
