using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;
using System.Reflection;

namespace ModularWorkshop
{
    public class ModularRevolver : Revolver , IModularWeapon
    {
        [Header("Modular Configuration")]
        public ModularFVRFireArm ModularFVRFireArm;
        public GameObject UIPrefab => ModularFVRFireArm.UIPrefab;
        public string ModularBarrelPartID => ModularFVRFireArm.ModularBarrelPartID;
        public Transform ModularBarrelPosition => ModularFVRFireArm.ModularBarrelPosition;
        public TransformProxy ModularBarrelUIPosition => ModularFVRFireArm.ModularBarrelUIPositionProxy;
        public Dictionary<string,GameObject> ModularBarrelPrefabsDictionary => ModularWorkshopManager.ModularWorkshopDictionary[ModularFVRFireArm.ModularBarrelPartID].PartsDictionary;
        public string ModularHandguardPartID => ModularFVRFireArm.ModularHandguardPartID;
        public Transform ModularHandguardPosition => ModularFVRFireArm.ModularHandguardPosition;
        public TransformProxy ModularHandguardUIPosition => ModularFVRFireArm.ModularHandguardUIPositionProxy;
        public Dictionary<string, GameObject> ModularHandguardPrefabsDictionary => ModularWorkshopManager.ModularWorkshopDictionary[ModularFVRFireArm.ModularHandguardPartID].PartsDictionary;
        public string ModularStockPartID => ModularFVRFireArm.ModularStockPartID;
        public Transform ModularStockPosition => ModularFVRFireArm.ModularStockPosition;
        public TransformProxy ModularStockUIPosition => ModularFVRFireArm.ModularStockUIPositionProxy;
        public Dictionary<string, GameObject> ModularStockPrefabsDictionary => ModularWorkshopManager.ModularWorkshopDictionary[ModularFVRFireArm.ModularStockPartID].PartsDictionary;

        public string SelectedModularBarrel => ModularFVRFireArm.SelectedModularBarrel;
        public string SelectedModularHandguard => ModularFVRFireArm.SelectedModularHandguard;
        public string SelectedModularStock => ModularFVRFireArm.SelectedModularStock;
        public ModularWeaponPartsAttachmentPoint[] ModularWeaponPartsAttachmentPoints => ModularFVRFireArm.ModularWeaponPartsAttachmentPoints;

        public ModularWorkshopPlatform WorkshopPlatform
        {
            get => ModularFVRFireArm.WorkshopPlatform;
            set => ModularFVRFireArm.WorkshopPlatform = value;
        }

        public List<ModularWeaponPartsAttachmentPoint> SubAttachmentPoints => ModularFVRFireArm.SubAttachmentPoints;

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
            if (ModularFVRFireArm.ModularBarrelPartID != string.Empty) ConfigureModularBarrel(ModularFVRFireArm.SelectedModularBarrel);
            if (ModularFVRFireArm.ModularHandguardPartID != string.Empty) ConfigureModularHandguard(ModularFVRFireArm.SelectedModularHandguard);
            if (ModularFVRFireArm.ModularStockPartID != string.Empty) ConfigureModularStock(ModularFVRFireArm.SelectedModularStock);
            foreach (ModularWeaponPartsAttachmentPoint attachmentPoint in ModularFVRFireArm.ModularWeaponPartsAttachmentPoints)
            {
                if (ModularWorkshopManager.ModularWorkshopDictionary.TryGetValue(attachmentPoint.PartID, out ModularWorkshopPartsDefinition prefabs) && prefabs.PartsDictionary.Count > 0) ConfigureModularWeaponPart(attachmentPoint, attachmentPoint.SelectedModularWeaponPart);
            }
        }

        public void ConvertTransformsToProxies()
        {
            ModularFVRFireArm.ConvertTransformsToProxies(this);
        }

        [ContextMenu("Copy Existing Firearm Component")]
        public void CopyFirearm()
        {
            Revolver[] attachments = GetComponents<Revolver>();
            Revolver toCopy = attachments.Single(c => c != this);
            toCopy.Cylinder.Revolver = this;

            foreach (var mount in toCopy.AttachmentMounts)
            {
                mount.MyObject = this;
                mount.Parent = this;
            }

            CopyRevolver(toCopy);
        }

        public void CopyRevolver(Revolver reference)
        {
            Type type = typeof(Revolver);
            //if (type != reference.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(this, pinfo.GetValue(reference, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(this, finfo.GetValue(reference));
            }
        }
    }
}
