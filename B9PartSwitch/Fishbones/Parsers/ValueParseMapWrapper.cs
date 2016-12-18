using System;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class ValueParseMapWrapper : IValueParseMap
    {
        private readonly IValueParseMap map;

        public ValueParseMapWrapper(IValueParseMap map)
        {
            map.ThrowIfNullArgument(nameof(map));
            this.map = map;
        }

        public IValueParser GetParser(Type parseType)
        {
            parseType.ThrowIfNullArgument(nameof(parseType));
            return map.GetParser(parseType);
        }
    }
}
