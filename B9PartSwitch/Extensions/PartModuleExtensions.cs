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

        public static ConfigNode GetConfigNode(this PartModule module)
        {
           return module.part.partInfo.partConfig.GetNode("MODULE", module.part.Modules.IndexOf(module));
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
