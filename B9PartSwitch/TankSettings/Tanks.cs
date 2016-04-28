using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace B9PartSwitch
{
    [Serializable]
    public class TankResource : CFGUtilObject
    {
        [ConfigField(configName = "name")]
        public PartResourceDefinition resourceDefinition;

        [ConfigField]
        public float unitsPerVolume = 1f;

        public string ResourceName => resourceDefinition.name;

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
        #region Loadable Fields
        [ConfigField(configName = "name")]
        public string tankName;
         
        [ConfigField]
        public float tankMass = 0f;

        [ConfigField]
        public float tankCost = 0f;

        [ConfigField(configName = "RESOURCE")]
        public List<TankResource> resources = new List<TankResource>();

        #endregion

        #region Properties

        public TankResource this[int index] => resources[index];

        public TankResource this[string name] => resources.FirstOrDefault(r => r.ResourceName == name);

        public bool ContainsResource(string resourceName) => resources.Any(r => r.ResourceName == resourceName);

        public int ResourcesCount => resources.Count;

        public IEnumerable<string> ResourceNames => resources.Select(r => r.ResourceName);

        public bool IsStructuralTankType => (tankName == "Structural") && (tankMass == 0f) && (tankCost == 0f) && (resources.Count == 0);

        public float ResourceUnitCost => resources.Sum(r => r.unitsPerVolume * r.resourceDefinition.unitCost);

        #endregion

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
