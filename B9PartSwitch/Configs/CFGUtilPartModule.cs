using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace B9PartSwitch
{
    public abstract class CFGUtilPartModule : PartModule
    {
        #region Fields

        [ConfigField(persistant = true)]
        public string moduleID;

        protected ConfigFieldList configFieldList;

        protected Part prefab;

        #endregion

        #region Setup

        public override void OnAwake()
        {
            base.OnAwake();

            configFieldList = new ConfigFieldList(this);
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            List<CFGUtilPartModule> partModules = part.FindModulesImplementing<CFGUtilPartModule>();
            for (int i = 0; i < partModules.Count; i++)
            {
                CFGUtilPartModule m = partModules[i];
                if (m == this) continue;
                if (m.GetType() == GetType())
                {
                    if (string.IsNullOrEmpty(m.moduleID) || string.IsNullOrEmpty(moduleID) || m.moduleID == moduleID)
                    {
                        LogError("Two " + GetType().Name + " modules on the same part must have different (and non-empty) moduleID identifiers.  The second " + GetType().Name + " will be removed");
                        part.Modules.Remove(m);
                        Destroy(m);
                    }
                }
            }

            prefab = part.GetPrefab();
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (!string.IsNullOrEmpty(moduleID) && node.HasValue("moduleID"))
            {
                string newID = node.GetValue("moduleID");
                if (!string.Equals(moduleID, newID))
                {
                    LogError("Attempt to load ConfigNode with a conflicting moduleID: '" + newID + "'");
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
                        if(string.Equals(module.moduleID, moduleID))
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

        #region Logging

        protected void LogWarning(string message)
        {
            string log = "Warning on ";
            log += this.ToString();
            log += ": ";
            log += message;
            Debug.LogWarning(message);
        }

        protected void LogError(string message)
        {
            string log = "Error on ";
            log += this.ToString();
            log += ": ";
            log += message;
            Debug.LogError(message);
        }

        public override string ToString()
        {
            string log = "PartModule " + this.GetType().Name;
            if (!string.IsNullOrEmpty(moduleID))
                log += " with moduleID '" + moduleID + "'";
            if (part != null)
                log += " on part " + part.name;
            return log;
        }

        #endregion
    }
}
