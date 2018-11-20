using System;
using UnityEngine;

namespace B9PartSwitch
{
    public class AttachNodeMover
    {
        public readonly AttachNode attachNode;
        private readonly Vector3 position;

        public AttachNodeMover(AttachNode attachNode, Vector3 position)
        {
            attachNode.ThrowIfNullArgument(nameof(attachNode));

            this.attachNode = attachNode;
            this.position = position;
        }

        public void ActivateOnStart()
        {
            attachNode.position = position;
        }

        public void ActivateOnSwitch()
        {
            Vector3 offset = position - attachNode.position;
            attachNode.position = position;

            if (!HighLogic.LoadedSceneIsEditor) return;
            if (attachNode.owner.parent != null && attachNode.owner.parent == attachNode.attachedPart)
            {
                attachNode.owner.transform.localPosition -= offset;
            }
            else if (attachNode.attachedPart != null)
            {
                attachNode.attachedPart.transform.localPosition += offset;
            }
        }

        public void DeactivateOnSwitch()
        {
            Vector3 offset = attachNode.originalPosition - attachNode.position;
            attachNode.position = attachNode.originalPosition;

            if (!HighLogic.LoadedSceneIsEditor) return;
            if (attachNode.owner.parent != null && attachNode.owner.parent == attachNode.attachedPart)
            {
                attachNode.owner.transform.localPosition -= offset;
            }
            else if (attachNode.attachedPart != null)
            {
                attachNode.attachedPart.transform.localPosition += offset;
            }
        }
    }
}
