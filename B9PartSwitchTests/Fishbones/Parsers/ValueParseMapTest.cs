using System;
using Xunit;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class ValueParseMapTest
    {
        [Fact]
        public void TestAddParser__GetParser()
        {
            ValueParseMap map = new ValueParseMap();
            map.AddParser(Exemplars.ValueParser);

            Assert.Same(Exemplars.ValueParser, map.GetParser(typeof(string)));
        }

        [Fact]
        public void TestAddParser__GetParser__Func()
        {
            ValueParseMap map = new ValueParseMap();
            map.AddParser<string>(s => $">{s}<", s => $"<{s}>");

            Assert.Equal(">abc<", map.GetParser(typeof(string)).Parse("abc"));
            Assert.Equal("<abc>", map.GetParser(typeof(string)).Format("abc"));
        }

        [Fact]
        public void TestAddParser__Null()
        {
            ValueParseMap map = new ValueParseMap();
            Assert.Throws<ArgumentNullException>(() => map.AddParser<int>(null, i => i.ToString()));
            Assert.Throws<ArgumentNullException>(() => map.AddParser<int>(int.Parse, null));
            Assert.Throws<ArgumentNullException>(() => map.AddParser(null));
        }

        [Fact]
        public void TestGetParser__NotRegistered()
        {
            ValueParseMap map = new ValueParseMap();
            map.AddParser(Exemplars.ValueParser);

            Assert.Throws<ParseTypeNotRegisteredException>(() => map.GetParser(typeof(double)));
        }

        [Fact]
        public void TestGetParser__Null()
        {
            ValueParseMap map = new ValueParseMap();
            map.AddParser(Exemplars.ValueParser);

            Assert.Throws<ArgumentNullException>(() => map.GetParser(null));
        }

        [Fact]
        public void TestClone()
        {
            IValueParser parser1 = new ValueParser<int>(s => 0, i => "");
            IValueParser parser2 = new ValueParser<bool>(s => false, b => "");
            IValueParser parser3 = new ValueParser<string>(s => s, s => s);

            ValueParseMap map = new ValueParseMap();
            map.AddParser(parser1);
            map.AddParser(parser2);

            ValueParseMap clone = map.Clone();

            Assert.Same(parser1, clone.GetParser(typeof(int)));
            Assert.Same(parser2, clone.GetParser(typeof(bool)));

            clone.AddParser(parser3);

            Assert.Same(parser3, clone.GetParser(typeof(string)));
            Assert.Throws<ParseTypeNotRegisteredException>(() => map.GetParser(typeof(string)));
        }
    }
}
