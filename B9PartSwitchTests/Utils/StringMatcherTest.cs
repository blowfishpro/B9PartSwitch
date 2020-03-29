using System;
using Xunit;
using B9PartSwitch.Utils;

namespace B9PartSwitchTests.Utils
{
    public class StringMatcherTest
    {
        [Fact]
        public void TestParse__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                IStringMatcher matcher = StringMatcher.Parse(null);
            });
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData("", "not empty", false)]
        [InlineData("a string", "a string", true)]
        [InlineData("a string", "", false)]
        [InlineData("a string", "a string within a string", false)]
        [InlineData("a*string", "astring", true)]
        [InlineData("a*string", "a longer string", true)]
        [InlineData("a*string", "", false)]
        [InlineData("a*string", "not a string", false)]
        [InlineData("a?string", "a string", true)]
        [InlineData("a?string", "a-string", true)]
        [InlineData("a?string", "aastring", true)]
        [InlineData("a?string", "astring", false)]
        [InlineData("a?string", "any string", false)]
        [InlineData("a*string.*", "a string.*", true)]
        [InlineData("a*string.*", "a string and stuff", false)]
        [InlineData("/a(?:\\slonger)?\\sstring/", "a string", true)]
        [InlineData("/a(?:\\slonger)?\\sstring/", "a longer string", true)]
        [InlineData("/a(?:\\slonger)?\\sstring/", "alongerstring", false)]
        [InlineData("/a(?:\\slonger)?\\sstring/", "a different string", false)]
        [InlineData("/a(?:\\slonger)?\\sstring/", "", false)]
        [InlineData("\\/a/path/", "/a/path/", true)]
        [InlineData("\\/a/path/", "a/path", false)]
        [InlineData("\\/a/path/", "", false)]
        public void TestParse__Match__ToString(string matchString, string testSting, bool result)
        {
            IStringMatcher matcher = StringMatcher.Parse(matchString);
            Assert.Equal(result, matcher.Match(testSting));

            Assert.Throws<ArgumentNullException>(delegate
            {
                matcher.Match(null);
            });

            string otherString = matcher.ToString();
            IStringMatcher matcher2 = StringMatcher.Parse(otherString);
            Assert.Equal(result, matcher2.Match(testSting));
        }
    }
}
