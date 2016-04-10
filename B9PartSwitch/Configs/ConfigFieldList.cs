using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP;

namespace B9PartSwitch
{
    // Note: this is not serializable and thus must be recreated in Awake()
    public class ConfigFieldList : IEnumerable<ConfigFieldInfo>
    {
        public object Parent { get; private set; }
        private List<ConfigFieldInfo> configFields = new List<ConfigFieldInfo>();

        public ConfigFieldList(object parent)
        {
            Parent = parent;
            configFields.Clear();

            foreach (var field in parent.GetType().GetFields())
            {
                object[] attributes = field.GetCustomAttributes(true);

                var configField = attributes.OfType<ConfigField>().FirstOrDefault();

                if (configField.IsNull())
                    continue;

                if (attributes.OfType<KSPField>().Any())
                    throw new NotSupportedException("The property ConfigField is not allowed on a field that also has the KSPField property");

                var isList = field.FieldType.IsListType();
                var elementType = isList ? field.FieldType.GetGenericArguments()[0] : field.FieldType;
                var isIConfigNode = elementType.DerivesFrom(typeof(IConfigNode));
                var isParsable = elementType.IsConfigParsableType();
                
                ConfigFieldInfo fieldInfo;

                if (isList && isParsable)
                    fieldInfo = new ValueListConfigFieldInfo(parent, field, configField);
                else if (isList && isIConfigNode)
                    fieldInfo = new NodeListConfigFieldInfo(parent, field, configField);
                else if (isParsable)
                    fieldInfo = new ValueScalarConfigFieldInfo(parent, field, configField);
                else if (isIConfigNode)
                    fieldInfo = new NodeScalarConfigFieldInfo(parent, field, configField);
                else
                    throw new NotImplementedException("Cannot find a suitable way to make the field '" + field.Name + "' on the type " + parent.GetType().Name + " into a ConfigField");

                if (configFields.Any(f => f.ConfigName == fieldInfo.ConfigName))
                    throw new NotSupportedException("Two ConfigField properties in the same class cannot have the same config name ('" + fieldInfo.ConfigName + "')");

                configFields.Add(fieldInfo);
            }
        }

        public IEnumerator<ConfigFieldInfo> GetEnumerator()
        {
            return configFields.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ConfigFieldInfo this[string name] => configFields.FirstOrDefault(cf => cf.Name == name);

        public void Load(ConfigNode node)
        {

#if DEBUG
            Debug.Log("Loading " + Parent.GetType().Name + " from config");
#endif
            foreach (var field in configFields)
            {
                field.LoadFromNode(node);
            }
        }

        public void Save(ConfigNode node, bool serializing = false)
        {
            foreach (var field in configFields)
            {
                if (!field.IsPersistant && !serializing)
                    continue;

                // Most component fields don't need to be serialized since Unity will do it
                if (serializing && Parent is Component && !field.Field.GetCustomAttributes(true).Any(a => a is ConfigNodeSerialized))
                    continue;

                field.SaveToNode(node, serializing);
            }
        }

        public void OnDestroy()
        {
            foreach (var field in configFields)
            {
                if (!field.Attribute.destroy)
                    continue;

                field.Destroy();
            }
        }
    }
}
