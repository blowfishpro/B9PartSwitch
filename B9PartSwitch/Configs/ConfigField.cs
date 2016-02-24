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
        public Func<string, object> parseFunction = null;
        public Func<object, string> formatFunction = null;
        public bool copy = true;
        public bool destroy = true;
    }

    public class ConfigFieldInfo
    {
        public Component Instance { get; private set; }
        public FieldInfo Field { get; private set; }
        public ConfigField Attribute { get; private set; }
        
        public ConstructorInfo Constructor { get; protected set; }

        public ConfigFieldInfo(Component instance, FieldInfo field, ConfigField attribute)
        {
            Instance = instance;
            Field = field;
            Attribute = attribute;

            if (string.IsNullOrEmpty(attribute.configName))
                Attribute.configName = Field.Name;

            RealType = Type;
        }

        protected void FindConstructor()
        {
            Constructor = null;
            ConstructorInfo[] constructors = RealType.GetConstructors();

            for (int i = 0; i < constructors.Length; i++)
            {
                ParameterInfo[] parameters = constructors[i].GetParameters();
                if (parameters.Length == 0)
                {
                    Constructor = constructors[i];
                    return;
                }
            }
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

                if (Attribute.parseFunction != null && Attribute.parseFunction.Method.GetGenericArguments()[0] != RealType)
                    throw new ArgumentException("Parse function on ConfigField attribute of field " + Field.Name + " of class " + Field.DeclaringType.Name + " should have return type " + RealType.Name + " (the same as the field), but instead has return type " + Attribute.parseFunction.Method.GetGenericArguments()[0].Name);

                IsComponentType = RealType.IsSubclassOf(typeof(Component));
                IsScriptableObjectType = RealType.IsSubclassOf(typeof(ScriptableObject));
                IsRegisteredParseType = CFGUtil.IsConfigParsableType(RealType);
                IsParsableType = IsRegisteredParseType || Attribute.parseFunction != null;
                IsFormattableType = IsRegisteredParseType || Attribute.formatFunction != null;
                IsConfigNodeType = IsParsableType ? false : RealType.GetInterfaces().Contains(typeof(IConfigNode));
                IsSerializableType = RealType.IsUnitySerializableType();
                IsCopyFieldsType = RealType.GetInterfaces().Contains(typeof(ICopyFields));
                if (!IsSerializableType)
                    Debug.LogWarning("The type " + RealType.Name + " is not a Unity serializable type and thus will not be serialized.  This may lead to unexpected behavior, e.g. the field is null after instantiating a prefab.");

                FindConstructor();
            }
        }
        public bool IsComponentType { get; private set; }
        public bool IsScriptableObjectType { get; private set; }
        public bool IsRegisteredParseType { get; private set; }
        public bool IsParsableType { get; private set; }
        public bool IsFormattableType { get; private set; }
        public bool IsConfigNodeType { get; private set; }
        public bool IsCopyFieldsType { get; private set; }
        public bool IsSerializableType { get; private set; }
        public bool IsPersistant => Attribute.persistant;
        public object Value
        {
            get
            {
                return Field.GetValue(Instance);
            }
            set
            {
                Field.SetValue(Instance, value);
            }
        }
    }

    public class ListFieldInfo : ConfigFieldInfo
    {
        public IList List { get; private set; }
        public int Count => List.Count;
        public ConstructorInfo ListConstructor { get; private set; }

        public ListFieldInfo(Component instance, FieldInfo field, ConfigField attribute)
            : base(instance, field, attribute)
        {
            if (!Type.IsListType())
                throw new ArgumentException("The field " + field.Name + " is not a list");
            List = Field.GetValue(Instance) as IList;

            RealType = Type.GetGenericArguments()[0];

            FindConstructor();

            ConstructorInfo[] constructors = Type.GetConstructors();

            for (int i = 0; i < constructors.Length; i++ )
            {
                ParameterInfo[] parameters = constructors[i].GetParameters();
                if (parameters.Length == 0)
                {
                    ListConstructor = constructors[i];
                    break;
                }
            }

            if (Attribute.parseFunction == null && IsConfigNodeType && Constructor == null)
            {
                throw new MissingMethodException("A default constructor is required for the IConfigNode type " + RealType.Name + " (constructor required to parse list field " + field.Name + " in class " + Instance.GetType().Name + ")");
            }
        }

        internal void CreateListIfNecessary()
        {
            if (List == null && ListConstructor != null)
            {
                Value = Constructor.Invoke(null);
                List = Value as IList;
            }

            if (List == null)
                throw new ArgumentNullException("Field is null and cannot initialize as new list of type " + Type.Name);
        }

        public void ParseNodes(ConfigNode[] nodes)
        {
            if (!IsConfigNodeType)
                throw new NotImplementedException("The generic type of this list (" + RealType.Name + ") is not an IConfigNode");
            if (nodes.Length == 0)
                return;

            CreateListIfNecessary();

            bool createNewItems = false;
            if (!IsCopyFieldsType || Count != nodes.Length)
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
            if (!IsCopyFieldsType || Count != values.Length)
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

        public ConfigNode[] FormatNodes()
        {
            if (!IsConfigNodeType)
                throw new NotImplementedException("The generic type of this list (" + RealType.Name + ") is not an IConfigNode");

            ConfigNode[] nodes = new ConfigNode[Count];

            for (int i = 0; i < Count; i++)
            {
                nodes[i] = new ConfigNode();
                IConfigNode obj = List[i] as IConfigNode;
                obj.Save(nodes[i]);
            }

            return nodes;
        }

        public string[] FormatValues()
        {
            if (IsConfigNodeType)
                throw new NotImplementedException("The generic type of this list (" + RealType.Name + ") is an IConfigNode");
            if (!IsFormattableType)
                throw new NotImplementedException("The generic type of this list (" + RealType.Name + ") is not a registered parse type");

            string[] values = new string[Count];

            Func<object, string> formatFunction = Attribute.formatFunction != null ? Attribute.formatFunction : CFGUtil.FormatConfigValue;

            String s = string.Empty;
            formatFunction(s);

            for (int i = 0; i < Count; i++)
            {
                values[i] = formatFunction(List[i]);
            }

            return values;
        }

        public void ClearList()
        {
            if ((IsComponentType || IsScriptableObjectType || IsCopyFieldsType) && Attribute.destroy)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (List[i] != null)
                    {
                        if (IsComponentType || IsScriptableObjectType)
                            UnityEngine.Object.Destroy(List[i] as UnityEngine.Object);
                        else if (IsCopyFieldsType)
                            (List[i] as ICopyFields).OnDestroy();
                    }
                }
            }

            List.Clear();
        }
    }
}
