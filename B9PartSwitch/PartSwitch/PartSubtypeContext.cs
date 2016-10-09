namespace B9PartSwitch
{
    public struct PartSubtypeContext
    {
        public readonly Part Part;
        public readonly ModuleB9PartSwitch Module;
        public readonly PartSubtype Subtype;

        public PartSubtypeContext(Part part, ModuleB9PartSwitch module, PartSubtype subtype)
        {
            Part = part;
            Module = module;
            Subtype = subtype;
        }
    }
}
