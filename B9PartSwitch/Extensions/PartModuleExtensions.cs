namespace B9PartSwitch
{
    public static class PartModuleExtensions
    {
        // partInfo is assigned after modules are created and loaded
        public static bool ParsedPrefab(this PartModule module) => module.part.partInfo != null;

        #region Logging

        public static void LogInfo(this PartModule module, object message) => module.part.LogInfo($"{module.LogTagString()} {message}");
        public static void LogWarning(this PartModule module, object message) => module.part.LogWarning($"{module.LogTagString()} {message}");
        public static void LogError(this PartModule module, object message) => module.part.LogError($"{module.LogTagString()} {message}");

        public static string LogTagString(this PartModule module)
        {
            string info = module.GetType().Name;

            CFGUtilPartModule utilModule = module as CFGUtilPartModule;
            if (utilModule.IsNotNull() && !string.IsNullOrEmpty(utilModule.moduleID))
                info += $" '{utilModule.moduleID}'";

            return $"[{info}]";
        }

        #endregion
    }
}
