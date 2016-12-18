using System;
using Xunit;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class ExceptionsTest
    {
        private class SomeClass { }

        [Fact]
        public void TestParseTypeNotRegisteredException()
        {
            Exception e = new ParseTypeNotRegisteredException(typeof(SomeClass));

            Assert.Equal("Attempted to get the parser for type 'B9PartSwitchTests.Fishbones.Parsers.ExceptionsTest+SomeClass', but it has not been registered", e.Message);
        }

        [Fact]
        public void TestParseTypeNotRegisteredException__Null()
        {
            Assert.Throws<ArgumentNullException>(() => new ParseTypeNotRegisteredException(null));
        }

        [Fact]
        public void TestParseTypeAlreadyRegisteredException()
        {
            Exception e = new ParseTypeAlreadyRegisteredException(typeof(SomeClass));

            Assert.Equal("Attempted to register perser for type 'B9PartSwitchTests.Fishbones.Parsers.ExceptionsTest+SomeClass', but it has already been registered", e.Message);
        }

        [Fact]
        public void TestParseTypeAlreadyRegisteredException__Null()
        {
            Assert.Throws<ArgumentNullException>(() => new ParseTypeAlreadyRegisteredException(null));
        }
    }
}
