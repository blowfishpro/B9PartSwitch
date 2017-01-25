using System;
using System.Collections;
using System.Collections.Generic;
using UniLinq;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch
{
    public class TankResource : IContextualNode
    {
        [NodeData(name = "name")]
        public PartResourceDefinition resourceDefinition;

        [NodeData]
        public float unitsPerVolume = 1f;

        [NodeData]
        public float? percentFilled;

        public string ResourceName => resourceDefinition.name;

        public void Load(ConfigNode node, OperationContext context) => this.LoadFields(node, context);
        public void Save(ConfigNode node, OperationContext context) => this.SaveFields(node, context);

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

    public class TankType : IContextualNode, IEnumerable<TankResource>
    {
        #region Loadable Fields

        [NodeData(name = "name")]
        public string tankName;

        [NodeData]
        public float tankMass = 0f;

        [NodeData]
        public float tankCost = 0f;

        [NodeData]
        public float? percentFilled;

        [NodeData]
        public bool? resourcesTweakable;

        [NodeData(name = "RESOURCE")]
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

        public void Load(ConfigNode node, OperationContext context) => this.LoadFields(node, context);
        public void Save(ConfigNode node, OperationContext context) => this.SaveFields(node, context);

        public List<TankResource>.Enumerator GetEnumerator() => resources.GetEnumerator();
        IEnumerator<TankResource> IEnumerable<TankResource>.GetEnumerator() => GetEnumerator();
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
