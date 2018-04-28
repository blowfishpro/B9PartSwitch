using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Localization;

namespace B9PartSwitch
{
    public static class PartSwitchFlightDialog
    {
        private static readonly string ResourcesWillBeDumpedWarning = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_resources_will_be_dumped_warning");
        private static readonly string ConfirmResourceRemovalDialogTitle = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_confirm_resource_removal_dialog_title");
        private static readonly string SelectNewSubtypeDialogTitle = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_select_new_subtype_dialog_title");
        private static readonly string CurrentSubtypeLabel = Localizer.GetStringByTag("#LOC_B9PartSwitch_PartSwitchFlightDialog_current_subtype_label");
        private static readonly string AcceptString = Localizer.GetStringByTag("#autoLOC_6001205"); // Accept
        private static readonly string CancelString = Localizer.GetStringByTag("#autoLOC_6001206"); // Cancel

        public static void Spawn(ModuleB9PartSwitch module)
        {
            bool showWarning = HighLogic.LoadedSceneIsFlight && module.CurrentTankType.ResourceNames.Any(name => module.part.Resources[name].amount > 0);

            if (showWarning)
            {
                CreateWarning(module);
            }
            else
            {
                CreateDialogue(module);
            }
        }

        private static void CreateWarning(ModuleB9PartSwitch module)
        {
            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog(
                    "B9PartSwitch_SwitchInFlightWarning",
                    Localizer.Format(ResourcesWillBeDumpedWarning, module.part.partInfo.title, module.switcherDescription), // <<1>> has resources that will be dumped by switching the <<2>>
                    ConfirmResourceRemovalDialogTitle, // Confirm Resource Removal
                    HighLogic.UISkin,
                    new DialogGUIButton(AcceptString, () => CreateDialogue(module)),
                    new DialogGUIButton(CancelString, delegate { } )
                ),
                false,
                HighLogic.UISkin
            );
        }

        private static void CreateDialogue(ModuleB9PartSwitch module)
        {
            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog(
                    "B9PartSwitch_SwitchInFlight",
                    Localizer.Format(SelectNewSubtypeDialogTitle, module.switcherDescription), // Select <<1>>
                    module.part.partInfo.title,
                    HighLogic.UISkin,
                    CreateOptions(module)
                ),
                false,
                HighLogic.UISkin
            );
        }

        private static DialogGUIBase[] CreateOptions(ModuleB9PartSwitch module)
        {
            List<DialogGUIBase> options = new List<DialogGUIBase>();

            foreach (PartSubtype subtype in module.subtypes)
            {
                if (subtype == module.CurrentSubtype)
                {
                    options.Add(new DialogGUILabel(Localizer.Format(CurrentSubtypeLabel, subtype.title), HighLogic.UISkin.button)); // <<1>> (Current)
                }
                else if (HighLogic.LoadedSceneIsEditor || subtype.allowSwitchInFlight)
                {
                    options.Add(new DialogGUIButton(subtype.title, () => module.SwitchSubtype(subtype.Name)));
                }
            }

            options.Add(new DialogGUIButton(CancelString, delegate { } ));

            return options.ToArray();
        }
    }
}
