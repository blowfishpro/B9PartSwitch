using System;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class ModuleDataHandlerrModuleEnginesFX : PartModifierBase
    {
        private readonly ModuleEnginesFX module;

        private readonly string originalFlameoutEffectName = null;
        private readonly string originalRunningEffectName = null;
        private readonly string originalPowerEffectName = null;
        private readonly string originalEngageEffectName = null;
        private readonly string originalDisengageEffectName = null;
        private readonly string originalDirectThrottleEffectName = null;
        private readonly string originalSpoolEffectName = null;

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

            originalFlameoutEffectName = module.flameoutEffectName;
            originalRunningEffectName = module.runningEffectName;
            originalPowerEffectName = module.powerEffectName;
            originalEngageEffectName = module.engageEffectName;
            originalDisengageEffectName = module.disengageEffectName;
            originalDirectThrottleEffectName = module.directThrottleEffectName;
            originalSpoolEffectName = module.spoolEffectName;

            flameoutEffectName = dataNode.GetValue("flameoutEffectName");
            runningEffectName = dataNode.GetValue("runningEffectName");
            powerEffectName = dataNode.GetValue("powerEffectName");
            engageEffectName = dataNode.GetValue("engageEffectName");
            disengageEffectName = dataNode.GetValue("disengageEffectName");
            directThrottleEffectName = dataNode.GetValue("directThrottleEffectName");
            spoolEffectName = dataNode.GetValue("spoolEffectName");
        }

        public override string Description => $"data on ModuleEnginesFX {module}";

        public override void ActivateOnStartFinishedFlight() => Activate();
        public override void DeactivateOnStartFinishedFlight() => Deactivate();
        public override void DeactivateOnSwitchFlight() => Deactivate();
        public override void ActivateOnSwitchFlight() => Activate();

        private void Activate()
        {
            if (flameoutEffectName.IsNotNull())
                module.part.Effect(originalFlameoutEffectName, effectPower: 0);

            if (runningEffectName.IsNotNull())
                module.part.Effect(originalRunningEffectName, effectPower: 0);

            if (powerEffectName.IsNotNull())
                module.part.Effect(originalPowerEffectName, effectPower: 0);

            if (engageEffectName.IsNotNull())
                module.part.Effect(originalEngageEffectName, effectPower: 0);

            if (disengageEffectName.IsNotNull())
                module.part.Effect(originalDisengageEffectName, effectPower: 0);

            if (directThrottleEffectName.IsNotNull())
                module.part.Effect(originalDirectThrottleEffectName, effectPower: 0);

            if (spoolEffectName.IsNotNull())
                module.part.Effect(originalSpoolEffectName, effectPower: 0);
        }

        private void Deactivate()
        {
            if (flameoutEffectName.IsNotNull())
                module.part.Effect(flameoutEffectName, effectPower: 0);

            if (runningEffectName.IsNotNull())
                module.part.Effect(runningEffectName, effectPower: 0);

            if (powerEffectName.IsNotNull())
                module.part.Effect(powerEffectName, effectPower: 0);

            if (engageEffectName.IsNotNull())
                module.part.Effect(engageEffectName, effectPower: 0);

            if (disengageEffectName.IsNotNull())
                module.part.Effect(disengageEffectName, effectPower: 0);

            if (directThrottleEffectName.IsNotNull())
                module.part.Effect(directThrottleEffectName, effectPower: 0);

            if (spoolEffectName.IsNotNull())
                module.part.Effect(spoolEffectName, effectPower: 0);
        }
    }
}
