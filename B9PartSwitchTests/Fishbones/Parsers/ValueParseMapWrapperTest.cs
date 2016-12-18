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
            public IValueParser GetParser(Type parseType) => parser;
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
    }
}
