using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class PartCoLOffsetModifier : PartModifierBase
    {
        public const string PART_ASPECT_LOCK = "CoLOffset";

        private readonly Part part;
        private readonly Vector3 origCoLOffset;
        private readonly Vector3 newCoLOffset;

        public override object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "a part's CoLOffset";

        public PartCoLOffsetModifier(Part part, Vector3 origCoLOffset, Vector3 newCoLOffset)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCoLOffset = origCoLOffset;
            this.newCoLOffset = newCoLOffset;
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
            part.CoLOffset = newCoLOffset;
        }

        private void Deactivate()
        {
            part.CoLOffset = origCoLOffset;
        }
    }
}
