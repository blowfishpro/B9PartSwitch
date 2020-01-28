using B9PartSwitch.Logging;

namespace B9PartSwitch
{
    public static class PartModuleExtensions
    {
        // partInfo is assigned after modules are created and loaded
        public static bool ParsedPrefab(this PartModule module) => module.part.partInfo != null;

        #region Logging

        public static ILogger CreateLogger(this PartModule module)
        {
            string partName = module.part.partInfo?.name ?? module.part.name;
            string moduleName = module.GetType().FullName;
            return new PrefixLogger(SystemLogger.Logger, $"{partName} {moduleName}");
        }

        #endregion
    }
}
