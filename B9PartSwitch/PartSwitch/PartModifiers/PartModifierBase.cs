using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public abstract class PartModifierBase : IPartModifier
    {
        public virtual string Description { get; }
        public virtual bool ChangesGeometry => false;

        public virtual void DeactivateOnStartEditor() { }
        public virtual void DeactivateOnStartFlight() { }
        public virtual void ActivateOnStartEditor() { }
        public virtual void ActivateOnStartFlight() { }
        public virtual void ActivateOnStartFinishedEditor() { }
        public virtual void ActivateOnStartFinishedFlight() { }
        public virtual void DeactivateOnStartFinishedEditor() { }
        public virtual void DeactivateOnStartFinishedFlight() { }
        public virtual void DeactivateOnSwitchEditor() { }
        public virtual void DeactivateOnSwitchFlight() { }
        public virtual void ActivateOnSwitchEditor() { }
        public virtual void ActivateOnSwitchFlight() { }
        public virtual void OnIconCreateInactiveSubtype() { }
        public virtual void OnIconCreateActiveSubtype() { }
        public virtual void UpdateVolumeEditor() { }
        public virtual void UpdateVolumeFlight() { }
        public virtual void OnWillBeCopiedActiveSubtype() { }
        public virtual void OnWillBeCopiedInactiveSubtype() { }
        public virtual void OnWasCopiedActiveSubtype() { }
        public virtual void OnWasCopiedInactiveSubtype() { }
        public virtual void OnBeforeReinitializeInactiveSubtype() { }
        public virtual void OnBeforeReinitializeActiveSubtype() { }
        public virtual void OnAfterReinitializeInactiveSubtype() { }
        public virtual void OnAfterReinitializeActiveSubtype() { }
    }
}
