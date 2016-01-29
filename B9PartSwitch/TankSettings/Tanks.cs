using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace B9PartSwitch
{
    [Serializable]
    public class TankResource : CFGUtilObject
    {
        [ConfigField(configName = "name")]
        public PartResourceDefinition resourceDefinition;

        [ConfigField]
        public float unitsPerVolume = 1f;

        public string ResourceName { get { return resourceDefinition.name; } }

        public override string ToString()
        {
            string outStr = "Tank Resource:";
            if (resourceDefinition != null)
                outStr += " Resource Name = " + ResourceName;
            outStr += " unitsPerVolume = " + unitsPerVolume.ToString();
            return outStr;
        }
    }

    [Serializable]
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

        public TankResource this[int index] { get { return resources[index]; } }

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

        public bool ContainsResource(string resourceName)
        {
            foreach(TankResource resource in resources)
            {
                if (resource.ResourceName == resourceName)
                    return true;
            }
            return false;
        }

        public int ResourcesCount { get { return resources.Count; } }

        public IEnumerator<string> GetResourceNames()
        {
            for (int i = 0; i < resources.Count; i++)
            {
                yield return resources[i].ResourceName;
            }
        }

        public bool IsStructuralTankType
        {
            get
            {
                return tankName == "Structural" && tankMass == 0f && tankCost == 0f && resources.Count == 0;
            }
        }

        public IEnumerator<TankResource> GetEnumerator()
        {
            return resources.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            string outStr = "TankType: " + tankName;
            outStr += ", tankMass = " + tankMass.ToString();
            outStr += ", tankCost = " + tankCost.ToString();
            outStr += ", number of resources: " + ResourcesCount.ToString();
            for (int i = 0; i < ResourcesCount; i++ )
            {
                outStr += "\n\t ";
                if (resources[i] != null)
                    outStr += resources[i].ToString();
                else
                    outStr += "Null Resource";
            }
            return outStr;
        }
    }
}
