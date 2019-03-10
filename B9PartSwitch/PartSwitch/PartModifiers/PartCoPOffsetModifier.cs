using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class PartCoPOffsetModifier : PartModifierBase
    {
        public const string PART_ASPECT_LOCK = "CoPOffset";

        private readonly Part part;
        private readonly Vector3 origCoPOffset;
        private readonly Vector3 newCoPOffset;

        public override object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "a part's CoPOffset";

        public PartCoPOffsetModifier(Part part, Vector3 origCoPOffset, Vector3 newCoPOffset)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCoPOffset = origCoPOffset;
            this.newCoPOffset = newCoPOffset;
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
            part.CoPOffset = newCoPOffset;
        }

        private void Deactivate()
        {
            part.CoPOffset = origCoPOffset;
        }
    }
}
