using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class PartCenterOfBuoyancyModifier : PartModifierBase
    {
        public const string PART_ASPECT_LOCK = "CenterOfBuoyancy";

        private readonly Part part;
        private readonly Vector3 origCenterOfBuoyancy;
        private readonly Vector3 newCenterOfBuoyancy;

        public override object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "a part's CenterOfBuoyancy";

        public PartCenterOfBuoyancyModifier(Part part, Vector3 origCenterOfBuoyancy, Vector3 newCenterOfBuoyancy)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCenterOfBuoyancy = origCenterOfBuoyancy;
            this.newCenterOfBuoyancy = newCenterOfBuoyancy;
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
            part.CenterOfBuoyancy = newCenterOfBuoyancy;
        }

        private void Deactivate()
        {
            part.CenterOfBuoyancy = origCenterOfBuoyancy;
        }
    }
}
