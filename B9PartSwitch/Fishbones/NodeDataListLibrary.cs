using System;
using System.Collections.Generic;
using UnityEngine;

namespace B9PartSwitch.Fishbones
{
    public static class NodeDataListLibrary
    {
        private static Dictionary<Type, NodeDataList> dict = new Dictionary<Type, NodeDataList>();

        public static NodeDataList Get<T>() => Get(typeof(T));

        public static NodeDataList Get(Type type)
        {
            type.ThrowIfNullArgument(nameof(type));

            NodeDataList list;

            if (dict.TryGetValue(type, out list))
                return list;

            try
            {
                NodeDataListBuilder builder = new NodeDataListBuilder(type);
                list = builder.CreateList();
            }
            catch(Exception e)
            {
                Debug.LogError($"Exception while generating field configuration for type {type}");
                Debug.LogError("This is fatal");
                Debug.LogError("Exception:");
                Debug.LogError(e.ToString());
                Debug.LogError("The game will now exit");
                Application.Quit();
            }

            dict[type] = list;
            return list;
        }
    }
}
