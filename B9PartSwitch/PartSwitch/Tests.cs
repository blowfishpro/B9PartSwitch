using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.UI.Screens;
using UnityEngine;
using B9PartSwitch.Fishbones;
using System.Reflection;

namespace B9PartSwitch.PartSwitch
{
    //class VeselModulePMManager : VesselModule
    //{
    //    int update = 1;

    //    protected override void OnStart()
    //    {
    //        Debug.Log("[TEST] VESSELMODULE : OnStart");
    //    }

    //    public override void OnLoadVessel()
    //    {
    //        Debug.Log("[TEST] VESSELMODULE : OnLoadVessel");
    //    }
        
    //    public void FixedUpdate()
    //    {
    //        if (update <= 3)
    //        {
    //            Debug.Log("[TEST] VESSELMODULE : FixedUpdate N° : " + update);
    //            update++;
    //        }
    //    }
    //}


        class PartModuleInitTest : PartModule
    {
        [KSPField]
        public bool test = false;

        private bool dummy = false;

        private int count = 60;
        private int uc = 1;
        private int fuc = 1;
        private int luc = 1;

        public void Start()
        {
            Debug.Log("[TEST] TestModule Start");
            //Debug.Log("[TEST] Partmodule Start : " + (vessel == null ? "null vessel" : vessel.vesselName) + "- BOOL = " + test);
        }

        public new void Awake()
        {
            base.Awake();
            Debug.Log("[TEST] TestModule Awake");
        }

        public override void OnInitialize()
        {
            Debug.Log("[TEST] TestModule OnInitialize");
        }

        public override void OnActive()
        {
            Debug.Log("[TEST] TestModule OnActive");
        }

        public override void OnStart(StartState state)
        {
            Debug.Log("[TEST] TestModule OnStart");
            //Debug.Log("[TEST] Partmodule OnStart : " + (vessel == null ? "null vessel" : vessel.vesselName) + "- BOOL = " + test);
        }

        public override void OnStartFinished(StartState state)
        {
            Debug.Log("[TEST] TestModule OnStartFinished");
            //Debug.Log("[TEST] Partmodule OnStart : " + (vessel == null ? "null vessel" : vessel.vesselName) + "- BOOL = " + test);
        }

        public override void OnAwake()
        {
            base.OnAwake();
            Debug.Log("[TEST] TestModule OnAwake");
            //Debug.Log("[TEST] Partmodule OnAwake : " + (vessel == null ? "null vessel" : vessel.vesselName) + "- BOOL = " + test);
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("[TEST] TestModule OnLoad");
            //Debug.Log("[TEST] Partmodule OnLoad : " + (vessel == null ? "null vessel" : vessel.vesselName) + "- BOOL = " + test);
        }

        public new void Load(ConfigNode node)
        {
            base.Load(node);
            Debug.Log("[TEST] TestModule Load");
        }

        public void FixedUpdate()
        {
            dummy = !dummy;
            if (fuc < 3)
            {
                Debug.Log("[TEST] TestModule FixedUpdate N°" + fuc);
                fuc++;
            }
        }

        public void Update()
        {
            if (uc < 3)
            {
                Debug.Log("[TEST] TestModule Update N°" + uc);
                uc++;
            }

            count--;
            if (count < 0)
            {
                count = 60;
                ScreenMessages.PostScreenMessage("MODULE MODIFICATION APPLIED : " + test, 1f, ScreenMessageStyle.UPPER_CENTER);
            } 
        }
        public void LateUpdate()
        {
            if (luc < 3)
            {
                Debug.Log("[TEST] TestModule LateUpdate N°" + luc);
                luc++;
            }
        }

    }

    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    class GlobalTests : MonoBehaviour
    {

        private bool firstFixedUpdate = true;

        private void Awake()
        {
            Debug.Log("[TEST] KSPAddon Awake");
            GameEvents.onVesselLoaded.Add(onVesselLoaded);
            GameEvents.onEditorPartEvent.Add(onEditorPartEvent);
            GameEvents.onEditorLoad.Add(onEditorLoad);
            GameEvents.onEditorUndo.Add(onEditorUndo);
            GameEvents.onEditorRedo.Add(onEditorRedo);
        }

        public void OnDisable()
        {
            GameEvents.onVesselLoaded.Remove(onVesselLoaded);
            GameEvents.onEditorPartEvent.Remove(onEditorPartEvent);
            GameEvents.onEditorLoad.Remove(onEditorLoad);
            GameEvents.onEditorUndo.Remove(onEditorUndo);
            GameEvents.onEditorRedo.Remove(onEditorRedo);
        }

        private void Start()
        {
            Debug.Log("[TEST] KSPAddon Start");
        }

        public void FixedUpdate()
        {
            if (firstFixedUpdate)
            {
                firstFixedUpdate = false;
                Debug.Log("[TEST] KSPAddon First FixedUpdate");
            }
            
        }

        private void ApplyModuleModifiers(ModuleB9PartSwitch switcher)
        {
            for (int i = 0; i < switcher.CurrentSubtype.moduleSwitches.Count; i++)
            {

                //switcher.CurrentSubtype.moduleSwitches[i].ApplyModuleModifiers(switcher.part);

                //if (!switcher.CurrentSubtype.moduleSwitches[i].disableModule)
                //{
                //    switcher.CurrentSubtype.moduleSwitches[i].ApplyModuleModifiers(switcher.part);
                //}
            }
        }



        private void EditorApplyModuleEdits(ShipConstruct ship)
        {
            for (int i = 0; i < ship.Parts.Count; i++)
            {
                for (int j = 0; j < ship.Parts[i].Modules.Count; j++)
                {
                    if (ship.Parts[i].Modules[j] is ModuleB9PartSwitch)
                    {
                        ApplyModuleModifiers((ModuleB9PartSwitch)ship.Parts[i].Modules[j]);

                        //((PartModuleInitTest)ship.Parts[i].Modules[j]).test = true;
                    }
                }
            }
        }

        private void onEditorLoad(ShipConstruct data, CraftBrowserDialog.LoadType data1) { EditorApplyModuleEdits(data); }
        private void onEditorRedo(ShipConstruct data) { EditorApplyModuleEdits(data); }
        private void onEditorUndo(ShipConstruct data) { EditorApplyModuleEdits(data); }

        private void onEditorPartEvent(ConstructionEventType data0, Part data1)
        {
            if (data0 == ConstructionEventType.PartCreated || data0 == ConstructionEventType.PartCopied)
            {
                Debug.Log("[TEST] KSPAddon onEditorPartEvent");
                for (int j = 0; j < data1.Modules.Count; j++)
                {
                    if (data1.Modules[j] is ModuleB9PartSwitch)
                    {
                        ApplyModuleModifiers((ModuleB9PartSwitch)data1.Modules[j]);
                        //((PartModuleInitTest)data1.Modules[j]).test = true;
                    }
                }
            }
        }

        private void onVesselLoaded(Vessel data)
        {
            Debug.Log("[TEST] KSPAddon onVesselLoaded : " + data.vesselName);

            for (int i = 0; i < data.Parts.Count; i++)
            {
                for (int j = 0; j < data.Parts[i].Modules.Count; j++)
                {
                    if (data.Parts[i].Modules[j] is ModuleB9PartSwitch)
                    {
                        ApplyModuleModifiers((ModuleB9PartSwitch)data.Parts[i].Modules[j]);
                    }
                }
            }
        }


    }
}
