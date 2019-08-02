using System;
using System.Collections.Generic;
using System.Linq;

namespace B9PartSwitch
{
    public class BestSubtypeDeterminator
    {
        public PartSubtype FindBestSubtype(IEnumerable<PartSubtype> subtypes, IEnumerable<string> resourceNamesOnPart)
        {
            // No managed resources, so no way to guess subtype
            if (!subtypes.Any(s => s.HasTank)) return subtypes.First();

            // Now use resources
            // This finds all the managed resources that currently exist on teh part
            IEnumerable<string> managedResourceNames = subtypes.SelectMany(s => s.ResourceNames).Distinct();
            string[] managedResourcesOnPart = managedResourceNames.Intersect(resourceNamesOnPart).ToArray();

            // If any of the part's current resources are managed, look for a subtype which has all of the managed resources (and all of its resources exist)
            // Otherwise, look for a structural subtype (no resources)
            if (managedResourcesOnPart.Any())
            {
                foreach (PartSubtype subtype in subtypes)
                {
                    if (!subtype.HasTank) continue;
                    if (!subtype.ResourceNames.SameElementsAs(managedResourcesOnPart)) continue;
                    return subtype;
                }
            }
            else
            {
                foreach (PartSubtype subtype in subtypes)
                {
                    if (subtype.HasTank) continue;
                    return subtype;
                }
            }

            // No useful way to determine correct subtype, just pick first
            return subtypes.First();
        }
    }
}
