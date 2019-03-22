using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    class PartCoMOffsetModifier : PartModifierBase
    {
        public const string PART_ASPECT_LOCK = "CoMOffset";

        private readonly Part part;
        private readonly Vector3 origCoMOffset;
        private readonly Vector3 newCoMOffset;

        public override object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "a part's CoMOffset";

        public PartCoMOffsetModifier(Part part, Vector3 origCoMOffset, Vector3 newCoMOffset)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCoMOffset = origCoMOffset;
            this.newCoMOffset = newCoMOffset;
        }

        public override void ActivateOnStartEditor() => Activate();
        public override void ActivateOnStartFlight() => Activate();
        public override void DeactivateOnStartEditor() => Deactivate();
        public override void DeactivateOnStartFlight() => Deactivate();
        public override void ActivateOnSwitchEditor() => Activate();
        public override void ActivateOnSwitchFlight() => Activate();
        public override void DeactivateOnSwitchEditor() => Deactivate();
        public override void DeactivateOnSwitchFlight() => Deactivate();

        private void Activate()
        {
            part.CoMOffset = newCoMOffset;
        }

        private void Deactivate()
        {
            part.CoMOffset = origCoMOffset;
        }
    }
}
