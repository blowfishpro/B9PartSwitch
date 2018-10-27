using System;

namespace B9PartSwitch.Fishbones.Parsers
{
    // No test for this because PartResourceLibrary can't exist outside of Unity, and not worth writing a wrapper for so little
    public class PartResourceDefinitionValueParser : ValueParser<PartResourceDefinition>
    {
        [Serializable]
        public class PartResourceNotFoundException : Exception
        {
            public PartResourceNotFoundException(string name) : base($"No resource definition named '{name}' could be found") { }
        }
        // This will raise an exception when the resource is not found
        public static PartResourceDefinition FindResourceDefinition(string name)
        {
            name.ThrowIfNullArgument(nameof(name));

            PartResourceDefinition resource = PartResourceLibrary.Instance.GetDefinition(name);
            if (resource.IsNull())
                throw new PartResourceNotFoundException(name);
            return resource;
        }

        public PartResourceDefinitionValueParser() : base(FindResourceDefinition, def => def.name) { }
    }
}
