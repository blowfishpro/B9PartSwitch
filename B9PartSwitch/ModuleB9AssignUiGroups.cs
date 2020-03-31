using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch
{
    public class ModuleB9AssignUiGroups : CustomPartModule
    {
        private static readonly string[] MODULE_NAME_BLACKLIST = { "ModuleSimpleAdjustableFairing" };
        private static readonly Type[] MODULE_TYPE_BLACKLIST = { typeof(ModuleB9PartSwitch), typeof(ModuleB9PartInfo), typeof(ModuleB9AssignUiGroups) };

        public class ModuleInfo : IContextualNode
        {
            [NodeData(name = "IDENTIFIER")]
            public ConfigNode identifierNode;

            [NodeData]
            public string uiGroupName;

            [NodeData]
            public string uiGroupDisplayName;

            public void Load(ConfigNode node, OperationContext context) => this.LoadFields(node, context);

            public void Save(ConfigNode node, OperationContext context) => this.SaveFields(node, context);

            public void Apply(Part part)
            {
                part.ThrowIfNullArgument(nameof(part));
                if (identifierNode == null) throw new InvalidOperationException("Identifier node is null!");

                ModuleMatcher moduleMatcher = new ModuleMatcher(identifierNode);
                PartModule module = moduleMatcher.FindModule(part);

                if (MODULE_TYPE_BLACKLIST.Any(type => module.GetType().Implements(type))) throw new Exception($"Cannot assign UI groups on {module}");
                if (MODULE_NAME_BLACKLIST.Any(typeName => module.GetType().Name == typeName)) throw new Exception($"Cannot assign UI groups on {module}");

                module.SetUiGroups(uiGroupName, uiGroupDisplayName);
            }
        }

        [NodeData(name = "MODULE", alwaysSerialize = true)]
        public List<ModuleInfo> moduleInfos = new List<ModuleInfo>();

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (moduleInfos.Count == 0) LogError("No MODULEs were specified");

            foreach (ModuleInfo moduleInfo in moduleInfos)
            {
                try
                {
                    moduleInfo.Apply(part);
                }
                catch (Exception ex)
                {
                    LogError("Exception when setting up UI group for MODULE");
                    Debug.LogException(ex);
                }
            }
        }
    }
}
