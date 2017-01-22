using B9PartSwitch.Fishbones.FieldWrappers;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones
{
    public class NodeDataField
    {
        public readonly IFieldWrapper field;
        public readonly IOperaitonManager operationManager;

        public NodeDataField(IFieldWrapper field, IOperaitonManager operationManager)
        {
            field.ThrowIfNullArgument(nameof(field));
            operationManager.ThrowIfNullArgument(nameof(operationManager));

            this.field = field;
            this.operationManager = operationManager;
        }

        public void Load(object subject, ConfigNode node, OperationContext context)
        {
            subject.ThrowIfNullArgument(nameof(subject));
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            INodeDataMapper mapper = operationManager.MapperFor(context.Operation);
            if (mapper.IsNull()) return;

            object value = field.GetValue(subject);

            if (mapper.Load(ref value, node, context))
                field.SetValue(subject, value);
        }

        public void Save(object subject, ConfigNode node, OperationContext context)
        {
            subject.ThrowIfNullArgument(nameof(subject));
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            INodeDataMapper mapper = operationManager.MapperFor(context.Operation);
            if (mapper.IsNull()) return;

            object value = field.GetValue(subject);
            mapper.Save(value, node, context);
        }
    }
}
