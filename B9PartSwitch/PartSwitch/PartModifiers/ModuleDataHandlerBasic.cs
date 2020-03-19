using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class ModuleDataHandlerBasic : PartModifierBase
    {
        protected readonly PartModule module;
        protected readonly ConfigNode originalNode;
        protected readonly ConfigNode dataNode;

        public ModuleDataHandlerBasic(PartModule module, ConfigNode originalNode, ConfigNode dataNode)
        {
            module.ThrowIfNullArgument(nameof(module));
            originalNode.ThrowIfNullArgument(nameof(originalNode));
            dataNode.ThrowIfNullArgument(nameof(dataNode));

            this.module = module;
            this.originalNode = originalNode;
            this.dataNode = dataNode;
        }

        public override string Description => $"data on module {module}";

        public override void ActivateOnStartEditor() => Activate();
        public override void ActivateOnStartFlight() => Activate();
        public override void DeactivateOnSwitchEditor() => Deactivate();
        public override void DeactivateOnSwitchFlight() => Deactivate();
        public override void ActivateOnSwitchEditor() => Activate();
        public override void ActivateOnSwitchFlight() => Activate();
        public override void OnWillBeCopiedActiveSubtype() => Deactivate();
        public override void OnWasCopiedActiveSubtype() => Activate();

        private void Activate()
        {
            module.Load(dataNode);
            module.Events.Send("ModuleDataChanged");
        }

        private void Deactivate()
        {
            module.Load(originalNode);
            module.Events.Send("ModuleDataChanged");
        }
    }
}
