using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public interface IPartModifier
    {
        string Description { get; }

        void DeactivateOnStartEditor();
        void DeactivateOnStartFlight();
        void ActivateOnStartEditor();
        void ActivateOnStartFlight();
        void ActivateOnStartFinishedEditor();
        void ActivateOnStartFinishedFlight();
        void DeactivateOnStartFinishedEditor();
        void DeactivateOnStartFinishedFlight();
        void DeactivateOnSwitchEditor();
        void DeactivateOnSwitchFlight();
        void ActivateOnSwitchEditor();
        void ActivateOnSwitchFlight();
        void OnIconCreateInactiveSubtype();
        void OnIconCreateActiveSubtype();
        void UpdateVolumeEditor();
        void UpdateVolumeFlight();
        void OnWillBeCopiedActiveSubtype();
        void OnWillBeCopiedInactiveSubtype();
        void OnWasCopiedActiveSubtype();
        void OnWasCopiedInactiveSubtype();
        void OnBeforeReinitializeInactiveSubtype();
        void OnBeforeReinitializeActiveSubtype();
        void OnAfterReinitializeInactiveSubtype();
        void OnAfterReinitializeActiveSubtype();
    }
}
