using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace B9PartSwitch
{
    public static class PartExtensions
    {
        public static Part GetPrefab(this Part part)
        {
            Part prefab = null;
            if (part.partInfo != null && part.partInfo.partPrefab != null)
            {
                if (!System.Object.ReferenceEquals(part.partInfo.partPrefab, part))
                {
                    return part.partInfo.partPrefab;
                }
            }

            if (PartLoader.Instance != null)
            {
                AvailablePart info = PartLoader.getPartInfoByName(part.name);
                if (info != null)
                {
                    prefab = info.partPrefab;
                    if (prefab == null)
                    {
                        Debug.LogWarning("Warning: no prefab could be found for part " + part.name);
                        return part;
                    }
                    else if (System.Object.ReferenceEquals(prefab, part))
                    {
                        Debug.LogWarning("Warning: Part " + part.name + " is its own prefab");
                        return part;
                    }
                    return prefab;
                }
            }

            Debug.LogWarning("Warning: no prefab could be found for part " + part.name);
            return part;
        }
    }
}
