using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class PartCenterOfDisplacementModifier : PartModifierBase
    {
        public const string PART_ASPECT_LOCK = "CenterOfDisplacement";

        private readonly Part part;
        private readonly Vector3 origCenterOfDisplacement;
        private readonly Vector3 newCenterOfDisplacement;

        public override object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "a part's CenterOfDisplacement";

        public PartCenterOfDisplacementModifier(Part part, Vector3 origCenterOfDisplacement, Vector3 newCenterOfDisplacement)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCenterOfDisplacement = origCenterOfDisplacement;
            this.newCenterOfDisplacement = newCenterOfDisplacement;
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
            part.CenterOfDisplacement = newCenterOfDisplacement;
        }

        private void Deactivate()
        {
            part.CenterOfDisplacement = origCenterOfDisplacement;
        }
    }
}
