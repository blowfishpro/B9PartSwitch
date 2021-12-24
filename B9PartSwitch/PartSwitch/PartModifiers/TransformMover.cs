using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class TransformMover : PartModifierBase
    {
        private readonly Transform transform;
        private readonly Vector3 offset;
        private bool isActive = false;

        public TransformMover(Transform transform, Vector3 offset)
        {
            transform.ThrowIfNullArgument(nameof(transform));

            this.transform = transform;
            this.offset = offset;
        }

        public override string Description => $"transform '{transform.name}' position offset";
        public override bool ChangesGeometry => true;

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

            transform.localPosition += offset;
            isActive = true;
        }

        private void Deactivate()
        {
            if (!isActive) return;

            transform.localPosition -= offset;
            isActive = false;
        }
    }
}
