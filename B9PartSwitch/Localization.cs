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
        private static string moduleB9PartSwitch_NextSubtype;
        private static string moduleB9PartSwitch_PreviousSubtype;

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

        public static string ModuleB9PartSwitch_NextSubtype(string switcherDescription)
        {
            return Localizer.Format(moduleB9PartSwitch_NextSubtype, switcherDescription);
        }

        public static string ModuleB9PartSwitch_PreviousSubtype(string switcherDescription)
        {
            return Localizer.Format(moduleB9PartSwitch_PreviousSubtype, switcherDescription);
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
            moduleB9PartSwitch_NextSubtype = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_next_subtype");
            moduleB9PartSwitch_PreviousSubtype = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_previous_subtype");
        }
    }
}
