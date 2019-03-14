using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class TextureReplacement : PartModifierBase
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

        public override object PartAspectLock => material.GetInstanceID() + "---" + shaderProperty;
        public override string Description => $"material {material.name} property {shaderProperty}";

        public override void ActivateOnStartEditor() => Activate();
        public override void ActivateOnStartFlight() => Activate();
        public override void ActivateOnSwitchEditor() => Activate();
        public override void ActivateOnSwitchFlight() => Activate();
        public override void DeactivateOnSwitchEditor() => Deactivate();
        public override void DeactivateOnSwitchFlight() => Deactivate();
        public override void OnIconCreateActiveSubtype() => Activate();
        public override void OnWillBeCopiedActiveSubtype() => Deactivate();
        public override void OnWasCopiedActiveSubtype() => Activate();
        public override void OnBeforeReinitialize() => Deactivate();

        private void Activate() => material.SetTexture(shaderProperty, newTexture);
        private void Deactivate() => material.SetTexture(shaderProperty, oldTexture);
    }
}
