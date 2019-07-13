using System;
using Xunit;
using UnityEngine;
using B9PartSwitch.Utils;

namespace B9PartSwitchTests.Utils
{
    public class ColorParserTest
    {
        [Fact]
        public void TestParseColor__Null()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                ColorParser.Parse(null);
            });
        }

        [Fact]
        public void TestParseColor__Empty()
        {
            Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("");
            });
        }

        [Fact]
        public void TestParseColor__HexStr__RGB()
        {
            Color color = ColorParser.Parse("#07f");

            Assert.Equal(0, color.r);
            Assert.Equal(0x7 / 15f, color.g);
            Assert.Equal(1, color.b);
            Assert.Equal(1, color.a);
        }

        [Fact]
        public void TestParseColor__HexStr__RGBA()
        {
            Color color = ColorParser.Parse("#fa50");

            Assert.Equal(1, color.r);
            Assert.Equal(0xa / 15f, color.g);
            Assert.Equal(0x5 / 15f, color.b);
            Assert.Equal(0, color.a);
        }

        [Fact]
        public void TestParseColor__HexStr__RRGGBB()
        {
            Color color = ColorParser.Parse("#002fff");

            Assert.Equal(0, color.r);
            Assert.Equal(0x2f / 255f, color.g);
            Assert.Equal(1, color.b);
            Assert.Equal(1, color.a);
        }

        [Fact]
        public void TestParseColor__HexStr__RRGGBBAA()
        {
            Color color = ColorParser.Parse("#ffab5600");

            Assert.Equal(1, color.r);
            Assert.Equal(0xab / 255f, color.g);
            Assert.Equal(0x56 / 255f, color.b);
            Assert.Equal(0, color.a);
        }

        [Fact]
        public void TestParseColor__HexStr__Upper()
        {
            Color color = ColorParser.Parse("#0AF");

            Assert.Equal(0, color.r);
            Assert.Equal(0xa / 15f, color.g);
            Assert.Equal(1, color.b);
            Assert.Equal(1, color.a);
        }

        [Fact]
        public void TestParseColor__HexStr__InvalidHex()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("#00g");
            });

            Assert.Equal("Invalid hexadecimal character: g", ex.Message);
        }

        [Fact]
        public void TestParseColor__HexStr__TooShort()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("#ab");
            });

            Assert.Equal("Value looks like HTML color (begins with #) but has wrong number of digits (must be 3, 4, 6, or 8): #ab", ex.Message);
        }

        [Fact]
        public void TestParseColor__HexStr__FiveDigits()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("#abcde");
            });

            Assert.Equal("Value looks like HTML color (begins with #) but has wrong number of digits (must be 3, 4, 6, or 8): #abcde", ex.Message);
        }

        [Fact]
        public void TestParseColor__HexStr__TooLong()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("#012345678");
            });

            Assert.Equal("Value looks like HTML color (begins with #) but has wrong number of digits (must be 3, 4, 6, or 8): #012345678", ex.Message);
        }

        [Fact]
        public void TestParseColor__NamedColor__red()
        {
            Color color = ColorParser.Parse("red");
            Assert.Equal(Color.red, color);
        }

        [Fact]
        public void TestParseColor__NamedColor__magenta()
        {
            Color color = ColorParser.Parse("magenta");
            Assert.Equal(Color.magenta, color);
        }

        [Fact]
        public void TestParseColor__NamedColor__xkcd__Red()
        {
            Color color = ColorParser.Parse("Red");
            Assert.Equal(XKCDColors.Red, color);
        }

        [Fact]
        public void TestParseColor__NamedColor__xkcd__Goldenrod()
        {
            Color color = ColorParser.Parse("Goldenrod");
            Assert.Equal(XKCDColors.Goldenrod, color);
        }

        [Fact]
        public void TestParseColor__NamedColor__xkcd__GoldenRod()
        {
            Color color = ColorParser.Parse("GoldenRod");
            Assert.Equal(XKCDColors.GoldenRod, color);
        }

        [Fact]
        public void TestParseColor__FloatList__RGB()
        {
            Color color = ColorParser.Parse("0, 0.5, 1");

            Assert.Equal(0, color.r);
            Assert.Equal(0.5f, color.g);
            Assert.Equal(1, color.b);
            Assert.Equal(1, color.a);
        }

        [Fact]
        public void TestParseColor__FloatList__RGBA()
        {
            Color color = ColorParser.Parse("1, 0.7, 0.3, 0");

            Assert.Equal(1, color.r);
            Assert.Equal(0.7f, color.g);
            Assert.Equal(0.3f, color.b);
            Assert.Equal(0, color.a);
        }

        [Fact]
        public void TestParseColor__FloatList__CommaSeparatedWithNoSpaces()
        {
            Color color = ColorParser.Parse("0,0.5,1");

            Assert.Equal(0, color.r);
            Assert.Equal(0.5f, color.g);
            Assert.Equal(1, color.b);
            Assert.Equal(1, color.a);
        }

        [Fact]
        public void TestParseColor__FloatList__SpaceSeparated()
        {
            Color color = ColorParser.Parse("0 0.5 1");

            Assert.Equal(0, color.r);
            Assert.Equal(0.5f, color.g);
            Assert.Equal(1, color.b);
            Assert.Equal(1, color.a);
        }

        [Fact]
        public void TestParseColor__FloatList__TabSeparated()
        {
            Color color = ColorParser.Parse("0\t0.5\t1");

            Assert.Equal(0, color.r);
            Assert.Equal(0.5f, color.g);
            Assert.Equal(1, color.b);
            Assert.Equal(1, color.a);
        }

        [Fact]
        public void TestParseColor__FloatList__TooShort()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("0, 1");
            });

            Assert.Equal("Could not value parse as color: 0, 1", ex.Message);
        }

        [Fact]
        public void TestParseColor__FloatList__TooLong()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("0, 1, 2, 3, 4");
            });

            Assert.Equal("Could not value parse as color: 0, 1, 2, 3, 4", ex.Message);
        }

        [Fact]
        public void TestParseColor__FloatList__InvalidFloat()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("0, x, 1");
            });

            Assert.Equal("Invalid float value when parsing color (should be 0-1): x", ex.Message);
        }

        [Fact]
        public void TestParseColor__FloatList__NaN()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("0, NaN, 1");
            });

            Assert.Equal("Invalid float value when parsing color (should be 0-1): NaN", ex.Message);
        }

        [Fact]
        public void TestParseColor__FloatList__TooSmall()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("0, -0.1, 1");
            });

            Assert.Equal("Invalid float value when parsing color (should be 0-1): -0.1", ex.Message);
        }

        [Fact]
        public void TestParseColor__FloatList__TooLarge()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("0, 1.1, 1");
            });

            Assert.Equal("Invalid float value when parsing color (should be 0-1): 1.1", ex.Message);
        }

        [Fact]
        public void TestParseColor__UnknownFormat()
        {
            FormatException ex = Assert.Throws<FormatException>(delegate
            {
                ColorParser.Parse("stuff");
            });

            Assert.Equal("Could not value parse as color: stuff", ex.Message);
        }
    }
}
