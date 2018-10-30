using System;
using Xunit;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitchTests.TestUtils;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class NodeObjectWrapperIContextualNodeTest
    {
        private readonly NodeObjectWrapperIContextualNode wrapper = new NodeObjectWrapperIContextualNode(typeof(DummyIContextualNode));
        #region Constructor

        [Fact]
        public void TestConstructor()
        {
            Assert.Equal(typeof(DummyIContextualNode), wrapper.type);
        }

        [Fact]
        public void TestConstructor__NullType()
        {
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(delegate
            {
                new NodeObjectWrapperIContextualNode(null);
            });

            Assert.Equal("type", ex.ParamName);
        }

        [Fact]
        public void TestConstructor__NotIConfigNode()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(delegate
            {
                new NodeObjectWrapperIContextualNode(typeof(string));
            });

            Assert.Contains("Type System.String does not implement B9PartSwitch.Fishbones.IContextualNode", ex.Message);
            Assert.Equal("type", ex.ParamName);
        }

        #endregion

        #region Load

        [Fact]
        public void TestLoad()
        {
            DummyIContextualNode obj = new DummyIContextualNode();
            object objRef = obj;

            ConfigNode node = new TestConfigNode
            {
                { "value", "blah" },
            };

            wrapper.Load(ref objRef, node, Exemplars.LoadPrefabContext);

            Assert.Same(obj, objRef);
            Assert.Equal("blah", obj.value);
        }

        [Fact]
        public void TestLoad__NullObj()
        {
            object obj = null;

            ConfigNode node = new TestConfigNode
            {
                { "value", "blah" },
            };

            wrapper.Load(ref obj, node, Exemplars.LoadPrefabContext);

            DummyIContextualNode newObj = Assert.IsType<DummyIContextualNode>(obj);
            Assert.Equal("blah", newObj.value);
        }

        [Fact]
        public void TestLoad__BadType()
        {
            object obj = "this is a string";

            ArgumentException ex = Assert.Throws<ArgumentException>(delegate
            {
                wrapper.Load(ref obj, new ConfigNode(), Exemplars.LoadPrefabContext);
            });

            Assert.Contains("Expected parameter of type B9PartSwitch.Fishbones.IContextualNode but got System.String", ex.Message);
            Assert.Equal("obj", ex.ParamName);
        }

        [Fact]
        public void TestLoad__NullNode()
        {
            object obj = null;

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(delegate
            {
                wrapper.Load(ref obj, null, Exemplars.LoadPrefabContext);
            });

            Assert.Equal("node", ex.ParamName);
        }

        #endregion

        #region Save

        [Fact]
        public void TestSave()
        {
            DummyIContextualNode obj = new DummyIContextualNode()
            {
                value = "stuff",
            };

            ConfigNode node = wrapper.Save(obj, Exemplars.SaveContext);
        }

        [Fact]
        public void TestSave__NullObj()
        {
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(delegate
            {
                wrapper.Save(null, Exemplars.LoadPrefabContext);
            });

            Assert.Equal("obj", ex.ParamName);
        }

        [Fact]
        public void TestSave__WrongType()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(delegate
            {
                wrapper.Save("this is a string", Exemplars.LoadPrefabContext);
            });

            Assert.Contains("Expected parameter of type B9PartSwitch.Fishbones.IContextualNode but got System.String", ex.Message);
            Assert.Equal("obj", ex.ParamName);
        }

        #endregion
    }
}
