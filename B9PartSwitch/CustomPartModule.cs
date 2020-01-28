using System;
using System.Diagnostics.CodeAnalysis;
using UniLinq;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.Logging;

namespace B9PartSwitch
{
    public abstract class CustomPartModule : PartModule, ISerializationCallbackReceiver
    {
        public const string CURRENT_UPGRADE = "CURRENTUPGRADE";

        #region Fields

        [NodeData(persistent = true)]
        public string moduleID;

        [SerializeField]
        private SerializedDataContainer serializedData;

        protected Logging.ILogger logger;

        #endregion

        #region Setup

        public override void OnAwake()
        {
            logger = CreateLogger();
        }

        [SuppressMessage("Code Quality", "IDE0051", Justification = "Called by Unity")]
        private void Start()
        {
            // Cast to array so that there aren't issues with modifying the enumerable in a loop
            var otherModules = part.Modules.OfType<CustomPartModule>().Where(m => m != this && m.GetType() == GetType()).ToArray();
            if (otherModules.Length > 0 && moduleID.IsNullOrEmpty())
            {
                logger.Error("Must have a moduleID defined if more than one " + GetType().Name + " is present on a part.  This module will be removed");
                part.Modules.Remove(this);
                Destroy(this);
                return;
            }

            foreach (var m in otherModules)
            {
                if (m.moduleID.IsNullOrEmpty() || m.moduleID == moduleID)
                {
                    logger.Error("Two " + GetType().Name + " modules on the same part must have different (and non-empty) moduleID identifiers.  The second " + GetType().Name + " will be removed");
                    part.Modules.Remove(m);
                    Destroy(m);
                }
            }
        }

        #endregion

        #region Load

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (!moduleID.IsNullOrEmpty() && node.HasValue(nameof(moduleID)))
            {
                string newID = node.GetValue(nameof(moduleID));
                if (!string.Equals(moduleID, newID))
                {
                    var correctModule = part.Modules.OfType<CustomPartModule>().FirstOrDefault(m => m != this && m.GetType() == GetType() && m.moduleID == newID);
                    if (correctModule.IsNotNull())
                    {
                        logger.Warning("OnLoad was called with the wrong ModuleID ('" + newID + "'), but found the correct module to load");
                        correctModule.Load(node);
                    }
                    else
                    {
                        logger.Error("OnLoad was called with the wrong ModuleID and the correct module could not be found");
                    }
                    return;
                }
            }

            bool loadingPrefab = part.partInfo.IsNull() || node.name == CURRENT_UPGRADE;
            Operation operation = loadingPrefab ? Operation.LoadPrefab : Operation.LoadInstance;
            OperationContext context = new OperationContext(operation, this);

            try
            {
                this.LoadFields(node, context);
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception($"Fatal exception while loading fields on module {this}", ex);
                logger.FatalException(ex2);
                throw ex2;
            }

            if (loadingPrefab)
                OnLoadPrefab(node);
            else
                OnLoadInstance(node);
        }

        protected virtual void OnLoadPrefab(ConfigNode node) { }
        protected virtual void OnLoadInstance(ConfigNode node) { }

        #endregion

        #region Save

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            OperationContext context = new OperationContext(Operation.Save, this);

            try
            {
                this.SaveFields(node, context);
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception($"Fatal exception while saving fields on module {this}", ex);
                logger.FatalException(ex2);
                throw ex2;
            }
        }

        #endregion

        #region Serialization Methods

        public virtual void OnBeforeSerialize()
        {
            serializedData = this.SerializeToContainer();
        }

        public virtual void OnAfterDeserialize()
        {
            this.DeserializeFromContainer(serializedData);

            Destroy(serializedData);
            serializedData = null;
        }

        #endregion

        #region Logging

        protected Logging.ILogger CreateLogger()
        {
            string partName = part?.partInfo?.name ?? part?.name;
            string moduleName = GetType().FullName;
            string tagString;
            if (moduleID.IsNullOrEmpty())
                tagString = $"{partName} {moduleName}";
            else
                tagString = $"{partName} {moduleName} '{moduleID}'";
            return new PrefixLogger(SystemLogger.Logger, tagString);
        }

        public override string ToString()
        {
            string log = this.GetType().Name;
            if (!moduleID.IsNullOrEmpty())
                log += " (moduleID='" + moduleID + "')";
            if (part != null)
                log += " on part " + part.partInfo?.name ?? part.name;
            return log;
        }

        #endregion
    }
}
