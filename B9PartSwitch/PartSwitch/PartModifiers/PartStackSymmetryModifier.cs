using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class PartStackSymmetryModifier : PartModifierBase
    {
        public const string PART_ASPECT_LOCK = "stackSymmetry";

        private readonly Part part;
        private readonly int origStackSymmetry;
        private readonly int newStackSymmetry;

        public override object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "a part's stackSymmetry";

        public PartStackSymmetryModifier(Part part, int origStackSymmetry, int newStackSymmetry)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origStackSymmetry = origStackSymmetry;
            this.newStackSymmetry = newStackSymmetry;
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
            part.stackSymmetry = newStackSymmetry;
        }

        private void Deactivate()
        {
            part.stackSymmetry = origStackSymmetry;
        }
    }
}
