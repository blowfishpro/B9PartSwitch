using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class PartCrashToleranceModifier : PartModifierBase
    {
        public const string PART_ASPECT_LOCK = "crashTolerance";

        private readonly Part part;
        private readonly float origCrashTolerance;
        private readonly float newCrashTolerance;

        public override object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "a part's crashTolerance";

        public PartCrashToleranceModifier(Part part, float origCrashTolerance, float newCrashTolerance)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCrashTolerance = origCrashTolerance;
            this.newCrashTolerance = newCrashTolerance;
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
            part.crashTolerance = newCrashTolerance;
        }

        private void Deactivate()
        {
            part.crashTolerance = origCrashTolerance;
        }
    }
}
