namespace B9PartSwitch
{
    public static class FARWrapper
    {
        public static bool FARLoaded { get; private set; }

        static FARWrapper()
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
