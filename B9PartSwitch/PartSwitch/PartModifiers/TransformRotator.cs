using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class TransformRotator : PartModifierBase, IPartAspectLock
    {
        private readonly Transform transform;
        private readonly Quaternion rotationOffset;
        private bool isActive = false;

        public TransformRotator(Transform transform, Quaternion rotationOffset)
        {
            transform.ThrowIfNullArgument(nameof(transform));

            this.transform = transform;
            this.rotationOffset = rotationOffset;
        }

        public override string Description => $"transform '{transform.name}' rotation offset";
        public object PartAspectLock => transform.GetInstanceID() + "---rotation";

        public override void ActivateOnStartEditor() => Activate();
        public override void ActivateOnStartFlight() => Activate();
        public override void ActivateOnSwitchEditor() => Activate();
        public override void ActivateOnSwitchFlight() => Activate();
        public override void DeactivateOnSwitchEditor() => Deactivate();
        public override void DeactivateOnSwitchFlight() => Deactivate();
        public override void OnIconCreateActiveSubtype() => Activate();
        public override void OnWillBeCopiedActiveSubtype() => Deactivate();
        public override void OnWasCopiedActiveSubtype() => Activate();
        public override void OnBeforeReinitializeActiveSubtype() => Deactivate();
        public override void OnAfterReinitializeActiveSubtype() => Activate();

        private void Activate()
        {
            if (isActive) return;

            transform.localRotation *= rotationOffset;
            isActive = true;
        }

        private void Deactivate()
        {
            if (!isActive) return;

            transform.localRotation *= Quaternion.Inverse(rotationOffset);
            isActive = false;
        }
    }
}
