using System;
using System.Collections;
using System.Collections.Generic;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public class ValueListMapper : INodeDataMapper
    {
        public readonly string name;
        public readonly IValueParser parser;

        public readonly Type elementType;
        public readonly Type listType;

        public ValueListMapper(string name, IValueParser parser)
        {
            name.ThrowIfNullArgument(nameof(name));
            parser.ThrowIfNullArgument(nameof(parser));

            this.name = name;
            this.parser = parser;

            elementType = parser.ParseType;
            listType = typeof(List<>).MakeGenericType(elementType);
        }

        public bool Load(ref object fieldValue, ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            fieldValue.EnsureArgumentType(listType, nameof(fieldValue));

            string[] values = node.GetValues(name);
            if (values.IsNullOrEmpty()) return false;

            if (fieldValue.IsNull()) fieldValue = Activator.CreateInstance(listType);
            IList list = (IList)fieldValue;

            if (context.Operation == Operation.Deserialize || context.Operation == Operation.LoadInstance)
                list.Clear();

            foreach (string value in values)
            {
                if (value.IsNull()) continue;
                object parsedValue = parser.Parse(value);
                if (parsedValue.IsNotNull()) list.Add(parsedValue);
            }

            return true;
        }

        public bool Save(object fieldValue, ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            fieldValue.EnsureArgumentType(listType, nameof(fieldValue));

            IList list = (IList)fieldValue;
            if (list.IsNullOrEmpty()) return false;

            foreach (object value in list)
            {
                if (value.IsNull()) continue;
                string formattedValue = parser.Format(value);
                if (formattedValue.IsNotNull()) node.AddValue(name, formattedValue);
            }

            return true;
        }
    }
}
