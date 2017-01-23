using System;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitchTests.TestUtils;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class ValueScalarMapperTest
    {
        private ValueScalarMapper mapper = new ValueScalarMapper("foo", Exemplars.ValueParser);

        #region Constructor

        [Fact]
        public void TestNew()
        {
            IValueParser parser = Substitute.For<IValueParser>();

            ValueScalarMapper mapper = new ValueScalarMapper("bar", parser);

            Assert.Equal("bar", mapper.name);
            Assert.Same(parser, mapper.parser);
        }

        #endregion

        #region Load

        [Fact]
        public void TestLoad()
        {
            TestConfigNode node = new TestConfigNode
            {
                { "foo", "bar" },
                { "omg", "bbq" }
            };

            object value = null;
            Assert.True(mapper.Load(ref value, node, Exemplars.LoadContext));
            Assert.Equal("!!bar!!", value);
        }

        [Fact]
        public void TestLoad__NoValue()
        {
            TestConfigNode node = new TestConfigNode
            {
                { "boo", "bar" },
                { "omg", "bbq" }
            };

            object value = "abc";
            Assert.False(mapper.Load(ref value, node, Exemplars.LoadContext));
            Assert.Equal("abc", value);
        }

        [Fact]
        public void TestLoad__NodeNull()
        {
            object value = null;
            Assert.Throws<ArgumentNullException>(() => mapper.Load(ref value, null, Exemplars.LoadContext));
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            ConfigNode node = new ConfigNode();
            object value = "bar";

            Assert.True(mapper.Save(value, node, Exemplars.SaveContext));
            Assert.Equal("$$bar$$", node.GetValue("foo"));
            Assert.Equal("bar", value);
        }

        [Fact]
        public void TestSave__NullValue()
        {
            ConfigNode node = new ConfigNode();
            object value = null;

            Assert.False(mapper.Save(value, node, Exemplars.SaveContext));
            Assert.Null(node.GetValue("foo"));
        }

        [Fact]
        public void TestSave__NodeNull()
        {
            object value = "bar";
            Assert.Throws<ArgumentNullException>(() => mapper.Save(value, null, Exemplars.SaveContext));
        }

        #endregion
    }
}
