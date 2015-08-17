using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace B9PartSwitch
{
    [Serializable]
    public abstract class CFGUtilObject : MonoBehaviour, IConfigNode, ICopyFields
    {
        protected ConfigFieldList configFieldList;

        public void Awake()
        {
            CreateFieldList();
            OnAwake();
        }

        protected void CreateFieldList()
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

        virtual public void OnAwake() { }

        virtual public void OnLoad(ConfigNode node) { }

        virtual public void OnSave(ConfigNode node) { }

        public void CopyFrom(ICopyFields source)
        {
            if (GetType() != source.GetType())
                throw new NotImplementedException("Can only copy fields from an object of the same type (this is " + GetType().Name + " but source is " + source.GetType().Name + ")");

            CFGUtilObject realSource = source as CFGUtilObject;

            ConfigFieldList.CopyList(ref realSource.configFieldList, ref configFieldList);
        }
    }
}
