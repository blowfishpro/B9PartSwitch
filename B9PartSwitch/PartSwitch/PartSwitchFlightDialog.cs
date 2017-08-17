using System;

namespace B9PartSwitch
{
    public static class PartSwitchFlightDialog
    {
        public static void Spawn(ModuleB9PartSwitch module)
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
            DialogGUIBase[] options = new DialogGUIBase[module.subtypes.Count + 1];

            for(int i = 0; i < module.subtypes.Count; i++)
            {
                PartSubtype subtype = module.subtypes[i];
                if (subtype == module.CurrentSubtype)
                {
                    options[i] = new DialogGUILabel(subtype.title + " (Current)", HighLogic.UISkin.button);
                }
                else
                {
                    int j = i; // Necessary due to capturing
                    options[i] = new DialogGUIButton(
                        subtype.title, () => module.SetSubtype(j)
                    );
                }
            }

            options[options.Length - 1] = new DialogGUIButton("Cancel", delegate { } );

            return options;
        }
    }
}
