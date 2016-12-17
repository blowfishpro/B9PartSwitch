using System;
using Xunit;

namespace B9PartSwitchTests.TestUtils
{
    public class AssertUtilTest
    {
        #region ConfigNodesEqual

        [Fact]
        public void TestAssertConfigNodesEqual__True()
        {
            ConfigNode node1 = new TestConfigNode("NODE0")
            {
                { "value1", "something1" },
                { "value2", "something2" },
                { "value2", "something3" },
                new TestConfigNode("NODE1")
                {
                    { "value3", "something4" },
                    { "value4", "something5" },
                    { "value4", "something6" },
                    new TestConfigNode("NODE2")
                    {
                        { "value5", "something7" },
                    }
                },
                new TestConfigNode("NODE3")
                {
                    { "value6", "something8" },
                    { "value7", "something9" },
                    { "value7", "something10" },
                    new TestConfigNode("NODE4")
                    {
                        { "value8", "something11" },
                    }
                },
                new TestConfigNode("NODE3")
                {
                    { "value6", "something12" },
                    { "value7", "something13" },
                    { "value7", "something14" },
                    new TestConfigNode("NODE4")
                    {
                        { "value8", "something15" },
                    }
                },
            };

            ConfigNode node2 = node1.CreateCopy();

            Assert.DoesNotThrow(() => AssertUtil.ConfigNodesEqual(node1, node2));
        }

        [Fact]
        public void TestAssertConfigNodesEqual__Null()
        {
            ConfigNode node = new ConfigNode();

            Assert.DoesNotThrow(() => AssertUtil.ConfigNodesEqual(null, null));
            Assert.Throws<Xunit.Sdk.SameException>(() => AssertUtil.ConfigNodesEqual(node, null));
            Assert.Throws<Xunit.Sdk.SameException>(() => AssertUtil.ConfigNodesEqual(null, node));
        }

        [Fact]
        public void TestAssertConfigNodesEqual__NamesDifferent()
        {
            ConfigNode node1 = new ConfigNode("NODE1");
            ConfigNode node2 = new ConfigNode("NODE2");

            Assert.Throws<Xunit.Sdk.EqualException>(() => AssertUtil.ConfigNodesEqual(node1, node2));
        }

        [Fact]
        public void TestAssertConfigNodesEqual__MissingValue()
        {
            ConfigNode node1 = new TestConfigNode
            {
                { "value1", "blah1" },
                { "value1", "blah2" },
            };

            ConfigNode node2 = new TestConfigNode
            {
                { "value1", "blah1" },
            };

            Assert.Throws<Xunit.Sdk.EqualException>(() => AssertUtil.ConfigNodesEqual(node1, node2));
        }

        [Fact]
        public void TestAssertConfigNodesEqual__Order()
        {
            ConfigNode node1 = new TestConfigNode
            {
                { "value1", "blah1" },
                { "value2", "blah2" },
            };

            ConfigNode node2 = new TestConfigNode
            {
                { "value2", "blah2" },
                { "value1", "blah1" },
            };

            Assert.Throws<Xunit.Sdk.EqualException>(() => AssertUtil.ConfigNodesEqual(node1, node2));
        }

        #endregion
    }
}
