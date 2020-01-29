using System;
using UnityEngine;

namespace B9PartSwitch
{
    public class ModuleB9DisableTransform : PartModule
    {
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            foreach (string transformName in node.GetValues("transform"))
            {
                Transform[] transforms = part.FindModelTransforms(transformName);

                if (transforms.Length == 0)
                {
                    this.LogError($"No transforms named '{transformName}' found in model");
                    continue;
                }

                foreach (Transform transform in transforms)
                {
                    transform.gameObject.SetActive(false);
                }
            }

            part.RemoveModule(this);
        }
    }
}
