using System;
using UnityEngine;
using Xunit;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitchTests.Fishbones.Parsers
{
    public class AttachNodeValueParserTest
    {
        #region ParseAttachNode

        [Fact]
        public void TestParseAttachNode()
        {
            AttachNode node = AttachNodeValueParser.ParseAttachNode("1.2, 2.3, 3.4, 4.5, 5.6, 6.7");
            AssertParsedAttachNode(node, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(4.5f, 5.6f, 6.7f));
            Assert.Equal(1, node.size);

        }

        [Fact]
        public void TestParseAttachNode__Size()
        {
            AttachNode node = AttachNodeValueParser.ParseAttachNode("1.2, 2.3, 3.4, 4.5, 5.6, 6.7, 0");
            AssertParsedAttachNode(node, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(4.5f, 5.6f, 6.7f));
            Assert.Equal(0, node.size);

            AttachNode node2 = AttachNodeValueParser.ParseAttachNode("1.2, 2.3, 3.4, 4.5, 5.6, 6.7, 2");
            AssertParsedAttachNode(node2, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(4.5f, 5.6f, 6.7f));
            Assert.Equal(2, node2.size);
        }

        [Fact]
        public void TestParseAttachNode__AttachNodeMethod()
        {
            AttachNode node = AttachNodeValueParser.ParseAttachNode("1.2, 2.3, 3.4, 4.5, 5.6, 6.7, 1, 0");
            AssertParsedAttachNode(node, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(4.5f, 5.6f, 6.7f));
            Assert.Equal(1, node.size);
            Assert.Equal(AttachNodeMethod.FIXED_JOINT, node.attachMethod);

            AttachNode node2 = AttachNodeValueParser.ParseAttachNode("1.2, 2.3, 3.4, 4.5, 5.6, 6.7, 1, 1");
            AssertParsedAttachNode(node2, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(4.5f, 5.6f, 6.7f));
            Assert.Equal(1, node2.size);
            Assert.Equal(AttachNodeMethod.HINGE_JOINT, node2.attachMethod);
        }

        [Fact]
        public void TestParseAttachNode__ResourceXFeed()
        {
            AttachNode node = AttachNodeValueParser.ParseAttachNode("1.2, 2.3, 3.4, 4.5, 5.6, 6.7, 1, 0, 0");
            AssertParsedAttachNode(node, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(4.5f, 5.6f, 6.7f));
            Assert.Equal(1, node.size);
            Assert.Equal(AttachNodeMethod.FIXED_JOINT, node.attachMethod);
            Assert.False(node.ResourceXFeed);

            AttachNode node2 = AttachNodeValueParser.ParseAttachNode("1.2, 2.3, 3.4, 4.5, 5.6, 6.7, 1, 0, 1");
            AssertParsedAttachNode(node2, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(4.5f, 5.6f, 6.7f));
            Assert.Equal(1, node2.size);
            Assert.Equal(AttachNodeMethod.FIXED_JOINT, node2.attachMethod);
            Assert.True(node2.ResourceXFeed);
        }

        [Fact]
        public void TestParseAttachNode__Rigid()
        {
            AttachNode node = AttachNodeValueParser.ParseAttachNode("1.2, 2.3, 3.4, 4.5, 5.6, 6.7, 1, 0, 0, 0");
            AssertParsedAttachNode(node, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(4.5f, 5.6f, 6.7f));
            Assert.Equal(1, node.size);
            Assert.Equal(AttachNodeMethod.FIXED_JOINT, node.attachMethod);
            Assert.False(node.ResourceXFeed);
            Assert.False(node.rigid);

            AttachNode node2 = AttachNodeValueParser.ParseAttachNode("1.2, 2.3, 3.4, 4.5, 5.6, 6.7, 1, 0, 0, 1");
            AssertParsedAttachNode(node2, new Vector3(1.2f, 2.3f, 3.4f), new Vector3(4.5f, 5.6f, 6.7f));
            Assert.Equal(1, node2.size);
            Assert.Equal(AttachNodeMethod.FIXED_JOINT, node2.attachMethod);
            Assert.False(node2.ResourceXFeed);
            Assert.True(node2.rigid);
        }

        [Fact]
        public void TestParseAttachNode__TooFewValues()
        {
            Assert.Throws<FormatException>(() => AttachNodeValueParser.ParseAttachNode("1.2, 2.3, 3.4, 4.5, 5.6"));
        }

        [Fact]
        public void TestParseAttachNode__Null()
        {
            Assert.Throws<ArgumentNullException>(() => AttachNodeValueParser.ParseAttachNode(null));
        }

        #endregion

        #region FormatAttachNode

        [Fact]
        public void TestFormatAttachNode()
        {
            Vector3 position = new Vector3(1.2f, 2.3f, 3.4f);
            Vector3 orientation = new Vector3(4.5f, 5.6f, 6.7f);
            AttachNode node = new AttachNode
            {
                position = position,
                orientation = orientation,
                size = 4,
                attachMethod = AttachNodeMethod.NONE,
                ResourceXFeed = true,
                rigid = false,
            };

            string expectedStr = "1.2, 2.3, 3.4, 4.5, 5.6, 6.7, 4, 5, 1, 0";
            Assert.Equal(expectedStr, AttachNodeValueParser.FormatAttachNode(node));
        }

        [Fact]
        public void TestFormatAttachNode__Null()
        {
            Assert.Throws<ArgumentNullException>(() => AttachNodeValueParser.FormatAttachNode(null));
        }

        #endregion

        private void AssertParsedAttachNode(AttachNode node, Vector3 position, Vector3 orientation)
        {
            Assert.NotNull(node);
            Assert.Equal(position, node.position);
            Assert.Equal(position, node.originalPosition);
            Assert.Equal(orientation, node.orientation);
            Assert.Equal(orientation, node.originalOrientation);
            Assert.Equal("parsed-attach-node", node.id);
        }
    }
}
