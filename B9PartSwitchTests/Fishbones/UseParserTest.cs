using System;
using Xunit;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones
{
    public class UseParserTest
    {
        private class DummyValueParser : IValueParser
        {
            public Type ParseType { get { throw new NotImplementedException(); } }

            public string Format(object value)
            {
                throw new NotImplementedException();
            }

            public object Parse(string value)
            {
                throw new NotImplementedException();
            }
        }

        private class DummyValueParserWithNoConstructor : DummyValueParser
        {
            private DummyValueParserWithNoConstructor() { }
        }

        [Fact]
        public void TestNew()
        {
            new UseParser(typeof(DummyValueParser));
        }

        [Fact]
        public void TestNew__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new UseParser(null));
        }

        [Fact]
        public void TestNew__BadType()
        {
            Assert.Throws<ArgumentException>(() => new UseParser(typeof(DummyClass)));
        }

        [Fact]
        public void TestNew__NoParameterlessConstructor()
        {
            Assert.Throws<ArgumentException>(() => new UseParser(typeof(DummyValueParserWithNoConstructor)));
        }

        [Fact]
        public void TestCreateParser()
        {
            UseParser useParser = new UseParser(typeof(DummyValueParser));

            Assert.IsType<DummyValueParser>(useParser.CreateParser());
        }
    }
}
