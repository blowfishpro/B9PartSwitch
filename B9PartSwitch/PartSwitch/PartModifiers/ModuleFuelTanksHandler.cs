
namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class ModuleFuelTanksHandler : PartModifierBase, IPartAspectLock
    {
        public const string PART_ASPECT_LOCK = "ModuleFuelTanks";

        private readonly PartModule module;
        private readonly ConfigNode originalNode;
        private readonly ConfigNode dataNode;
        private readonly BaseEventDetails moduleDataChangedEventDetails;
        public ModuleFuelTanksHandler(PartModule module, ConfigNode originalNode, ConfigNode dataNode, BaseEventDetails moduleDataChangedEventDetails)
        {
            this.module = module;
            this.originalNode = originalNode;
            this.dataNode = dataNode;
            this.moduleDataChangedEventDetails = moduleDataChangedEventDetails;
        }

        public object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "blah, FIXME";
        public override void DeactivateOnStartEditor() => Deactivate();
        public override void ActivateOnStartEditor() => Activate();
        public override void DeactivateOnSwitchEditor() => Deactivate();
        public override void ActivateOnSwitchEditor() => Activate();
        public override void OnWillBeCopiedInactiveSubtype() => Activate();
        public override void OnWasCopiedInactiveSubtype() => Deactivate();
        public override void OnWasCopiedActiveSubtype() => Activate();
        public override void OnBeforeReinitializeInactiveSubtype() => Activate();

        private void Activate() => ApplyNode(dataNode);
        private void Deactivate() => ApplyNode(originalNode);

        private void ApplyNode(ConfigNode sourceNode) {
            double volume = 0;
            bool setsVolume = sourceNode.TryGetValue("volume", ref volume);

            if (setsVolume) {
                var evtDetails = new BaseEventDetails(BaseEventDetails.Sender.USER);
                evtDetails.Set<string>("volName", "Tankage");
                evtDetails.Set<double>("newTotalVolume", volume);
                module.part.SendEvent("OnPartVolumeChanged", evtDetails, 0);
            }
            module.Events.Send("ModuleDataChanged", moduleDataChangedEventDetails);
        }
    }
}