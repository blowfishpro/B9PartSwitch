using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class PartAttachNodeModifier : PartModifierBase, IPartAspectLock
    {
        public const string PART_ASPECT_LOCK = "attachNode";

        private readonly AttachNode partAttachNode;
        private readonly AttachNode referenceAttachNode;
        private readonly AttachNode newAttachNode;
        private readonly ILinearScaleProvider linearScaleProvider;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public override string Description => "a part's surface attach node";

        public PartAttachNodeModifier(AttachNode partAttachNode, AttachNode referenceAttachNode, AttachNode newAttachNode, ILinearScaleProvider linearScaleProvider)
        {
            partAttachNode.ThrowIfNullArgument(nameof(partAttachNode));
            linearScaleProvider.ThrowIfNullArgument(nameof(linearScaleProvider));

            this.partAttachNode = partAttachNode;
            this.referenceAttachNode = referenceAttachNode;
            this.newAttachNode = newAttachNode;
            this.linearScaleProvider = linearScaleProvider;
        }

        public override void ActivateOnStartEditor() => Activate();
        public override void ActivateOnStartFlight() => Activate();
        public override void DeactivateOnStartEditor() => Deactivate();
        public override void DeactivateOnStartFlight() => Deactivate();
        public override void ActivateOnSwitchEditor() => Activate();
        public override void ActivateOnSwitchFlight() => Activate();
        public override void DeactivateOnSwitchEditor() => Deactivate();
        public override void DeactivateOnSwitchFlight() => Deactivate();

        private void Activate()
        {
            partAttachNode.position = newAttachNode.position * linearScaleProvider.LinearScale;
            partAttachNode.orientation = newAttachNode.orientation;
        }

        private void Deactivate()
        {
            partAttachNode.position = referenceAttachNode.position * linearScaleProvider.LinearScale;
            partAttachNode.orientation = referenceAttachNode.orientation;
        }
    }
}
