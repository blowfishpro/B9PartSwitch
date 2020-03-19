using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class TransformScaleModifier : PartModifierBase
    {
        private readonly Transform transform;
        private readonly Vector3 scale;
        private bool isActive = false;

        public TransformScaleModifier(Transform transform, Vector3 scale)
        {
            transform.ThrowIfNullArgument(nameof(transform));

            this.transform = transform;
            this.scale = scale;
        }

        public override string Description => $"transform '{transform.name}' relative scale";

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

            Vector3 localScale = transform.localScale;
            localScale.Scale(scale);
            transform.localScale = localScale;
            isActive = true;
        }

        private void Deactivate()
        {
            if (!isActive) return;

            Vector3 localScale = transform.localScale;
            localScale.x /= scale.x;
            localScale.y /= scale.y;
            localScale.z /= scale.z;
            transform.localScale = localScale;
            isActive = false;
        }
    }
}
