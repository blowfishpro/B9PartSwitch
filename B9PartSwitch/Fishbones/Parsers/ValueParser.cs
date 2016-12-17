using System;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class ValueParser<T> : IValueParser
    {
        private Func<string, T> parseFunction;
        private Func<T, string> formatFunction;

        public ValueParser(Func<string, T> parseFunction, Func<T, string> formatFunction)
        {
            parseFunction.ThrowIfNullArgument(nameof(parseFunction));
            formatFunction.ThrowIfNullArgument(nameof(formatFunction));

            this.parseFunction = parseFunction;
            this.formatFunction = formatFunction;
        }

        public object Parse(string value)
        {
            value.ThrowIfNullArgument(nameof(value));

            return parseFunction(value);
        }

        public string Format(object value)
        {
            value.ThrowIfNullArgument(nameof(value));
            value.EnsureArgumentType<T>(nameof(value));

            return formatFunction((T)value);
        }

        public Type ParseType => typeof(T);
    }
}
