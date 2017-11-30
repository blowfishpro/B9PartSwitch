using System.Collections.Generic;
using UnityEngine;

namespace B9PartSwitch
{
    public static class TransformExtensions
    {
        public static void Enable(this Transform trans) => trans.gameObject.SetActive(true);
        public static void Disable(this Transform trans) => trans.gameObject.SetActive(false);

        public static IEnumerable<Transform> GetChildrenNamedRecursive(this Transform transform, string name)
        {
            transform.ThrowIfNullArgument(nameof(transform));
            name.ThrowIfNullOrEmpty(nameof(name));

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child.name == name) yield return child;

                foreach (Transform childChild in child.GetChildrenNamedRecursive(name))
                {
                    yield return childChild;
                }
            }
        }
    }
}
