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
        private Dictionary<string, TankPartType> tankPartTypes = new Dictionary<string,TankPartType>();

        public static MiniMFTSettings Instance { get; private set; }

        public static bool LoadedTankDefs { get; private set; }
        public static bool LoadedPartDefs { get; private set; }

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                Debug.LogWarning("Cannot create more than one MiniMFTSettings instance");
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadedTankDefs = false;
        }

        public void ModuleManagerPostLoad()
        {
            ReloadTankDefs();
        }

        public void ReloadTankDefs()
        {
            foreach (KeyValuePair<string, TankType> pair in tankTypes)
                DestroyImmediate(pair.Value);

            tankTypes.Clear();
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("MINIMFT_TANK_TYPE"))
            {
                TankType t = gameObject.AddComponent<TankType>();
                t.Load(node);
                tankTypes.Add(t.tankName, t);
                Debug.Log("Loaded TankType " + t.tankName);
                Debug.Log("Is null: " + (tankTypes[t.tankName] == null).ToString());
            }

            LoadedTankDefs = true;

            Debug.Log("MiniMFT: Found " + tankTypes.Count + " tank types");

            foreach (KeyValuePair<string,TankPartType> pair in tankPartTypes)
                DestroyImmediate(pair.Value);

            tankPartTypes.Clear();
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("MINIMFT_TANK_PART_TYPE"))
            {
                TankPartType t = gameObject.AddComponent<TankPartType>();
                t.Load(node);
                tankPartTypes.Add(t.typeName, t);
                Debug.Log("Loaded TankPartType " + t.typeName);
                Debug.Log("Is null: " + (tankPartTypes[t.typeName] == null).ToString());
            }

            Debug.Log("MiniMFT: Found " + tankPartTypes.Count + " tank part types");

            LoadedPartDefs = true;
        }

        public static TankType TankType(string name)
        {
            return Instance.GetTankType(name);
        }

        public static TankPartType TankPartType(string name)
        {
            return Instance.GetTankPartType(name);
        }

        public TankType GetTankType(string name)
        {
            if (!LoadedTankDefs)
                throw new InvalidOperationException("The tank definitions have not been loaded yet (done after game database load)");
            return tankTypes[name];
        }

        public TankPartType GetTankPartType(string name)
        {
            if (!LoadedPartDefs)
                throw new InvalidOperationException("The tank part definitions have not been loaded yet (done after game database load)");
            return tankPartTypes[name];
        }

        static MiniMFTSettings()
        {
            CFGUtil.RegisterParseType<PartResourceDefinition>(FindResourceDefinition, x => x.name);
            CFGUtil.RegisterParseType<TankType>(MiniMFTSettings.TankType, x => x.tankName);
            CFGUtil.RegisterParseType<TankPartType>(MiniMFTSettings.TankPartType, x => x.typeName);
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
