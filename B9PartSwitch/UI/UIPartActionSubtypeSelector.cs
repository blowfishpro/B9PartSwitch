using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI.TooltipTypes;

namespace B9PartSwitch.UI
{
    [UI_SubtypeSelector]
    public class UIPartActionSubtypeSelector : UIPartActionFieldItem
    {
        private static UIPartActionSubtypeSelector partActionSubtypeSelectorPrefab;

        [SerializeField]
        private GameObject prefabVariantButton;

        [SerializeField]
        private Button buttonPrevious;

        [SerializeField]
        private Button buttonNext;

        [SerializeField]
        private ScrollRect scrollMain;

        [SerializeField]
        private TextMeshProUGUI switcherDescriptionText;

        [SerializeField]
        private TextMeshProUGUI subtypeTitleText;

        [SerializeField]
        private TooltipController_TitleAndText buttonPreviousTooltipController;

        [SerializeField]
        private TooltipController_TitleAndText buttonNextTooltipController;

        private List<UIPartActionSubtypeButton> subtypeButtons;

        public static void EnsurePrefab()
        {
            if (UIPartActionController.Instance.IsNull()) throw new InvalidOperationException("UIPartActionController.Instance is null");

            if (partActionSubtypeSelectorPrefab.IsNull()) partActionSubtypeSelectorPrefab = CreatePrefab();
            else if (UIPartActionController.Instance.fieldPrefabs.Contains(partActionSubtypeSelectorPrefab)) return;

            UIPartActionController.Instance.fieldPrefabs.Add(partActionSubtypeSelectorPrefab);
            Debug.Log("[B9PartSwitch.UI.UIPartActionSubtypeSelector] added prefab to UIPartActionController");
        }

        public override void Setup(UIPartActionWindow window, Part part, PartModule partModule, UI_Scene scene, UI_Control control, BaseField field)
        {
            base.Setup(window, part, partModule, scene, control, field);

            ModuleB9PartSwitch switcherModule = (ModuleB9PartSwitch)partModule;

            SwitcherSubtypeDescriptionGenerator subtypeDescriptionGenerator = new SwitcherSubtypeDescriptionGenerator(switcherModule);

            subtypeButtons = new List<UIPartActionSubtypeButton>(switcherModule.subtypes.Count);
            for (int i = 0; i < switcherModule.subtypes.Count; i++)
            {
                PartSubtype subtype = switcherModule.subtypes[i];
                GameObject buttonGameObject = Instantiate(prefabVariantButton, scrollMain.content);
                UIPartActionSubtypeButton subtypeButton = buttonGameObject.GetComponent<UIPartActionSubtypeButton>();

                // prevent capturing in closures
                int index = i;

                subtypeButton.Setup(
                    subtype.title,
                    subtypeDescriptionGenerator.GetFullSubtypeDescription(subtype),
                    subtype.PrimaryColor,
                    subtype.SecondaryColor,
                    () => SetSubtype(index)
                );

                subtypeButtons.Add(subtypeButton);
            }

            subtypeButtons[0].previousItem = subtypeButtons[subtypeButtons.Count - 1];
            subtypeButtons[0].nextItem = subtypeButtons[1];
            for (int i = 1; i < subtypeButtons.Count - 1; i++)
            {
                subtypeButtons[i].previousItem = subtypeButtons[i - 1];
                subtypeButtons[i].nextItem = subtypeButtons[i + 1];
            }
            subtypeButtons[subtypeButtons.Count - 1].previousItem = subtypeButtons[subtypeButtons.Count - 2];
            subtypeButtons[subtypeButtons.Count - 1].nextItem = subtypeButtons[0];

            switcherDescriptionText.text = switcherModule.switcherDescription;

            TooltipHelper.SetupSubtypeInfoTooltip(buttonPreviousTooltipController, "", "");
            TooltipHelper.SetupSubtypeInfoTooltip(buttonNextTooltipController, "", "");

            SetTooltips(switcherModule.currentSubtypeIndex);

            buttonPrevious.onClick.AddListener(PreviousSubtype);
            buttonNext.onClick.AddListener(NextSubtype);

            subtypeTitleText.text = switcherModule.CurrentSubtype.title;

            subtypeButtons[switcherModule.currentSubtypeIndex].Activate();
        }

        public override void UpdateItem()
        {
            transform.SetAsLastSibling();
        }

        private void PreviousSubtype()
        {
            int currentIndex = field.GetValue<int>(field.host);
            subtypeButtons[currentIndex].previousItem.SetSubtype();
        }

        private void NextSubtype()
        {
            int currentIndex = field.GetValue<int>(field.host);
            subtypeButtons[currentIndex].nextItem.SetSubtype();
        }

        private void SetSubtype(int newIndex)
        {
            int currentIndex = field.GetValue<int>(field.host);
            if (newIndex == currentIndex) return;

            subtypeButtons[currentIndex].Deactivate();
            subtypeButtons[newIndex].Activate();
            subtypeTitleText.text = subtypeButtons[newIndex].Title;

            SetTooltips(newIndex);

            SetFieldValue(newIndex);
        }

        private void SetTooltips(int index)
        {
            buttonPreviousTooltipController.titleString = subtypeButtons[index].previousItem.Title;
            buttonPreviousTooltipController.textString = subtypeButtons[index].previousItem.Description;

            buttonNextTooltipController.titleString = subtypeButtons[index].nextItem.Title;
            buttonNextTooltipController.textString = subtypeButtons[index].nextItem.Description;
        }

        public static UIPartActionSubtypeSelector CreatePrefab()
        {
            GameObject partActionVariantSelectorPrefabGameObject = UIPartActionController.Instance.fieldPrefabs.OfType<UIPartActionVariantSelector>().FirstOrDefault()?.gameObject;

            if (partActionVariantSelectorPrefabGameObject.IsNull())
                throw new Exception("Could not find FieldVariantSelector prefab");

            GameObject partActionSubtypeSelectorGameObject = Instantiate(partActionVariantSelectorPrefabGameObject);
            DontDestroyOnLoad(partActionSubtypeSelectorGameObject);

            UIPartActionVariantSelector partActionVariantSelector = partActionSubtypeSelectorGameObject.GetComponent<UIPartActionVariantSelector>();
            UIPartActionSubtypeSelector partActionSubtypeSelector = partActionSubtypeSelectorGameObject.AddComponent<UIPartActionSubtypeSelector>();

            partActionSubtypeSelector.buttonPrevious = partActionVariantSelector.buttonPrevious;
            partActionSubtypeSelector.buttonNext = partActionVariantSelector.buttonNext;
            partActionSubtypeSelector.scrollMain = partActionVariantSelector.scrollMain;
            partActionSubtypeSelector.subtypeTitleText = partActionVariantSelector.variantName;
            partActionSubtypeSelector.buttonPreviousTooltipController = partActionSubtypeSelector.buttonPrevious.gameObject.AddComponent<TooltipController_TitleAndText>();
            partActionSubtypeSelector.buttonNextTooltipController = partActionSubtypeSelector.buttonNext.gameObject.AddComponent<TooltipController_TitleAndText>();

            partActionSubtypeSelector.switcherDescriptionText = partActionSubtypeSelectorGameObject.GetChild("Label_Variants").GetComponent<TMPro.TextMeshProUGUI>();

            partActionSubtypeSelector.subtypeTitleText.alignment = TMPro.TextAlignmentOptions.Right;

            RectTransform switcherDescriptionRectTransform = (RectTransform)partActionSubtypeSelector.switcherDescriptionText.gameObject.transform;
            switcherDescriptionRectTransform.anchorMin = new Vector2(0, 1);
            switcherDescriptionRectTransform.anchorMax = new Vector2(0, 1);
            switcherDescriptionRectTransform.pivot = new Vector2(0, 0.5f);
            switcherDescriptionRectTransform.anchoredPosition = new Vector2(0, -5);
            switcherDescriptionRectTransform.sizeDelta += new Vector2(45, 0);

            RectTransform subypeTitleRectTransform = (RectTransform)partActionSubtypeSelector.subtypeTitleText.gameObject.transform;
            subypeTitleRectTransform.pivot = new Vector2(1, 0.5f);
            subypeTitleRectTransform.anchoredPosition = new Vector2(0, -5);
            subypeTitleRectTransform.sizeDelta -= new Vector2(20, 0);

            partActionSubtypeSelector.prefabVariantButton = UIPartActionSubtypeButton.CreatePrefab(partActionVariantSelector.prefabVariantButton);

            Destroy(partActionVariantSelector);

            return partActionSubtypeSelector;
        }
    }
}
