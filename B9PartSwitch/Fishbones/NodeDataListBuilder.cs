using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using B9PartSwitch.Fishbones.FieldWrappers;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitch.Fishbones
{
    public class NodeDataListBuilder
    {
        public const BindingFlags FIELD_BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Type type;

        public NodeDataListBuilder(Type type)
        {
            type.ThrowIfNullArgument(nameof(type));

            this.type = type;
        }

        public virtual NodeDataList CreateList()
        {
            List<INodeDataField> builtFields = new List<INodeDataField>();

            IValueParseMap parseMap = DefaultValueParseMap.Instance;

            foreach (FieldInfo field in type.GetFields(FIELD_BINDING_FLAGS))
            {
                NodeData nodeData = (NodeData)field.GetCustomAttributes(typeof(NodeData), true).FirstOrDefault();

                if (nodeData == null) continue;

                IFieldWrapper fieldWrapper = new FieldWrapper(field);

                INodeDataBuilder builder = CreateFieldBuilder(nodeData, fieldWrapper, parseMap);

                builtFields.Add(builder.CreateNodeDataField());
            }

            foreach (PropertyInfo property in type.GetProperties(FIELD_BINDING_FLAGS))
            {
                NodeData nodeData = (NodeData)property.GetCustomAttributes(typeof(NodeData), true).FirstOrDefault();

                if (nodeData == null) continue;

                IFieldWrapper fieldWrapper = new PropertyWrapper(property);

                INodeDataBuilder builder = CreateFieldBuilder(nodeData, fieldWrapper, parseMap);

                builtFields.Add(builder.CreateNodeDataField());
            }

            return new NodeDataList(builtFields.ToArray());
        }

        public virtual INodeDataBuilder CreateFieldBuilder(NodeData nodeData, IFieldWrapper fieldWrapper, IValueParseMap defaultValueParseMap)
        {
            return new NodeDataBuilder(nodeData, fieldWrapper, defaultValueParseMap);
        }
    }
}
