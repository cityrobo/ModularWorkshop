﻿using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularClosedBoltWeapon : ClosedBoltWeapon , IModularWeapon
    {
        [Header("Modular Configuration")]
        public ModularFVRFireArm ModularFVRFireArm;

        public bool AllowExternalBoltReleaseButtonModification = true;
        public GameObject UIPrefab => ModularFVRFireArm.UIPrefab;
        public string ModularBarrelPartsID => ModularFVRFireArm.ModularBarrelAttachmentPoint.ModularPartsGroupID;
        public Transform ModularBarrelPoint => ModularFVRFireArm.ModularBarrelAttachmentPoint.ModularPartPoint;
        public TransformProxy ModularBarrelUIPointProxy => ModularFVRFireArm.ModularBarrelAttachmentPoint.ModularPartUIPointProxy;
        public Dictionary<string, GameObject> ModularBarrelPrefabsDictionary => ModularFVRFireArm.ModularBarrelPrefabsDictionary;
        public string ModularHandguardPartsID => ModularFVRFireArm.ModularHandguardAttachmentPoint.ModularPartsGroupID;
        public Transform ModularHandguardPoint => ModularFVRFireArm.ModularHandguardAttachmentPoint.ModularPartPoint;
        public TransformProxy ModularHandguardUIPointProxy => ModularFVRFireArm.ModularHandguardAttachmentPoint.ModularPartUIPointProxy;
        public Dictionary<string, GameObject> ModularHandguardPrefabsDictionary => ModularFVRFireArm.ModularHandguardPrefabsDictionary;
        public string ModularStockPartsID => ModularFVRFireArm.ModularStockAttachmentPoint.ModularPartsGroupID;
        public Transform ModularStockPoint => ModularFVRFireArm.ModularStockAttachmentPoint.ModularPartPoint;
        public TransformProxy ModularStockUIPointProxy => ModularFVRFireArm.ModularStockAttachmentPoint.ModularPartUIPointProxy;
        public Dictionary<string, GameObject> ModularStockPrefabsDictionary => ModularFVRFireArm.ModularStockPrefabsDictionary;

        public string SelectedModularBarrel => ModularFVRFireArm.ModularBarrelAttachmentPoint.SelectedModularWeaponPart;
        public string SelectedModularHandguard => ModularFVRFireArm.ModularHandguardAttachmentPoint.SelectedModularWeaponPart;
        public string SelectedModularStock => ModularFVRFireArm.ModularStockAttachmentPoint.SelectedModularWeaponPart;
        public ModularWeaponPartsAttachmentPoint[] ModularWeaponPartsAttachmentPoints => ModularFVRFireArm.ModularWeaponPartsAttachmentPoints;
        public ModularWorkshopPlatform WorkshopPlatform
        {
            get => ModularFVRFireArm.WorkshopPlatform;
            set => ModularFVRFireArm.WorkshopPlatform = value;
        }
        public List<ModularWeaponPartsAttachmentPoint> SubAttachmentPoints => ModularFVRFireArm.SubAttachmentPoints;
        public ModularFVRFireArm GetModularFVRFireArm => ModularFVRFireArm;

        public Dictionary<string, ModularWeaponPartsAttachmentPoint> AllAttachmentPoints
        {
            get
            {
                Dictionary<string, ModularWeaponPartsAttachmentPoint> allAttachmentPoints = ModularFVRFireArm.AllAttachmentPoints;

                return allAttachmentPoints;
            }
        }

        public override void Awake()
        {
            base.Awake();

            ConvertTransformsToProxies();

            ModularFVRFireArm.Awake(this);

            ConfigureAll();
        }

        public override void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            base.ConfigureFromFlagDic(f);

            ModularFVRFireArm.ConfigureFromFlagDic(f, this);
        }

        public override Dictionary<string, string> GetFlagDic()
        {
            Dictionary<string, string> flagDic = base.GetFlagDic();

            flagDic = ModularFVRFireArm.GetFlagDic(flagDic);
            return flagDic;
        }

        public ModularWeaponPart ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, string selectedPart, bool isRandomized = false)
        {
            return ModularFVRFireArm.ConfigureModularWeaponPart(modularWeaponPartsAttachmentPoint, selectedPart, isRandomized);
        }
        public ModularBarrel ConfigureModularBarrel(string selectedPart, bool isRandomized = false)
        {
            return ModularFVRFireArm.ConfigureModularBarrel(selectedPart, isRandomized);
        }
        public ModularHandguard ConfigureModularHandguard(string selectedPart, bool isRandomized = false)
        {
            return ModularFVRFireArm.ConfigureModularHandguard(selectedPart, isRandomized);
        }
        public ModularStock ConfigureModularStock(string selectedPart, bool isRandomized = false)
        {
            return ModularFVRFireArm.ConfigureModularStock(selectedPart, isRandomized);
        }

        public void ConfigureAll()
        {
            string selectedPart;
            if (ModularBarrelPartsID != string.Empty)
            {
                selectedPart = ModularFVRFireArm.IsInTakeAndHold && !ModularFVRFireArm.WasUnvaulted && !ModularFVRFireArm.ModularBarrelAttachmentPoint.DisallowTakeAndHoldRandomization ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[ModularBarrelPartsID].GetRandomPart() : SelectedModularBarrel;
                ConfigureModularBarrel(selectedPart);
            }
            if (ModularHandguardPartsID != string.Empty)
            {
                selectedPart = ModularFVRFireArm.IsInTakeAndHold && !ModularFVRFireArm.WasUnvaulted && !ModularFVRFireArm.ModularHandguardAttachmentPoint.DisallowTakeAndHoldRandomization ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[ModularHandguardPartsID].GetRandomPart() : SelectedModularHandguard;

                ConfigureModularHandguard(selectedPart);
            }
            if (ModularStockPartsID != string.Empty)
            {
                selectedPart = ModularFVRFireArm.IsInTakeAndHold && !ModularFVRFireArm.WasUnvaulted && !ModularFVRFireArm.ModularStockAttachmentPoint.DisallowTakeAndHoldRandomization ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[ModularStockPartsID].GetRandomPart() : SelectedModularStock;

                ConfigureModularStock(selectedPart);
            }
            foreach (ModularWeaponPartsAttachmentPoint attachmentPoint in ModularFVRFireArm.ModularWeaponPartsAttachmentPoints)
            {
                if (attachmentPoint.IsPointDisabled) continue;

                if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.TryGetValue(attachmentPoint.ModularPartsGroupID, out ModularWorkshopPartsDefinition prefabs) && prefabs.PartsDictionary.Count > 0)
                {
                    selectedPart = ModularFVRFireArm.IsInTakeAndHold && !ModularFVRFireArm.WasUnvaulted && !attachmentPoint.DisallowTakeAndHoldRandomization ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[attachmentPoint.ModularPartsGroupID].GetRandomPart() : attachmentPoint.SelectedModularWeaponPart;

                    ConfigureModularWeaponPart(attachmentPoint, selectedPart, ModularFVRFireArm.IsInTakeAndHold);
                }
                else if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.ContainsKey(attachmentPoint.ModularPartsGroupID) && prefabs.PartsDictionary.Count == 0)
                {
                    ModularWorkshopManager.LogError(this, $"PartsAttachmentPoint Error: Parts group \"{attachmentPoint.ModularPartsGroupID}\" found in ModularWorkshopManager dictionary, but it is empty!");
                    attachmentPoint.IsPointDisabled = true;
                }
                else if (!ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.ContainsKey(attachmentPoint.ModularPartsGroupID) && attachmentPoint.UsesExternalParts)
                {
                    ModularWorkshopManager.Log(this, $"PartsAttachmentPoint Info: Parts group \"{attachmentPoint.ModularPartsGroupID}\" disabled due to using external parts and no external parts found.");
                    attachmentPoint.IsPointDisabled = true;
                }
                else if (!ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.ContainsKey(attachmentPoint.ModularPartsGroupID))
                {
                    ModularWorkshopManager.LogWarning(this, $"PartsAttachmentPoint Warning: Parts group \"{attachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary! Disabling part point!");
                    attachmentPoint.IsPointDisabled = true;
                }
            }
        }

        public void ConvertTransformsToProxies()
        {
            ModularFVRFireArm.ConvertTransformsToProxies(this);
        }

        public void ApplySkin(string ModularPartsGroupID, string SkinName)
        {
            AllAttachmentPoints[ModularPartsGroupID].ApplySkin(SkinName);
        }

        [ContextMenu("Copy Existing Firearm Component")]
        public void CopyFirearm()
        {
            ClosedBoltWeapon[] weapon = GetComponents<ClosedBoltWeapon>();
            ClosedBoltWeapon toCopy = weapon.Single(c => c != this);
            if (toCopy.Bolt != null) toCopy.Bolt.Weapon = this;
            if (toCopy.Chamber != null) toCopy.Chamber.Firearm = this;
            if (toCopy.Handle != null) toCopy.Handle.Weapon = this;

            if (toCopy.Foregrip != null)
            {
                toCopy.Foregrip.GetComponent<FVRAlternateGrip>().PrimaryObject = this;
            }

            ClosedBoltMagEjectionTrigger grabTrigger = toCopy.GetComponentInChildren<ClosedBoltMagEjectionTrigger>();
            if (grabTrigger != null) grabTrigger.Receiver = this;
            FVRFireArmReloadTriggerWell magWell = toCopy.GetComponentInChildren<FVRFireArmReloadTriggerWell>();
            if (magWell != null) magWell.FireArm = this;

            toCopy.AttachmentMounts = toCopy.AttachmentMounts.Where(mount => mount != null).ToList();
            foreach (var mount in toCopy.AttachmentMounts)
            {
                mount.MyObject = this;
                mount.Parent = this;
            }

            this.CopyComponent(toCopy);
        }

        [ContextMenu("Populate Receiver Mesh Renderer List")]
        public void PopulateReceiverMeshList()
        {
            ModularFVRFireArm.GetReceiverMeshRenderers(this);
        }

        [ContextMenu("Update Selected Parts")]
        public void UpdateSelectedParts()
        {
            ModularFVRFireArm.UpdateSelectedParts();
        }
    }
}
