using System;
using System.Collections.Generic;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.PartSwitch.PartModifiers;

namespace B9PartSwitch
{
    public class ColorPropertyModifierInfo : IContextualNode
    {
        [NodeData(name = "color")]
        public Color newColor;

        [NodeData(name = "shaderProperty")]
        public string shaderPropName = "_Color";

        public void Load(ConfigNode node, OperationContext context) => this.LoadFields(node, context);

        public void Save(ConfigNode node, OperationContext context) => this.SaveFields(node, context);

        public IEnumerable<IPartModifier> CreateModifiers(IEnumerable<Renderer> renderers)
        {
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.sharedMaterial.HasProperty(shaderPropName)) continue;

                yield return new ColorPropertyModifier(renderer, shaderPropName, newColor);
            }
        }
    }
}
