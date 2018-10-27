using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch
{
    public class PartModuleSwitchInfo : IContextualNode
    {
        [NodeData(name = "name")]
        public string name;

        [NodeData(name = "disableModule")]
        public bool disableModule = false;

        [NodeData(name = "findAll")]
        public bool findAll = false;

        [NodeData(name = "moduleIndex")]
        public int moduleIndex = -1;

        [NodeData(name = "fieldIdentifier")]
        public string fieldIdentifier = "";

        [NodeData(name = "valueIdentifier")]
        public string valueIdentifier = "";

        private ConfigNode moduleNode;
        private ConfigNode modifiersNode;
        private List<PartModule> modules;
        private Type moduleType;

        public void Load(ConfigNode node, OperationContext context)
        {
            this.LoadFields(node, context);
        }

        public void Save(ConfigNode node, OperationContext context)
        {
            this.SaveFields(node, context);
        }

        public void Enable(bool enable)
        {
            if (moduleNode == null || modules == null || moduleType == null) return;

            // Get PartModule constructor
            ConstructorInfo ctor = moduleType.GetConstructor(new Type[] { });
            // Get PartModule.Start()
            MethodInfo start = moduleType.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            // Get PartModule.FixedUpdate()
            MethodInfo fixedUpdate = moduleType.GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            // Get PartModule.Update()
            MethodInfo update = moduleType.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            // Get PartModule.LateUpdate()
            MethodInfo lateUpdate = moduleType.GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int j = 0; j < modules.Count; j++)
                {
                // Call the module constructor, this helps in resetting private fields
                ctor.Invoke(modules[j], null);
                // Call PartModule.Awake(), this will call PartModule.OnAwake()
                modules[j].Awake();

                // Ensure module is enabled -> NOT NEEDED ?
                //node.SetValue("IsEnabled", true, true);
                //node.SetValue("moduleIsEnabled", true, true);
                //modules[j].enabled = true;
                //modules[j].isEnabled = true;
                //modules[j].moduleIsEnabled = true;


                ApplyEarlySpecifics(modules[j]);
                // Call PartModule.Load(), this will call PartModule.OnLoad()
                modules[j].Load(moduleNode);
                if (enable && modifiersNode != null)
                {
                    modules[j].Load(modifiersNode);
                }

                // Call OnStart, then start
                // In the editor, OnStartFinished is called in between
                modules[j].OnStart(modules[j].part.GetModuleStartState());
                if (HighLogic.LoadedSceneIsEditor) modules[j].OnStartFinished(modules[j].part.GetModuleStartState());
                if (start != null) start.Invoke(modules[j], null);
                
                // And manually call update methods to make sure that any "first run" code is being called
                if (disableModule)
                {
                    if (enable)
                    {
                        if (fixedUpdate != null) fixedUpdate.Invoke(modules[j], null);
                        if (update != null) update.Invoke(modules[j], null);
                        if (lateUpdate != null) lateUpdate.Invoke(modules[j], null);

                        // Replicate stock behavior
                        if (HighLogic.LoadedSceneIsFlight) modules[j].OnStartFinished(modules[j].part.GetModuleStartState());
                        if (HighLogic.LoadedSceneIsFlight) modules[j].OnInitialize();
                    }
                    // Enable/disable the unity component
                    modules[j].enabled = !enable;
                    // Enable/disable the KSP PartModule 
                    modules[j].isEnabled = !enable;
                    modules[j].moduleIsEnabled = !enable;
                }
                ApplyLateSpecifics(modules[j], enable);
            }
        }

        public void SetupSwitcher(string moduleID, string subtypeName, Part part)
        {
            if (!(HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) || moduleNode != null) return;

            if (!TryFindModule(part, out modules, out moduleNode))
            {
                modules = null;
                moduleNode = null;
                return;
            }

            moduleType = modules[0].GetType();

            modifiersNode = part.partInfo.partConfig
                .GetNode("MODULE", "moduleID", moduleID)
                .GetNode("SUBTYPE", "name", subtypeName)
                .GetNode("MODULESWITCH", "name", name)
                .GetNode("MODIFIERS");
        }

        public bool TryFindModule(Part part, out List<PartModule> modules, out ConfigNode moduleNode)
        {
            modules = new List<PartModule>();
            int index = 0;
            // for each module in the part
            for (int i = 0; i < part.Modules.Count; i++)
            {
                // if the module type match
                if (part.Modules[i].moduleName == name)
                {
                    // we are asked to find all modules of this type
                    if (findAll)
                    {
                        modules.Add(part.Modules[i]);
                        continue;
                    }

                    // else we try to get the correct module
                    // try to use the field identifier
                    if (fieldIdentifier.Length != 0)
                    {
                        string moduleValue = "";
                        ConfigNode prefabModuleNode;
                        if (modules[i].TryGetConfigNode(out prefabModuleNode))
                        {
                            if (prefabModuleNode.TryGetValue(fieldIdentifier, ref moduleValue) && moduleValue == valueIdentifier)
                            {
                                modules.Add(part.Modules[i]);
                                break;
                            }
                        }

                    }
                    // try to use the module index (index derived from modules of the "moduleName" type only)
                    else if (moduleIndex > -1)
                    {
                        if (index == moduleIndex)
                        {
                            modules.Add(part.Modules[i]);
                            break;
                        }
                    }
                    // else return the first found module
                    else
                    {
                        modules.Add(part.Modules[i]);
                        break;
                    }
                    index++;
                }
            }

            if (modules.Count > 0)
            {
                ConfigNode prefabModuleNode;
                if (modules[0].TryGetConfigNode(out prefabModuleNode))
                {
                    if (prefabModuleNode.CountNodes > 0 || prefabModuleNode.CountValues > 0)
                    {
                        moduleNode = prefabModuleNode.CreateCopy();
                        return true;
                    }
                }
            }

            moduleNode = null;
            if (fieldIdentifier.Length != 0) Debug.LogError($"Cannot find a PartModule of type '{name}' with the field '{fieldIdentifier}' and value '{valueIdentifier}'");
            else if (moduleIndex > -1) Debug.LogError($"Cannot find a PartModule of type '{name}' with index '{moduleIndex}'");
            else Debug.LogError($"Cannot find a PartModule of type '{name}'");
            return false;
        }

        // Specific support for some stock modules, mainly used to reset state when disabling modules.
        private ConfigNode ApplyEarlySpecifics(PartModule module)
        {
            if (module is ModuleEngines || module is ModuleEnginesFX)
            {
                module.part.stackIcon.ClearInfoBoxes();
            }
            if (disableModule)
            {
                if (module is ModuleColorChanger)
                {
                    moduleNode.SetValue("animState", false, true);
                }
                else if (module is ModuleLight)
                {
                    // Doesn't work in flight
                    moduleNode.SetValue("IsOn", false, true);
                    ((ModuleLight)module).SetLightState(false);
                }
                else if (module is ModuleAnimateGeneric)
                {
                    moduleNode.SetValue("aniState", "LOCKED", true);
                    moduleNode.SetValue("animTime", 0f, true);
                }
                else if (module is ModuleAnimationGroup)
                {
                    moduleNode.SetValue("isDeployed", false, true);
                }
                else if (module is ModuleDeployablePart)
                {
                    // This one is tricky because the animation is private and is handled trough FixedUpdate
                    moduleNode.SetValue("storedAnimationTime", 0f, true);
                    moduleNode.SetValue("deployState", "RETRACTED", true);
                    Animation[] anims = module.GetComponentsInChildren<Animation>();
                    string animName = ((ModuleDeployablePart)module).animationName;
                    Animation deployAnim = null;
                    for (int k = 0; k < anims.Count(); k++)
                    {
                        if (anims[k].GetClip(animName) != null) deployAnim = anims[k];
                    }
                    if (deployAnim == null && anims.Count() > 0) deployAnim = anims[0];
                    if (deployAnim != null)
                    {
                        ((ModuleDeployablePart)module).panelRotationTransform.localRotation = ((ModuleDeployablePart)module).originalRotation;
                        deployAnim[animName].normalizedTime = 0;
                        deployAnim.Play(animName);
                        deployAnim.Sample();
                    }
                }
            }
            return moduleNode;
        }

        private void ApplyLateSpecifics(PartModule module, bool enable)
        {
            // Update staging icon visibility
            if (disableModule && module.IsStageable())
            {
                module.stagingEnabled = !enable;
                module.part.UpdateStageability(false, true);
            }

            if (!(disableModule && enable))
            {
                // Unfortunatly ModuleDataTransmitter OnLoad create the reshandler on the prefab 
                // and never again because it checks for part.partInfo == null
                if (module is ModuleDataTransmitter && module.resHandler.inputResources.Count == 0)
                {
                    string value = "ElectricCharge";
                    if (modifiersNode != null && !modifiersNode.TryGetValue("requiredResource", ref value))
                    {
                        moduleNode.TryGetValue("requiredResource", ref value);
                    }
                    ModuleResource moduleResource = new ModuleResource();
                    moduleResource.name = value;
                    moduleResource.title = KSPUtil.PrintModuleName(value);
                    moduleResource.id = value.GetHashCode();
                    moduleResource.rate = 1.0;
                    module.resHandler.inputResources.Add(moduleResource);
                }
                else if (module is ModuleEngines && HighLogic.LoadedSceneIsFlight)
                {
                    FixEnginesFX((ModuleEngines)module);
                }
            }
        }

        private void FixEnginesFX(ModuleEngines engine)
        {
            // Why this is needed :
            // When engines OnStart is called, the module does this :
            // - It find the FX XXX on the part
            // - It duplicate the particule emitter of this FX into a new FX in the engine XXXGroups list, one for each thrusttransform
            // - It remove the particule emitter from the original FX on the part
            // - It copy back the new FX named prefix + XXX + index of transform to the part FX list
            // So when we call Onstart again, the init method can't find the emitters since they are removed from the base FX on the part
            // To fix this we need to manually re-link the engine XXXGroups with the FX that still are on the part, and call the setup method AutoPlaceFXGroup

            // Remove garbage FX on the part created by previous loads
            engine.part.fxGroups.FindAll(p =>
            (p.name.Contains(engine.fxGroupPrefix + "Flameout")
            || p.name.Contains(engine.fxGroupPrefix + "Running")
            || p.name.Contains(engine.fxGroupPrefix + "Power"))
            && !p.isValid).Clear();

            // Reset the module FX groups
            engine.flameoutGroups.Clear(); 
            engine.powerGroups.Clear(); 
            engine.runningGroups.Clear();

            // Relink the part FX to the module FX list
            for (int i = 0; i < engine.thrustTransforms.Count; i++)
            {
                FXGroup flameoutFX = engine.part.fxGroups.Find(p => p.name == engine.fxGroupPrefix + "Flameout" + i && p.isValid);
                if (flameoutFX != null)
                {
                    engine.AutoPlaceFXGroup(flameoutFX, engine.thrustTransforms[i]);
                    engine.flameoutGroups.Add(flameoutFX);
                }

                FXGroup runningFX = engine.part.fxGroups.Find(p => p.name == engine.fxGroupPrefix + "Running" + i && p.isValid);
                if (runningFX != null)
                {
                    engine.AutoPlaceFXGroup(runningFX, engine.thrustTransforms[i]);
                    engine.runningGroups.Add(runningFX);
                }

                FXGroup powerFX = engine.part.fxGroups.Find(p => p.name == engine.fxGroupPrefix + "Power" + i && p.isValid);
                if (powerFX != null)
                {
                    engine.AutoPlaceFXGroup(powerFX, engine.thrustTransforms[i]);
                    engine.powerGroups.Add(powerFX);
                }
            }
        }
    }
}
