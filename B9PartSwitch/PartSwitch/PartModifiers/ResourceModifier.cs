using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public interface IVolumeProvider
    {
        float Volume { get; }
    }

    public class ZeroVolumeProvider : IVolumeProvider
    {
        public float Volume => 0f;
    }

    public class SubtypeVolumeProvider : IVolumeProvider
    {
        private readonly ModuleB9PartSwitch parent;
        private readonly float volumeMultiplier;
        public readonly float volumeAdded;

        public SubtypeVolumeProvider(ModuleB9PartSwitch parent, float volumeMultiplier, float volumeAdded)
        {
            parent.ThrowIfNullArgument(nameof(parent));
            this.parent = parent;
            this.volumeMultiplier = volumeMultiplier;
            this.volumeAdded = volumeAdded;
        }

        public float Volume => (parent.baseVolume * volumeMultiplier + volumeAdded + parent.VolumeFromChildren) * parent.VolumeScale;
    }

    public class ResourceModifier : PartModifierBase
    {
        private readonly TankResource tankResource;
        private readonly IVolumeProvider volumeProvider;
        private readonly Part part;
        private readonly float filledProportion;
        private readonly bool? tweakable;

        public ResourceModifier(TankResource tankResource, IVolumeProvider volumeProvider, Part part, float filledProportion, bool? tweakable)
        {
            tankResource.ThrowIfNullArgument(nameof(tankResource));
            volumeProvider.ThrowIfNullArgument(nameof(volumeProvider));
            part.ThrowIfNullArgument(nameof(part));

            this.tankResource = tankResource;
            this.volumeProvider = volumeProvider;
            this.part = part;
            this.filledProportion = filledProportion;
            this.tweakable = tweakable;
        }

        public override void ActivateOnStartEditor() => UpsertResource(false, false);
        public override void ActivateOnStartFlight() => UpsertResource(false, false);
        public override void DeactivateOnSwitchEditor() => RemoveResource();
        public override void ActivateOnSwitchEditor() => UpsertResource(true, false);
        public override void ActivateOnSwitchFlight() => UpsertResource(true, true);
        public override void UpdateVolumeEditor() => UpsertResource(true, false);
        public override void UpdateVolumeFlight() => UpsertResource(true, true);

        public override object PartAspectLock => tankResource.ResourceName;
        public override string Description => $"resource '{tankResource.ResourceName}'";

        private void UpsertResource(bool fillTanks, bool zeroAmount)
        {
            float maxAmount = volumeProvider.Volume * tankResource.unitsPerVolume;
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
