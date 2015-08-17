using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace B9PartSwitch
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class MiniMFTSettings : MonoBehaviour
    {
        private Dictionary<string, TankType> tankTypes = new Dictionary<string,TankType>();

        public static MiniMFTSettings Instance { get; private set; }

        public static bool LoadedTankDefs { get; private set; }

        private static TankType structuralTankType;

        public static TankType StructuralTankType
        {
            get
            {
                if (!LoadedTankDefs)
                    throw new InvalidOperationException("The tank definitions have not been loaded yet (done after game database load)");
                return structuralTankType;
            }
        }

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                Debug.LogWarning("Cannot create more than one MiniMFTSettings instance");
                return;
            }

            Instance = this;

            CFGUtil.RegisterParseType<PartResourceDefinition>(FindResourceDefinition, x => x.name);
            CFGUtil.RegisterParseType<TankType>(MiniMFTSettings.Instance.GetTankType, x => x.tankName);

            DontDestroyOnLoad(gameObject);
            LoadedTankDefs = false;

            // Structural tank type is hard coded
            structuralTankType = gameObject.AddComponent<TankType>();
            structuralTankType.tankName = "Structural";
            structuralTankType.tankMass = 0f;
            structuralTankType.tankCost = 0f;
        }

        public void ModuleManagerPostLoad()
        {
            ReloadTankDefs();
        }

        public void ReloadTankDefs()
        {
            foreach (KeyValuePair<string,TankType> pair in tankTypes)
            {
                if (pair.Value != structuralTankType)
                    Destroy(pair.Value);
            }
            tankTypes.Clear();

            // Structural tank type is hard coded
            tankTypes.Add(structuralTankType.tankName, structuralTankType);

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("MINIMFT_TANK_TYPE"))
            {
                TankType t = gameObject.AddComponent<TankType>();
                t.Load(node);
                if (tankTypes.ContainsKey(t.tankName))
                {
                    Debug.LogError("The tank type " + t.tankName + " already exists");
                    Destroy(t);
                    continue;
                }
                tankTypes.Add(t.tankName, t);
                Debug.Log("MiniMFTSettings: registered tank type " + t.tankName);
            }

            LoadedTankDefs = true;
        }

        public TankType GetTankType(string name)
        {
            if (!LoadedTankDefs)
                throw new InvalidOperationException("The tank definitions have not been loaded yet (done after game database load)");
            if (string.IsNullOrEmpty(name))
                return StructuralTankType;
            return tankTypes[name];
        }

        public bool TankTypeExists(string name)
        {
            if (!LoadedTankDefs)
                throw new InvalidOperationException("The tank definitions have not been loaded yet (done after game database load)");
            return tankTypes.ContainsKey(name);
        }

        // This will raise an exception when the resource is not found
        public static PartResourceDefinition FindResourceDefinition(string name)
        {
            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(name);
            if (resource == null)
                throw new KeyNotFoundException("No resource with the name " + name + " could be found");
            return resource;
        }
    }
}
