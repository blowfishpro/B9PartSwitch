using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using B9PartSwitch.UI;
using UnityEngine;
using UnityEngine.UI;

namespace B9PartSwitch
{
    public static class PartSwitchFlightDialog
    {
        public static void Spawn(ModuleB9PartSwitch module)
        {
            try
            {
                MaybeCreateResourceRemovalWarning(module, () => CreateDialogue(module));
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                FatalErrorHandler.HandleFatalError(ex);
            }
        }

        private static void MaybeCreateResourceRemovalWarning(ModuleB9PartSwitch module, Action onConfirm)
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
                    Localization.PartSwitchFlightDialog_ResourcesWillBeDumpedWarning(module.part.partInfo.title, module.switcherDescription), // <<1>> has resources that will be dumped by switching the <<2>>
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
            List<Callback> afterCreateCallbacks = new List<Callback>();
            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog(
                    "B9PartSwitch_SwitchInFlight",
                    Localization.PartSwitchFlightDialog_SelectNewSubtypeDialogTitle(module.switcherDescription), // Select <<1>>
                    module.part.partInfo.title,
                    HighLogic.UISkin,
                    CreateOptions(module, afterCreateCallbacks)
                ),
                false,
                HighLogic.UISkin
            );

            foreach (Callback callback in afterCreateCallbacks)
            {
                callback();
            }
        }

        private static DialogGUIBase[] CreateOptions(ModuleB9PartSwitch module, IList<Callback> afterCreateCallbacks)
        {
            List<DialogGUIBase> options = new List<DialogGUIBase>();

            SwitcherSubtypeDescriptionGenerator subtypeDescriptionGenerator = new SwitcherSubtypeDescriptionGenerator(module);

            foreach (PartSubtype subtype in module.subtypes)
            {
                if (!subtype.IsUnlocked()) continue;

                if (subtype == module.CurrentSubtype)
                {
                    string currentSubtypeText = Localization.PartSwitchFlightDialog_CurrentSubtypeLabel(subtype.title);  // <<1>> (Current)
                    DialogGUILabel label = new DialogGUILabel(currentSubtypeText, HighLogic.UISkin.button);
                    afterCreateCallbacks.Add(delegate
                    {
                        if (!(label.uiItem.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI textUI))
                            throw new Exception("Could not find TextMeshProUGUI");
                        else
                            textUI.raycastTarget = true;
                    });
                    afterCreateCallbacks.Add(() => TooltipHelper.SetupSubtypeInfoTooltip(label.uiItem, subtype.title, subtypeDescriptionGenerator.GetFullSubtypeDescription(subtype)));
                    options.Add(label);
                }
                else if (HighLogic.LoadedSceneIsEditor || subtype.allowSwitchInFlight)
                {
                    DialogGUIButton button = new DialogGUIButton(subtype.title, () => module.SwitchSubtype(subtype.Name));
                    afterCreateCallbacks.Add(() => TooltipHelper.SetupSubtypeInfoTooltip(button.uiItem, subtype.title, subtypeDescriptionGenerator.GetFullSubtypeDescription(subtype)));
                    options.Add(button);
                }
            }

            options.Add(new DialogGUIButton(Localization.PartSwitchFlightDialog_CancelString, delegate { } ));

            const float buttonHeight = 35;
            if (buttonHeight * options.Count < 0.75f * Screen.height) return options.ToArray();

            options.Insert(0, new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize));
            DialogGUIBase[] scrollList = {
                new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize),
                new DialogGUIScrollList(
                    new Vector2(1, 0.75f * Screen.height),
                    false,
                    true,
                    new DialogGUIVerticalLayout(
                        true,
                        true,
                        4,
                        new RectOffset(6, 24, 10, 10),
                        TextAnchor.MiddleCenter,
                        options.ToArray()
                    )
                )
            };
            return scrollList;
        }
    }
}
