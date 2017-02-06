using System;
using UnityEngine;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones
{
    public static class NodeDataObjectExtensions
    {
        public const string SERIALIZED_NODE = "SERIALIZED_NODE";
        public const string CURRENT_UPGRADE = "CURRENTUPGRADE";

        public static OperationContext LoadFields(this object obj, ConfigNode node, OperationContext context)
        {
            obj.ThrowIfNullArgument(nameof(obj));
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            NodeDataList list = NodeDataListLibrary.Get(obj.GetType());

            OperationContext newContext = new OperationContext(context, obj);
            list.Load(node, newContext);
            return newContext;
        }

        public static OperationContext SaveFields(this object obj, ConfigNode node, OperationContext context)
        {
            obj.ThrowIfNullArgument(nameof(obj));
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            NodeDataList list = NodeDataListLibrary.Get(obj.GetType());

            OperationContext newContext = new OperationContext(context, obj);
            list.Save(node, newContext);
            return newContext;
        }

        public static void LoadFields(this PartModule module, ConfigNode node)
        {
            module.ThrowIfNullArgument(nameof(module));
            node.ThrowIfNullArgument(nameof(node));

            bool loadingPrefab = module.part.partInfo.IsNull() || node.name == CURRENT_UPGRADE;
            Operation operation = loadingPrefab ? Operation.LoadPrefab : Operation.LoadInstance;

            OperationContext context = new OperationContext(operation, module);

            NodeDataList list = NodeDataListLibrary.Get(module.GetType());
            list.Load(node, context);
        }

        public static void SaveFields(this PartModule module, ConfigNode node)
        {
            module.ThrowIfNullArgument(nameof(module));
            node.ThrowIfNullArgument(nameof(node));

            OperationContext context = new OperationContext(Operation.Save, module);

            NodeDataList list = NodeDataListLibrary.Get(module.GetType());
            list.Save(node, context);
        }

        public static SerializedDataContainer SerializeToContainer(this object obj)
        {
            obj.ThrowIfNullArgument(nameof(obj));

            SerializedDataContainer container = ScriptableObject.CreateInstance<SerializedDataContainer>();
            container.data = obj.SerializeToString();
            return container;
        }

        public static string SerializeToString(this object obj)
        {
            obj.ThrowIfNullArgument(nameof(obj));

            return obj.SerializeToNode().ToString();
        }

        public static ConfigNode SerializeToNode(this object obj)
        {
            obj.ThrowIfNullArgument(nameof(obj));

            ConfigNode node = new ConfigNode(SERIALIZED_NODE);

            NodeDataList list = NodeDataListLibrary.Get(obj.GetType());
            OperationContext context = new OperationContext(Operation.Serialize, obj);
            list.Save(node, context);

            return node;
        }

        public static void DeserializeFromContainer(this object obj, SerializedDataContainer container)
        {
            obj.ThrowIfNullArgument(nameof(obj));
            container.ThrowIfNullArgument(nameof(container));

            if (container.data.IsNullOrEmpty())
                throw new ArgumentException("Container must have data", nameof(container));

            obj.DeserializeFromString(container.data);
        }

        public static void DeserializeFromString(this object obj, string serializedData)
        {
            obj.ThrowIfNullArgument(nameof(obj));
            serializedData.ThrowIfNullArgument(nameof(serializedData));

            ConfigNode node;

            try
            {
                node = ConfigNode.Parse(serializedData);
            }
            catch (Exception ex)
            {
                throw new FormatException("Failed to parse a ConfigNode from serialized data", ex);
            }

            ConfigNode dataNode = node.GetNode(SERIALIZED_NODE);
            if (dataNode.IsNull()) throw new FormatException("No serialized data node found");

            obj.DeserializeFromNode(dataNode);
        }

        public static void DeserializeFromNode(this object obj, ConfigNode node)
        {
            obj.ThrowIfNullArgument(nameof(obj));
            node.ThrowIfNullArgument(nameof(node));

            NodeDataList list = NodeDataListLibrary.Get(obj.GetType());
            OperationContext context = new OperationContext(Operation.Deserialize, obj);
            list.Load(node, context);
        }

        public static T CloneUsingFields<T>(this T obj) where T : new()
        {
            T newObject = new T();
            ConfigNode node = obj.SerializeToNode();
            newObject.DeserializeFromNode(node);
            return newObject;
        }
    }
}
