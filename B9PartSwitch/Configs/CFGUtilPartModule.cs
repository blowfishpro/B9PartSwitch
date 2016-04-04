using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace B9PartSwitch
{
    public abstract class CFGUtilPartModule : PartModule, ISerializationCallbackReceiver
    {
        #region Fields

        [ConfigField(persistant = true)]
        public string moduleID;

        protected ConfigFieldList configFieldList;

        [SerializeField]
        private SerializedDataContainer serializedData;

        #endregion

        #region Setup

        public override void OnAwake()
        {
            base.OnAwake();

            CreateConfigFieldList();
        }

        public void CreateConfigFieldList()
        {
            if (configFieldList.IsNull())
                configFieldList = new ConfigFieldList(this);
        }

        private void Start()
        {
            // Cast to array so that there aren't issues with modifying the enumerable in a loop
            var otherModules = part.Modules.OfType<CFGUtilPartModule>().Where(m => m != this && m.GetType() == this.GetType()).ToArray();
            if (otherModules.Length > 0 && string.IsNullOrEmpty(moduleID))
            {
                LogError("Must have a moduleID defined if more than one " + this.GetType().Name + " is present on a part.  This module will be removed");
                part.Modules.Remove(this);
                Destroy(this);
                return;
            }

            foreach (var m in otherModules)
            {
                if (string.IsNullOrEmpty(m.moduleID) || m.moduleID == moduleID)
                {
                    LogError("Two " + GetType().Name + " modules on the same part must have different (and non-empty) moduleID identifiers.  The second " + GetType().Name + " will be removed");
                    part.Modules.Remove(m);
                    Destroy(m);
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (!string.IsNullOrEmpty(moduleID) && node.HasValue(nameof(moduleID)))
            {
                string newID = node.GetValue(nameof(moduleID));
                if (!string.Equals(moduleID, newID))
                {
                    var correctModule = part.Modules.OfType<CFGUtilPartModule>().FirstOrDefault(m => m != this && m.GetType() == this.GetType() && m.moduleID == newID);
                    if (correctModule.IsNotNull())
                    {
                        LogWarning("OnLoad was called with the wrong ModuleID ('" + newID + "'), but found the correct module to load");
                        correctModule.Load(node);
                    }
                    else
                    {
                        LogError("OnLoad was called with the wrong ModuleID and the correct module could not be found");
                    }
                    return;
                }
            }

            configFieldList.Load(node);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            configFieldList.Save(node);
        }

        public void OnDestroy()
        {
            configFieldList.OnDestroy();
        }

        #endregion

        #region Public Methods

        public List<CFGUtilPartModule> FindSymmetryCounterparts()
        {
            List<CFGUtilPartModule> returnList = new List<CFGUtilPartModule>();
            if (part == null)
                return returnList;

            for (int i = 0; i < part.symmetryCounterparts.Count; i++)
            {
                Part symPart = part.symmetryCounterparts[i];
                bool foundCounterpart = false;

                for (int j = 0; j < symPart.Modules.Count; j++)
                {
                    if (symPart.Modules[j].GetType() == this.GetType())
                    {
                        CFGUtilPartModule module = symPart.Modules[j] as CFGUtilPartModule;
                        if (string.Equals(module.moduleID, moduleID))
                        {
                            returnList.Add(module);
                            foundCounterpart = true;
                        }
                    }
                }

                if (!foundCounterpart)
                    LogWarning("No symmetry counterpart found on part counterpart " + i.ToString());
            }

            return returnList;
        }

        #endregion

        #region Serialization Methods

        public void OnBeforeSerialize()
        {
            ConfigNode node = new ConfigNode("SERIALIZED_NODE");

            configFieldList.Save(node, true);

            serializedData = ScriptableObject.CreateInstance<SerializedDataContainer>();
            serializedData.data = node.ToString();
        }

        public void OnAfterDeserialize()
        {
            if (serializedData.IsNull())
            {
                LogError("The serialized data container is null");
                return;
            }
            if (serializedData.data.IsNull())
            {
                LogError("The serialized data is null");
                return;
            }

            ConfigNode node = ConfigNode.Parse(serializedData.data);

            CreateConfigFieldList();

            configFieldList.Load(node.GetNode("SERIALIZED_NODE"));

            Destroy(serializedData);
            serializedData = null;
        }

        #endregion

        #region Logging

        protected string LogString(string message)
        {
            string log = this.ToString();
            log += ": ";
            log += message;
            return log;
        }

        protected void LogInfo(string message)
        {
            Debug.Log(LogString(message));
        }

        protected void LogWarning(string message)
        {
            Debug.LogWarning(LogString(message));
        }

        protected void LogError(string message)
        {
            Debug.LogError(LogString(message));
        }

        public override string ToString()
        {
            string log = this.GetType().Name;
            if (!string.IsNullOrEmpty(moduleID))
                log += " (moduleID='" + moduleID + "')";
            if (part != null)
                log += " on part " + part.name;
            return log;
        }

        #endregion
    }
}
