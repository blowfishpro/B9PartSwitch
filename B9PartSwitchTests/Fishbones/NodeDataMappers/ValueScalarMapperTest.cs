﻿using System;
using Xunit;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitchTests.TestUtils;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class ValueScalarMapperTest
    {
        private ValueScalarMapper mapper = new ValueScalarMapper("foo", Exemplars.ValueParser);

        [Fact]
        public void TestLoad()
        {
            TestConfigNode node = new TestConfigNode
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
            TestConfigNode node = new TestConfigNode
            {
                { "boo", "bar" },
                { "omg", "bbq" }
            };

            object value = "abc";
            Assert.False(mapper.Load(node, ref value));
            Assert.Equal("abc", value);
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
            Assert.Equal("bar", value);
        }

        [Fact]
        public void TestSave__NullValue()
        {
            ConfigNode node = new ConfigNode();
            object value = null;

            Assert.False(mapper.Save(node, ref value));
            Assert.Null(node.GetValue("foo"));
        }

        [Fact]
        public void TestSave__NodeNull()
        {
            object value = "bar";
            Assert.Throws<ArgumentNullException>(() => mapper.Load(null, ref value));
        }
    }
}