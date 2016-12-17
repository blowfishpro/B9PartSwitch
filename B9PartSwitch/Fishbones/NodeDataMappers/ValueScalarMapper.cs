using System;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public class ValueScalarMapper : INodeDataMapper
    {
        private string name;
        private IValueParser parser;

        public ValueScalarMapper(string name, IValueParser parser)
        {
            name.ThrowIfNullArgument(nameof(name));
            parser.ThrowIfNullArgument(nameof(parser));

            this.name = name;
            this.parser = parser;
        }

        public bool Load(ConfigNode node, ref object fieldValue)
        {
            node.ThrowIfNullArgument(nameof(node));

            string value = node.GetValue(name);
            if (value.IsNull()) return false;

            fieldValue = parser.Parse(value);
            return true;
        }

        public bool Save(ConfigNode node, object fieldValue)
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
