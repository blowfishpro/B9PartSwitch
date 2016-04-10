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

    public abstract class ConfigFieldInfo
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
        }

        public string Name => Field.Name;
        public Type Type => Field.FieldType;
        public virtual Type ElementType => Field.FieldType;

        public string ConfigName => Attribute.configName;
        public bool Copy => Attribute.copy;
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

        public virtual void LoadFromNode(ConfigNode node) { }
        public virtual void SaveToNode(ConfigNode node, bool serializing = false) { }

        public virtual void Destroy() { }
    }

    public abstract class ScalarConfigFieldInfo : ConfigFieldInfo
    {
        public ScalarConfigFieldInfo(object parent, FieldInfo field, ConfigField attribute) : base(parent, field, attribute) { }

        public override void Destroy()
        {
            base.Destroy();

            if (Attribute.destroy && Value.IsNotNull() && ElementType.IsSubclassOf(typeof(UnityEngine.Object)))
                UnityEngine.Object.Destroy((UnityEngine.Object)Value);
        }
    }

    public class ValueScalarConfigFieldInfo : ScalarConfigFieldInfo
    {
        public ValueScalarConfigFieldInfo(object parent, FieldInfo field, ConfigField attribute) : base(parent, field, attribute) { }

        public override void LoadFromNode(ConfigNode node)
        {
            base.LoadFromNode(node);

            var value = node.GetValue(ConfigName);

            if (value.IsNull()) return;
            
            Value = CFGUtil.ParseConfigValue(ElementType, value);
        }

        public override void SaveToNode(ConfigNode node, bool serializing)
        {
            base.SaveToNode(node, serializing);

            if (Value.IsNull()) return;

            node.AddValue(ConfigName, CFGUtil.FormatConfigValue(Value)); 
        }
    }

    public class NodeScalarConfigFieldInfo : ScalarConfigFieldInfo
    {
        public NodeScalarConfigFieldInfo(object parent, FieldInfo field, ConfigField attribute) : base(parent, field, attribute)
        {
            if (!ElementType.DerivesFrom(typeof(IConfigNode)))
                throw new ArgumentException("The type " + ElementType.Name + " does not derive from IConfigNode");
        }

        public override void LoadFromNode(ConfigNode node)
        {
            base.LoadFromNode(node);

            var newNode = node.GetNode(ConfigName);

            if (newNode.IsNull()) return;

            var value = (IConfigNode)Value;
            CFGUtil.AssignConfigObject(this, newNode, ref value);
            Value = value;
        }

        public override void SaveToNode(ConfigNode node, bool serializing)
        {
            base.SaveToNode(node, serializing);

            if (Value.IsNull()) return;

            ConfigNode newNode = new ConfigNode(ConfigName);
            ((IConfigNode)Value).Save(newNode);
            node.AddNode(newNode);
        }
    }

    public abstract class ListConfigFieldInfo : ConfigFieldInfo
    {
        private Type elementType;

        public override Type ElementType => elementType;

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

        public ListConfigFieldInfo(object parent, FieldInfo field, ConfigField attribute) : base(parent, field, attribute)
        {
            if (!Type.IsListType())
                throw new ArgumentException("The field " + field.Name + " is not a list");

            elementType = Type.GetGenericArguments()[0];
        }

        protected void CreateListIfNecessary()
        {
            if (List.IsNotNull()) return;

            // If this is going to throw an exception, let it.  If we cannot create a new instance of a list, then something is seriously fucked.
            Value = Activator.CreateInstance(Type);
        }

        protected void ClearList()
        {
            DestroyUnityObjects();

            List.Clear();
        }

        protected void DestroyUnityObjects()
        {
            if (Attribute.destroy && ElementType.DerivesFrom(typeof(UnityEngine.Object)))
            {
                foreach (var item in List)
                {
                    if (item.IsNotNull())
                    {
                        UnityEngine.Object.Destroy((UnityEngine.Object)item);
                    }
                }
            }
        }

        public override void Destroy()
        {
            DestroyUnityObjects();
        }
    }

    public class ValueListConfigFieldInfo : ListConfigFieldInfo
    {
        public ValueListConfigFieldInfo(object parent, FieldInfo field, ConfigField attribute) : base(parent, field, attribute) { }

        public override void LoadFromNode(ConfigNode node)
        {
            base.LoadFromNode(node);

            var values = node.GetValues(ConfigName);

            if (values.Length == 0) return;

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

        public override void SaveToNode(ConfigNode node, bool serializing)
        {
            base.SaveToNode(node, serializing);

            foreach (var value in List)
            {
                node.AddValue(ConfigName, CFGUtil.FormatConfigValue(value));
            }
        }
    }

    public class NodeListConfigFieldInfo : ListConfigFieldInfo
    {
        public NodeListConfigFieldInfo(object parent, FieldInfo field, ConfigField attribute) : base(parent, field, attribute)
        {
            if (!ElementType.DerivesFrom(typeof(IConfigNode)))
                throw new ArgumentException("The type " + ElementType.Name + " does not derive from IConfigNode");
        }

        public override void LoadFromNode(ConfigNode node)
        {
            var nodes = node.GetNodes(ConfigName);
            if (nodes.Length == 0) return;

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

        public override void SaveToNode(ConfigNode node, bool serializing)
        {
            base.SaveToNode(node, serializing);

            foreach (var value in List)
            {
                var newNode = new ConfigNode(ConfigName);
                if (serializing && value is IConfigNodeSerializable)
                    (value as IConfigNodeSerializable).SerializeToNode(newNode);
                else
                    (value as IConfigNode).Save(newNode);

                node.AddNode(newNode);
            }
        }
    }
}
