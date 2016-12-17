using System;
using Xunit;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class ValueParserTest
    {
        [Fact]
        public void TestNew__NullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ValueParser<string>(null, s => s));
            Assert.Throws<ArgumentNullException>(() => new ValueParser<string>(s => s, null));
        }

        #region Parse

        [Fact]
        public void TestParse()
        {
            ValueParser<string> parser = new ValueParser<string>(s => "PARSED VALUE >>>" + s + "<<<", s => s);

            Assert.Equal("PARSED VALUE >>>SOME TEXT<<<", parser.Parse("SOME TEXT"));
            Assert.Equal("PARSED VALUE >>>OTHER STUFF<<<", parser.Parse("OTHER STUFF"));
        }

        [Fact]
        public void TestParse__NullThrows()
        {
            ValueParser<string> parser = new ValueParser<string>(s => "PARSED VALUE >>>" + s + "<<<", s => s);

            Assert.Throws<ArgumentNullException>(() => parser.Parse(null));
        }

        #endregion

        #region Format

        [Fact]
        public void TestFormat()
        {
            ValueParser<string> parser = new ValueParser<string>(s => s, s => "STUFF!!" + s + "!!");

            Assert.Equal("STUFF!!blah!!", parser.Format("blah"));
            Assert.Equal("STUFF!!bleh!!", parser.Format("bleh"));
        }

        [Fact]
        public void TestFormat__NullThrows()
        {
            ValueParser<string> parser = new ValueParser<string>(s => s, s => "STUFF!!" + s + "!!");

            Assert.Throws<ArgumentNullException>(() => parser.Format(null));
        }

        [Fact]
        public void TestForm__WrongType()
        {
            ValueParser<string> parser = new ValueParser<string>(s => s, s => s);

            Assert.Throws<ArgumentException>(() => parser.Format(123));
        }

        #endregion

        [Fact]
        public void TestParseType()
        {
            ValueParser<string> parser1 = new ValueParser<string>(s => s, s => s);

            Assert.Equal(typeof(string), parser1.ParseType);

            ValueParser<int> parser2 = new ValueParser<int>(s => 0, i => "int");

            Assert.Equal(typeof(int), parser2.ParseType);
        }
    }
}
