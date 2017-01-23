using System;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitch.Fishbones
{
    public interface IUseParser
    {
        IValueParser CreateParser();
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UseParser : Attribute, IUseParser
    {
        private readonly Type valueParserType;

        public UseParser(Type valueParserType)
        {
            valueParserType.ThrowIfNullArgument(nameof(valueParserType));

            if (!valueParserType.Implements<IValueParser>())
                throw new ArgumentException($"Type {valueParserType} does not implement {typeof(IValueParser)}");

            if (valueParserType.GetConstructor(Type.EmptyTypes).IsNull())
                throw new ArgumentException($"Type {valueParserType} does not have a parameterless constructor");

            this.valueParserType = valueParserType;
        }

        public IValueParser CreateParser()
        {
            return (IValueParser)Activator.CreateInstance(valueParserType);
        }
    }
}
