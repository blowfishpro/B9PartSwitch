using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace B9PartSwitch
{
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
                outStr += $" Resource Name = {ResourceName}";
            else
                outStr += " Null resource";
            outStr += $" unitsPerVolume = {unitsPerVolume}";
            return outStr;
        }
    }
    
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

        public bool IsStructuralTankType => (tankName == B9TankSettings.structuralTankName) && (tankMass == 0f) && (tankCost == 0f) && (resources.Count == 0);

        public float ResourceUnitCost => resources.Sum(r => r.unitsPerVolume * r.resourceDefinition.unitCost);
        public float TotalUnitCost => ResourceUnitCost + tankCost;

        public bool ChangesMass => (tankMass != 0f) || (resources.Any(r => r.resourceDefinition.density != 0f));
        public bool ChangesCost => (tankCost != 0f) || (resources.Any(r => r.resourceDefinition.unitCost != 0f));

        #endregion

        public IEnumerator<TankResource> GetEnumerator() => resources.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            string outStr = $"TankType: {tankName}, mass = {tankMass}, cost = {tankCost}";
            foreach (var resource in resources)
            {
                outStr += "\n\t ";
                if (resource != null)
                    outStr += resource.ToString();
                else
                    outStr += "Null Tank Resource";
            }
            return outStr;
        }
    }
}
