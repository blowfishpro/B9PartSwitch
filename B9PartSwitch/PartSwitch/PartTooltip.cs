using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.TooltipTypes;
using KSP.UI;
using UnityEngine.UI;
using KSP.UI.Screens.Editor;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

namespace B9PartSwitch.PartSwitch
{

    public class DragPanelOffsettable : MonoBehaviour, IPointerDownHandler, IDragHandler, IEventSystemHandler
    {
        private RectTransform panelRectTransform;
        [SerializeField]
        public int edgeOffset = 60;
        private void Start()
        {
            panelRectTransform = (transform as RectTransform);
            GameEvents.OnGameSettingsApplied.Add(new EventVoid.OnEvent(OnGameSettingsApplied));
        }
        private void OnDestroy()
        {
            GameEvents.OnGameSettingsApplied.Remove(new EventVoid.OnEvent(OnGameSettingsApplied));
        }
        private void OnGameSettingsApplied()
        {
            if (UIMasterController.AnyCornerOffScreen(panelRectTransform))
            {
                UIMasterController.DragTooltip(panelRectTransform, Vector2.zero, Vector3.one * edgeOffset);
            }
        }
        public void OnPointerDown(PointerEventData data)
        {
            panelRectTransform.SetAsLastSibling();
        }
        public void OnDrag(PointerEventData data)
        {
            if (panelRectTransform == null)
            {
                return;
            }
            UIMasterController.DragTooltip(panelRectTransform, data.delta, Vector3.one * edgeOffset);
        }
    }

    public class ButtonWidget : MonoBehaviour
    {
        public PartModule switcherModule;
        public PartListTooltipWidget widget;
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class PartTooltip : MonoBehaviour //, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // UI Objects
        GameObject infoToolTip;
        PartListTooltip infoToolTipInstance;

        GameObject descriptionText;
        GameObject descriptionTopObject;
        GameObject descriptionContent;
        GameObject manufacturerPanel; 
        GameObject primaryInfoTopObject; 
        GameObject footerTopObject;
        GameObject infoButton;
        GameObject extWidgetsContent;

        private Part hoveredPart;
        private Part selectedPart;
        private bool infoToolTipVisible = false;

        private int delayedTooltipUpdate = -1;

        private bool objectsCreated = false;


        private void Start()
        {
            GameEvents.onEditorPartEvent.Add(OnEditorPartEvent);
            GameEvents.onEditorShipModified.Add(OnEditorShipModified);
            GameEvents.onPartActionUICreate.Add(OnPartActionUICreate);
        }

        public void OnDisable()
        {
            GameEvents.onEditorPartEvent.Remove(OnEditorPartEvent);
            GameEvents.onEditorShipModified.Remove(OnEditorShipModified);
            GameEvents.onPartActionUICreate.Remove(OnPartActionUICreate);
            infoToolTip.DestroyGameObject();
        }

        private void OnEditorShipModified(ShipConstruct data)
        {
            if (infoToolTipVisible) UpdateInfoToolTip(false);
        }

        private void OnEditorPartEvent(ConstructionEventType data, Part part)
        {
            if (data == ConstructionEventType.PartCreated)
            {
                for (int i = 0; i < part.Modules.Count; i++)
                {
                    if (part.Modules[i] is ModuleB9PartInfo)
                    {
                        part.Modules[i].enabled = false;
                        part.Modules[i].isEnabled = false;
                    }
                    else if (part.Modules[i] is ModuleB9PartSwitch || part.Modules[i] is ModulePartVariants)
                    {
                        for (int j = 0; j < part.Modules[i].Fields.Count; j++)
                        {
                            part.Modules[i].Fields[j].guiActiveEditor = false;
                        }
                    }
                }
            }

            if (!infoToolTipVisible) return;

            if (data != ConstructionEventType.PartTweaked)
            {
                ShowInfoToolTip(false);
            }
        }

        private void OnPartActionUICreate(Part part)
        {
            // Get the PAW window
            UIPartActionWindow paw = UIPartActionController.Instance.GetItem(part);

            // Prevent the button to be added once created
            if (paw.ListItems.Exists(p => p is UIPartActionButton && ((UIPartActionButton)p).Evt.name == "ConfigurePart"))
            {
                return;
            }

            BaseEventDelegate del = new BaseEventDelegate(delegate { OnPawConfigure(part);});

            string buttonTitle;
            if (part.Modules.Contains<ModuleB9PartSwitch>() || part.Modules.Contains<ModulePartVariants>())
            {
                buttonTitle = "Part info and configuration";
            }
            else
            {
                buttonTitle = "Part info";
            }

            BaseEvent baseEvent = new BaseEvent(null, "ConfigurePart", del, new KSPEvent());
            baseEvent.guiName = buttonTitle;
            baseEvent.active = true;
            baseEvent.guiActive = true;
            baseEvent.guiActiveEditor = true;
            paw.AddEventControl(baseEvent, part, null);
        }

        private void OnPawConfigure(Part part)
        {
            if (infoToolTipVisible)
            {
                if (selectedPart != part)
                {
                    selectedPart = part;
                    UpdateInfoToolTip(false);
                }
                else
                {
                    selectedPart = null;
                    ShowInfoToolTip(false);
                }
            }
            else
            {
                selectedPart = part;
                UpdateInfoToolTip(false);
                ShowInfoToolTip(true);
            }
        }

        protected void Update()
        {
            if (selectedPart == null)
            {
                ShowInfoToolTip(false);
            }

            if (EditorLogic.RootPart == null || EditorLogic.fetch.editorScreen != EditorScreen.Parts)
            {
                return;
            }

            hoveredPart = getHoveredPart();

            if (hoveredPart != null && Input.GetKeyDown(KeyCode.Mouse2))
            {
                if (!infoToolTipVisible)
                {
                    // show and update partinfo tooltip
                    UpdateInfoToolTip();
                    ShowInfoToolTip(true);
                    return;
                }
                else if (hoveredPart != selectedPart)
                {
                    UpdateInfoToolTip();
                    return;
                }
            }
            if (infoToolTipVisible && Input.GetKeyDown(KeyCode.Mouse1))
            {
                // hide partinfo tooltip
                ShowInfoToolTip(false);
                return;
            }

            if (delayedTooltipUpdate >= 0)
            {
                if (delayedTooltipUpdate == 0)
                {
                    UpdateInfoToolTip(false);
                    delayedTooltipUpdate = -1;
                }
                else
                {
                    delayedTooltipUpdate--;
                }
            }
        }

        private void ShowInfoToolTip(bool visible)
        {
            if (infoToolTip == null) return;
            infoToolTip.SetActive(visible);
            infoToolTipVisible = visible;
        }

        private void UpdateInfoToolTip(bool getHoveredpart = true)
        {
            // Get the part if requested
            if (getHoveredpart) selectedPart = hoveredPart;
            if (selectedPart == null)
            {
                ShowInfoToolTip(false);
                return;
            }

            // Create the UI
            if (!objectsCreated)
            {
                //UIPartActionWindow customPAW = Instantiate(UIPartActionController.Instance.windowPrefab);
                //customPAW.transform.SetParent(UIMasterController.Instance.actionCanvas.transform, false);
                //customPAW.gameObject.SetActive(true);
                //customPAW.gameObject.name = "FloatingPartTooltip";
                //customPAW.Setup(selectedPart, UIPartActionWindow.DisplayType.Selected, UI_Scene.Editor);
                //customPAW.gameObject.GetChild("VerticalLayout").DestroyGameObject();

                //infoToolTipInstance = Instantiate(FindObjectOfType<PartListTooltipController>().tooltipPrefab, customPAW.gameObject.transform);
                //infoToolTip = infoToolTipInstance.gameObject;
                //infoToolTip.transform.SetSiblingIndex(2);

                // Create tooltip object
                infoToolTipInstance = Instantiate(FindObjectOfType<PartListTooltipController>().tooltipPrefab, DialogCanvasUtil.DialogCanvasRect);
                infoToolTip = infoToolTipInstance.gameObject;

                // Get various UI GameObjects
                descriptionText = infoToolTip.GetChild("DescriptionText");
                descriptionTopObject = infoToolTip.GetChild("Scroll View");
                descriptionContent = descriptionText.transform.parent.gameObject;
                manufacturerPanel = infoToolTip.GetChild("ManufacturerPanel");
                primaryInfoTopObject = infoToolTip.GetChild("ThumbAndPrimaryInfo").GetChild("Scroll View");
                footerTopObject = infoToolTip.GetChild("Footer");
                extWidgetsContent = infoToolTipInstance.panelExtended.GetChild("Content");

                // Make it draggable
                DragPanelOffsettable dragpanel = infoToolTip.AddComponent<DragPanelOffsettable>();
                dragpanel.edgeOffset = 20;

                // Remove part thumbnail
                infoToolTip.GetChild("ThumbContainer").DestroyGameObject();
                // Remove RMBHint
                infoToolTip.GetChild("RMBHint").DestroyGameObject();
                // Remove module widget list variant spacer
                infoToolTipInstance.extInfoListSpacerVariants.gameObject.DestroyGameObject();

                // Adjust size
                LayoutElement leftPanel = infoToolTip.GetChild("StandardInfo").GetComponent<LayoutElement>();
                leftPanel.preferredWidth = 240;
                leftPanel.minWidth = 240;
                LayoutElement costPanel = infoToolTip.GetChild("CostPanel").GetComponent<LayoutElement>();
                costPanel.preferredWidth = 150;

                // Add switcher toggle
                infoButton = CreateButton(footerTopObject.transform, OnSwitcherToggle);
                infoButton.GetComponent<LayoutElement>().minWidth = 106;
                infoButton.GetComponent<LayoutElement>().preferredHeight = 24;
                infoButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Show part info");

                objectsCreated = true;
            }

            // Destroy module/ressource widgets
            PartListTooltipWidget[] extWidgets = extWidgetsContent.GetComponentsInChildren<PartListTooltipWidget>(true);
            for (int i = 0; i < extWidgets.Count(); i++)
            {
                extWidgets[i].gameObject.DestroyGameObject();
            }

            // Destroy switcher widgets
            PartListTooltipWidget[] switchWidgets = descriptionContent.GetComponentsInChildren<PartListTooltipWidget>(true);
            for (int i = 0; i < switchWidgets.Count(); i++)
            {
                switchWidgets[i].gameObject.DestroyGameObject();
            }

            // TODO: revert scrollbars to top

            bool hasSwitcher = false;

            // Create partmodule and switcher widgets
            foreach (PartModule pm in selectedPart.Modules)
            {
                if (!pm.enabled || !pm.isEnabled) continue;

                AvailablePart.ModuleInfo pmInfo = GetModuleInfo(pm);

                if (pm is ModuleB9PartSwitch || pm is ModulePartVariants)
                {
                    SetupSwitcherWidget(pm, pmInfo, descriptionContent.transform, infoToolTipInstance.extInfoVariantsWidgePrefab);
                    hasSwitcher = true;
                    continue;
                }

                if (string.IsNullOrEmpty(pmInfo.info)) continue;

                PartListTooltipWidget widget = Instantiate(infoToolTipInstance.extInfoModuleWidgetPrefab);
                widget.Setup(pmInfo.moduleDisplayName, pmInfo.info);
                widget.transform.SetParent(infoToolTipInstance.extInfoListContainer, false);
            }

            // Move or hide module spacer
            if (infoToolTipInstance.extInfoListContainer.childCount > 2 && selectedPart.Resources.Count > 0)
            {
                infoToolTipInstance.extInfoListSpacer.gameObject.SetActive(true);
                infoToolTipInstance.extInfoListSpacer.SetSiblingIndex(infoToolTipInstance.extInfoListContainer.childCount - 1);
            }
            else
            {
                infoToolTipInstance.extInfoListSpacer.gameObject.SetActive(false);
            }

            // Add ressource widgets
            foreach (PartResource pr in selectedPart.Resources)
            {
                AvailablePart.ResourceInfo prInfo = GetResourceInfo(pr);

                if (string.IsNullOrEmpty(prInfo.info)) continue;
                //if (RemoveControlCharacters(pm.GetInfo()).Equals("")) continue;

                PartListTooltipWidget widget = Instantiate(infoToolTipInstance.extInfoRscWidgePrefab);
                widget.Setup(prInfo.displayName, prInfo.info);
                widget.transform.SetParent(infoToolTipInstance.extInfoListContainer, false);
            }

            // Setup info
            infoToolTipInstance.textName.text = selectedPart.partInfo.title;
            infoToolTipInstance.textManufacturer.text = selectedPart.partInfo.manufacturer;
            infoToolTipInstance.textDescription.text = selectedPart.partInfo.description;
            infoToolTipInstance.textInfoBasic.text = GetPartInfo(selectedPart);
            infoToolTipInstance.textCost.text = 
                Localizer.Format("#autoLOC_456128", new string[]
                {
                    "<sprite=\"CurrencySpriteAsset\" name=\"Funds\" tint=1>",
                    (
                        selectedPart.partInfo.cost 
                        + selectedPart.GetModuleCosts(0.0f, ModifierStagingSituation.CURRENT) //TODO: this is wrong if ressource amount != maxamount
                    ).ToString("N2")
                });

            infoToolTipInstance.panelExtended.SetActive(infoToolTipInstance.extInfoListContainer.GetComponentsInChildren<PartListTooltipWidget>(true).Count() > 0);

            // Reposition tooltip if new part selected
            if (getHoveredpart)
            {
                UIMasterController.RepositionTooltip((RectTransform)infoToolTip.transform, Vector2.one, 8f);
            }

            // Update switcher button visibility
            infoButton.SetActive(hasSwitcher);
            ShowSwitcherWidgets(hasSwitcher);
        }

        private void ShowSwitcherWidgets(bool visible)
        {
            infoButton.GetComponentInChildren<TextMeshProUGUI>().SetText(visible ? "Show part info" : "Configure part");
            descriptionText.SetActive(!visible);
            manufacturerPanel.SetActive(!visible);

            // show/hide switcher widgets
            PartListTooltipWidget[] widgets = descriptionContent.GetComponentsInChildren<PartListTooltipWidget>(true);
            for (int i = 0; i < widgets.Count(); i++)
            {
                widgets[i].gameObject.SetActive(visible);
            }

            RectOffset padding = descriptionContent.GetComponent<VerticalLayoutGroup>().padding;
            if (visible)
            {
                padding.left = 1;
                padding.right = 1;
                padding.top = 3;
                padding.bottom = 0;
            }
            else
            {
                padding.left = 6;
                padding.right = 6;
                padding.top = 6;
                padding.bottom = 6;
            }

            descriptionTopObject.GetComponent<LayoutElement>().preferredHeight = visible ? 128 + 64 : 128;
            primaryInfoTopObject.GetComponent<LayoutElement>().preferredHeight = visible ? 128 - 32 : 128;
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)descriptionTopObject.transform);
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)primaryInfoTopObject.transform);
        }

        private void OnSwitcherToggle()
        {
            ShowSwitcherWidgets(descriptionText.activeInHierarchy);
        }

        private void SetupSwitcherWidget(PartModule pm, AvailablePart.ModuleInfo pmInfo, Transform parent, PartListTooltipWidget extInfoVariantsWidgePrefab)
        {
            string title = "";
            string info = "";
            if (pm is ModuleB9PartSwitch)
            {
                GetB9Info((ModuleB9PartSwitch)pm, out title, out info);
            }
            else if (pm is ModulePartVariants)
            {
                GetVariantsInfo((ModulePartVariants)pm, out title, out info);
            }

            PartListTooltipWidget widget = Instantiate(extInfoVariantsWidgePrefab);
            widget.Setup(title, info);
            widget.transform.SetParent(parent, false);

            // Create a button
            Button button = widget.gameObject.AddComponent<Button>();
            button.enabled = true;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color32(242, 213, 50, 100);
            button.colors = colors;
            button.onClick.AddListener(OnSwitcherWidgetClick);

            ButtonWidget handler = widget.gameObject.AddComponent<ButtonWidget>();
            handler.switcherModule = pm;
            handler.widget = widget;
        }

        private void GetVariantsInfo(ModulePartVariants mv, out string title, out string info)
        {
            title = mv.GetModuleDisplayName();
            info = string.Empty;
            for (int i = 0; i < mv.variantList.Count; i++)
            {
                if (mv.SelectedVariant == mv.variantList[i])
                {
                    info += $"<b><color=#ffd200>> {Localizer.Format(mv.variantList[i].DisplayName)}</color></b>\n";
                }
                else
                {
                    info += $"- {Localizer.Format(mv.variantList[i].DisplayName)}\n";
                }
            }
        }

        private void GetB9Info(ModuleB9PartSwitch b9, out string title, out string info)
        {
            title = b9.switcherDescription + " (" + b9.subtypes.Count + " subtypes)";
            info = "";
            for (int i = 0; i < b9.subtypes.Count; i++)
            {
                if (b9.CurrentSubtype == b9.subtypes[i])
                {
                    info += $"<b><color=#ffd200>> {b9.subtypes[i].title}</color></b>";
                }
                else
                {
                    info += $"- {b9.subtypes[i].title}";
                }

                ModuleB9PartSwitch parent = b9.part.Modules.OfType<ModuleB9PartSwitch>().FirstOrDefault(module => module.moduleID == b9.parentID);
                if (b9.parentID != "" && parent != null && parent.CurrentSubtype.HasTank && parent.baseVolume != 0)
                {
                    info += $" - Volume : {parent.baseVolume + b9.subtypes[i].volumeAddedToParent}";
                }

                if (b9.subtypes[i].HasTank && b9.part.Modules.OfType<ModuleB9PartSwitch>().FirstOrDefault(module => module.parentID == b9.moduleID) == null)
                {
                    info += $" - Volume : {b9.subtypes[i].TotalVolume}";
                }

                foreach (var resource in b9.subtypes[i].tankType)
                {
                    info += $"\n  <color=#99ff00ff>- {resource.resourceDefinition.displayName}</color> : {resource.unitsPerVolume * b9.subtypes[i].TotalVolume:F1}";
                }
                if (i != b9.subtypes.Count) info += "\n"; ;
            }
        }

        private void OnSwitcherWidgetClick()
        {
            ButtonWidget handler = EventSystem.current.currentSelectedGameObject.GetComponent<ButtonWidget>();
            if (handler.switcherModule is ModuleB9PartSwitch)
            {
                ModuleB9PartSwitch b9Switch = (ModuleB9PartSwitch)handler.switcherModule;
                int nextSubtype = b9Switch.currentSubtypeIndex == b9Switch.SubtypesCount - 1 ? 0 : b9Switch.currentSubtypeIndex + 1;
                b9Switch.SwitchSubtype(b9Switch.subtypes[nextSubtype].subtypeName);
                delayedTooltipUpdate = 1;
            }
            else if (handler.switcherModule is ModulePartVariants)
            {
                ModulePartVariants pmv = (ModulePartVariants)handler.switcherModule;
                int currentVariantIndex = pmv.variantList.IndexOf(pmv.SelectedVariant);
                int nextvariant = currentVariantIndex == pmv.variantList.Count - 1 ? 0 : currentVariantIndex + 1;
                string nextVariantName = pmv.variantList[nextvariant].Name;
                pmv.SetVariant(nextVariantName);
                for (int i = 0; i < pmv.part.symmetryCounterparts.Count; i++)
                {
                    PartModule pmSym = pmv.part.symmetryCounterparts[i].Modules[pmv.part.Modules.IndexOf(pmv)];
                    if (pmSym != null && pmSym is ModulePartVariants)
                    {
                        ((ModulePartVariants)pmSym).SetVariant(nextVariantName);
                    }
                }
                
                UpdateInfoToolTip(false);
            }
        }

        private Part getHoveredPart()
        {
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
            {
                return rayHit.transform.GetComponent<Part>();
            }
            else
            {
                return EditorLogic.fetch.ship.parts.Find(p => p.HighlightActive) ?? EditorLogic.SelectedPart;
            }
        }

        public string RemoveControlCharacters(string input)
        {
            return
                input.Where(character => !char.IsControl(character))
                .Aggregate(new StringBuilder(), (builder, character) => builder.Append(character))
                .ToString();
        }

        private string GetPartInfo(Part part)
        {
            // Rebuilding the global part stats string :
            string basicInfo = "<color=#acfffc>";

            // Basic part stats in blue (note : added dry mass)
            // TODO: Use stringbuilders
            basicInfo += (part.GetResourceMass() < Single.Epsilon) ?
              "<b>Mass: </b>" + part.mass.ToString("0.###") + "t\n" :
              "<b>Mass: </b>" + (part.mass + part.GetResourceMass()).ToString("0.###") + "t (<b>Dry mass: </b>" + part.mass.ToString("0.###") + "t)\n";
            basicInfo +=
              "<b>Tolerance: </b>" + part.crashTolerance.ToString("G") + " m/s impact\n" +
              "<b>Tolerance: </b>" + part.gTolerance.ToString("G") + " G, " + part.maxPressure.ToString("G") + " kPA Pressure\n" +
              "<b>Max. Temp. Int / Skin: </b>" + part.maxTemp.ToString("G") + " / " + part.skinMaxTemp.ToString("G") + " K\n";

            if (part.CrewCapacity > 0) { basicInfo += "<b>Crew capacity: </b>" + part.CrewCapacity + "\n"; }
            basicInfo += "</color>";

            // Crossfeed info in red
            ModuleToggleCrossfeed mtc = part.Modules.GetModule<ModuleToggleCrossfeed>();
            if (mtc != null)
            {
                basicInfo += "<color=#f3a413>";
                if (mtc.toggleEditor && mtc.toggleFlight) { basicInfo += "Crossfeed toggles in Editor and Flight\n"; }
                else if (mtc.toggleEditor) { basicInfo += "Crossfeed toggles in Editor\n"; }
                else if (mtc.toggleFlight) { basicInfo += "Crossfeed toggles in Flight\n"; }
                basicInfo += mtc.crossfeedStatus ? "Default On\n" : "Default Off\n";
                basicInfo += "</color>";
            }
            else if (!part.fuelCrossFeed)
            {
                basicInfo += "<color=#f3a413>No fuel crossfeed</color>\n";
            }

            // Module/resource info is in light green
            basicInfo += "\n<color=#b4d455>";

            // Info from modules (note : revamped engine test) :
            string moduleInfo = "";
            if (part.Modules.GetModule<MultiModeEngine>() != null)
            {
                moduleInfo += "<b>Multi-mode engine:</b>\n";
            }
            foreach (PartModule pm in part.Modules)
            {
                if (pm is ModuleEngines)
                {
                    ModuleEngines me = (ModuleEngines)pm;
                    moduleInfo += "<b>";
                    moduleInfo += (me.engineType != EngineType.Generic) ? me.engineType.ToString() + " engine" : "Engine";
                    moduleInfo += (part.Modules.GetModule<MultiModeEngine>() != null) ? " (" + me.engineID + "):</b>\n" : ":</b>\n";

                    if (me.engineType == EngineType.Turbine)
                    {
                        moduleInfo += "<b>Stationary Thrust:</b> " + me.maxThrust.ToString("F1") + "kN\n";
                    }
                    else
                    {
                        float ispVAC = me.atmosphereCurve.Evaluate(0.0f);
                        float ispASL = me.atmosphereCurve.Evaluate(1.0f);
                        float thrustASL = me.maxThrust * (ispASL / ispVAC);
                        moduleInfo +=
                          "<b>Thrust:</b> " + me.maxThrust.ToString("F1") + " kN, <b>ISP:</b> " + ispVAC.ToString("F0") + " (Vac.)\n" +
                          "<b>Thrust:</b> " + thrustASL.ToString("F1") + " kN, <b>ISP:</b> " + ispASL.ToString("F0") + " (ASL)\n";
                    }
                }
                if (pm is ModuleB9PartInfo || pm is ModuleB9PartSwitch || pm is ModulePartVariants)
                {
                    continue;
                }
                else
                {
                    IModuleInfo info = pm as IModuleInfo;
                    if (info != null && !string.IsNullOrEmpty(info.GetPrimaryField()))
                    {
                        moduleInfo += info.GetPrimaryField() + "\n" ;
                    }
                }
            }

            if (moduleInfo != "")
            {
                basicInfo += moduleInfo;
            }

            // Resource list in green (note : GetInfo() doesn't have the same format as stock)
            foreach (PartResource pr in part.Resources)
            {
                basicInfo += "<b>" + pr.info.displayName + ": </b>" + pr.maxAmount.ToString("F1") + "\n";
            }

            basicInfo += "</color>";
            return basicInfo;
        }

        // See CompilePartInfo
        public static AvailablePart.ModuleInfo GetModuleInfo(PartModule pm)
        {
            AvailablePart.ModuleInfo moduleInfo = new AvailablePart.ModuleInfo();
            if (pm is IModuleInfo)
            {
                IModuleInfo iModuleInfo = pm as IModuleInfo;
                moduleInfo.moduleName = iModuleInfo.GetModuleTitle();
                moduleInfo.info = iModuleInfo.GetInfo().Trim();
                moduleInfo.primaryInfo = iModuleInfo.GetPrimaryField();
            }
            else
            {
                moduleInfo.moduleName = (pm.GUIName ?? KSPUtil.PrintModuleName(pm.moduleName));
                moduleInfo.info = pm.GetInfo().Trim();
            }

            if (pm.showUpgradesInModuleInfo && pm.HasUpgrades())
            {
                moduleInfo.info += "\n" + pm.PrintUpgrades();
            }

            moduleInfo.moduleDisplayName = pm.GetModuleDisplayName();
            if (moduleInfo.moduleDisplayName == string.Empty)
            {
                moduleInfo.moduleDisplayName = moduleInfo.moduleName;
            }
            return moduleInfo;
        }


        // See CompilePartInfo
        public static AvailablePart.ResourceInfo GetResourceInfo(PartResource pr)
        {
            AvailablePart.ResourceInfo resourceInfo = new AvailablePart.ResourceInfo();
            resourceInfo.resourceName = pr.resourceName;
            resourceInfo.displayName = pr.info.displayName.LocalizeRemoveGender();
            StringBuilder infoString = new StringBuilder();
            infoString.Append(Localizer.Format("#autoLOC_166269", pr.amount.ToString("F1")));
            infoString.Append((pr.amount == pr.maxAmount) ? string.Empty : (" " + Localizer.Format("#autoLOC_6004042", pr.maxAmount.ToString("F1"))));
            infoString.Append(Localizer.Format("#autoLOC_166270", (pr.amount * pr.info.density).ToString("F2")));
            infoString.Append((pr.info.unitCost <= 0f) ? string.Empty : Localizer.Format("#autoLOC_166271", (pr.amount * (double)pr.info.unitCost).ToString("F2")));
            resourceInfo.info = infoString.ToStringAndRelease();
            if (pr.maxAmount > 0.0)
            {
                resourceInfo.primaryInfo = "<b>" + resourceInfo.displayName + ": </b>" + KSPUtil.LocalizeNumber(pr.maxAmount, "F1");
            }
            return resourceInfo;
        }

        // DialogGUIButton
        public static GameObject CreateButton(Transform parent, Callback onClick, UISkinDef skin = null)
        {
            if (skin == null) skin = HighLogic.UISkin;

            GameObject gameObject = Instantiate(UISkinManager.GetPrefab("UIButtonPrefab"));
            Button button = gameObject.GetComponent<Button>();
            gameObject.transform.SetParent(parent, false);
            gameObject.SetActive(true);
            // base.SetupTransformAndLayout(); --> add layoutelement and configure, do we need it ?

            gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            UIStyle uIStyle = skin.button;
            if (uIStyle.normal.background != null)
            {
                button.spriteState = new SpriteState
                {
                    disabledSprite = uIStyle.disabled.background,
                    highlightedSprite = uIStyle.highlight.background,
                    pressedSprite = uIStyle.active.background
                };
            }
            else
            {
                button.transition = Selectable.Transition.ColorTint;
            }
            button.onClick.AddListener(delegate { onClick(); });
            return gameObject;
        }

    }
}
