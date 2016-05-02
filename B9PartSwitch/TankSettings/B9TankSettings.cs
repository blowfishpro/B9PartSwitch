using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace B9PartSwitch
{
    public static class B9TankSettings
    {
        private static Dictionary<string, TankType> tankTypes = new Dictionary<string,TankType>();

        public static bool LoadedTankDefs { get; private set; } = false;

        public static TankType StructuralTankType
        {
            get
            {
                var t = new TankType();
                t.tankName = "Structural";
                t.tankMass = 0f;
                t.tankCost = 0f;
                return t;
            }
        }

        static B9TankSettings()
        {
            CFGUtil.RegisterParseType<PartResourceDefinition>(FindResourceDefinition, x => x.name);
            CFGUtil.RegisterParseType<TankType>(B9TankSettings.GetTankType, x => x.tankName);
        }

        public static void ModuleManagerPostLoad()
        {
            ReloadTankDefs();
        }

        public static void ReloadTankDefs()
        {
            tankTypes.Clear();

            // Structural tank type is hard coded
            tankTypes.Add(StructuralTankType.tankName, StructuralTankType);

            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("B9_TANK_TYPE");
            for (int i = 0; i < nodes.Length; i++)
            {
                TankType t = new TankType();
                t.Load(nodes[i]);
                if (tankTypes.ContainsKey(t.tankName))
                {
                    Debug.LogError("The tank type " + t.tankName + " already exists");
                    continue;
                }
                tankTypes.Add(t.tankName, t);
                Debug.Log("B9TankSettings: registered tank type " + t.tankName);
            }

            LoadedTankDefs = true;
        }

        public static TankType GetTankType(string name)
        {
            CheckTankDefs();
            if (string.IsNullOrEmpty(name))
                return StructuralTankType;
            return tankTypes[name].Clone() as TankType;
        }

        public static bool TankTypeExists(string name)
        {
            CheckTankDefs();
            return tankTypes.ContainsKey(name);
        }

        private static void CheckTankDefs()
        {
            if (!LoadedTankDefs)
                throw new InvalidOperationException("The tank definitions have not been loaded yet (done after game database load). Perhaps ModuleManager is missing or out of date?");
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
