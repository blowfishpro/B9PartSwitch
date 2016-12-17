using System;
using System.Collections.Generic;
using Xunit;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitchTests.TestUtils;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class ValueListMapperTest
    {
        private ValueListMapper mapper = new ValueListMapper("someValue", Exemplars.ValueParser);

        [Fact]
        public void TestLoad()
        {
            List<string> list = new List<string>();
            object value = list;

            ConfigNode node = new TestConfigNode
            {
                { "someValue", "blah1" },
                { "someValue", "blah2" },
                { "someOtherValue", "blah3" },
            };

            Assert.True(mapper.Load(node, ref value));
            Assert.Same(list, value);
            Assert.Equal(2, list.Count);

            Assert.Equal("!!blah1!!", list[0]);
            Assert.Equal("!!blah2!!", list[1]);
        }

        [Fact]
        public void TestLoad__Null()
        {
            object value = null;

            ConfigNode node = new TestConfigNode
            {
                { "someValue", "blah1" },
                { "someValue", "blah2" },
                { "someOtherValue", "blah3" },
            };

            Assert.True(mapper.Load(node, ref value));
            Assert.NotNull(value);
            Assert.IsType<List<string>>(value);

            List<string> list = (List<string>)value;

            Assert.Equal(2, list.Count);

            Assert.Equal("!!blah1!!", list[0]);
            Assert.Equal("!!blah2!!", list[1]);
        }

        [Fact]
        public void TestLoad__ExistingValue()
        {
            List<string> list = new List<string> { "blah0" };
            object value = list;

            ConfigNode node = new TestConfigNode
            {
                { "someValue", "blah1" },
                { "someValue", "blah2" },
                { "someOtherValue", "blah3" },
            };

            Assert.True(mapper.Load(node, ref value));
            Assert.Same(list, value);
            Assert.Equal(3, list.Count);

            Assert.Equal("blah0", list[0]);
            Assert.Equal("!!blah1!!", list[1]);
            Assert.Equal("!!blah2!!", list[2]);
        }

        [Fact]
        public void TestLoad__NoValues()
        {
            List<string> list = new List<string> { "blah0" };
            object value = list;

            ConfigNode node = new TestConfigNode
            {
                { "someOtherValue", "blah3" },
            };

            Assert.False(mapper.Load(node, ref value));
            Assert.Same(list, value);
            Assert.Equal(1, list.Count);

            Assert.Equal("blah0", list[0]);
        }

        [Fact]
        public void TestLoad__NullNode()
        {
            object dummy = new List<string> { "blah0" };
            Assert.Throws<ArgumentNullException>(() => mapper.Load(null, ref dummy));
        }

        [Fact]
        public void TestLoad__WrongType()
        {
            object dummy = "why are you passing a string?";

            ConfigNode node = new TestConfigNode
            {
                { "someValue", "something" },
            };

            Assert.Throws<ArgumentException>(() => mapper.Load(new ConfigNode(), ref dummy));
            Assert.Throws<ArgumentException>(() => mapper.Load(node, ref dummy));
        }

        [Fact]
        public void TestSave()
        {
            List<string> list = new List<string> { "blah1", "blah2" };
            ConfigNode node = new ConfigNode();

            Assert.True(mapper.Save(node, list));
            Assert.Equal(new[] { "$$blah1$$", "$$blah2$$" }, node.GetValues("someValue"));

            Assert.Equal(new[] { "blah1", "blah2" }, list.ToArray());
        }

        [Fact]
        public void TestSave__Null()
        {
            object value = null;
            ConfigNode node = new ConfigNode();

            Assert.False(mapper.Save(node, value));
            Assert.Empty(node.GetValues("someValue"));
            Assert.Null(value);
        }

        [Fact]
        public void TestSave__Empty()
        {
            List<string> value = new List<string>();
            ConfigNode node = new TestConfigNode
            {
                { "someValue", "blah0" },
            };

            Assert.False(mapper.Save(node, value));
            Assert.Equal(new[] { "blah0" }, node.GetValues("someValue"));
            Assert.Empty(value);
        }

        [Fact]
        public void TestSave__ExistingValue()
        {
            List<string> list = new List<string> { "blah1", "blah2" };
            ConfigNode node = new TestConfigNode
            {
                { "someValue", "blah0" },
            };

            Assert.True(mapper.Save(node, list));
            Assert.Equal(new[] { "blah0", "$$blah1$$", "$$blah2$$" }, node.GetValues("someValue"));

            Assert.Equal(new[] { "blah1", "blah2" }, list.ToArray());
        }

        [Fact]
        public void TestSave__NullInList()
        {
            List<string> list = new List<string> { "blah1", null, "blah2" };
            ConfigNode node = new ConfigNode();

            Assert.True(mapper.Save(node, list));
            Assert.Equal(new[] { "$$blah1$$", "$$blah2$$" }, node.GetValues("someValue"));
        }

        [Fact]
        public void TestSave__NullNode()
        {
            List<string> dummy = new List<string> { "blah0" };
            Assert.Throws<ArgumentNullException>(() => mapper.Save(null, dummy));
        }

        [Fact]
        public void TestSave__WrongType()
        {
            string dummy = "why are you passing a string?";

            ConfigNode node = new TestConfigNode
            {
                { "someValue", "something" },
            };

            Assert.Throws<ArgumentException>(() => mapper.Save(new ConfigNode(), dummy));
            Assert.Throws<ArgumentException>(() => mapper.Save(node, dummy));
        }
    }
}
