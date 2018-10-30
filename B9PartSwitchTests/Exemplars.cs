using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitchTests
{
    internal static class Exemplars
    {
        public static readonly ValueParser<string> ValueParser = new ValueParser<string>(s => "!!" + s + "!!", s => "$$" + s + "$$");

        public static ValueParser<T> DummyValueParser<T>() => new ValueParser<T>(s => default(T), t => "");

        public static readonly OperationContext LoadPrefabContext = new OperationContext(Operation.LoadPrefab, "some object");
        public static readonly OperationContext SaveContext = new OperationContext(Operation.Save, "some object");
    }
}
