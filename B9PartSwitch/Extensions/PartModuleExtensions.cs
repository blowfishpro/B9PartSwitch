namespace B9PartSwitch
{
    public static class PartModuleExtensions
    {
        // partInfo is assigned after modules are created and loaded
        public static bool ParsedPrefab(this PartModule module) => module.part.partInfo != null;

        public static void SetUiGroups(this PartModule module, string uiGroupName, string uiGroupDisplayName)
        {
            module.ThrowIfNullArgument(nameof(module));

            foreach (BaseField field in module.Fields)
            {
                if (!field.group.name.IsNullOrEmpty()) continue;

                field.group.name = uiGroupName;
                field.group.displayName = uiGroupDisplayName;
            }

            foreach (BaseEvent baseEvent in module.Events)
            {
                if (!baseEvent.group.name.IsNullOrEmpty()) continue;

                baseEvent.group.name = uiGroupName;
                baseEvent.group.displayName = uiGroupDisplayName;
            }
        }

        #region Logging

        public static void LogInfo(this PartModule module, object message) => module.part.LogInfo($"{module.LogTagString()} {message}");
        public static void LogWarning(this PartModule module, object message) => module.part.LogWarning($"{module.LogTagString()} {message}");
        public static void LogError(this PartModule module, object message) => module.part.LogError($"{module.LogTagString()} {message}");

        public static string LogTagString(this PartModule module)
        {
            string info = module.GetType().Name;

            if (module is CustomPartModule utilModule && !utilModule.moduleID.IsNullOrEmpty())
                info += $" '{utilModule.moduleID}'";

            return $"[{info}]";
        }

        #endregion
    }
}
