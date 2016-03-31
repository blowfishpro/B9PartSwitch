using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B9PartSwitch
{
    public static class AttachNodeExtensions
    {
        public static void Unhide(this AttachNode node)
        {
            node.nodeType = AttachNode.NodeType.Stack;
            node.radius = 0.4f;
        }

        public static void Hide(this AttachNode node)
        {
            node.nodeType = AttachNode.NodeType.Dock;
            node.radius = 0.001f;
        }
    }
}
