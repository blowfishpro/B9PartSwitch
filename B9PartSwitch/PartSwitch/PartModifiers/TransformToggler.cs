using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class TransformToggler : PartModifierBase
    {
        private readonly Transform transform;
        private readonly Part part;

        public TransformToggler(Transform transform, Part part)
        {
            transform.ThrowIfNullArgument(nameof(transform));
            part.ThrowIfNullArgument(nameof(part));

            this.transform = transform;
            this.part = part;
        }

        public override bool ChangesGeometry => true;

        public override void DeactivateOnStartEditor() => Deactivate();
        public override void DeactivateOnStartFlight() => Deactivate();
        public override void ActivateOnStartEditor() => Activate();
        public override void ActivateOnStartFlight() => Activate();
        public override void DeactivateOnSwitchEditor() => Deactivate();
        public override void DeactivateOnSwitchFlight() => Deactivate();
        public override void ActivateOnSwitchEditor() => Activate();
        public override void ActivateOnSwitchFlight() => Activate();
        public override void OnIconCreateInactiveSubtype() => Deactivate();
        public override void OnIconCreateActiveSubtype() => Activate();

        private void Activate()
        {
            part.UpdateTransformEnabled(transform);
        }

        private void Deactivate()
        {
            transform.Disable();

            if (part.partRendererBoundsIgnore.Contains(transform.name)) part.partRendererBoundsIgnore.Add(transform.name);
        }
    }
}
