using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace B9PartSwitch
{
    public static class CustomPartModuleExtensions
    {
        public static IEnumerable<T> FindSymmetryCounterparts<T>(this T module) where T : CustomPartModule
        {
            if (module.part == null)
                yield break;

            foreach (var symPart in module.part.symmetryCounterparts)
            {
                var otherModule = symPart.Modules.OfType<T>().FirstOrDefault(m => m.GetType() == module.GetType() && m.moduleID == module.moduleID);

                if (otherModule.IsNotNull())
                    yield return otherModule;
                else
                    Debug.LogWarning("No symmetry counterpart found on part counterpart");
            }
        }
    }
}
