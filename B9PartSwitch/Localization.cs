using System;
using KSP.Localization;

namespace B9PartSwitch
{
    public static class Localization
    {
        public static string PartSwitchFlightDialog_ResourcesWillBeDumpedWarning { get; private set; }
        public static string PartSwitchFlightDialog_ConfirmResourceRemovalDialogTitle { get; private set; }
        public static string PartSwitchFlightDialog_SelectNewSubtypeDialogTitle { get; private set; }
        public static string PartSwitchFlightDialog_CurrentSubtypeLabel { get; private set; }
        public static string PartSwitchFlightDialog_AcceptString { get; private set; }
        public static string PartSwitchFlightDialog_CancelString { get; private set; }

        public static string ModuleB9PartSwitch_ModuleTitle { get; private set; }
        public static string ModuleB9PartSwitch_TankVolumeString { get; private set; }
        public static string ModuleB9PartSwitch_DefaultSwitcherDescription { get; private set; }
        public static string ModuleB9PartSwitch_DefaultSwitcherDescriptionPlural { get; private set; }

        static Localization()
        {
            GameEvents.onLanguageSwitched.debugEvent = true;
            GameEvents.onLanguageSwitched.Add(() => RefreshLocalization());
            RefreshLocalization();
        }

        private static void RefreshLocalization()
        {
            PartSwitchFlightDialog_ResourcesWillBeDumpedWarning = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_resources_will_be_dumped_warning");
            PartSwitchFlightDialog_ConfirmResourceRemovalDialogTitle = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_confirm_resource_removal_dialog_title");
            PartSwitchFlightDialog_SelectNewSubtypeDialogTitle = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_select_new_subtype_dialog_title");
            PartSwitchFlightDialog_CurrentSubtypeLabel = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_current_subtype_label");
            PartSwitchFlightDialog_AcceptString = Localizer.GetStringByTag("#autoLOC_6001205"); // Accept
            PartSwitchFlightDialog_CancelString = Localizer.GetStringByTag("#autoLOC_6001206"); // Cancel

            ModuleB9PartSwitch_ModuleTitle = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_title");
            ModuleB9PartSwitch_TankVolumeString = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_tank_volume");
            ModuleB9PartSwitch_DefaultSwitcherDescription = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_default_switcher_description");
            ModuleB9PartSwitch_DefaultSwitcherDescriptionPlural = Localizer.GetStringByTag("#LOC_B9PartSwitch_ModuleB9PartSwitch_default_switcher_description_plural");
        }
    }
}
