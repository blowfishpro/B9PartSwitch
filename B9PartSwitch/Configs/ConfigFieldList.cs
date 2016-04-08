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
                    return;

                if (attributes.OfType<KSPField>().Any())
                    throw new NotSupportedException("The property ConfigField is not allowed on a field that also has the KSPField property");
                
                ConfigFieldInfo fieldInfo;

                if (field.FieldType.IsListType())
                    fieldInfo = new ListFieldInfo(parent, field, configField);
                else
                    fieldInfo = new ConfigFieldInfo(parent, field, configField);

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
            Debug.Log("Loading " + Instance.GetType().Name + " from config");
#endif
            foreach (var field in configFields)
            {
                if (field is ListFieldInfo)
                {
                    ListFieldInfo listInfo = field as ListFieldInfo;
                    if (listInfo.IsParsableType)
                    {
                        listInfo.ParseValues(node.GetValues(field.ConfigName));
                    }
                    else if (listInfo.IsConfigNodeType)
                    {
                        listInfo.ParseNodes(node.GetNodes(field.ConfigName));
                    }
                    else
                    {
                        throw new NotImplementedException("Cannot find a suitable way to parse type " + listInfo.RealType.Name + " from a string in a ConfigNode");
                    }
                }
                else
                {
                    if (field.IsParsableType)
                    {
                        string value = node.GetValue(field.ConfigName);
                        if (value.IsNull()) continue;

                        object result = field.Value;

                        CFGUtil.AssignConfigObject(field, value, ref result);
                        field.Value = result;
                    }
                    else if (field.IsConfigNodeType)
                    {
                        ConfigNode newNode = node.GetNode(field.ConfigName);
                        if (newNode.IsNull()) continue;

                        IConfigNode result = field.Value as IConfigNode;

                        CFGUtil.AssignConfigObject(field, newNode, ref result);
                        field.Value = result;

                    }
                    else
                    {
                        throw new NotImplementedException("Cannot find a suitable way to parse type " + field.Type.Name + " from a string in a ConfigNode");
                    }
                }
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

                if (field.Value.IsNull())
                    continue;

                if (field is ListFieldInfo)
                {
                    ListFieldInfo listInfo = field as ListFieldInfo;
                    // if (listInfo.Attribute.formatFunction != null || !listInfo.IsConfigNodeType)
                    if (listInfo.IsFormattableType)
                    {
                        foreach (var value in listInfo.FormatValues())
                        {
                            node.SetValue(field.ConfigName, value, -1, createIfNotFound: true); // -1 will create a new node
                        }
                    }
                    else if (listInfo.IsConfigNodeType)
                    {
                        foreach (var subnode in listInfo.FormatNodes(serializing))
                        {
                            node.SetNode(field.ConfigName, subnode, -1, createIfNotFound: true); // -1 will create a new node
                        }
                    }
                    else
                    {
                        Debug.LogError("This code should never be reached.  Please report this");
                    }
                }
                else
                {
                    // if (field.Attribute.formatFunction != null)
                    // {
                    //     string value = field.Attribute.formatFunction(field.Value);
                    //     node.SetValue(field.ConfigName, value, createIfNotFound: true);
                    // }
                    if (field.IsConfigNodeType)
                    {
                        ConfigNode newNode = new ConfigNode();
                        if (serializing && field.Value is IConfigNodeSerializable)
                            (field.Value as IConfigNodeSerializable).SerializeToNode(newNode);
                        else
                            (field.Value as IConfigNode).Save(newNode);

                        node.SetNode(field.ConfigName, newNode, createIfNotFound: true);
                    }
                    else if (field.IsRegisteredParseType)
                    {
                        string value = CFGUtil.FormatConfigValue(field.Value);
                        node.SetValue(field.ConfigName, value, createIfNotFound: true);
                    }
                    else
                    {
                        throw new NotImplementedException("Cannot find a suitable way to save an object of type " + field.RealType.Name + " in a ConfigNode");
                    }
                }
            }
        }

        public void OnDestroy()
        {
            foreach (var field in configFields)
            {
                if (!field.Attribute.destroy)
                    continue;

                if (field.IsComponentType || field.IsScriptableObjectType)
                {
                    if (field is ListFieldInfo)
                        (field as ListFieldInfo).ClearList();
                    else
                        UnityEngine.Object.Destroy(field.Value as UnityEngine.Object);
                }
            }
        }
    }
}
