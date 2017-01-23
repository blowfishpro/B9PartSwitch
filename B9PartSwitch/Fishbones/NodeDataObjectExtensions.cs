using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones
{
    public static class NodeDataObjectExtensions
    {
        public const string SERIALIZED_NODE = "SERIALIZED_NODE";
        public const string CURRENT_UPGRADE = "CURRENTUPGRADE";

        public static void Load(this object obj, ConfigNode node, OperationContext context)
        {
            NodeDataList list = NodeDataListLibrary.Get(obj.GetType());

            OperationContext newContext = new OperationContext(context, obj);
            list.Load(node, newContext);
        }

        public static void Save(this object obj, ConfigNode node, OperationContext context)
        {
            NodeDataList list = NodeDataListLibrary.Get(obj.GetType());

            OperationContext newContext = new OperationContext(context, obj);
            list.Save(node, newContext);
        }

        public static void LoadFields(this PartModule module, ConfigNode node)
        {
            bool loadingPrefab = module.part.partInfo.IsNull() || node.name == CURRENT_UPGRADE;
            Operation operation = loadingPrefab ? Operation.LoadPrefab : Operation.LoadInstance;

            OperationContext context = new OperationContext(operation, module);

            NodeDataList list = NodeDataListLibrary.Get(module.GetType());
            list.Load(node, context);
        }

        public static void SaveFields(this PartModule module, ConfigNode node)
        {
            OperationContext context = new OperationContext(Operation.Save, module);

            NodeDataList list = NodeDataListLibrary.Get(module.GetType());
            list.Save(node, context);
        }

        public static string Serialize(this object obj)
        {
            ConfigNode node = new ConfigNode(SERIALIZED_NODE);

            NodeDataList list = NodeDataListLibrary.Get(obj.GetType());
            OperationContext context = new OperationContext(Operation.Serialize, obj);
            list.Save(node, context);

            return node.ToString();
        }

        public static void Deserialize(this object obj, string serializedData)
        {
            ConfigNode node = ConfigNode.Parse(serializedData).GetNode(SERIALIZED_NODE);

            NodeDataList list = NodeDataListLibrary.Get(obj.GetType());
            OperationContext context = new OperationContext(Operation.Deserialize, obj);
            list.Load(node, context);
        }
    }
}
