using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.NodeDataMappers;

namespace B9PartSwitchTests
{
    internal static class Exemplars
    {
        public static ValueParser<string> ValueParser => new ValueParser<string>(s => "!!" + s + "!!", s => "$$" + s + "$$");
    }
}
