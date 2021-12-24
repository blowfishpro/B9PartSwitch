using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class PartMaxTempModifier : PartModifierBase, IPartAspectLock
    {
        public const string PART_ASPECT_LOCK = "maxTemp";

        private readonly Part part;
        private readonly double origMaxTemp;
        private readonly double newMaxTemp;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "a part's maxTemp";

        public PartMaxTempModifier(Part part, double origMaxTemp, double newMaxTemp)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origMaxTemp = origMaxTemp;
            this.newMaxTemp = newMaxTemp;
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
            part.maxTemp = newMaxTemp;
        }

        private void Deactivate()
        {
            part.maxTemp = origMaxTemp;
        }
    }
}
