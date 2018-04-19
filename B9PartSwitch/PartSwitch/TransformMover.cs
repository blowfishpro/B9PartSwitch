using System;
using UnityEngine;

namespace B9PartSwitch
{
    public class TransformMover
    {
        private readonly Transform transform;
        private readonly Vector3 positionOffset;
        private readonly Quaternion rotationOffset;

        public TransformMover(Transform transform, Vector3 positionOffset, Quaternion rotationOffset)
        {
            this.transform = transform ?? throw new ArgumentNullException(nameof(transform));
            this.positionOffset = positionOffset;
            this.rotationOffset = rotationOffset;
        }

        public void Activate()
        {
            transform.localPosition += positionOffset;
            transform.localRotation *= rotationOffset;
        }

        public void Deactivate()
        {
            transform.localPosition -= positionOffset;
            transform.localRotation *= Quaternion.Inverse(rotationOffset);
        }
    }
}
