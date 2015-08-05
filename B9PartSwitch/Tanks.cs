using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace B9PartSwitch
{
    public class TankResource : CFGUtilObject
    {
        [ConfigField(configName = "name")]
        public PartResourceDefinition resourceDefinition;

        [ConfigField]
        public float unitsPerVolume = 1f;

        public string ResourceName { get { return resourceDefinition.name; } }
    }

    public class TankType : CFGUtilObject, IEnumerable<TankResource>
    {
        [ConfigField(configName = "name")]
        public string tankName;
         
        [ConfigField]
        public float tankMass = 0f;

        [ConfigField]
        public float tankCost = 0f;

        [ConfigField(configName = "RESOURCE")]
        public List<TankResource> resources = new List<TankResource>();

        public TankResource this[int index]
        {
            get
            {
                return resources[index];
            }
        }

        public TankResource this[string name]
        {
            get
            {
                foreach (TankResource resource in resources)
                {
                    if (resource.resourceDefinition.name == name)
                        return resource;
                }
                return null;
            }
        }

        public bool ContainsResource(PartResourceDefinition def)
        {
            foreach(TankResource resource in resources)
            {
                if (resource.resourceDefinition == def)
                    return true;
            }
            return false;
        }

        public IEnumerator<TankResource> GetEnumerator()
        {
            return resources.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class TankPartType : CFGUtilObject, IEnumerable<TankType>
    {
        [ConfigField(configName = "name")]
        public string typeName;

        [ConfigField]
        public float addedMass = 0f;

        [ConfigField]
        public float addedCost = 0f;

        [ConfigField(configName = "tankType")]
        public List<TankType> allowedTankTypes = new List<TankType>();

        [SerializeField]
        private List<string> managedResources = new List<string>();

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (allowedTankTypes.Count == 0)
                throw new ConfigValueException("TankPartType " + typeName + " must have at least one allowed tankType");

            foreach (TankType tank in allowedTankTypes)
            {
                if (tank != null)
                {
                    foreach (TankResource resource in tank.resources)
                        managedResources.Add(resource.ResourceName);
                }
            }
        }

        public bool IsManagedResource(string resourceName)
        {
            return managedResources.Contains(resourceName);
        }

        public TankType this[int index]
        {
            get
            {
                return allowedTankTypes[index];
            }
        }

        public IEnumerator<TankType> GetEnumerator()
        {
            return allowedTankTypes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
