using System;
using System.Linq;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class OverrideValueParseMap : IValueParseMap
    {
        private readonly IValueParseMap innerParseMap;
        private readonly IValueParser[] overrides;

        public OverrideValueParseMap(IValueParseMap innerParseMap, params IValueParser[] overrides)
        {
            innerParseMap.ThrowIfNullArgument(nameof(innerParseMap));
            overrides.ThrowIfNullArgument(nameof(overrides));

            this.overrides = new IValueParser[overrides.Length];

            for (int i = 0; i < overrides.Length; i++)
            {
                IValueParser parser = overrides[i];

                if (parser.IsNull())
                    throw new ArgumentNullException($"Encountered null value at index {i}", nameof(overrides));

                if (this.overrides.Any(x => x?.ParseType == parser.ParseType))
                    throw new ArgumentException($"Attempted to register override for type {parser.ParseType} more than once", nameof(overrides));

                this.overrides[i] = parser;
            }

            this.innerParseMap = innerParseMap;
        }

        public bool CanParse(Type parseType)
        {
            parseType.ThrowIfNullArgument(nameof(parseType));

            return overrides.Any(parser => parser.ParseType == parseType) || innerParseMap.CanParse(parseType);
        }

        public IValueParser GetParser(Type parseType)
        {
            parseType.ThrowIfNullArgument(nameof(parseType));

            return overrides.FirstOrDefault(parser => parser.ParseType == parseType) ?? innerParseMap.GetParser(parseType);
        }
    }
}
