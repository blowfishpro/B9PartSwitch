using System;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class EnumValueParser : IValueParser
    {
        private readonly Type enumType;

        public EnumValueParser(Type enumType)
        {
            enumType.ThrowIfNullArgument(nameof(enumType));
            if (!enumType.IsEnum) throw new ArgumentException($"Expecting enum type but got '{enumType}'", nameof(enumType));

            this.enumType = enumType;
        }

        public object Parse(string value)
        {
            value.ThrowIfNullArgument(nameof(value));
            return Enum.Parse(enumType, value);
        }

        public string Format(object value)
        {
            value.ThrowIfNullArgument(nameof(value));
            value.EnsureArgumentType(enumType, nameof(value));

            return Enum.Format(enumType, value, "g");
        }

        public Type ParseType => enumType;
    }
}
