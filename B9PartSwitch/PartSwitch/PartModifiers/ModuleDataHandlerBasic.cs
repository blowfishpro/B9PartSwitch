using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class ModuleDataHandlerBasic : IModuleDataHandler
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

        public void Activate() => module.Load(dataNode);
        public void Deactivate() => module.Load(originalNode);
    }
}
