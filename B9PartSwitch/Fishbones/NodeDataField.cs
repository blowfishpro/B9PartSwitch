using B9PartSwitch.Fishbones.FieldWrappers;
using B9PartSwitch.Fishbones.NodeDataMappers;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones
{
    public interface INodeDataField
    {
        string Name { get; }

        void Load(ConfigNode node, OperationContext context);
        void Save(ConfigNode node, OperationContext context);
    }

    public class NodeDataField : INodeDataField
    {
        public readonly IFieldWrapper field;
        public readonly IOperaitonManager operationManager;

        public string Name => field.Name;

        public NodeDataField(IFieldWrapper field, IOperaitonManager operationManager)
        {
            field.ThrowIfNullArgument(nameof(field));
            operationManager.ThrowIfNullArgument(nameof(operationManager));

            this.field = field;
            this.operationManager = operationManager;
        }

        public void Load(ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            INodeDataMapper mapper = operationManager.MapperFor(context.Operation);
            if (mapper.IsNull()) return;

            object value = field.GetValue(context.Subject);

            if (mapper.Load(ref value, node, context))
                field.SetValue(context.Subject, value);
        }

        public void Save(ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            INodeDataMapper mapper = operationManager.MapperFor(context.Operation);
            if (mapper.IsNull()) return;

            object value = field.GetValue(context.Subject);
            mapper.Save(value, node, context);
        }
    }
}
