using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Localization;

namespace B9PartSwitch
{
    public static class PartSwitchFlightDialog
    {
        public static void Spawn(ModuleB9PartSwitch module)
        {
            MaybeCreateResourceRemovalWarning(module, () => CreateDialogue(module));
        }

        public static void MaybeCreateResourceRemovalWarning(ModuleB9PartSwitch module, Action onConfirm)
        {
            if (HighLogic.LoadedSceneIsFlight && module.CurrentTankType.ResourceNames.Any(name => module.part.Resources[name].amount > 0))
            {
                CreateWarning(module, onConfirm);
            }
            else
            {
                onConfirm();
            }
        }

        private static void CreateWarning(ModuleB9PartSwitch module, Action onConfirm)
        {
            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog(
                    "B9PartSwitch_SwitchInFlightWarning",
                    Localizer.Format(Localization.PartSwitchFlightDialog_ResourcesWillBeDumpedWarning, module.part.partInfo.title, module.switcherDescription), // <<1>> has resources that will be dumped by switching the <<2>>
                    Localization.PartSwitchFlightDialog_ConfirmResourceRemovalDialogTitle, // Confirm Resource Removal
                    HighLogic.UISkin,
                    new DialogGUIButton(Localization.PartSwitchFlightDialog_AcceptString, () => onConfirm()),
                    new DialogGUIButton(Localization.PartSwitchFlightDialog_CancelString, delegate { })
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
                    Localizer.Format(Localization.PartSwitchFlightDialog_SelectNewSubtypeDialogTitle, module.switcherDescription), // Select <<1>>
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
                    options.Add(new DialogGUILabel(Localizer.Format(Localization.PartSwitchFlightDialog_CurrentSubtypeLabel, subtype.title), HighLogic.UISkin.button)); // <<1>> (Current)
                }
                else if (HighLogic.LoadedSceneIsEditor || subtype.allowSwitchInFlight)
                {
                    options.Add(new DialogGUIButton(subtype.title, () => module.SwitchSubtype(subtype.Name)));
                }
            }

            options.Add(new DialogGUIButton(Localization.PartSwitchFlightDialog_CancelString, delegate { } ));

            return options.ToArray();
        }
    }
}
