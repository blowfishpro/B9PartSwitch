using System;
using System.Collections.Generic;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch
{
    public static class B9TankSettings
    {
        public const string structuralTankName = "Structural";

        private static Dictionary<string, TankType> tankTypes = new Dictionary<string,TankType>();

        public static bool LoadedTankDefs { get; private set; } = false;

        public static TankType StructuralTankType
        {
            get
            {
                TankType t = new TankType
                {
                    tankName = structuralTankName,
                    tankMass = 0f,
                    tankCost = 0f
                };
                return t;
            }
        }

        public static void ModuleManagerPostLoad() => ReloadTankDefs();

        public static void ReloadTankDefs()
        {
            tankTypes.Clear();

            // Structural tank type is hard coded
            tankTypes.Add(structuralTankName, StructuralTankType);
            
            foreach (var node in GameDatabase.Instance.GetConfigNodes("B9_TANK_TYPE"))
            {
                TankType t = new TankType();
                OperationContext context = new OperationContext(Operation.LoadPrefab, t);
                t.Load(node, context);
                if (tankTypes.ContainsKey(t.tankName))
                {
                    Debug.LogError($"B9TankSettings: The tank type {t.tankName} already exists");
                    continue;
                }
                tankTypes.Add(t.tankName, t);
                Debug.Log($"B9TankSettings: registered tank type {t.tankName}");
            }

            LoadedTankDefs = true;
        }

        public static TankType GetTankType(string name)
        {
            CheckTankDefs();
            if (name.IsNullOrEmpty())
                return StructuralTankType;
            else if (!tankTypes.ContainsKey(name))
                throw new KeyNotFoundException($"No tank type named '{name}' exists");

            return tankTypes[name].CloneUsingFields();
        }

        private static void CheckTankDefs()
        {
            if (!LoadedTankDefs)
                throw new InvalidOperationException("The tank definitions have not been loaded yet (done after game database load).  This is likely caused by an earlier error or by ModuleManager being missing or out of date");
        }
    }
}
