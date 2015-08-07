using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace B9PartSwitch
{
    [Serializable]
    class ModuleMiniMFT : CFGUtilPartModule, IPartMassModifier2, IPartCostModifier
    {
        #region Fields

        [ConfigField]
        public TankPartType tankPartType;

        [ConfigField]
        public float volume = 0f;

        [ConfigField(persistant = true)]
        public int currentTankType;

        [KSPField(guiActiveEditor = true, guiName = "Current Tank Type")]
        public string currentTankTypeString;

        private float moduleMass = 0f;

        private PartMassModifierModule massModifier = null;

        private List<Action<TankType>> tankModifiedActions = new List<Action<TankType>>();

        private bool loaded = false;

        #endregion

        #region Events

        [KSPEvent(guiActiveEditor = true, guiName = "Previous Tank Setup")]
        public void PrevTankSetup()
        {
            if (currentTankType == 0)
                currentTankType += tankPartType.allowedTankTypes.Count;
            currentTankType -= 1;

            UpdateTankSetup(true);
        }

        [KSPEvent(guiActiveEditor = true, guiName = "Next Tank Setup")]
        public void NextTankSetup()
        {
            if (currentTankType == tankPartType.allowedTankTypes.Count - 1)
                currentTankType = -1;
            currentTankType += 1;

            UpdateTankSetup(true);
        }

        #endregion

        #region Properties

        public TankType CurrentTankType { get { return tankPartType[currentTankType]; } }

        #endregion

        #region Setup

        public override void OnAwake()
        {
            base.OnAwake();
            Debug.Log("OnAwake");
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            if (tankPartType == null)
                Debug.Log("tankPartType is null!");

            if (currentTankType > tankPartType.allowedTankTypes.Count)
                currentTankType = 0;

            massModifier = part.FindModuleImplementing<PartMassModifierModule>();

            if (massModifier == null)
                throw new PartModuleMissingException<PartMassModifierModule>(part);

            UpdateTankSetup(false);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            loaded = true;
            Debug.Log("Loaded...");
            Debug.Log((tankPartType == null).ToString());
            Debug.Log("volume :" + volume.ToString());
        }

        #endregion

        #region Interface Methods

        public float GetModuleMass(float defaultMass)
        {
            return volume * (tankPartType.addedMass + CurrentTankType.tankMass);
        }

        public float GetModuleCost(float defaultCost)
        {
            return volume * (tankPartType.addedCost + CurrentTankType.tankCost);
        }

        #endregion

        #region Public Methods

        public void SetVolume(float newVolume, bool forceFull = true)
        {
            volume = newVolume;
            UpdateTankSetup(forceFull);
        }

        public void RegisterTankChangedAction(Action<TankType> action)
        {
            if (!tankModifiedActions.Contains(action))
                tankModifiedActions.Add(action);
        }

        public void UnregisterTankChangedAction(Action<TankType> action)
        {
            tankModifiedActions.Remove(action);
        }

        #endregion

        #region Private Methods

        private void UpdateTankSetup(bool forceFull)
        {
            List<PartResource> partResources = part.Resources.list;
            int[] resourceIndices = Enumerable.Repeat<int>(-1, CurrentTankType.resources.Count).ToArray();
            bool tmp = false;

            for (int i = 0; i < partResources.Count; i++)
            {
                string resourceName = partResources[i].resourceName;
                tmp = false;

                for (int j = 0; j < CurrentTankType.resources.Count; j++)
                {
                    if (resourceName == CurrentTankType.resources[j].ResourceName)
                    {
                        resourceIndices[j] = i;
                        tmp = true;
                        break;
                    }
                }

                if (tmp)
                    continue;

                if (tankPartType.IsManagedResource(resourceName))
                {
                    DestroyImmediate(partResources[i]);
                    partResources.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < CurrentTankType.resources.Count; i++)
            {
                TankResource resource = CurrentTankType[i];
                float resourceAmount = resource.unitsPerVolume * volume;
                PartResource partResource = null;
                if (resourceIndices[i] < 0)
                {
                    partResource = part.gameObject.AddComponent<PartResource>();
                    partResource.SetInfo(resource.resourceDefinition);
                    partResource.maxAmount = resourceAmount;
                    partResource.amount = resourceAmount;
                    partResource.flowState = true;
                    partResource.isTweakable = resource.resourceDefinition.isTweakable;
                    partResource.hideFlow = false;
                    partResource.flowMode = PartResource.FlowMode.Both;
                    partResources.Add(partResource);
                }
                else
                {
                    partResource = part.Resources[i];
                    partResource.maxAmount = resourceAmount;
                    if (forceFull)
                    {
                        partResource.amount = resourceAmount;
                    }
                    else
                    {
                        if (partResource.amount > resourceAmount)
                            partResource.amount = resourceAmount;
                    }
                }
            }

            currentTankTypeString = CurrentTankType.tankName;
            part.Resources.UpdateList();
            massModifier.UpdateMass();

            Debug.Log("[MiniMFT] Switched tank on part " + part.name + " to " + CurrentTankType.tankName);

            foreach (Action<TankType> action in tankModifiedActions)
                action(CurrentTankType);

            foreach (UIPartActionWindow window in FindObjectsOfType(typeof(UIPartActionWindow)))
            {
                if (window.part == part)
                {
                    window.displayDirty = true;
                }
            }
        }

        #endregion
    }
}
