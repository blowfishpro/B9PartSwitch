using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace B9PartSwitch.ModularMassModifier
{
    public interface IPartMassModifier2
    {
        float GetModuleMass(float baseMass);
    }

    public class PartMassModifierModule : PartModule, IPartMassModifier
    {
        public float BaseMass { get; private set; }
        public float ModuleMass { get; private set; }
        public float Mass { get { return BaseMass + ModuleMass; } }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            BaseMass = part.GetPrefab().mass;

            if (state == PartModule.StartState.Editor)
                GameEvents.onEditorPartEvent.Add(OnEditorPartUpdate);
            else
                GameEvents.onVesselWasModified.Add(OnVesselUpdate);
        }

        private void OnDestroy()
        {
            GameEvents.onEditorPartEvent.Remove(OnEditorPartUpdate);
            GameEvents.onVesselWasModified.Remove(OnVesselUpdate);
        }

        [KSPEvent]
        public void UpdateMass()
        {
            ModuleMass = 0f;
            foreach (PartModule m in part.Modules)
            {
                if (m is IPartMassModifier2)
                    ModuleMass += (m as IPartMassModifier2).GetModuleMass(BaseMass);
            }

            part.mass = Mass;
        }

        private void OnEditorPartUpdate(ConstructionEventType eventType, Part part)
        {
            if (object.ReferenceEquals(part, this.part))
                UpdateMass();
        }

        private void OnVesselUpdate(Vessel vessel)
        {
            if (object.ReferenceEquals(vessel, this.part.vessel))
                UpdateMass();
        }

        public float GetModuleMass(float baseMass)
        {
            if (baseMass == BaseMass)
                return ModuleMass;
            else if (baseMass == part.mass)
                return 0f;
            else
            {
                Debug.LogWarning("Warning: unrecognized mass detected");
                return 0f;
            }
        }
    }
}
