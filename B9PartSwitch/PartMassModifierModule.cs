using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;

namespace B9PartSwitch
{
    public interface IPartMassModifier2
    {
        float GetModuleMass(float baseMass);
    }

    public class PartMassModifierModule : PartModule
    {
        public float baseMass { get; private set; }
        public float mass { get; private set; }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            baseMass = part.partInfo.partPrefab.mass;
            mass = baseMass;

            UpdateMass();
        }

        public void UpdateMass()
        {
            mass = baseMass;
            foreach (PartModule m in part.Modules)
            {
                if (m is IPartMassModifier2)
                    mass += (m as IPartMassModifier2).GetModuleMass(baseMass);
            }

            part.mass = mass;
        }
    }
}
