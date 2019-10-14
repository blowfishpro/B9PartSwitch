using System;
using System.Collections.Generic;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.PartSwitch.PartModifiers;

namespace B9PartSwitch
{
    public class TransformModifierInfo : IContextualNode
    {
        [NodeData(name = "name")]
        public string transformName;

        [NodeData]
        public Vector3? positionOffset;

        [NodeData]
        public Vector3? rotationOffset;

        [NodeData]
        [UseParser(typeof(Fishbones.Parsers.ScaleParser))]
        public Vector3? scaleOffset;

        public void Load(ConfigNode node, OperationContext context) => this.LoadFields(node, context);
        public void Save(ConfigNode node, OperationContext context) => this.SaveFields(node, context);

        public IEnumerable<IPartModifier> CreatePartModifiers(Part part, Action<string> onError)
        {
            part.ThrowIfNullArgument(nameof(part));
            onError.ThrowIfNullArgument(nameof(onError));

            if (string.IsNullOrEmpty(transformName))
            {
                onError("transform name is empty");
                yield break;
            }

            bool foundTransform = false;

            foreach (Transform transform in part.GetModelTransforms(transformName))
            {
                foundTransform = true;

                if (positionOffset.HasValue)
                {
                    yield return new TransformMover(transform, positionOffset.Value);
                }
                if (rotationOffset.HasValue)
                {
                    yield return new TransformRotator(transform, Quaternion.Euler(rotationOffset.Value));
                }
                if (scaleOffset.HasValue)
                {
                    yield return new TransformScaleModifier(transform, scaleOffset.Value);
                }
            }

            if (!foundTransform) onError($"Could not find any transform named '{transformName}'");
        }
    }
}
