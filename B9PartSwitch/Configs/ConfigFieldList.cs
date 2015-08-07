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
        public Behaviour Instance { get; private set; }
        private List<ConfigFieldInfo> configFields = new List<ConfigFieldInfo>();

        public ConfigFieldList(Behaviour instance)
        {
            Instance = instance;
            configFields.Clear();
            FieldInfo[] fields = instance.GetType().GetFields();
            foreach (FieldInfo field in fields)
            {
                bool kspField = false;
                ConfigField configField = null;

                object[] attributes = field.GetCustomAttributes(true);
                foreach (object attribute in attributes)
                {
                    if (attribute is ConfigField)
                    {
                        configField = attribute as ConfigField;
                    }
                    else if (attribute is KSPField)
                    {
                        kspField = true;
                    }
                }

                if (configField != null)
                {
                    if (kspField)
                        throw new NotSupportedException("The property ConfigField is not allowed on a field that also has the KSPField property");

                    foreach (ConfigFieldInfo theField in configFields)
                    {
                        if (theField.ConfigName == configField.configName)
                            throw new NotSupportedException("Two ConfigField properties in the same class cannot have the same name (fields are " + theField.Name + " and " + field.Name + ")");
                    }

                    ConfigFieldInfo fieldInfo;

                    if (field.FieldType.IsListType())
                        fieldInfo = new ListFieldInfo(instance, field, configField);
                    else
                        fieldInfo = new ConfigFieldInfo(instance, field, configField);

                    configFields.Add(fieldInfo);
                }
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

        public ConfigFieldInfo this[string name]
        {
            get
            {
                foreach (ConfigFieldInfo field in configFields)
                {
                    if (field.Name == name)
                        return field;
                }
                return null;
            }
        }

        public void Load(ConfigNode node)
        {
            foreach (ConfigFieldInfo field in configFields)
            {
                if (field is ListFieldInfo)
                {
                    ListFieldInfo listInfo = field as ListFieldInfo;
                    if (listInfo.IsRegisteredParseType)
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
                    if (field.IsRegisteredParseType)
                    {
                        string value = node.GetValue(field.ConfigName);
                        if (value != null)
                            field.Value = CFGUtil.ParseConfigValue(field.Type, value);
                    }
                    else if (field.IsConfigNodeType)
                    {
                        ConfigNode newNode = node.GetNode(field.ConfigName);
                        if (newNode != null)
                        {
                            if (field.Value == null)
                                if (field.IsComponentType)
                                    field.Value = Instance.gameObject.AddComponent(field.RealType);
                                else
                                    if (field.Constructor != null)
                                        field.Value = field.Constructor.Invoke(null);
                                else
                                    throw new MissingMethodException("The value of the field " + field.Name + " is null but no default constructor could be found");
                            IConfigNode nodeObj = field.Value as IConfigNode;
                            nodeObj.Load(newNode);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("Cannot find a suitable way to parse type " + field.Type.Name + " from a string in a ConfigNode");
                    }
                }
            }
        }

        public void Save(ConfigNode node)
        {
            foreach (ConfigFieldInfo field in configFields)
            {
                if (!field.IsPersistant)
                    continue;

                if (field is ListFieldInfo)
                {
                    ListFieldInfo listInfo = field as ListFieldInfo;
                    if (listInfo.IsConfigNodeType)
                    {
                        ConfigNode[] nodes = listInfo.FormatNodes();
                        for (int i = 0; i < nodes.Length; i++)
                        {
                            node.SetNode(field.ConfigName, nodes[i], i, createIfNotFound: true);
                        }
                    }
                    else
                    {
                        String[] values = listInfo.FormatValues();
                        for (int i = 0; i < values.Length; i++)
                        {
                            node.SetValue(field.ConfigName, values[i], i, createIfNotFound: true);
                        }
                    }
                }
                else
                {
                    if (field.IsRegisteredParseType)
                    {
                        string value = CFGUtil.FormatConfigValue(field.Value);
                        node.SetValue(field.ConfigName, value, createIfNotFound: true);
                    }
                    else if (field.IsConfigNodeType)
                    {
                        ConfigNode newNode = new ConfigNode();
                        IConfigNode nodeObj = field.Value as IConfigNode;
                        nodeObj.Save(newNode);
                        node.SetNode(field.ConfigName, newNode, createIfNotFound: true);
                    }
                    else
                    {
                        throw new NotImplementedException("Cannot find a suitable way to save an object of type " + field.Type.Name + " in a ConfigNode");
                    }
                }
            }
        }
    }
}
