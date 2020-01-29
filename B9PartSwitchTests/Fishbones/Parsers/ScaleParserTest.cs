using System;
using Xunit;
using UnityEngine;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class ScaleParserTest
    {
        private readonly ScaleParser parser = new ScaleParser();

        [Fact]
        public void TestParse__Single()
        {
            Assert.Equal(new Vector3(1.23f, 1.23f, 1.23f), parser.Parse("1.23"));
        }

        [Fact]
        public void TestParse__Vector__Space()
        {
            Assert.Equal(new Vector3(1.23f, 2.34f, 3.45f), parser.Parse("1.23 2.34  3.45"));
        }

        [Fact]
        public void TestParse__Vector__Tab()
        {
            Assert.Equal(new Vector3(1.23f, 2.34f, 3.45f), parser.Parse("1.23\t2.34\t\t3.45"));
        }

        [Fact]
        public void TestParse__Vector__Comma()
        {
            Assert.Equal(new Vector3(1.23f, 2.34f, 3.45f), parser.Parse("1.23,2.34, 3.45"));
        }

        [Fact]
        public void TestParse__ValueNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                parser.Parse(null);
            });
        }

        [Fact]
        public void TestParse__Emptyish()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                parser.Parse(", ");
            });

            Assert.Equal("Could not parse value as scale because it split into 0 values: ', '", ex.Message);
        }

        [Fact]
        public void TestParse__TwoValues()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                parser.Parse("1.23, 3.45");
            });

            Assert.Equal("Could not parse value as scale because it split into 2 values: '1.23, 3.45'", ex.Message);
        }

        [Fact]
        public void TestParse__FourValues()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                parser.Parse("1.23, 2.34, 3.45, 4.56");
            });

            Assert.Equal("Could not parse value as scale because it split into 4 values: '1.23, 2.34, 3.45, 4.56'", ex.Message);
        }

        [Fact]
        public void TestParse__NotAFloat()
        {
            Assert.Throws<FormatException>(delegate
            {
                parser.Parse("1.23, aaaaaa, 3.45");
            });
        }
    }
}
