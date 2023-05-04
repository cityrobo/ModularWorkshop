using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularHandgun : Handgun, IModularWeapon
    {
        [Header("Modular Configuration")]
        public ModularFVRFireArm ModularFVRFireArm;
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

            ConfigureAll();
            ConvertTransformsToProxies();

            ModularFVRFireArm.Awake(this);
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

        public ModularWeaponPart ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, string selectedPart)
        {
            return ModularFVRFireArm.ConfigureModularWeaponPart(modularWeaponPartsAttachmentPoint, selectedPart, this);
        }
        public ModularBarrel ConfigureModularBarrel(string selectedPart)
        {
            return ModularFVRFireArm.ConfigureModularBarrel(selectedPart, this);
        }
        public ModularHandguard ConfigureModularHandguard(string selectedPart)
        {
            return ModularFVRFireArm.ConfigureModularHandguard(selectedPart, this);
        }
        public ModularStock ConfigureModularStock(string selectedPart)
        {
            return ModularFVRFireArm.ConfigureModularStock(selectedPart, this);
        }

        public void ConfigureAll()
        {
            if (ModularBarrelPartsID != string.Empty) ConfigureModularBarrel(SelectedModularBarrel);
            if (ModularHandguardPartsID != string.Empty) ConfigureModularHandguard(SelectedModularHandguard);
            if (ModularStockPartsID != string.Empty) ConfigureModularStock(SelectedModularStock);
            foreach (ModularWeaponPartsAttachmentPoint attachmentPoint in ModularFVRFireArm.ModularWeaponPartsAttachmentPoints)
            {
                if (ModularWorkshopManager.ModularWorkshopDictionary.TryGetValue(attachmentPoint.ModularPartsGroupID, out ModularWorkshopPartsDefinition prefabs) && prefabs.PartsDictionary.Count > 0) ConfigureModularWeaponPart(attachmentPoint, attachmentPoint.SelectedModularWeaponPart);
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
            Handgun[] weapon = GetComponents<Handgun>();
            Handgun toCopy = weapon.Single(c => c != this);
            toCopy.Slide.Handgun = this;

            if (toCopy.Foregrip != null)
            {
                toCopy.Foregrip.GetComponent<FVRAlternateGrip>().PrimaryObject = this;
            }

            HandgunMagReleaseTrigger grabTrigger = toCopy.GetComponentInChildren<HandgunMagReleaseTrigger>();
            if (grabTrigger != null) grabTrigger.Handgun = this;

            foreach (var mount in toCopy.AttachmentMounts)
            {
                mount.MyObject = this;
                mount.Parent = this;
            }

            this.CopyComponent(toCopy);
        }
    }
}
