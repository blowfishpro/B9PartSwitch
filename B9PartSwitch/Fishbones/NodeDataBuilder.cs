using System;
using System.Linq;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitch.Fishbones.FieldWrappers;

namespace B9PartSwitch.Fishbones
{
    public class NodeDataBuilder
    {
        public readonly NodeData nodeData;
        public readonly IValueParseMap valueParseMap;
        public readonly IFieldWrapper fieldWrapper;
        public readonly Attribute[] fieldAttributes;

        public NodeDataBuilder(NodeData nodeData, IFieldWrapper fieldWrapper, IValueParseMap defaultValueParseMap)
        {
            nodeData.ThrowIfNullArgument(nameof(nodeData));
            fieldWrapper.ThrowIfNullArgument(nameof(fieldWrapper));
            defaultValueParseMap.ThrowIfNullArgument(nameof(defaultValueParseMap));

            this.nodeData = nodeData;
            this.fieldWrapper = fieldWrapper;

            object[] attributes = fieldWrapper.MemberInfo.GetCustomAttributes(true);

            if (attributes.OfType<IUseParser>().Any())
            {
                IValueParser[] overrides = attributes.OfType<IUseParser>().Select(x => x.CreateParser()).ToArray();
                valueParseMap = new OverrideValueParseMap(defaultValueParseMap, overrides);
            }
            else
            {
                valueParseMap = defaultValueParseMap;
            }
        }

        public NodeDataField CreateNodeDataField()
        {
            return new NodeDataField(fieldWrapper, CreateOperationManager());
        }

        public virtual IOperaitonManager CreateOperationManager()
        {
            return new OperationManager(CreateParseMapper(), CreateLoadSaveMapper(), CreateMapperWithSerializePriority());
        }

        public virtual INodeDataMapper CreateParseMapper()
        {
            return CreateMapperWithParsePriority();
        }

        public virtual INodeDataMapper CreateLoadSaveMapper()
        {
            if (!nodeData.persistent) return null;
            return CreateMapperWithParsePriority();
        }

        public virtual INodeDataMapper CreateSerializeMapper()
        {
            if (!nodeData.alwaysSerialize && fieldWrapper.MemberInfo.ReflectedType.Implements<UnityEngine.Object>()) return null;
            return CreateMapperWithSerializePriority();
        }

        public virtual INodeDataMapper CreateMapperWithParsePriority()
        {
            return BuildFromPrioritizedList(CreateValueScalarMapperBuilder(), CreateNodeScalarMapperBuilder(), CreateValueListMapperBuilder(), CreateNodeListMapperBuilder());
        }

        public virtual INodeDataMapper CreateMapperWithSerializePriority()
        {
            return BuildFromPrioritizedList(CreateNodeListMapperBuilder(), CreateNodeScalarMapperBuilder(), CreateValueListMapperBuilder(), CreateValueScalarMapperBuilder());
        }

        public virtual INodeDataMapper BuildFromPrioritizedList(params INodeDataMapperBuilder[] list)
        {
            INodeDataMapperBuilder builder = list.FirstOrDefault(x => x.CanBuild);

            if (builder.IsNotNull())
                return builder.BuildMapper();
            else
                throw new NotImplementedException($"Cannot find a suitable way to load node data into field {fieldWrapper.MemberInfo.Name}");
        }

        #region Mapper Builders

        public virtual INodeDataMapperBuilder CreateValueScalarMapperBuilder() => new ValueScalarMapperBuilder(NodeDataName, fieldWrapper.FieldType, valueParseMap);
        public virtual INodeDataMapperBuilder CreateValueListMapperBuilder() => new ValueListMapperBuilder(NodeDataName, fieldWrapper.FieldType, valueParseMap);
        public virtual INodeDataMapperBuilder CreateNodeScalarMapperBuilder() => new NodeScalarMapperBuilder(NodeDataName, fieldWrapper.FieldType);
        public virtual INodeDataMapperBuilder CreateNodeListMapperBuilder() => new NodeListMapperBuilder(NodeDataName, fieldWrapper.FieldType);

        #endregion

        #region Utility Methods

        public virtual string NodeDataName => string.IsNullOrEmpty(nodeData.name) ? fieldWrapper.MemberInfo.Name : nodeData.name;

        #endregion
    }
}
