using System;
using System.Collections.Generic;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class ValueParseMap : IMutableValueParseMap
    {
        protected Dictionary<Type, IValueParser> parsers = new Dictionary<Type, IValueParser>();

        public virtual IValueParser GetParser(Type parseType)
        {
            parseType.ThrowIfNullArgument(nameof(parseType));

            IValueParser parser;
            if (parsers.TryGetValue(parseType, out parser))
                return parser;
            else
                throw new ParseTypeNotRegisteredException(parseType);
        }

        public virtual void AddParser<T>(Func<string, T> parse, Func<T, string> format)
        {
            parse.ThrowIfNullArgument(nameof(parse));
            format.ThrowIfNullArgument(nameof(format));

            AddParser(new ValueParser<T>(parse, format));
        }

        public virtual void AddParser(IValueParser parser)
        {
            parser.ThrowIfNullArgument(nameof(parser));
            if (!CanAdd(parser.ParseType)) throw new ParseTypeAlreadyRegisteredException(parser.ParseType);
            parsers[parser.ParseType] = parser;
        }

        public virtual bool CanParse(Type parseType)
        {
            parseType.ThrowIfNullArgument(nameof(parseType));
            return parsers.ContainsKey(parseType);
        }

        public virtual bool CanAdd(Type parseType)
        {
            parseType.ThrowIfNullArgument(nameof(parseType));
            return !parsers.ContainsKey(parseType);
        }

        public ValueParseMap Clone()
        {
            ValueParseMap clone = new ValueParseMap();

            foreach(IValueParser parser in parsers.Values)
            {
                clone.AddParser(parser);
            }

            return clone;
        }
    }
}
