using System;
using Xunit;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class EnumValueParserTest
    {
        private enum BlahEnum
        {
            VALUE1 = 0,
            VALUE2 = 1,
            VALUE3 = 2,
        }

        [Fact]
        public void TestNew()
        {
            Assert.DoesNotThrow(() => new EnumValueParser(typeof(BlahEnum)));
        }

        [Fact]
        public void TestNew__Null()
        {
            Assert.Throws<ArgumentNullException>(() => new EnumValueParser(null));
        }

        [Fact]
        public void TestNew__NonEnum()
        {
            Assert.Throws<ArgumentException>(() => new EnumValueParser(typeof(string)));
            Assert.Throws<ArgumentException>(() => new EnumValueParser(typeof(int)));
        }

        [Fact]
        public void TestParse()
        {
            EnumValueParser parser = new EnumValueParser(typeof(BlahEnum));

            Assert.Equal(BlahEnum.VALUE1, parser.Parse("VALUE1"));
            Assert.Equal(BlahEnum.VALUE2, parser.Parse("VALUE2"));
            Assert.Equal(BlahEnum.VALUE2, parser.Parse("1"));
        }

        [Fact]
        public void TestParse__Null()
        {
            EnumValueParser parser = new EnumValueParser(typeof(BlahEnum));

            Assert.Throws<ArgumentNullException>(() => parser.Parse(null));
        }

        [Fact]
        public void TestParse__Empty()
        {
            EnumValueParser parser = new EnumValueParser(typeof(BlahEnum));

            Assert.Throws<ArgumentException>(() => parser.Parse(""));
        }

        [Fact]
        public void TestFormat()
        {
            EnumValueParser parser = new EnumValueParser(typeof(BlahEnum));

            Assert.Equal("VALUE1", parser.Format(BlahEnum.VALUE1));
            Assert.Equal("VALUE2", parser.Format(BlahEnum.VALUE2));
        }

        [Fact]
        public void TestFormat__Null()
        {
            EnumValueParser parser = new EnumValueParser(typeof(BlahEnum));

            Assert.Throws<ArgumentNullException>(() => parser.Format(null));
        }
    }
}
