using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B9PartSwitch
{
    public static class PartModuleExtensions
    {
        public static void Enable(this PartModule module)
        {
            module.enabled = true;
            module.isEnabled = true;
        }

        public static void Disable(this PartModule module)
        {
            module.enabled = false;
            module.isEnabled = false;
        }

        public static bool GetEnabled(this PartModule module)
        {
            return module.enabled && module.isEnabled;
        }
    }
}
