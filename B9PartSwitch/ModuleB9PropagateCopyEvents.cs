using System;

namespace B9PartSwitch
{
    // Hack for these methods not being fired for child parts when copying in the editor - remove if this gets fixed in KSP
    public class ModuleB9PropagateCopyEvents : PartModule
    {
        public override void OnAwake()
        {
            base.OnAwake();
            enabled = false;
            isEnabled = false;
        }

        public override void OnWillBeCopied(bool asSymCounterpart)
        {
            base.OnWillBeCopied(asSymCounterpart);

            foreach (Part child in part.children)
            {
                child.OnWillBeCopied(asSymCounterpart);
            }
        }

        public override void OnWasCopied(PartModule copyPartModule, bool asSymCounterpart)
        {
            base.OnWasCopied(copyPartModule, asSymCounterpart);

            if (copyPartModule.part.children.Count == part.children.Count)
            {
                for (int i = 0; i < part.children.Count; i++)
                {
                    part.children[i].OnWasCopied(copyPartModule.part.children[i], asSymCounterpart);
                }
            }
            else
            {
                this.LogInfo("Cannot fire OnWasCopied for children as child counts differ");
            }
        }
    }
}
