using UnityEngine;

namespace B9PartSwitch
{
    public static class TransformExtensions
    {
        public static void Enable(this Transform trans) => trans.gameObject.SetActive(true);
        public static void Disable(this Transform trans) => trans.gameObject.SetActive(false);
    }
}
