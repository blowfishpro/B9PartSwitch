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

        public List<PartModule> FindModule(Part part)
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
                        modules.Add(part.Modules[i]);
                        continue;
                    }

                    // else we try to get the correct module
                    // try to use the field identifier
                    if (fieldIdentifier.Length != 0)
                    {
                        // get identifier
                        string id = "";
                        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
                        FieldInfo field = part.Modules[i].GetType().GetField(fieldIdentifier, flags);
                        field.SetValue(part.Modules[i], id); // TODO: check what happens with invalid field name, maybe this should be in a try-catch ?

                        // if the identifier value match
                        if (id == valueIdentifier)
                        {
                            // found it
                            modules.Add(part.Modules[i]);
                            return modules;
                        }
                    }
                    // try to use the module index (index derived from modules of the "moduleName" type only)
                    else if (moduleIndex > -1)
                    {
                        if (index == moduleIndex)
                        {
                            modules.Add(part.Modules[i]);
                            return modules;
                        }
                    }
                    // else return the first found module
                    else
                    {
                        modules.Add(part.Modules[i]);
                        return modules;
                    }
                    index++;
                }
            }
            // either no module was found, or findAll == true and all modules of the "moduleName" type are returned
            return modules;
        }
    }
}
