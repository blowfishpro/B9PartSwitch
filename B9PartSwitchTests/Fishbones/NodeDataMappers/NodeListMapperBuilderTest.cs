using System;
using System.Collections.Generic;
using Xunit;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitchTests.TestUtils.DummyTypes;

namespace B9PartSwitchTests.Fishbones.NodeDataMappers
{
    public class NodeListMapperBuilderTest
    {
        #region Constructor

        [Fact]
        public void TestNew()
        {
            NodeListMapperBuilder builder = new NodeListMapperBuilder("foo", typeof(List<DummyIConfigNode>));

            Assert.Equal("foo", builder.nodeDataName);
            Assert.Same(typeof(DummyIConfigNode), builder.elementType);
        }

        [Fact]
        public void TestNew__NullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new NodeListMapperBuilder(null, typeof(List<DummyIConfigNode>)));
            Assert.Throws<ArgumentNullException>(() => new NodeListMapperBuilder("foo", null));
        }

        #endregion

        #region CanBuild

        [Fact]
        public void TestCanBuild__IConfigNode()
        {
            NodeListMapperBuilder builder = new NodeListMapperBuilder("foo", typeof(List<DummyIConfigNode>));

            Assert.True(builder.CanBuild);
        }

        [Fact]
        public void TestCanBuild__IContextualNode()
        {
            NodeListMapperBuilder builder = new NodeListMapperBuilder("foo", typeof(List<DummyIContextualNode>));

            Assert.True(builder.CanBuild);
        }

        [Fact]
        public void TestCanBuild__False()
        {
            NodeListMapperBuilder builder = new NodeListMapperBuilder("foo", typeof(List<DummyClass>));

            Assert.False(builder.CanBuild);
        }

        [Fact]
        public void TestCanBuild__NotList()
        {
            NodeListMapperBuilder builder = new NodeListMapperBuilder("foo", typeof(DummyIConfigNode));

            Assert.False(builder.CanBuild);
        }

        #endregion

        #region Build

        [Fact]
        public void TestBuildMapper__IConfigNode()
        {
            NodeListMapperBuilder builder = new NodeListMapperBuilder("foo", typeof(List<DummyIConfigNode>));

            NodeListMapper mapper = Assert.IsType<NodeListMapper>(builder.BuildMapper());

            Assert.Equal("foo", mapper.name);
            Assert.Same(typeof(DummyIConfigNode), mapper.elementType);
            Assert.Same(typeof(List<DummyIConfigNode>), mapper.listType);
        }

        [Fact]
        public void TestBuildMapper__IContextualNode()
        {
            NodeListMapperBuilder builder = new NodeListMapperBuilder("foo", typeof(List<DummyIContextualNode>));

            NodeListMapper mapper = Assert.IsType<NodeListMapper>(builder.BuildMapper());

            Assert.Equal("foo", mapper.name);
            Assert.Same(typeof(DummyIContextualNode), mapper.elementType);
            Assert.Same(typeof(List<DummyIContextualNode>), mapper.listType);
        }

        [Fact]
        public void TestBuildMapper__CantBuild()
        {
            NodeListMapperBuilder builder = new NodeListMapperBuilder("foo", typeof(List<DummyClass>));

            Assert.Throws<InvalidOperationException>(() => builder.BuildMapper());
        }

        #endregion
    }
}
