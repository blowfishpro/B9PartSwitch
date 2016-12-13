using System;
using Xunit;
using B9PartSwitch.Fishbones.NodeDataMappers;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class ValueScalarMapperTest
    {
        private ValueScalarMapper mapper = new ValueScalarMapper("foo", Exemplars.ValueParser);

        [Fact]
        public void TestLoad()
        {
            WrappedConfigNode node = new WrappedConfigNode
            {
                { "foo", "bar" },
                { "omg", "bbq" }
            };

            object value = null;
            Assert.True(mapper.Load(node, ref value));
            Assert.Equal("!!bar!!", value);
        }

        [Fact]
        public void TestLoad__NoValue()
        {
            WrappedConfigNode node = new WrappedConfigNode
            {
                { "boo", "bar" },
                { "omg", "bbq" }
            };

            object value = null;
            Assert.False(mapper.Load(node, ref value));
            Assert.Null(value);
        }

        [Fact]
        public void TestLoad__NodeNull()
        {
            object value = null;
            Assert.Throws<ArgumentNullException>(() => mapper.Load(null, ref value));
        }

        [Fact]
        public void TestSave()
        {
            ConfigNode node = new ConfigNode();
            object value = "bar";

            Assert.True(mapper.Save(node, ref value));
            Assert.Equal("$$bar$$", node.GetValue("foo"));
        }

        [Fact]
        public void TestSave__NodeNull()
        {
            object value = "bar";
            Assert.Throws<ArgumentNullException>(() => mapper.Load(null, ref value));
        }
    }
}
