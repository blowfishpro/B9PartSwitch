using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class ModuleDeactivator : PartModifierBase
    {
        public readonly PartModule module;
        protected readonly PartModule parent;

        public ModuleDeactivator(PartModule module, PartModule parent)
        {
            module.ThrowIfNullArgument(nameof(module));
            parent.ThrowIfNullArgument(nameof(parent));

            this.module = module;
            this.parent = parent;
        }

        public override string Description => $"module {module.ToString()} activated status";

        public override void ActivateOnStartEditor() => Activate();
        public override void ActivateOnStartFlight() => Activate();
        public override void DeactivateOnSwitchEditor() => MaybeDeactivate();
        public override void DeactivateOnSwitchFlight() => MaybeDeactivate();
        public override void ActivateOnSwitchEditor() => Activate();
        public override void ActivateOnSwitchFlight() => Activate();
        public override void OnWillBeCopiedActiveSubtype() => Deactivate();
        public override void OnWasCopiedActiveSubtype() => Activate();
        public override void OnBeforeReinitializeActiveSubtype() => Deactivate();

        protected virtual void Activate()
        {
            module.enabled = false;
            module.isEnabled = false;
        }

        protected virtual void MaybeDeactivate()
        {
            foreach (PartModule otherModule in module.part.Modules)
            {
                if (otherModule == parent) continue;
                if (!(otherModule is ModuleB9PartSwitch switchModule)) continue;
                if (switchModule.ModuleShouldBeEnabled(module)) return;
            }

            Deactivate();
        }

        protected virtual void Deactivate()
        {
            module.enabled = true;
            module.isEnabled = true;
        }
    }
}
