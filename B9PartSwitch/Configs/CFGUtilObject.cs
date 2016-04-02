using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace B9PartSwitch
{
    [Serializable]
    public abstract class CFGUtilObject : IConfigNodeSerializable, ICloneable
    {
        [NonSerialized]
        protected ConfigFieldList configFieldList;

        public CFGUtilObject()
        {
            configFieldList = new ConfigFieldList(this);
        }

        public void Load(ConfigNode node)
        {
            configFieldList.Load(node);
            OnLoad(node);
        }

        public void Save(ConfigNode node)
        {
            configFieldList.Save(node);
            OnSave(node);
        }

        public void SerializeToNode(ConfigNode node)
        {
            configFieldList.Save(node, true);
            OnSave(node);
        }

        virtual public void OnLoad(ConfigNode node) { }

        virtual public void OnSave(ConfigNode node) { }

        public object Clone()
        {
            ConstructorInfo constructor = this.GetType().GetConstructor(Type.EmptyTypes);
            if (constructor.IsNull())
                throw new MissingMemberException("Cannot clone because no public parameterless constructor could be found");

            var obj = constructor.Invoke(null) as CFGUtilObject;

            ConfigNode node = new ConfigNode();
            SerializeToNode(node);
            obj.Load(node);

            return obj;
        }

        ~CFGUtilObject()
        {
            configFieldList.OnDestroy();
        }
    }
}
