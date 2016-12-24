using System;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.Parsers
{
    public static class NodeObjectWrapper
    {
        public static void Load(object obj, ConfigNode node, OperationContext context)
        {
            obj.ThrowIfNullArgument(nameof(obj));
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            if (!context.Operation.Loading) throw new ArgumentException("Operation must be a loading operation");

            if (obj is IContextualNode)
                ((IContextualNode)obj).Load(node, context);
            else if (obj is IConfigNode)
                ((IConfigNode)obj).Load(node);
            else
                throw new ArgumentException($"Object must be a {nameof(IContextualNode)} or {nameof(IConfigNode)}, but got {obj.GetType()}", nameof(obj));
        }

        public static void Save(object obj, ConfigNode node, OperationContext context)
        {
            obj.ThrowIfNullArgument(nameof(obj));
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));
            
            if (!context.Operation.Saving) throw new ArgumentException("Operation must be a saving operation");

            if (obj is IContextualNode)
                ((IContextualNode)obj).Save(node, context);
            else if (obj is IConfigNode)
                ((IConfigNode)obj).Save(node);
            else
                throw new ArgumentException($"Object must be a {nameof(IContextualNode)} or {nameof(IConfigNode)}, but got {obj.GetType()}", nameof(obj));
        }
    }
}
