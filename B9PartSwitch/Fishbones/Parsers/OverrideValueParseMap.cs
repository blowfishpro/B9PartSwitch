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

            if (overrides.Any(parser => parser.ParseType == parseType)) return true;

            if (parseType.IsNullableValueType())
            {
                Type valueType = parseType.GetGenericArguments()[0];
                if (overrides.Any(parser => parser.ParseType == valueType)) return true;
            }

            return innerParseMap.CanParse(parseType);
        }

        public IValueParser GetParser(Type parseType)
        {
            parseType.ThrowIfNullArgument(nameof(parseType));

            if (overrides.FirstOrDefault(testParser => testParser.ParseType == parseType) is IValueParser parser) return parser;

            if (parseType.IsNullableValueType())
            {
                Type valueType = parseType.GetGenericArguments()[0];
                foreach (IValueParser testParser in overrides)
                {
                    if (testParser.ParseType == valueType) return testParser;
                }
            }

            return overrides.FirstOrDefault(testParser => testParser.ParseType == parseType) ?? innerParseMap.GetParser(parseType);
        }
    }
}
