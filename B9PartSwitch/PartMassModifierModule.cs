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

    public class PartMassModifierModule : PartModule, IPartMassModifier
    {
        public float BaseMass { get; private set; }
        public float ModuleMass { get; private set; }
        public float Mass { get { return BaseMass + ModuleMass; } }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            BaseMass = part.partInfo.partPrefab.mass;

            UpdateMass();
        }

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

        public float GetModuleMass(float baseMass)
        {
            if (baseMass == part.mass)
                return 0f;
            else
                return ModuleMass;
        }
    }
}
