namespace B9PartSwitch
{
    public static class PartModuleExtensions
    {
        // partInfo is assigned after modules are created and loaded
        public static bool ParsedPrefab(this PartModule module) => module.part.partInfo != null;

        // get the corresponding PartModule in the part prefab
        public static PartModule GetPrefab(this PartModule module)
        {
            return module.part.GetPrefab().Modules[module.part.Modules.IndexOf(module)];
        }

        // enable a PartModule
        public static void Enable(this PartModule module)
        {
            module.isEnabled = true;
            module.moduleIsEnabled = true;
            module.enabled = true;

            // update staging icon visibility
            if (module.IsStageable())
            {
                module.stagingEnabled = module.GetPrefab().stagingEnabled;
                module.part.UpdateStageability(false, true);
            }
        }

        public static void Disable(this PartModule module)
        {

            /* Reset state on these modules using IScalarModule, assuming disabled = 0
             ModuleAnimateGeneric : BREAK HARD : anim is not played fully because it needs its update to run -> if in open state when disabling the state becomes inconsistent and module break
             ModuleAnimationSetter : ?
             ModuleColorChanger (used for cabin lights) : OK
             ModuleDeployablePart : OK
             ModuleJettison : NOT OK, but the shroud will reappear if the part is detached/reattached, and potentially in other situations (stock texture switch...)
             ModuleLight : OK
             ModuleProceduralFairing : NOT OK doesn't work, may break things as state will likely be inconsistent
             ModuleServiceModule (shroud switcher): doesn't work + staging icon doesn't disappear when disabling 
            */

            // Change state of some modules when switching in the editor
            // TODO: add specific support for more stock modules : jettison (shrouds), fairings, AnimateGeneric, AnimationGroup...
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (module is ModuleColorChanger || module is ModuleDeployablePart || module is ModuleLight)
                {
                    ((IScalarModule)module).SetScalar(0.0f);
                }
            }

            // update staging icon visibility
            if (module.IsStageable())
            {
                module.stagingEnabled = false;
                module.part.UpdateStageability(false, true);
            }

            module.isEnabled = false;
            module.moduleIsEnabled = false;
            module.enabled = false;
        }

        #region Logging

        public static void LogInfo(this PartModule module, object message) => module.part.LogInfo($"{module.LogTagString()} {message}");
        public static void LogWarning(this PartModule module, object message) => module.part.LogWarning($"{module.LogTagString()} {message}");
        public static void LogError(this PartModule module, object message) => module.part.LogError($"{module.LogTagString()} {message}");

        public static string LogTagString(this PartModule module)
        {
            string info = module.GetType().Name;

            CustomPartModule utilModule = module as CustomPartModule;
            if (utilModule.IsNotNull() && !utilModule.moduleID.IsNullOrEmpty())
                info += $" '{utilModule.moduleID}'";

            return $"[{info}]";
        }

        #endregion
    }
}
