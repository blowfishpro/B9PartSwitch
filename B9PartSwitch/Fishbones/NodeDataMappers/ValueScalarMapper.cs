using System;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public class ValueScalarMapper : INodeDataMapper
    {
        private readonly string name;
        private readonly IValueParser parser;

        public ValueScalarMapper(string name, IValueParser parser)
        {
            name.ThrowIfNullArgument(nameof(name));
            parser.ThrowIfNullArgument(nameof(parser));

            this.name = name;
            this.parser = parser;
        }

        public bool Load(ref object fieldValue, ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));

            string value = node.GetValue(name);
            if (value.IsNull()) return false;

            fieldValue = parser.Parse(value);
            return true;
        }

        public bool Save(object fieldValue, ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            if (fieldValue.IsNull()) return false;

            string value = parser.Format(fieldValue);
            if (value.IsNull()) return false;

            node.SetValue(name, value, true);
            return true;
        }
    }
}
