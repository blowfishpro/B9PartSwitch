using System;

namespace B9PartSwitch
{
    public interface IParseType
    {
        object Parse(string s);
        string Format(object o);
    }

    public class ParseType<T> : IParseType
    {
        public readonly Func<string, T> parseFunction;
        public readonly Func<T, string> formatFunction;

        public ParseType(Func<string, T> parseFunction, Func<T, string> formatFunction)
        {
            this.parseFunction = parseFunction;
            this.formatFunction = formatFunction;
        }

        public object Parse(string s)
        {
            return parseFunction(s);
        }

        public string Format(object o)
        {
            if (!(o is T))
                throw new ArgumentException("Object to format must be of type " + typeof(T).Name);
            if (formatFunction != null)
                return formatFunction((T)o);
            else
                return o.ToString();
        }
    }
}
