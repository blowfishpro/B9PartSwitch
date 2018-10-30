using System;
using Xunit;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class NodeObjectWrapperTest
    {
        #region For

        [Fact]
        public void TestFor__IConfigNode()
        {
            NodeObjectWrapperIConfigNode wrapper = Assert.IsType<NodeObjectWrapperIConfigNode>(NodeObjectWrapper.For(typeof(DummyIConfigNode)));
            Assert.Equal(typeof(DummyIConfigNode), wrapper.type);
        }

        [Fact]
        public void TestFor__IContextualNode()
        {
            NodeObjectWrapperIContextualNode wrapper = Assert.IsType<NodeObjectWrapperIContextualNode>(NodeObjectWrapper.For(typeof(DummyIContextualNode)));
            Assert.Equal(typeof(DummyIContextualNode), wrapper.type);
        }

        [Fact]
        public void TestFor__ConfigNode()
        {
            Assert.IsType<NodeObjectWrapperConfigNode>(NodeObjectWrapper.For(typeof(ConfigNode)));
        }

        [Fact]
        public void TestFor__OtherType()
        {
            NotImplementedException ex = Assert.Throws<NotImplementedException>(delegate
            {
                NodeObjectWrapper.For(typeof(string));
            });

            Assert.Equal("No way to build node object wrapper for type System.String", ex.Message);
        }

        #endregion

        #region IsNodeType

        [Fact]
        public void TestIsNodeType()
        {
            Assert.True(NodeObjectWrapper.IsNodeType(typeof(DummyIConfigNode)));
            Assert.True(NodeObjectWrapper.IsNodeType(typeof(DummyIContextualNode)));
            Assert.True(NodeObjectWrapper.IsNodeType(typeof(ConfigNode)));
            Assert.False(NodeObjectWrapper.IsNodeType(typeof(DummyClass)));
        }

        #endregion
    }
}
