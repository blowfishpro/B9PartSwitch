using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class OverrideValueParseMapTest
    {
        private class DummyClass1 { }
        private class DummyClass2 { }
        private class DummyClass3 { }
        private class DummyClass4 { }

        private IValueParseMap innerParseMap = Substitute.For<IValueParseMap>();
        private IValueParser parser1 = Substitute.For<IValueParser>();
        private IValueParser parser2 = Substitute.For<IValueParser>();

        private OverrideValueParseMap parseMap;

        public OverrideValueParseMapTest()
        {
            parser1.ParseType.Returns(typeof(DummyClass1));
            parser2.ParseType.Returns(typeof(DummyClass2));

            parseMap = new OverrideValueParseMap(innerParseMap, parser1, parser2);
        }

        #region Constructor

        [Fact]
        public void TestNew__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new OverrideValueParseMap(null));
            Assert.Throws<ArgumentNullException>(() => new OverrideValueParseMap(innerParseMap, null));
        }

        [Fact]
        public void TestNew__NullInArray()
        {
            Assert.Throws<ArgumentNullException>(() => new OverrideValueParseMap(innerParseMap, parser1, null));
        }

        [Fact]
        public void TestNew__DuplicateType()
        {
            IValueParser parser3 = Substitute.For<IValueParser>();
            parser3.ParseType.Returns(typeof(DummyClass2));

            Assert.Throws<ArgumentException>(() => new OverrideValueParseMap(innerParseMap, parser1, parser2, parser3));
        }

        #endregion

        #region CanParse

        [Fact]
        public void TestCanParse__InnerParseMap()
        {
            innerParseMap.CanParse(typeof(DummyClass3)).Returns(true);
            innerParseMap.CanParse(typeof(DummyClass4)).Returns(false);

            Assert.True(parseMap.CanParse(typeof(DummyClass3)));
            Assert.False(parseMap.CanParse(typeof(DummyClass4)));
        }

        [Fact]
        public void TestCanParse__Override()
        {
            innerParseMap.CanParse(typeof(DummyClass2)).Returns(false);

            Assert.True(parseMap.CanParse(typeof(DummyClass2)));
        }

        [Fact]
        public void TestCanParse__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => parseMap.CanParse(null));
        }

        #endregion

        #region GetParser

        [Fact]
        public void TestGetParser__InnerParseMap()
        {
            IValueParser parser3 = Substitute.For<IValueParser>();
            innerParseMap.GetParser(typeof(DummyClass3)).Returns(parser3);

            Assert.Same(parser3, parseMap.GetParser(typeof(DummyClass3)));
        }

        [Fact]
        public void TestGetParser__Override__InnerMapCantParse()
        {
            innerParseMap.CanParse(typeof(DummyClass2)).Returns(false);

            Assert.Same(parser2, parseMap.GetParser(typeof(DummyClass2)));

            innerParseMap.DidNotReceiveWithAnyArgs().GetParser(null);
        }

        [Fact]
        public void TestGetParser__Override__InnerMapCanParse()
        {
            innerParseMap.CanParse(typeof(DummyClass2)).Returns(true);

            Assert.Same(parser2, parseMap.GetParser(typeof(DummyClass2)));

            innerParseMap.DidNotReceiveWithAnyArgs().GetParser(null);
        }

        [Fact]
        public void TestGetParse__None()
        {
            innerParseMap.WhenForAnyArgs(x => x.GetParser(null)).Throw<Exception>();

            Assert.Throws<Exception>(() => parseMap.GetParser(typeof(DummyClass3)));
        }

        [Fact]
        public void TestGetParse__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => parseMap.GetParser(null));
        }

        #endregion
    }
}
