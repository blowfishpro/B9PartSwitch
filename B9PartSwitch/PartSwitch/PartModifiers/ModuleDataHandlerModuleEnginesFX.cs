using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class ModuleDataHandlerrModuleEnginesFX : IModuleDataHandler
    {
        private readonly ModuleEnginesFX module;

        private readonly string flameoutEffectName = null;
        private readonly string runningEffectName = null;
        private readonly string powerEffectName = null;
        private readonly string engageEffectName = null;
        private readonly string disengageEffectName = null;
        private readonly string directThrottleEffectName = null;
        private readonly string spoolEffectName = null;

        public ModuleDataHandlerrModuleEnginesFX(ModuleEnginesFX module, ConfigNode dataNode)
        {
            module.ThrowIfNullArgument(nameof(module));
            dataNode.ThrowIfNullArgument(nameof(dataNode));

            this.module = module;
            
            flameoutEffectName = dataNode.GetValue("flameoutEffectName");
            runningEffectName = dataNode.GetValue("runningEffectName");
            powerEffectName = dataNode.GetValue("powerEffectName");
            engageEffectName = dataNode.GetValue("engageEffectName");
            disengageEffectName = dataNode.GetValue("disengageEffectName");
            directThrottleEffectName = dataNode.GetValue("directThrottleEffectName");
            spoolEffectName = dataNode.GetValue("spoolEffectName");
        }

        public void Activate()
        {
            if (flameoutEffectName.IsNotNull())
                module.part.Effect(module.flameoutEffectName, 0);

            if (runningEffectName.IsNotNull())
                module.part.Effect(module.runningEffectName, 0);

            if (powerEffectName.IsNotNull())
                module.part.Effect(module.powerEffectName, 0);

            if (engageEffectName.IsNotNull())
                module.part.Effect(module.engageEffectName, 0);

            if (disengageEffectName.IsNotNull())
                module.part.Effect(module.disengageEffectName, 0);

            if (directThrottleEffectName.IsNotNull())
                module.part.Effect(module.directThrottleEffectName, 0);

            if (spoolEffectName.IsNotNull())
                module.part.Effect(module.spoolEffectName, 0);
        }

        public void Deactivate()
        {
            if (flameoutEffectName.IsNotNull())
                module.part.Effect(flameoutEffectName, 0);

            if (runningEffectName.IsNotNull())
                module.part.Effect(runningEffectName, 0);

            if (powerEffectName.IsNotNull())
                module.part.Effect(powerEffectName, 0);

            if (engageEffectName.IsNotNull())
                module.part.Effect(engageEffectName, 0);

            if (disengageEffectName.IsNotNull())
                module.part.Effect(disengageEffectName, 0);

            if (directThrottleEffectName.IsNotNull())
                module.part.Effect(directThrottleEffectName, 0);

            if (spoolEffectName.IsNotNull())
                module.part.Effect(spoolEffectName, 0);
        }
    }
}
