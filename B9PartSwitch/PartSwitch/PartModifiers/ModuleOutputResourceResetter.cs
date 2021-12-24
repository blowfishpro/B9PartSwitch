using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class ModuleOutputResourceResetter : PartModifierBase
    {
        private readonly PartModule module;

        public ModuleOutputResourceResetter(PartModule module)
        {
            module.ThrowIfNullArgument(nameof(module));

            this.module = module;
        }

        public override string Description => $"output resources on {module}";

        public override void ActivateOnStartEditor() => Fire();
        public override void ActivateOnStartFlight() => Fire();
        public override void DeactivateOnSwitchEditor() => Fire();
        public override void DeactivateOnSwitchFlight() => Fire();
        public override void ActivateOnSwitchEditor() => Fire();
        public override void ActivateOnSwitchFlight() => Fire();
        public override void OnWillBeCopiedActiveSubtype() => Fire();
        public override void OnWasCopiedActiveSubtype() => Fire();

        private void Fire() => module.resHandler.outputResources.Clear();
    }
}
