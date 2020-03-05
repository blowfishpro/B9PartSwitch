using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class ModuleDataHandlerBasic : PartModifierBase
    {
        protected readonly PartModule module;
        protected readonly ConfigNode originalNode;
        protected readonly ConfigNode dataNode;

        private readonly BaseEventDetails moduleDataChangedEventDetails;

        public ModuleDataHandlerBasic(PartModule module, ConfigNode originalNode, ConfigNode dataNode, BaseEventDetails moduleDataChangedEventDetails)
        {
            module.ThrowIfNullArgument(nameof(module));
            originalNode.ThrowIfNullArgument(nameof(originalNode));
            dataNode.ThrowIfNullArgument(nameof(dataNode));
            moduleDataChangedEventDetails.ThrowIfNullArgument(nameof(moduleDataChangedEventDetails));

            this.module = module;
            this.originalNode = originalNode;
            this.dataNode = dataNode;
            this.moduleDataChangedEventDetails = moduleDataChangedEventDetails;
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

        protected virtual void Activate()
        {
            module.Load(dataNode);
            module.Events.Send("ModuleDataChanged", moduleDataChangedEventDetails);
        }

        protected virtual void Deactivate()
        {
            module.Load(originalNode);
            module.Events.Send("ModuleDataChanged", moduleDataChangedEventDetails);
        }
    }

    public class ModuleFuelTanksHandler : ModuleDataHandlerBasic
    {
        public ModuleFuelTanksHandler(
            PartModule module, ConfigNode originalNode, ConfigNode dataNode
        ) : base(module, originalNode, dataNode)
        { }

        protected override void Activate() => applyNode(dataNode);
        protected override void Deactivate() => applyNode(originalNode);

        private void applyNode(ConfigNode sourceNode) {
            double volume = 0;
            bool setsVolume = sourceNode.TryGetValue("volume", ref volume);
            string type = null;
            bool setsType = sourceNode.TryGetValue("type", ref type);

            if (setsVolume) {
                // Update the tank volume by sending a volume-change event in the format that Procedural Parts uses.
                // Procedural Parts reports the outside volume of the tank in cubic meters (or equivalently,
                // kiloliters). ModuleFuelTanks scales the reported volume by the `tankVolumeConversion` and
                // `utilization` fields to compute the available internal volume in liters.
                // Since the `volume` configuration field is meant to set the available volume in liters directly,
                // we need to read the scaling values and apply the inverse scaling to the value that we send.
                float scaleFactor = module.Fields.GetValue<float>("tankVolumeConversion");
                float utilization = module.Fields.GetValue<float>("utilization");
                var evtDetails = new BaseEventDetails(BaseEventDetails.Sender.USER);
                evtDetails.Set<string>("volName", "Tankage");
                evtDetails.Set<double>("newTotalVolume", volume * 100 / utilization / scaleFactor);
                module.part.SendEvent("OnPartVolumeChanged", evtDetails, 0);
            }
            if (setsType) {
                module.Fields.SetValue("type", type);
            }
        }
    }
}

