using System;
using Xunit;
using B9PartSwitch.Utils;

namespace B9PartSwitchTests.Utils
{
    public class GroupedStringBuilderTest
    {
        private readonly GroupedStringBuilder stringBuilder = new GroupedStringBuilder();
        [Fact]
        public void TestBeginGroup()
        {
            stringBuilder.BeginGroup();
            stringBuilder.Append("abc");
            stringBuilder.BeginGroup();
            stringBuilder.AppendLine("def");
            stringBuilder.AppendLine("ghi");
            stringBuilder.BeginGroup();
            stringBuilder.BeginGroup();
            stringBuilder.AppendLine("jkl");
            stringBuilder.BeginGroup();

            Assert.Equal("abc\n\ndef\nghi\n\njkl", stringBuilder.ToString());
        }

        [Fact]
        public void TestAppend()
        {
            stringBuilder.Append("a");
            stringBuilder.Append("b {0:0.0} c", 1);
            stringBuilder.Append("d {0:0.0} e {1:0.0} f", 1, 2);
            stringBuilder.Append("g {0:0.0} h {1:0.0} i {2:0.0} j", 1, 2, 3);

            Assert.Equal("ab 1.0 cd 1.0 e 2.0 fg 1.0 h 2.0 i 3.0 j", stringBuilder.ToString());
        }

        [Fact]
        public void TestAppendLine()
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("a");
            stringBuilder.Append("b");
            stringBuilder.AppendLine("c {0:0.0} d", 1);
            stringBuilder.AppendLine("e {0:0.0} f {1:0.0} g", 1, 2);

            Assert.Equal("\na\nbc 1.0 d\ne 1.0 f 2.0 g", stringBuilder.ToString());
        }

        [Fact]
        public void TestClear()
        {
            stringBuilder.Append("abc");
            stringBuilder.BeginGroup();
            Assert.Equal("abc", stringBuilder.ToString());
            stringBuilder.Clear();
            stringBuilder.Append("def");
            Assert.Equal("def", stringBuilder.ToString());
        }
    }
}
