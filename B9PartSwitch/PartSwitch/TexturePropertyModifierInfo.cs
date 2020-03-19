using System;
using System.Collections.Generic;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.PartSwitch.PartModifiers;

namespace B9PartSwitch
{
    public class TexturePropertyModifierInfo : IContextualNode
    {
        [NodeData(name = "currentTexture")]
        public string currentTextureName;

        [NodeData(name = "texture")]
        public string newTexturePath;

        [NodeData]
        public bool isNormalMap = false;

        [NodeData(name = "shaderProperty")]
        public string shaderPropName;

        public void Load(ConfigNode node, OperationContext context) => this.LoadFields(node, context);

        public void Save(ConfigNode node, OperationContext context) => this.SaveFields(node, context);

        public IEnumerable<IPartModifier> CreateModifiers(IEnumerable<Renderer> renderers, Action<string> onError)
        {
            if (string.IsNullOrEmpty(newTexturePath))
            {
                onError("texture name is empty");
                yield break;
            }

            Texture newTexture = GameDatabase.Instance.GetTexture(newTexturePath, isNormalMap);

            if (newTexture == null)
            {
                onError($"Texture '{newTexturePath}' not found!");
                yield break;
            }

            string shaderProperty = shaderPropName;
            if (string.IsNullOrEmpty(shaderProperty))
            {
                if (isNormalMap) shaderProperty = "_BumpMap";
                else shaderProperty = "_MainTex";
            }

            foreach (Renderer renderer in renderers)
            {
                Material sharedMaterial = renderer.sharedMaterial;
                Texture texture = sharedMaterial.GetTexture(shaderProperty);
                if (texture == null) continue;

                if (!currentTextureName.IsNullOrEmpty())
                {
                    string baseTextureName = texture.name.Substring(texture.name.LastIndexOf('/') + 1);
                    if (baseTextureName != currentTextureName) continue;
                }

                yield return new TextureReplacement(renderer, shaderProperty, newTexture);
            }
        }
    }
}
