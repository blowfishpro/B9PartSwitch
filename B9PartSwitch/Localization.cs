using System;
using KSP.Localization;

namespace B9PartSwitch
{
    public static class Localization
    {
        private static string partSwitchFlightDialog_ResourcesWillBeDumpedWarning;
        public static string PartSwitchFlightDialog_ConfirmResourceRemovalDialogTitle { get; private set; }
        private static string partSwitchFlightDialog_SelectNewSubtypeDialogTitle;
        private static string partSwitchFlightDialog_CurrentSubtypeLabel;
        public static string PartSwitchFlightDialog_AcceptString { get; private set; }
        public static string PartSwitchFlightDialog_CancelString { get; private set; }

        public static string ModuleB9PartSwitch_ModuleTitle { get; private set; }
        public static string ModuleB9PartSwitch_TankVolumeString { get; private set; }
        public static string ModuleB9PartSwitch_DefaultSwitcherDescription { get; private set; }
        public static string ModuleB9PartSwitch_DefaultSwitcherDescriptionPlural { get; private set; }
        private static string moduleB9PartSwitch_SelectSubtype;

        public static string SwitcherSubtypeDescriptionGenerator_Resources { get; private set; }
        public static string SwitcherSubtypeDescriptionGenerator_Mass { get; private set; }
        public static string SwitcherSubtypeDescriptionGenerator_Cost { get; private set; }
        public static string SwitcherSubtypeDescriptionGenerator_MaxTemp { get; private set; }
        public static string SwitcherSubtypeDescriptionGenerator_MaxSkinTemp { get; private set; }
        public static string SwitcherSubtypeDescriptionGenerator_CrashTolerance { get; private set; }
        public static string SwitcherSubtypeDescriptionGenerator_TankEmpty { get; private set; }
        public static string SwitcherSubtypeDescriptionGenerator_TankFull { get; private set; }
        private static string switcherSubtypeDescriptionGenerator_MassTons;
        private static string switcherSubtypeDescriptionGenerator_TemperatureKelvins;
        private static string switcherSubtypeDescriptionGenerator_SpeedMetersPerSecond;

        public static string PartSwitchFlightDialog_ResourcesWillBeDumpedWarning(string partName, string switcherDescription)
        {
            return Localizer.Format(partSwitchFlightDialog_ResourcesWillBeDumpedWarning, partName, switcherDescription);
        }

        public static string PartSwitchFlightDialog_SelectNewSubtypeDialogTitle(string switcherDescription)
        {
            return Localizer.Format(partSwitchFlightDialog_SelectNewSubtypeDialogTitle, switcherDescription);
        }

        public static string PartSwitchFlightDialog_CurrentSubtypeLabel(string currentSubtypeName)
        {
            return Localizer.Format(partSwitchFlightDialog_CurrentSubtypeLabel, currentSubtypeName);
        }

        public static string ModuleB9PartSwitch_SelectSubtype(string switcherDescription)
        {
            return Localizer.Format(moduleB9PartSwitch_SelectSubtype, switcherDescription);
        }

        public static string SwitcherSubypeDescriptionGenerator_MassTons(float massTons, string format)
        {
            return Localizer.Format(switcherSubtypeDescriptionGenerator_MassTons, massTons.ToString(format));
        }

        public static string SwitcherSubypeDescriptionGenerator_TemperatureKelvins(float temperatureKelvins, string format)
        {
            return Localizer.Format(switcherSubtypeDescriptionGenerator_TemperatureKelvins, temperatureKelvins.ToString(format));
        }

        public static string SwitcherSubypeDescriptionGenerator_SpeedMetersPerSecond(float speedMetersPerSecond, string format)
        {
            return Localizer.Format(switcherSubtypeDescriptionGenerator_SpeedMetersPerSecond, speedMetersPerSecond.ToString(format));
        }

        static Localization()
        {
            GameEvents.onLanguageSwitched.Add(() => RefreshLocalization());
            RefreshLocalization();
        }

        private static void RefreshLocalization()
        {
            partSwitchFlightDialog_ResourcesWillBeDumpedWarning = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_resources_will_be_dumped_warning");
            PartSwitchFlightDialog_ConfirmResourceRemovalDialogTitle = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_confirm_resource_removal_dialog_title");
            partSwitchFlightDialog_SelectNewSubtypeDialogTitle = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_select_new_subtype_dialog_title");
            partSwitchFlightDialog_CurrentSubtypeLabel = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_current_subtype_label");
            PartSwitchFlightDialog_AcceptString = Localizer.GetStringByTag("#autoLOC_6001205"); // Accept
            PartSwitchFlightDialog_CancelString = Localizer.GetStringByTag("#autoLOC_6001206"); // Cancel

            ModuleB9PartSwitch_ModuleTitle = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_title");
            ModuleB9PartSwitch_TankVolumeString = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_tank_volume");
            ModuleB9PartSwitch_DefaultSwitcherDescription = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_default_switcher_description");
            ModuleB9PartSwitch_DefaultSwitcherDescriptionPlural = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_default_switcher_description_plural");
            moduleB9PartSwitch_SelectSubtype = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_select_subtype");

            SwitcherSubtypeDescriptionGenerator_Resources = Localizer.GetStringByTag("#autoLOC_6001746"); // Resources
            SwitcherSubtypeDescriptionGenerator_Mass = Localizer.GetStringByTag("#autoLOC_900529"); // Mass
            SwitcherSubtypeDescriptionGenerator_Cost = Localizer.GetStringByTag("#autoLOC_900528"); // Cost
            SwitcherSubtypeDescriptionGenerator_MaxTemp = Localizer.GetStringByTag("#LOC_B9PartSwitch_SwitcherSubtypeDescriptionGenerator_max_temp");
            SwitcherSubtypeDescriptionGenerator_MaxSkinTemp = Localizer.GetStringByTag("#LOC_B9PartSwitch_SwitcherSubtypeDescriptionGenerator_max_skin_temp");
            SwitcherSubtypeDescriptionGenerator_CrashTolerance = Localizer.GetStringByTag("#LOC_B9PartSwitch_SwitcherSubtypeDescriptionGenerator_crash_tolerance");
            SwitcherSubtypeDescriptionGenerator_TankEmpty = Localizer.GetStringByTag("#LOC_B9PartSwitch_SwitcherSubtypeDescriptionGenerator_tank_empty");
            SwitcherSubtypeDescriptionGenerator_TankFull = Localizer.GetStringByTag("#LOC_B9PartSwitch_SwitcherSubtypeDescriptionGenerator_tank_full");
            switcherSubtypeDescriptionGenerator_MassTons = Localizer.GetStringByTag("#autoLOC_5050023").Replace("<<1>>", "<<1>> "); // <<1>>t // <<1>> t
            switcherSubtypeDescriptionGenerator_TemperatureKelvins = Localizer.GetStringByTag("#autoLOC_5050037"); // <<1>> m/s
            switcherSubtypeDescriptionGenerator_SpeedMetersPerSecond = Localizer.GetStringByTag("#autoLOC_5050034"); // <<1>> m/s
        }
    }
}
