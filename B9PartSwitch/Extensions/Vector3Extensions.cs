using UnityEngine;

namespace B9PartSwitch
{
    public static class Vector3Extensions
    {
        public static Vector3 NaN() => new Vector3(float.NaN, float.NaN, float.NaN);

        public static bool IsFinite(this Vector3 vector)
        {
            return
                vector.x > float.NegativeInfinity && vector.x < float.PositiveInfinity &&
                vector.y > float.NegativeInfinity && vector.y < float.PositiveInfinity &&
                vector.z > float.NegativeInfinity && vector.z < float.PositiveInfinity;
        }
    }
}
