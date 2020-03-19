using System;
using UnityEngine;

namespace B9PartSwitch.PartSwitch.PartModifiers
{
    public class FloatPropertyModifier : PartModifierBase, IPartAspectLock
    {
        private readonly Renderer renderer;
        private readonly string shaderProperty;
        private readonly float originalValue;
        private readonly float newValue;

        public FloatPropertyModifier(Renderer renderer, string shaderProperty, float newValue)
        {
            renderer.ThrowIfNullArgument(nameof(renderer));
            shaderProperty.ThrowIfNullOrEmpty(nameof(shaderProperty));
            newValue.ThrowIfNullArgument(nameof(newValue));

            this.renderer = renderer;
            this.shaderProperty = shaderProperty;
            this.newValue = newValue;

            if (!renderer.sharedMaterial.HasProperty(shaderProperty)) throw new ArgumentException($"{renderer.sharedMaterial.name} has no property {shaderProperty}");

            originalValue = renderer.sharedMaterial.GetFloat(shaderProperty);
        }

        public object PartAspectLock => renderer.GetInstanceID() + "---" + shaderProperty;
        public override string Description => $"object {renderer.name} shader property {shaderProperty}";

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
            Activate();
        }

        public override void OnBeforeReinitializeActiveSubtype() => Deactivate();

        private void Activate() => renderer.material.SetFloat(shaderProperty, newValue);
        private void Deactivate() => renderer.material.SetFloat(shaderProperty, originalValue);
    }
}
