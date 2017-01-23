using System;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones
{
    public class NodeDataList
    {
        private INodeDataField[] fields;

        public NodeDataList(params INodeDataField[] fields)
        {
            fields.ThrowIfNullArgument(nameof(fields));

            this.fields = new INodeDataField[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                INodeDataField field = fields[i];

                if (field.IsNull()) throw new ArgumentNullException($"Encountered null in list at position {i}", nameof(fields));

                this.fields[i] = field;
            }
        }

        public void Load(object subject, ConfigNode node, OperationContext context)
        {
            subject.ThrowIfNullArgument(nameof(subject));
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            OperationContext newContext = new OperationContext(context, subject);

            foreach (INodeDataField field in fields)
            {
                field.Load(subject, node, newContext);
            }
        }

        public void Save(object subject, ConfigNode node, OperationContext context)
        {
            subject.ThrowIfNullArgument(nameof(subject));
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            OperationContext newContext = new OperationContext(context, subject);

            foreach (INodeDataField field in fields)
            {
                field.Save(subject, node, newContext);
            }
        }
    }
}
