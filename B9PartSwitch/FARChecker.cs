using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B9PartSwitch
{
    public static class FARChecker
    {
        public static bool FARLoaded { get; private set; }

        static FARChecker()
        {
            FARLoaded = false;

            for (int i = 0; i < AssemblyLoader.loadedAssemblies.Count; i++)
            {
                var assembly = AssemblyLoader.loadedAssemblies[i];
                if (assembly.name == "FerramAerospaceResearch")
                {
                    FARLoaded = true;
                    break;
                }
            }
        }
    }
}
