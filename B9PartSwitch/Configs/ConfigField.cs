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
    }

    public class ConfigFieldInfo
    {
        public Behaviour Instance { get; private set; }
        public FieldInfo Field { get; private set; }
        public ConfigField Attribute { get; private set; }
        
        public ConstructorInfo Constructor { get; protected set; }

        public ConfigFieldInfo(Behaviour instance, FieldInfo field, ConfigField attribute)
        {
            Instance = instance;
            Field = field;
            Attribute = attribute;

            if (Attribute.configName == null || Attribute.configName == string.Empty)
                Attribute.configName = Field.Name;

            // On derived classes this might be null in which FindConstructor needs to be called
            if (RealType != null)
                FindConstructor();
        }

        protected void FindConstructor()
        {
            Constructor = null;
            ConstructorInfo[] constructors = RealType.GetConstructors();

            foreach (ConstructorInfo constructor in constructors)
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                if (parameters.Length == 0)
                {
                    Constructor = constructor;
                    return;
                }
            }
        }

        public string Name { get { return Field.Name; } }
        public string ConfigName { get { return Attribute.configName; } }
        public Type Type { get { return Field.FieldType; } }
        public virtual Type RealType { get { return Type; } }
        public virtual bool IsRegisteredParseType
        {
            get
            {
                return CFGUtil.ParseTypeRegistered(RealType);
            }
        }
        public virtual bool IsConfigNodeType
        {
            get
            {
                if (IsRegisteredParseType) return false;
                return RealType.GetInterfaces().Contains(typeof(IConfigNode));
            }
        }
        public bool IsPersistant { get { return Attribute.persistant; } }
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
        public Type ListType { get; private set; }
        public override Type RealType { get { return ListType; } }
        public int Count { get { return List.Count; } }

        public ListFieldInfo(Behaviour instance, FieldInfo field, ConfigField attribute)
            : base(instance, field, attribute)
        {
            List = Field.GetValue(Instance) as IList;
            if (List == null)
                throw new ArgumentNullException("Cannot initialize with a null list (or object is not a list)");
            ListType = Type.GetGenericArguments()[0];

            FindConstructor();

            if (IsConfigNodeType && Constructor == null)
            {
                throw new MissingMethodException("A default constructor is required for the IConfigNode type " + ListType.Name + " (constructor required to parse list field " + field.Name + " in class " + Instance.GetType().Name + ")");
            }
        }

        public void ParseNodes(ConfigNode[] nodes)
        {
            if (!IsConfigNodeType)
                throw new NotImplementedException("The generic type of this list (" + ListType.Name + ") is not an IConfigNode");
            if (nodes.Length == 0)
                return;

            List.Clear();
            foreach (ConfigNode node in nodes)
            {
                IConfigNode obj;
                if (ListType.IsSubclassOf(typeof(Component)))
                    obj = Instance.gameObject.AddComponent(ListType) as IConfigNode;
                else
                    obj = Constructor.Invoke(null) as IConfigNode;

                obj.Load(node);
                List.Add(obj);
            }
        }

        public void ParseValues(string[] values)
        {
            if (!IsRegisteredParseType)
                throw new NotImplementedException("The generic type of this list (" + ListType.Name + ") is not a registered parse type");
            if (values.Length == 0)
                return;

            List.Clear();
            foreach (string value in values)
            {
                object obj = CFGUtil.ParseConfigValue(ListType, value);
                List.Add(obj);
            }
        }

        public ConfigNode[] FormatNodes()
        {
            if (!IsConfigNodeType)
                throw new NotImplementedException("The generic type of this list (" + ListType.Name + ") is not an IConfigNode");

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
                throw new NotImplementedException("The generic type of this list (" + ListType.Name + ") is an IConfigNode");
            if (!IsRegisteredParseType)
                throw new NotImplementedException("The generic type of this list (" + ListType.Name + ") is not a registered parse type");

            string[] values = new string[Count];

            for (int i = 0; i < Count; i++)
            {
                values[i] = CFGUtil.FormatConfigValue(List[i]);
            }

            return values;
        }
    }
}
