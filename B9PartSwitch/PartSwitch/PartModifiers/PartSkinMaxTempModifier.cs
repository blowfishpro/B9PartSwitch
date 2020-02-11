using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class PartSkinMaxTempModifier : PartModifierBase, IPartAspectLock
    {
        public const string PART_ASPECT_LOCK = "skinMaxTemp";

        private readonly Part part;
        private readonly double origSkinMaxTemp;
        private readonly double newSkinMaxTemp;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "a part's skinMaxTemp";

        public PartSkinMaxTempModifier(Part part, double origSkinMaxTemp, double newSkinMaxTemp)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origSkinMaxTemp = origSkinMaxTemp;
            this.newSkinMaxTemp = newSkinMaxTemp;
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
            part.skinMaxTemp = newSkinMaxTemp;
        }

        private void Deactivate()
        {
            part.skinMaxTemp = origSkinMaxTemp;
        }
    }
}
