using System;

namespace B9PartSwitch.Fishbones.Parsers
{
    public interface IValueParseMap
    {
        IValueParser GetParser(Type parseType);
        bool CanParse(Type parseType);
    }

    public interface IMutableValueParseMap : IValueParseMap
    {
        void AddParser<T>(Func<string, T> parse, Func<T, string> format);
        void AddParser(IValueParser parser);
    }
}
