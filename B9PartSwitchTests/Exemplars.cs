using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.NodeDataMappers;

namespace B9PartSwitchTests
{
    internal static class Exemplars
    {
        public static readonly ValueParser<string> ValueParser = new ValueParser<string>(s => "!!" + s + "!!", s => "$$" + s + "$$");

        public static ValueParser<T> DummyValueParser<T>() => new ValueParser<T>(s => default(T), t => "");
    }
}
