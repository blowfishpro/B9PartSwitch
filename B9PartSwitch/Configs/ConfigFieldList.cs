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
        public Component Instance { get; private set; }
        private List<ConfigFieldInfo> configFields = new List<ConfigFieldInfo>();

        public ConfigFieldList(Component instance)
        {
            Instance = instance;
            configFields.Clear();
            FieldInfo[] fields = instance.GetType().GetFields();
            for (int i = 0; i < fields.Length; i++ )
            {
                FieldInfo field = fields[i];
                bool kspField = false;
                ConfigField configField = null;

                object[] attributes = field.GetCustomAttributes(true);
                for (int j = 0; j < attributes.Length; j++)
                {
                    if (attributes[j] is ConfigField)
                    {
                        configField = attributes[j] as ConfigField;
                    }
                    else if (attributes[j] is KSPField)
                    {
                        kspField = true;
                    }
                }

                if (configField != null)
                {
                    if (kspField)
                        throw new NotSupportedException("The property ConfigField is not allowed on a field that also has the KSPField property");

                    for (int j = 0; j < configFields.Count; j++)
                    {
                        if (configFields[j].ConfigName == configField.configName)
                            throw new NotSupportedException("Two ConfigField properties in the same class cannot have the same name (fields are " + configFields[j].Name + " and " + field.Name + ")");
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
                for (int i = 0; i < configFields.Count; i++)
                {
                    if (configFields[i].Name == name)
                        return configFields[i];
                }
                return null;
            }
        }

        public void Load(ConfigNode node)
        {
            Debug.Log("Loading " + Instance.GetType().Name + " from config");
            for (int i = 0; i < configFields.Count; i++)
            {
                ConfigFieldInfo field = configFields[i];
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
                        if (value != null)
                        {
                            object result = field.Value;

                            CFGUtil.AssignConfigObject(field, value, ref result);
                            field.Value = result;
                        }
                    }
                    else if (field.IsConfigNodeType)
                    {
                        ConfigNode newNode = node.GetNode(field.ConfigName);
                        if (newNode != null)
                        {
                            IConfigNode result = field.Value as IConfigNode;
                            CFGUtil.AssignConfigObject(field, newNode, ref result);
                            field.Value = result;
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
            for (int i = 0; i < configFields.Count; i++)
            {
                ConfigFieldInfo field = configFields[i];
                if (!field.IsPersistant)
                    continue;

                if (field is ListFieldInfo)
                {
                    ListFieldInfo listInfo = field as ListFieldInfo;
                    if (listInfo.Attribute.formatFunction != null || !listInfo.IsConfigNodeType)
                    {
                        if (!listInfo.IsParsableType)
                            throw new NotImplementedException("No suitable way to format values in list field " + listInfo.Name + " of type " + listInfo.RealType.Name);

                        String[] values = listInfo.FormatValues();
                        for (int j = 0; j < values.Length; j++)
                        {
                            node.SetValue(field.ConfigName, values[j], j, createIfNotFound: true);
                        }
                    }
                    else if (listInfo.IsConfigNodeType)
                    {
                        ConfigNode[] nodes = listInfo.FormatNodes();
                        for (int j = 0; j < nodes.Length; j++)
                        {
                            node.SetNode(field.ConfigName, nodes[j], j, createIfNotFound: true);
                        }
                    }
                    else
                    {
                        Debug.LogError("This code should never be reached.  Please report this");
                    }
                }
                else
                {
                    if (field.Attribute.formatFunction != null)
                    {
                        string value = field.Attribute.formatFunction(field.Value);
                        node.SetValue(field.ConfigName, value, createIfNotFound: true);
                    }
                    else if (field.IsConfigNodeType)
                    {
                        ConfigNode newNode = new ConfigNode();
                        IConfigNode nodeObj = field.Value as IConfigNode;
                        nodeObj.Save(newNode);
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

        public static void CopyList(ref ConfigFieldList source, ref ConfigFieldList dest)
        {
            if (System.Object.ReferenceEquals(source, dest))
                return;

            Debug.Log("Initiating copy on type " + source.Instance.GetType().Name);

            if (source.Instance.GetType() != dest.Instance.GetType())
                throw new ArgumentException("Source and destination must be of the same type");
            if (source.configFields.Count != dest.configFields.Count)
                throw new ArgumentException("Source and destination must have the same number of fields");

            int count = source.configFields.Count;

            for (int i = 0; i < count; i++)
            {
                ConfigFieldInfo sourceField = source.configFields[i];
                ConfigFieldInfo destField = dest.configFields[i];

                if (sourceField.Name != destField.Name)
                    throw new ArgumentException("Source and dest fields with the same index do not have the same name (source field is " + sourceField.Name + " and dest field is " + destField.Name + ")");

                if (sourceField.GetType() != destField.GetType())
                    throw new ArgumentException("Source and dest ConfigFieldInfo with the same index do not have the same type (source field is " + sourceField.GetType().Name + " and dest field is " + destField.GetType().Name + ")");

                if (sourceField.RealType != destField.RealType)
                    throw new ArgumentException("Source and dest fields with the same index do not have the same type (source field is " + sourceField.RealType.Name + " and dest field is " + destField.RealType.Name + ")");
            }

            for (int i = 0; i < count; i++)
            {
                ConfigFieldInfo sourceField = source.configFields[i];
                ConfigFieldInfo destField = dest.configFields[i];

                if (!sourceField.Copy)
                    continue;

                if (sourceField.Value == null)
                {
                    destField.Value = null;
                    continue;
                }

                Type realType = sourceField.RealType;

                if (sourceField is ListFieldInfo)
                {
                    ListFieldInfo sourceListInfo = sourceField as ListFieldInfo;
                    ListFieldInfo destListInfo = destField as ListFieldInfo;

                    destListInfo.CreateListIfNecessary();

                    int listCount = sourceListInfo.Count;

                    bool createNewItems = false;
                    if (destListInfo.Count != listCount)
                    {
                        createNewItems = true;
                        destListInfo.ClearList();
                    }

                    for (int j = 0; j < listCount; j++)
                    {
                        object sourceItem = sourceListInfo.List[j];
                        object destItem = null;
                        if (!createNewItems)
                            destItem = destListInfo.List[j];
                        if (sourceListInfo.IsCopyFieldsType)
                        {
                            if (destItem == null)
                            {
                                if (destField.IsComponentType)
                                    destItem = dest.Instance.gameObject.AddComponent(realType);
                                else if (destField.IsScriptableObjectType)
                                    destItem = ScriptableObject.CreateInstance(realType);
                                else if (destField.Constructor != null)
                                    destItem = destField.Constructor.Invoke(null);
                                else
                                    throw new MissingMethodException("No default constructor could be found for type " + realType.Name);
                            }
                            (destItem as ICopyFields).CopyFrom(sourceItem as ICopyFields);
                        }
                        else if (realType.GetInterfaces().Contains(typeof(ICloneable)))
                        {
                            destItem = (sourceItem as ICloneable).Clone();
                        }
                        else if (realType.IsValueType)
                        {
                            destItem = realType;
                        }
                        else if (CFGUtil.IsParsableType(realType))
                        {
                            destItem = CFGUtil.ParseConfigValue(realType, CFGUtil.FormatConfigValue(sourceItem));
                        }
                        else
                        {
                            destItem = sourceItem;
                        }

                        if (createNewItems)
                            destListInfo.List.Add(destItem);
                        else
                            destListInfo.List[j] = destItem;
                    }
                }
                else
                {
                    if (sourceField.IsCopyFieldsType)
                    {
                        if (destField.Value == null)
                        {
                            if (destField.IsComponentType)
                                destField.Value = dest.Instance.gameObject.AddComponent(realType);
                            else if (destField.IsScriptableObjectType)
                                destField.Value = ScriptableObject.CreateInstance(realType);
                            else if (destField.Constructor != null)
                                destField.Value = destField.Constructor.Invoke(null);
                        }
                        (destField.Value as ICopyFields).CopyFrom(sourceField.Value as ICopyFields);
                    }
                    else if (realType.GetInterfaces().Contains(typeof(ICloneable)))
                    {
                        destField.Value = (sourceField.Value as ICloneable).Clone();
                    }
                    else if (realType.IsValueType)
                    {
                        destField.Value = sourceField.Value;
                    }
                    else if (CFGUtil.IsParsableType(realType))
                    {
                        destField.Value = CFGUtil.ParseConfigValue(realType, CFGUtil.FormatConfigValue(sourceField.Value));
                    }
                    else
                    {
                        destField.Value = sourceField.Value;
                    }
                }
            }
        }

        public void OnDestroy()
        {
            for (int i = 0; i < configFields.Count; i++)
            {
                ConfigFieldInfo field = configFields[i];

                if (!field.Attribute.destroy)
                    continue;

                if (field is ListFieldInfo)
                    (field as ListFieldInfo).ClearList();
                else if (field.IsComponentType || field.IsScriptableObjectType)
                    UnityEngine.Object.Destroy(field.Value as UnityEngine.Object);
                else if (field.IsCopyFieldsType)
                    (field.Value as ICopyFields).OnDestroy();
            }
        }
    }

    public interface ICopyFields
    {
        void CopyFrom(ICopyFields source);
        void OnDestroy();
    }
}
