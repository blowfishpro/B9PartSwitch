using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class ModuleDataLoader : PartModifierBase
    {
        protected readonly string moduleDescription;

        private readonly IModuleDataHandler[] dataHandlers;

        public ModuleDataLoader(string moduleDescription, params IModuleDataHandler[] dataHandlers)
        {
            moduleDescription.ThrowIfNullArgument(nameof(moduleDescription));
            dataHandlers.ThrowIfNullArgument(nameof(dataHandlers));

            this.moduleDescription = moduleDescription;
            this.dataHandlers = dataHandlers;
        }

        public override string Description => $"data on module {moduleDescription}";

        public override void ActivateOnStartEditor() => Activate();
        public override void ActivateOnStartFlight() => Activate();
        public override void DeactivateOnSwitchEditor() => Deactivate();
        public override void DeactivateOnSwitchFlight() => Deactivate();
        public override void ActivateOnSwitchEditor() => Activate();
        public override void ActivateOnSwitchFlight() => Activate();
        public override void OnWillBeCopiedActiveSubtype() => Deactivate();
        public override void OnWasCopiedActiveSubtype() => Activate();
        public override void OnBeforeReinitializeActiveSubtype() => Deactivate();

        private void Activate()
        {
            foreach (IModuleDataHandler handler in dataHandlers)
            {
                handler.Activate();
            }
        }

        private void Deactivate()
        {
            foreach (IModuleDataHandler handler in dataHandlers)
            {
                handler.Deactivate();
            }
        }
    }
}
