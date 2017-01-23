using System;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitch
{
    public class TankTypeValueParser : IValueParser
    {
        public object Parse(string value)
        {
            value.ThrowIfNullArgument(nameof(value));
            return B9TankSettings.GetTankType(value);
        }

        public string Format(object value)
        {
            value.ThrowIfNullArgument(nameof(value));
            return ((TankType)value).tankName;
        }

        public Type ParseType => typeof(TankType);
    }
}
