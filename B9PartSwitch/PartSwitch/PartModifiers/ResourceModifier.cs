using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public delegate float GetVolumeDelegate();

    public class ResourceModifier : PartModifierBase, IPartAspectLock
    {
        private readonly TankResource tankResource;
        private readonly GetVolumeDelegate getVolumeDelegate;
        private readonly Part part;
        private readonly float filledProportion;
        private readonly bool? tweakable;

        public ResourceModifier(TankResource tankResource, GetVolumeDelegate getVolumeDelegate, Part part, float filledProportion, bool? tweakable)
        {
            tankResource.ThrowIfNullArgument(nameof(tankResource));
            getVolumeDelegate.ThrowIfNullArgument(nameof(getVolumeDelegate));
            part.ThrowIfNullArgument(nameof(part));

            this.tankResource = tankResource;
            this.getVolumeDelegate = getVolumeDelegate;
            this.part = part;
            this.filledProportion = filledProportion;
            this.tweakable = tweakable;
        }

        public override void ActivateOnStartEditor() => UpsertResource(false, false);
        public override void ActivateOnStartFlight() => UpsertResource(false, false);
        public override void DeactivateOnSwitchEditor() => RemoveResource();
        public override void DeactivateOnSwitchFlight() => RemoveResource();
        public override void ActivateOnSwitchEditor() => UpsertResource(true, false);
        public override void ActivateOnSwitchFlight() => UpsertResource(true, true);
        public override void UpdateVolumeEditor() => UpsertResource(true, false);
        public override void UpdateVolumeFlight() => UpsertResource(true, true);

        public object PartAspectLock => tankResource.ResourceName;
        public override string Description => $"resource '{tankResource.ResourceName}'";

        private void UpsertResource(bool fillTanks, bool zeroAmount)
        {
            float maxAmount = getVolumeDelegate() * tankResource.unitsPerVolume;
            float amount;
            if (zeroAmount && fillTanks)
                amount = 0f;
            else
                amount = maxAmount * filledProportion;
            PartResource partResource = part.AddOrCreateResource(tankResource.resourceDefinition, maxAmount, amount, fillTanks);

            if (tweakable.HasValue)
                partResource.isTweakable = tweakable.Value;
        }

        private void RemoveResource()
        {
            part.RemoveResource(tankResource.ResourceName);
        }
    }
}
