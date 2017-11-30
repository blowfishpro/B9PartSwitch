using System;
using UnityEngine;

namespace B9PartSwitch
{
    public class TextureReplacement
    {
        public readonly Material material;
        public readonly string shaderProperty;
        public readonly Texture oldTexture;
        public readonly Texture newTexture;

        public TextureReplacement(Material material, string shaderProperty, Texture newTexture)
        {
            material.ThrowIfNullArgument(nameof(material));
            shaderProperty.ThrowIfNullOrEmpty(nameof(shaderProperty));
            newTexture.ThrowIfNullArgument(nameof(newTexture));

            this.material = material;
            this.shaderProperty = shaderProperty;
            this.newTexture = newTexture;

            oldTexture = material.GetTexture(shaderProperty);

            if (oldTexture == null)
                throw new ArgumentException($"{material.name} has no texture on the property {shaderProperty}");
        }

        public void Activate() => material.SetTexture(shaderProperty, newTexture);
        public void Deactivate() => material.SetTexture(shaderProperty, oldTexture);
    }
}
