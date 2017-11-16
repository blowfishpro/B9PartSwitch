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
            
            if (dict.TryGetValue(type, out NodeDataList list))
                return list;

            try
            {
                Debug.Log($"Generating field configuration for type {type}");
                NodeDataListBuilder builder = new NodeDataListBuilder(type);
                list = builder.CreateList();
            }
            catch(Exception e)
            {
                Debug.LogError($"Fatal exception while generating field configuration for type {type}");
                Debug.LogException(e);
                FatalErrorHandler.HandleFatalError(e.Message);
                throw;
            }

            dict[type] = list;
            return list;
        }
    }
}
