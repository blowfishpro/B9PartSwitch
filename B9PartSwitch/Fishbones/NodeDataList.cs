using System;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones
{
    public class NodeDataList
    {
        private readonly INodeDataField[] fields;

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

        public void Load(ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            foreach (INodeDataField field in fields)
            {
                try
                {
                    field.Load(node, context);
                }
                catch(Exception ex)
                {
                    throw new Exception($"Exception while loading field {field.Name} on type {context.Subject?.GetType()}", ex);
                }
            }
        }

        public void Save(ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            foreach (INodeDataField field in fields)
            {
                try
                {
                    field.Save(node, context);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Exception while saving field {field.Name} on type {context.Subject?.GetType()}", ex);
                }
            }
        }
    }
}
