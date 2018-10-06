using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;
using System.Reflection;

namespace B9PartSwitch
{
    public class PartModuleSwitchInfo : IContextualNode
    {
        [NodeData(name = "type")]
        public string type;

        [NodeData(name = "findAll")]
        public bool findAll = false;

        [NodeData(name = "moduleIndex")]
        public int moduleIndex = -1;

        [NodeData(name = "fieldIdentifier")]
        public string fieldIdentifier = "";

        [NodeData(name = "valueIdentifier")]
        public string valueIdentifier = "";

        public void Load(ConfigNode node, OperationContext context)
        {
            this.LoadFields(node, context);
        }

        public void Save(ConfigNode node, OperationContext context)
        {
            this.SaveFields(node, context);
        }

        public IEnumerable<PartModule> FindModule(Part part)
        {
            List<PartModule> modules = new List<PartModule>();
            int index = 0;
            // for each module in the part
            for (int i = 0; i < part.Modules.Count; i++)
            {
                // if the module type match
                if (part.Modules[i].moduleName == type)
                {
                    // we are asked to find all modules of this type
                    if (findAll)
                    {
                        yield return part.Modules[i];
                    }

                    // else we try to get the correct module
                    // try to use the field identifier
                    if (fieldIdentifier.Length != 0)
                    {
                        // get identifier
                        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
                        FieldInfo field = part.Modules[i].GetType().GetField(fieldIdentifier, flags);

                        if (field.FieldType == typeof(string))
                        {
                            if (string.Equals(valueIdentifier, (string)field.GetValue(part.Modules[i]), StringComparison.OrdinalIgnoreCase))
                            {
                                yield return part.Modules[i];
                            }
                        }
                        else if (field.FieldType == typeof(int))
                        {
                            int value;
                            if (int.TryParse(valueIdentifier, out value) && value == (int)field.GetValue(part.Modules[i]))
                            {
                                yield return part.Modules[i];
                            }
                        }
                        else if (field.FieldType == typeof(float))
                        {
                            float value;
                            if (float.TryParse(valueIdentifier, out value) && value - (float)field.GetValue(part.Modules[i]) < float.Epsilon)
                            {
                                yield return part.Modules[i];
                            }
                        }
                        else if (field.FieldType.IsEnum)
                        {
                            if (Enum.Parse(field.FieldType, valueIdentifier, true) == Enum.ToObject(field.FieldType, field.GetValue(part.Modules[i])))
                            {
                                yield return part.Modules[i];
                            }
                        }
                    }
                    // try to use the module index (index derived from modules of the "moduleName" type only)
                    else if (moduleIndex > -1)
                    {
                        if (index == moduleIndex)
                        {
                            yield return part.Modules[i];
                        }
                    }
                    // else return the first found module
                    else
                    {
                        yield return part.Modules[i];
                    }
                    index++;
                }
            }
        }
    }
}
