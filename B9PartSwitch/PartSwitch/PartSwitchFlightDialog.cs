using System;
using System.Collections.Generic;
using System.Linq;

namespace B9PartSwitch
{
    public static class PartSwitchFlightDialog
    {
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
                    $"{module.part.partInfo.title} has resources that will be dumped by switching the {module.switcherDescription}",
                    "Confirm Resource Removal",
                    HighLogic.UISkin,
                    new DialogGUIButton("Confirm", () => CreateDialogue(module)),
                    new DialogGUIButton("Cancel", delegate { } )
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
                    $"Select {module.switcherDescription}",
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

            for(int i = 0; i < module.subtypes.Count; i++)
            {
                PartSubtype subtype = module.subtypes[i];
                if (subtype == module.CurrentSubtype)
                {
                    options.Add(new DialogGUILabel(subtype.title + " (Current)", HighLogic.UISkin.button));
                }
                else if (HighLogic.LoadedSceneIsEditor || subtype.allowSwitchInFlight)
                {
                    int j = i; // Necessary due to capturing
                    options.Add(new DialogGUIButton(subtype.title, () => module.SetSubtype(j)));
                }
            }

            options.Add(new DialogGUIButton("Cancel", delegate { } ));

            return options.ToArray();
        }
    }
}
