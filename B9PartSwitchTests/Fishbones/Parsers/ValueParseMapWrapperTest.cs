using System;
using Xunit;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class ValueParseMapWrapperTest
    {
        private static IValueParser parser = new ValueParser<string>(s => s, s => s);
        private class DummyParseMap : IValueParseMap
        {
            public bool canParse;

            public IValueParser GetParser(Type parseType) => parser;
            public bool CanParse(Type parseType) => canParse;
        }

        [Fact]
        public void TestNew__Null()
        {
            Assert.Throws<ArgumentNullException>(() => new ValueParseMapWrapper(null));
        }

        [Fact]
        public void TestGetParser()
        {
            DummyParseMap map = new DummyParseMap();
            ValueParseMapWrapper wrapper = new ValueParseMapWrapper(map);

            Assert.Same(parser, wrapper.GetParser(typeof(string)));
        }

        [Fact]
        public void TestGet__Null()
        {
            DummyParseMap map = new DummyParseMap();
            ValueParseMapWrapper wrapper = new ValueParseMapWrapper(map);

            Assert.Throws<ArgumentNullException>(() => wrapper.GetParser(null));
        }

        [Fact]
        public void TestCanParse()
        {
            DummyParseMap map = new DummyParseMap();
            ValueParseMapWrapper wrapper = new ValueParseMapWrapper(map);

            map.canParse = true;
            Assert.True(wrapper.CanParse(typeof(string)));

            map.canParse = false;
            Assert.False(wrapper.CanParse(typeof(string)));
        }

        [Fact]
        public void TestCanParse__Null()
        {
            DummyParseMap map = new DummyParseMap();
            ValueParseMapWrapper wrapper = new ValueParseMapWrapper(map);

            Assert.Throws<ArgumentNullException>(() => wrapper.CanParse(null));
        }
    }
}
