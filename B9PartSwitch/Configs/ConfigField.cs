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
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigField : Attribute
    {
        public bool persistant = false;
        public string configName = null;
        // public Func<string, object> parseFunction = null;
        // public Func<object, string> formatFunction = null;
        public bool copy = true;
        public bool destroy = true;
    }

    public class ConfigFieldInfo
    {
        public object Parent { get; private set; }
        public FieldInfo Field { get; private set; }
        public ConfigField Attribute { get; private set; }

        public ConfigFieldInfo(object parent, FieldInfo field, ConfigField attribute)
        {
            Parent = parent;
            Field = field;
            Attribute = attribute;

            if (string.IsNullOrEmpty(attribute.configName))
                Attribute.configName = Field.Name;

            RealType = Type;
        }

        public string Name => Field.Name;
        public string ConfigName => Attribute.configName;
        public Type Type => Field.FieldType;
        public bool Copy => Attribute.copy;
        private Type realType;
        public Type RealType
        {
            get
            {
                return realType;
            }
            protected set
            {
                realType = value;

                // if (Attribute.parseFunction != null && Attribute.parseFunction.Method.GetGenericArguments()[0] != RealType)
                //     throw new ArgumentException("Parse function on ConfigField attribute of field " + Field.Name + " of class " + Field.DeclaringType.Name + " should have return type " + RealType.Name + " (the same as the field), but instead has return type " + Attribute.parseFunction.Method.GetGenericArguments()[0].Name);

                IsComponentType = RealType.IsSubclassOf(typeof(Component));
                IsScriptableObjectType = RealType.IsSubclassOf(typeof(ScriptableObject));
                IsRegisteredParseType = CFGUtil.IsConfigParsableType(RealType);
                IsParsableType = IsRegisteredParseType; // || Attribute.parseFunction != null;
                IsFormattableType = IsRegisteredParseType; // || Attribute.formatFunction != null;
                IsConfigNodeType = IsParsableType ? false : RealType.GetInterfaces().Contains(typeof(IConfigNode));
                IsSerializableType = RealType.IsUnitySerializableType();
                if (!IsSerializableType)
                    Debug.LogWarning("The type " + RealType.Name + " is not a Unity serializable type and thus will not be serialized.  This may lead to unexpected behavior, e.g. the field is null after instantiating a prefab.");
            }
        }
        public bool IsComponentType { get; private set; }
        public bool IsScriptableObjectType { get; private set; }
        public bool IsRegisteredParseType { get; private set; }
        public bool IsParsableType { get; private set; }
        public bool IsFormattableType { get; private set; }
        public bool IsConfigNodeType { get; private set; }
        public bool IsSerializableType { get; private set; }
        public bool IsPersistant => Attribute.persistant;
        public object Value
        {
            get
            {
                return Field.GetValue(Parent);
            }
            set
            {
                Field.SetValue(Parent, value);
            }
        }

        public virtual void Destroy()
        {
            if (Attribute.destroy && Value.IsNotNull() && (IsComponentType || IsScriptableObjectType))
                UnityEngine.Object.Destroy((UnityEngine.Object)Value);
        }
    }

    public class ListFieldInfo : ConfigFieldInfo
    {
        public IList List
        {
            get
            {
                return (IList)Value;
            }
            set
            {
                Value = value;
            }
        }

        public int Count => List.Count;

        public ListFieldInfo(object parent, FieldInfo field, ConfigField attribute)
            : base(parent, field, attribute)
        {
            if (!Type.IsListType())
                throw new ArgumentException("The field " + field.Name + " is not a list");

            RealType = Type.GetGenericArguments()[0];
        }

        internal void CreateListIfNecessary()
        {
            if (List.IsNotNull()) return;

            // If this is going to throw an exception, let it.  If we cannot create a new instance of a list, then something is seriously fucked.
            Value = Activator.CreateInstance(Type);
        }

        public void ParseNodes(ConfigNode[] nodes)
        {
            if (!IsConfigNodeType)
                throw new NotImplementedException("The generic type of this list (" + RealType.Name + ") is not an IConfigNode");
            if (nodes.Length == 0)
                return;

            CreateListIfNecessary();

            bool createNewItems = false;
            if (Count != nodes.Length)
            {
                ClearList();
                createNewItems = true;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                IConfigNode obj = null;
                if (!createNewItems)
                    obj = List[i] as IConfigNode;

                CFGUtil.AssignConfigObject(this, nodes[i], ref obj);

                if (createNewItems)
                    List.Add(obj);
                else
                    List[i] = obj; // This may be self-assignment under certain circumstances
            }
        }

        public void ParseValues(string[] values)
        {
            if (!IsParsableType)
                throw new NotImplementedException("The generic type of this list (" + RealType.Name + ") is not a registered parse type");
            if (values.Length == 0)
                return;

            CreateListIfNecessary();
            bool createNewItems = false;
            if (Count != values.Length)
            {
                ClearList();
                createNewItems = true;
            }

            for (int i = 0; i < values.Length; i++)
            {
                object obj = null;

                if (!createNewItems)
                    obj = List[i];

                CFGUtil.AssignConfigObject(this, values[i], ref obj);

                if (createNewItems)
                    List.Add(obj);
                else
                    List[i] = obj; // This may be self-assignment under certain circumstances
            }
        }

        public IEnumerable<ConfigNode> FormatNodes(bool serializing = false)
        {
            if (!IsConfigNodeType)
                throw new NotImplementedException("The generic type of this list (" + RealType.Name + ") is not an IConfigNode");

            foreach (var value in List)
            {
                var node = new ConfigNode();
                if (serializing && value is IConfigNodeSerializable)
                    (value as IConfigNodeSerializable).SerializeToNode(node);
                else
                    (value as IConfigNode).Save(node);
                yield return node;
            }
        }

        public IEnumerable<string> FormatValues()
        {
            if (IsConfigNodeType)
                throw new NotImplementedException("The generic type of this list (" + RealType.Name + ") is an IConfigNode");
            if (!IsFormattableType)
                throw new NotImplementedException("The generic type of this list (" + RealType.Name + ") is not a registered parse type");

            // Func<object, string> formatFunction = Attribute.formatFunction != null ? Attribute.formatFunction : CFGUtil.FormatConfigValue;

            // String s = string.Empty;
            // CFGUtil.FormatConfigValue(s);

            foreach (var value in List)
            {
                yield return CFGUtil.FormatConfigValue(value);
            }
        }

        public void ClearList()
        {
            if (Attribute.destroy && (IsComponentType || IsScriptableObjectType))
            {
                foreach (var item in List)
                {
                    if (item.IsNotNull())
                    {
                        UnityEngine.Object.Destroy((UnityEngine.Object)item);
                    }
                }
            }

            List.Clear();
        }

        public override void Destroy()
        {
            ClearList();
        }
    }
}
