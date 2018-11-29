using System;
using UnityEngine;

namespace B9PartSwitch
{
    public class AttachNodeMover
    {
        public readonly AttachNode attachNode;
        private readonly Vector3 position;
        private readonly ILinearScaleProvider linearScaleProvider;

        public AttachNodeMover(AttachNode attachNode, Vector3 position, ILinearScaleProvider linearScaleProvider)
        {
            attachNode.ThrowIfNullArgument(nameof(attachNode));
            linearScaleProvider.ThrowIfNullArgument(nameof(linearScaleProvider));

            this.attachNode = attachNode;
            this.position = position;
            this.linearScaleProvider = linearScaleProvider;
        }

        public void ActivateOnStart()
        {
            SetAttachNodePosition();
        }

        public void ActivateAfterStart()
        {
            // TweakScale resets node positions, therefore we need to wait a frame and fix them
            SetAttachNodePosition();
        }

        public void ActivateOnSwitch()
        {
            Vector3 offset = (position * linearScaleProvider.LinearScale) - attachNode.position;
            SetAttachNodePosition();

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
            Vector3 offset = attachNode.position - (position * linearScaleProvider.LinearScale);
            attachNode.position = attachNode.originalPosition * linearScaleProvider.LinearScale;

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

        private void SetAttachNodePosition()
        {
            attachNode.position = position * linearScaleProvider.LinearScale;
        }
    }
}
