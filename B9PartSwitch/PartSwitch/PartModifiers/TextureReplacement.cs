using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class TextureReplacement : PartModifierBase
    {
        private readonly Renderer renderer;
        private Material material;
        private readonly string shaderProperty;
        private readonly Texture oldTexture;
        private readonly Texture newTexture;

        public TextureReplacement(Renderer renderer, string shaderProperty, Texture newTexture)
        {
            renderer.ThrowIfNullArgument(nameof(renderer));
            shaderProperty.ThrowIfNullOrEmpty(nameof(shaderProperty));
            newTexture.ThrowIfNullArgument(nameof(newTexture));

            this.renderer = renderer;
            this.shaderProperty = shaderProperty;
            this.newTexture = newTexture;

            // Instantiate material here rather than using sharedMaterial (which might be used by many things)
            // Tried sharing a material accross all renderers that used it here, but it lead to weirdness
            material = renderer.material;
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
        public override void OnWasCopiedActiveSubtype()
        {
            // At this point, the copy hasn't been initialized yet, so it still shares a material with this
            // So make a copy of the material and assign it to avoid affecting the copy too
            renderer.material = new Material(renderer.material);
            material = renderer.material;
            Activate();
        }

        public override void OnBeforeReinitializeActiveSubtype() => Deactivate();

        private void Activate() => material.SetTexture(shaderProperty, newTexture);
        private void Deactivate() => material.SetTexture(shaderProperty, oldTexture);
    }
}
