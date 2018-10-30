using System;
using System.Collections.Generic;
using Xunit;
using NSubstitute;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitchTests.TestUtils;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class ValueListMapperTest
    {
        private ValueListMapper mapper = new ValueListMapper("someValue", Exemplars.ValueParser);

        #region Constructor

        [Fact]
        public void TestNew()
        {
            IValueParser parser = Substitute.For<IValueParser>();
            parser.ParseType.Returns(typeof(DummyClass));

            ValueListMapper mapper = new ValueListMapper("bar", parser);

            Assert.Equal("bar", mapper.name);
            Assert.Same(parser, mapper.parser);

            Assert.Same(typeof(DummyClass), mapper.elementType);
            Assert.Same(typeof(List<DummyClass>), mapper.listType);
        }

        #endregion

        #region Load

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

            Assert.True(mapper.Load(ref value, node, Exemplars.LoadPrefabContext));
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

            Assert.True(mapper.Load(ref value, node, Exemplars.LoadPrefabContext));
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

            Assert.True(mapper.Load(ref value, node, Exemplars.LoadPrefabContext));
            Assert.Same(list, value);
            Assert.Equal(3, list.Count);

            Assert.Equal("blah0", list[0]);
            Assert.Equal("!!blah1!!", list[1]);
            Assert.Equal("!!blah2!!", list[2]);
        }

        [Fact]
        public void TestLoad__ExistingValue__Deserialize()
        {
            List<string> list = new List<string> { "blah0" };
            object value = list;

            ConfigNode node = new TestConfigNode
            {
                { "someValue", "blah1" },
                { "someValue", "blah2" },
                { "someOtherValue", "blah3" },
            };

            OperationContext context = new OperationContext(Operation.Deserialize, new object());
            Assert.True(mapper.Load(ref value, node, context));
            Assert.Same(list, value);
            Assert.Equal(2, list.Count);

            Assert.Equal("!!blah1!!", list[0]);
            Assert.Equal("!!blah2!!", list[1]);
        }

        [Fact]
        public void TestLoad__ExistingValue__LoadInstance()
        {
            List<string> list = new List<string> { "blah0" };
            object value = list;

            ConfigNode node = new TestConfigNode
            {
                { "someValue", "blah1" },
                { "someValue", "blah2" },
                { "someOtherValue", "blah3" },
            };

            OperationContext context = new OperationContext(Operation.LoadInstance, new object());
            Assert.True(mapper.Load(ref value, node, context));
            Assert.Same(list, value);
            Assert.Equal(2, list.Count);
            
            Assert.Equal("!!blah1!!", list[0]);
            Assert.Equal("!!blah2!!", list[1]);
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

            Assert.False(mapper.Load(ref value, node, Exemplars.LoadPrefabContext));
            Assert.Same(list, value);
            Assert.Single(list);

            Assert.Equal("blah0", list[0]);
        }

        [Fact]
        public void TestLoad__NullNode()
        {
            object dummy = new List<string> { "blah0" };
            Assert.Throws<ArgumentNullException>(() => mapper.Load(ref dummy, null, Exemplars.LoadPrefabContext));
        }

        [Fact]
        public void TestLoad__WrongType()
        {
            object dummy = "why are you passing a string?";

            ConfigNode node = new TestConfigNode
            {
                { "someValue", "something" },
            };

            Assert.Throws<ArgumentException>(() => mapper.Load(ref dummy, new ConfigNode(), Exemplars.LoadPrefabContext));
            Assert.Throws<ArgumentException>(() => mapper.Load(ref dummy, node, Exemplars.LoadPrefabContext));
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            List<string> list = new List<string> { "blah1", "blah2" };
            ConfigNode node = new ConfigNode();

            Assert.True(mapper.Save(list, node, Exemplars.SaveContext));
            Assert.Equal(new[] { "$$blah1$$", "$$blah2$$" }, node.GetValues("someValue"));

            Assert.Equal(new[] { "blah1", "blah2" }, list.ToArray());
        }

        [Fact]
        public void TestSave__Null()
        {
            object value = null;
            ConfigNode node = new ConfigNode();

            Assert.False(mapper.Save(value, node, Exemplars.SaveContext));
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

            Assert.False(mapper.Save(value, node, Exemplars.SaveContext));
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

            Assert.True(mapper.Save(list, node, Exemplars.SaveContext));
            Assert.Equal(new[] { "blah0", "$$blah1$$", "$$blah2$$" }, node.GetValues("someValue"));

            Assert.Equal(new[] { "blah1", "blah2" }, list.ToArray());
        }

        [Fact]
        public void TestSave__NullInList()
        {
            List<string> list = new List<string> { "blah1", null, "blah2" };
            ConfigNode node = new ConfigNode();

            Assert.True(mapper.Save(list, node, Exemplars.SaveContext));
            Assert.Equal(new[] { "$$blah1$$", "$$blah2$$" }, node.GetValues("someValue"));
        }

        [Fact]
        public void TestSave__NullNode()
        {
            List<string> dummy = new List<string> { "blah0" };
            Assert.Throws<ArgumentNullException>(() => mapper.Save(dummy, null, Exemplars.SaveContext));
        }

        [Fact]
        public void TestSave__WrongType()
        {
            string dummy = "why are you passing a string?";

            ConfigNode node = new TestConfigNode
            {
                { "someValue", "something" },
            };

            Assert.Throws<ArgumentException>(() => mapper.Save(dummy, new ConfigNode(), Exemplars.SaveContext));
            Assert.Throws<ArgumentException>(() => mapper.Save(dummy, node, Exemplars.SaveContext));
        }

        #endregion
    }
}
