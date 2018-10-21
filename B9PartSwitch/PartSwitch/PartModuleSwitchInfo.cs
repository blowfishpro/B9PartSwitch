using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.Extensions;

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

        private ConfigNode moduleInfo;
        private ConfigNode modifiedModuleInfo;
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
            if (!(HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)) return;

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

                // Get confignode and apply module specific support for disabling
                ConfigNode node = enable ? modifiedModuleInfo : moduleInfo;
                ApplyEarlyDisableSpecifics(node, modules[j]);

                // Call PartModule.Awake(), this will call PartModule.OnAwake()
                modules[j].Awake();

                // Ensure module is enabled
                modules[j].enabled = true;
                modules[j].isEnabled = true;
                modules[j].moduleIsEnabled = true;

                // Call PartModule.Load(), this will call PartModule.OnLoad()
                modules[j].Load(node);

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

                    // Update staging icon visibility
                    if (modules[j].IsStageable())
                    {
                        modules[j].stagingEnabled = !enable;
                        modules[j].part.UpdateStageability(false, true);
                    }

                    // Enable/disable the unity component
                    modules[j].enabled = !enable;
                    // Enable/disable the KSP PartModule 
                    modules[j].isEnabled = !enable;
                    modules[j].moduleIsEnabled = !enable;
                }
            }
        }

        // Specific support for some stock modules, mainly used to reset state when disabling modules.
        private ConfigNode ApplyEarlyDisableSpecifics(ConfigNode configNode, PartModule module)
        {
            if (!disableModule)
            {
                return configNode;
            }
            else if (module is ModuleColorChanger)
            {
                configNode.SetValue("animState", false, true);
            }
            else if (module is ModuleLight)
            {
                // Doesn't work in flight
                configNode.SetValue("IsOn", false, true);
                ((ModuleLight)module).SetLightState(false);
            }
            else if (module is ModuleAnimateGeneric)
            {
                configNode.SetValue("aniState", "LOCKED", true);
                configNode.SetValue("animTime", 0f, true);
            }
            else if (module is ModuleAnimationGroup)
            {
                configNode.SetValue("isDeployed", false, true);
            }
            else if (module is ModuleDeployablePart)
            {
                // This one is tricky because the animation is private and is handled trough FixedUpdate
                configNode.SetValue("storedAnimationTime", 0f, true);
                configNode.SetValue("deployState", "RETRACTED", true);
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
            return configNode;
        }

        public void SetupModuleNode(PartSubtype subtype, ModuleB9PartSwitch parent)
        {

            if (!(HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight || moduleInfo != null)) return;

            moduleInfo = FindModuleConfigNode(parent.part).CreateCopy();
            moduleInfo.SetValue("isEnabled", true, true);
            modifiedModuleInfo = moduleInfo.CreateCopy();
            modules = FindModule(parent.part);
            moduleType = modules[0].GetType();

            ConfigNode node = parent.part.partInfo.partConfig
                .GetNode("MODULE", "moduleID", parent.moduleID)
                .GetNode("SUBTYPE", "name", subtype.Name)
                .GetNode("MODULESWITCH", "name", name);

            ConfigNode modifiersNode = new ConfigNode();
            if (node.TryGetNode("MODIFIERS", ref modifiersNode))
            {
                modifiersNode.CopyToAsModifier(modifiedModuleInfo);
            }
        }

        public ConfigNode FindModuleConfigNode(Part part)
        {
            Part prefab = part.GetPrefab();
            // for each module in the part
            for (int i = 0; i < prefab.Modules.Count; i++)
            {
                // if the module type match
                if (prefab.Modules[i].moduleName == name)
                {
                    int index = 0;
                    // we are asked to find all modules of this type
                    if (findAll)
                    {
                        return prefab.partInfo.partConfig.GetNode("MODULE", "name", name);
                    }

                    // else we try to get the correct module
                    // try to use the field identifier
                    if (fieldIdentifier.Length != 0)
                    {
                        return prefab.partInfo.partConfig.GetNode("MODULE", fieldIdentifier, valueIdentifier);
                    }
                    else
                    {
                        ConfigNode[] moduleNodes = prefab.partInfo.partConfig.GetNodes("MODULE", "name", name);
                        // try to use the module index (index derived from modules of the "moduleName" type only)
                        if (moduleIndex > -1)
                        {
                            for (int j = 0; j < moduleNodes.Count(); j++)
                            {
                                if (moduleIndex == j)
                                {
                                    return moduleNodes[j];
                                }
                            }
                        }
                        // else return the first found module
                        else if (moduleNodes.Count() > 0)
                        {
                            return moduleNodes[0];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    index++;
                }
            }
            return null;
        }

        public List<PartModule> FindModule(Part part)
        {
            List<PartModule> modules = new List<PartModule>();
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
                        // get identifier
                        string id = "";
                        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
                        FieldInfo field = part.Modules[i].GetType().GetField(fieldIdentifier, flags);
                        field.SetValue(part.Modules[i], id); // TODO: check what happens with invalid field name, maybe this should be in a try-catch ?

                        // if the identifier value match
                        if (id == valueIdentifier)
                        {
                            // found it
                            modules.Add(part.Modules[i]);
                            return modules;
                        }
                    }
                    // try to use the module index (index derived from modules of the "moduleName" type only)
                    else if (moduleIndex > -1)
                    {
                        if (index == moduleIndex)
                        {
                            modules.Add(part.Modules[i]);
                            return modules;
                        }
                    }
                    // else return the first found module
                    else
                    {
                        modules.Add(part.Modules[i]);
                        return modules;
                    }
                    index++;
                }
            }
            // either no module was found, or findAll == true and all modules of the "moduleName" type are returned
            return modules;
        }
    }
}
