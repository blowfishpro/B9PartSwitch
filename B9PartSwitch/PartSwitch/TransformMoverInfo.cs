using System;
using System.Collections.Generic;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch
{
    public class TransformMoverInfo : IContextualNode
    {
        [NodeData(name = "name")]
        public string transformName;

        [NodeData]
        public Vector3 positionOffset = Vector3.zero;

        [NodeData]
        public Vector3 rotationOffset = Vector3.zero;

        public void Load(ConfigNode node, OperationContext context)
        {
            this.LoadFields(node, context);
        }

        public void Save(ConfigNode node, OperationContext context)
        {
            this.SaveFields(node, context);
        }

        public IEnumerable<TransformMover> CreateTransformMovers(Part part)
        {
            part.ThrowIfNullArgument(nameof(part));
            if (string.IsNullOrEmpty(transformName)) throw new InvalidOperationException("transformName is empty");

            Quaternion rotationOffsetQ = Quaternion.Euler(rotationOffset.x, rotationOffset.y, rotationOffset.z);
            foreach (Transform transform in part.GetModelTransforms(transformName))
            {
                yield return new TransformMover(transform, positionOffset, rotationOffsetQ);
            }
        }
    }
}
