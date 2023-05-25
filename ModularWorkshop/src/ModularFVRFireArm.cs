using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;
using HarmonyLib;
using static ModularWorkshop.ModularWorkshopSkinsDefinition;

namespace ModularWorkshop
{
    [Serializable]
    public class ModularFVRFireArm
    {
        public GameObject UIPrefab;
        public ModularWeaponPartsAttachmentPoint ModularBarrelAttachmentPoint;

        public ModularWeaponPartsAttachmentPoint ModularHandguardAttachmentPoint;

        public ModularWeaponPartsAttachmentPoint ModularStockAttachmentPoint;

        public ModularWeaponPartsAttachmentPoint[] ModularWeaponPartsAttachmentPoints;

        [Header("Receiver Skins")]
        [Tooltip("This is a combination of ModularPartsGroupID and PartName of a Skins definition, with a \"/\" in between. A requirement of the system. You should choose ModularPartsGroupID and PartName so that it doesn't conflict with anything else. Formatting Example: \"ModularPartsGroupID/PartName\". I would personally recommend something like \"ReceiverName/Receiver\" as a standard.")]
        public string SkinPath;
        public Transform ReceiverSkinUIPoint;
        [HideInInspector]
        public TransformProxy ReceiverSkinUIPointProxy;
        public string CurrentSelectedReceiverSkinID = "Default";

        [Tooltip("Can be populated with the context menu on the gun.")]
        public MeshRenderer[] ReceiverMeshRenderers;

        public ModularWorkshopSkinsDefinition ReceiverSkinsDefinition 
        { 
            get 
            {
                if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(SkinPath, out ModularWorkshopSkinsDefinition skinsDefinition)) return skinsDefinition;
                else
                {
                    Debug.LogError($"No SkinsDefinition found for {SkinPath}!");
                    return null;
                }
            }
        }

        [Header("Optional")]
        [Tooltip("Contains all physics colliders of the part. Use for even better performance by flattening out the hierarchy.")]
        public Transform PhysContainer;

        // Dictionaries
        public Dictionary<string, GameObject> ModularBarrelPrefabsDictionary => ModularWorkshopManager.ModularWorkshopDictionary[ModularBarrelAttachmentPoint.ModularPartsGroupID].PartsDictionary;
        public Dictionary<string, GameObject> ModularHandguardPrefabsDictionary => ModularWorkshopManager.ModularWorkshopDictionary[ModularHandguardAttachmentPoint.ModularPartsGroupID].PartsDictionary;
        public Dictionary<string, GameObject> ModularStockPrefabsDictionary => ModularWorkshopManager.ModularWorkshopDictionary[ModularStockAttachmentPoint.ModularPartsGroupID].PartsDictionary;

        [HideInInspector]
        public List<ModularWeaponPartsAttachmentPoint> SubAttachmentPoints;
        [HideInInspector]
        public ModularWorkshopPlatform WorkshopPlatform;

        private const string c_modularBarrelKey = "ModulBarrel";
        private const string c_modularHandguardKey = "ModulHandguard";
        private const string c_modularStockKey = "ModulStock";

        [HideInInspector]
        public bool WasUnvaulted = false;
        [HideInInspector]
        public FVRFireArm FireArm;

        // Original Barrel Parameters
        [HideInInspector]
        public FVRFireArmMechanicalAccuracyClass OrigMechanicalAccuracyClass;
        [HideInInspector]
        public MuzzleEffectSize OrigMuzzleEffectSize;
        [HideInInspector]
        public MuzzleEffect[] OrigMuzzleEffects;
        [HideInInspector]
        public FireArmRoundType OrigRoundType;
        [HideInInspector]
        public FVRFireArmRecoilProfile OrigRecoilProfile;
        [HideInInspector]
        public FVRFireArmRecoilProfile OrigRecoilProfileStocked;
        [HideInInspector]
        public TransformProxy OrigMagMountPos;
        [HideInInspector]
        public TransformProxy OrigMagEjectPos;
        [HideInInspector]
        public FireArmMagazineType OrigMagazineType;
        [HideInInspector]
        public FVRFirearmAudioSet OrigAudioSet;

        // Original Stock Parameters
        [HideInInspector]
        public TransformProxy OrigPoseOverride;
        [HideInInspector]
        public TransformProxy OrigPoseOverride_Touch;

        public Dictionary<string, ModularWeaponPartsAttachmentPoint> AllAttachmentPoints
        {
            get
            {
                Dictionary<string, ModularWeaponPartsAttachmentPoint> keyValuePairs = new();

                if (ModularBarrelAttachmentPoint.ModularPartsGroupID != string.Empty) keyValuePairs.Add(ModularBarrelAttachmentPoint.ModularPartsGroupID, ModularBarrelAttachmentPoint);
                if (ModularHandguardAttachmentPoint.ModularPartsGroupID != string.Empty) keyValuePairs.Add(ModularHandguardAttachmentPoint.ModularPartsGroupID, ModularHandguardAttachmentPoint);
                if (ModularStockAttachmentPoint.ModularPartsGroupID != string.Empty) keyValuePairs.Add(ModularStockAttachmentPoint.ModularPartsGroupID, ModularStockAttachmentPoint);

                foreach (var point in ModularWeaponPartsAttachmentPoints)
                {
                    try
                    {
                        keyValuePairs.Add(point.ModularPartsGroupID, point);
                    }
                    catch (Exception)
                    {
                        Debug.LogError($"{point.ModularPartsGroupID} already in dictionary!");
                    }
                }
                foreach (var subPoint in SubAttachmentPoints)
                {
                    try
                    {
                        keyValuePairs.Add(subPoint.ModularPartsGroupID, subPoint);
                    }
                    catch (Exception)
                    {
                        Debug.LogError($"{subPoint.ModularPartsGroupID} already in dictionary!");
                    }
                }

                return keyValuePairs;
            }
        }

        public void Awake(FVRFireArm fireArm)
        {
            FireArm = fireArm;

            OrigMuzzleEffectSize = fireArm.DefaultMuzzleEffectSize;
            OrigMuzzleEffects = fireArm.MuzzleEffects;
            OrigMechanicalAccuracyClass = fireArm.AccuracyClass;

            OrigRoundType = fireArm.RoundType;

            OrigRecoilProfile = fireArm.RecoilProfile;
            OrigRecoilProfileStocked = fireArm.RecoilProfileStocked;

            if (fireArm.MagazineMountPos != null) OrigMagMountPos = new(fireArm.MagazineMountPos);
            if (fireArm.MagazineEjectPos != null) OrigMagEjectPos = new(fireArm.MagazineEjectPos);
            OrigMagazineType = fireArm.MagazineType;

            OrigAudioSet = fireArm.AudioClipSet;

            OrigPoseOverride = new(fireArm.PoseOverride);
            OrigPoseOverride_Touch = new(fireArm.PoseOverride_Touch);

            if (SkinPath == string.Empty) SkinPath = fireArm.gameObject.name.Replace("(Clone)","") + "/" + "Receiver";
            if (ReceiverMeshRenderers == null || ReceiverMeshRenderers.Length == 0) GetReceiverMeshRenderers(fireArm);
            CheckForDefaultReceiverSkin(fireArm);

            ApplyReceiverSkin(CurrentSelectedReceiverSkinID);

            if (PhysContainer != null)
            {
                foreach (Transform child in PhysContainer)
                {
                    child.SetParent(fireArm.transform);
                }

                UnityEngine.Object.Destroy(PhysContainer.gameObject);
            }
        }

        public void AddSubAttachmentPoint(ModularWeaponPartsAttachmentPoint subPoint, FVRFireArm fireArm, Dictionary<string,string> oldSubParts, Dictionary<string, string> oldSkins, string selectedPart)
        {
            SubAttachmentPoints.Add(subPoint);
            ConfigureModularWeaponPart(subPoint, selectedPart, fireArm, oldSubParts, oldSkins);

            WorkshopPlatform?.CreateUIForPoint(subPoint);
        }

        public void RemoveSubAttachmentPoint(ModularWeaponPartsAttachmentPoint subPoint, FVRFireArm fireArm, Dictionary<string,string> oldSubParts)
        {
            SubAttachmentPoints.Remove(subPoint);

            FVRFireArmAttachment[] attachments;
            ModularWeaponPart part = subPoint.ModularPartPoint.GetComponent<ModularWeaponPart>();
            if (part != null)
            {
                foreach (var mount in part.AttachmentMounts)
                {
                    attachments = mount.AttachmentsList.ToArray();
                    foreach (var attachment in attachments)
                    {
                        attachment.SetAllCollidersToLayer(false, "Default");
                        attachment.DetachFromMount();
                    }

                    fireArm.AttachmentMounts.Remove(mount);
                }

                foreach (var subSubPoint in part.SubAttachmentPoints)
                {
                    oldSubParts.Add(subSubPoint.ModularPartsGroupID, subSubPoint.SelectedModularWeaponPart);

                    RemoveSubAttachmentPoint(subSubPoint, fireArm, oldSubParts);
                }
            }

            UnityEngine.Object.Destroy(subPoint.ModularPartPoint.gameObject);
        }

        public void ConfigureFromFlagDic(Dictionary<string, string> f, FVRFireArm fireArm)
        {
            string selectedPart;
            string selectedSkin;
            if (f.TryGetValue(c_modularBarrelKey, out selectedPart)) ConfigureModularBarrel(selectedPart, fireArm);
            if (f.TryGetValue(c_modularHandguardKey, out selectedPart)) ConfigureModularHandguard(selectedPart, fireArm);
            if (f.TryGetValue(c_modularStockKey, out selectedPart)) ConfigureModularStock(selectedPart, fireArm);

            foreach (var modularWeaponPartsAttachmentPoint in ModularWeaponPartsAttachmentPoints)
            {
                if (f.TryGetValue("Modul" + modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out selectedPart)) ConfigureModularWeaponPart(modularWeaponPartsAttachmentPoint, selectedPart, fireArm);
            }
            for (int i = 0; i < SubAttachmentPoints.Count; i++)
            {
                if (f.TryGetValue("Modul" + SubAttachmentPoints.ElementAt(i).ModularPartsGroupID, out selectedPart)) ConfigureModularWeaponPart(SubAttachmentPoints.ElementAt(i), selectedPart, fireArm);
            }

            if (f.TryGetValue(SkinPath, out selectedSkin)) ApplyReceiverSkin(selectedSkin); 

            foreach (var anyPoint in AllAttachmentPoints.Values)
            {
                string key = anyPoint.ModularPartsGroupID + "/" + anyPoint.SelectedModularWeaponPart;

                if (f.TryGetValue(key, out selectedSkin)) anyPoint.ApplySkin(selectedSkin);
            }

            WasUnvaulted = true;
        }

        public Dictionary<string, string> GetFlagDic(Dictionary<string, string> flagDic)
        {
            if (ModularBarrelAttachmentPoint.ModularPartsGroupID != string.Empty) flagDic.Add(c_modularBarrelKey, ModularBarrelAttachmentPoint.SelectedModularWeaponPart);
            if (ModularHandguardAttachmentPoint.ModularPartsGroupID != string.Empty) flagDic.Add(c_modularHandguardKey, ModularHandguardAttachmentPoint.SelectedModularWeaponPart);
            if (ModularStockAttachmentPoint.ModularPartsGroupID != string.Empty) flagDic.Add(c_modularStockKey, ModularStockAttachmentPoint.SelectedModularWeaponPart);
            ModularWorkshopPartsDefinition partDefinition;
            foreach (var modularWeaponPartsAttachmentPoint in ModularWeaponPartsAttachmentPoints)
            {
                if (ModularWorkshopManager.ModularWorkshopDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out partDefinition) && partDefinition.ModularPrefabs.Count > 0) flagDic.Add("Modul" + modularWeaponPartsAttachmentPoint.ModularPartsGroupID, modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart);
            }
            foreach (var subPoint in SubAttachmentPoints)
            {
                if (ModularWorkshopManager.ModularWorkshopDictionary.TryGetValue(subPoint.ModularPartsGroupID, out partDefinition) && partDefinition.ModularPrefabs.Count > 0) flagDic.Add("Modul" + subPoint.ModularPartsGroupID, subPoint.SelectedModularWeaponPart);
            }

            flagDic.Add(SkinPath, CurrentSelectedReceiverSkinID);

            foreach (var anyPoint in AllAttachmentPoints.Values)
            {
                string key = anyPoint.ModularPartsGroupID + "/" + anyPoint.SelectedModularWeaponPart;
                string value = anyPoint.CurrentSkin;
                flagDic.Add(key, value);
            }

            return flagDic;
        }

        public ModularWeaponPart ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, string selectedPart, FVRFireArm fireArm, Dictionary<string, string> oldSubParts = null, Dictionary<string, string> oldSkins = null)
        {
            if (ModularWorkshopManager.ModularWorkshopDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out ModularWorkshopPartsDefinition partsDefinition))
            {
                if (!partsDefinition.PartsDictionary.ContainsKey(selectedPart))
                {
                    OpenScripts2_BepInExPlugin.LogError(fireArm, $"Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" does not contain part with name \"{selectedPart}\"");
                    return null;
                }
            }
            else if (selectedPart != string.Empty) OpenScripts2_BepInExPlugin.LogError(fireArm, $"Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary!");
            else if (selectedPart == string.Empty)
            {
                OpenScripts2_BepInExPlugin.LogWarning(fireArm, $"Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary, but current part name also empty. Treating as future attachment point!");
                return null;
            }

            GameObject modularWeaponPartPrefab = UnityEngine.Object.Instantiate(partsDefinition.PartsDictionary[selectedPart], modularWeaponPartsAttachmentPoint.ModularPartPoint.position, modularWeaponPartsAttachmentPoint.ModularPartPoint.rotation, modularWeaponPartsAttachmentPoint.ModularPartPoint.parent);
            ModularWeaponPart oldPart = modularWeaponPartsAttachmentPoint.ModularPartPoint.GetComponentInChildren<ModularWeaponPart>();

            if (oldSubParts == null) oldSubParts = new();
            if (oldSkins == null) oldSkins = new();

            if (oldPart != null)
            {
                oldPart.DisablePart();

                if (!oldSkins.ContainsKey(modularWeaponPartsAttachmentPoint.ModularPartsGroupID)) oldSkins.Add(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, modularWeaponPartsAttachmentPoint.CurrentSkin);

                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    oldSubParts.Add(point.ModularPartsGroupID, point.SelectedModularWeaponPart);
                    oldSkins.Add(point.ModularPartsGroupID, point.CurrentSkin);

                    RemoveSubAttachmentPoint(point, fireArm, oldSubParts);
                }
            }

            ModularWeaponPart newPart = modularWeaponPartPrefab.GetComponent<ModularWeaponPart>();
            modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart = selectedPart;
            UpdateFirearm(oldPart, newPart, fireArm);

            UnityEngine.Object.Destroy(modularWeaponPartsAttachmentPoint.ModularPartPoint.gameObject);
            modularWeaponPartsAttachmentPoint.ModularPartPoint = newPart.transform;

            newPart.ConfigurePart();

            modularWeaponPartsAttachmentPoint.CheckForDefaultSkin();

            if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.SkinPath, out ModularWorkshopSkinsDefinition definition))
            {
                if (oldSkins.TryGetValue(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out string skinName))
                {
                    //Debug.Log($"Old Skin on attachment point: {skinName}");
                    if (definition.SkinDictionary.ContainsKey(skinName))
                    {
                        //Debug.Log($"Old Skin {skinName} found in SkinDictionary!");
                        modularWeaponPartsAttachmentPoint.ApplySkin(skinName);
                    }
                    else if (modularWeaponPartsAttachmentPoint.PreviousSkins.TryGetValue(selectedPart, out skinName))
                    {
                        //Debug.Log($"Previous Skin found! {selectedPart}: {skinName}");
                        if (definition.SkinDictionary.ContainsKey(skinName))
                        {
                            //Debug.Log($"Previous Skin {skinName} found in SkinDictionary!");
                            modularWeaponPartsAttachmentPoint.ApplySkin(skinName);
                        }
                    }
                }
                else if (modularWeaponPartsAttachmentPoint.PreviousSkins.TryGetValue(selectedPart, out skinName))
                {
                    //Debug.Log($"Previous Skin found! {selectedPart}: {skinName}");
                    if (definition.SkinDictionary.ContainsKey(skinName))
                    {
                        //Debug.Log($"Previous Skin {skinName} found in SkinDictionary!");
                        modularWeaponPartsAttachmentPoint.ApplySkin(skinName);
                    }
                }
            } 

            foreach (var point in newPart.SubAttachmentPoints)
            {
                if (oldSubParts.TryGetValue(point.ModularPartsGroupID, out string oldSelectedPart)) AddSubAttachmentPoint(point, fireArm, oldSubParts, oldSkins, oldSelectedPart);
                else AddSubAttachmentPoint(point, fireArm, oldSubParts, oldSkins, point.SelectedModularWeaponPart);
            }

            return newPart;
        }

        public ModularBarrel ConfigureModularBarrel(string selectedPart, FVRFireArm fireArm)
        {
            //Debug.Log($"Selected Barrel Name: {selectedPart}");

            //if (ModularBarrelPrefabsDictionary.Count == 0)
            //{
            //    Debug.LogError($"ModularBarrelPrefabsDictionary is empty!");
            //    ModularWorkshopPartsDefinition definition = ModularWorkshopManager.ModularWorkshopDictionary[ModularBarrelAttachmentPoint.ModularPartsGroupID];
            //    if (definition.ModularPrefabs.Count == 0) Debug.LogError($"{definition.name} doesn't have any prefabs in it either!");
            //}
            //for (int i = 0; i < ModularBarrelPrefabsDictionary.Count; i++)
            //{
            //    if (ModularBarrelPrefabsDictionary.ElementAt(i).Key == null) Debug.LogError($"ModularBarrelPrefabsDictionary: Key at index {i} is null!");
            //    else if (ModularBarrelPrefabsDictionary.ElementAt(i).Key == selectedPart) Debug.LogWarning($"ModularBarrelPrefabsDictionary: Key at index {i} is Selected part with name {ModularBarrelPrefabsDictionary.ElementAt(i).Key}!");
            //    else Debug.Log($"ModularBarrelPrefabsDictionary: Key at index {i} is part name {ModularBarrelPrefabsDictionary.ElementAt(i).Key}!");
            //    if (ModularBarrelPrefabsDictionary.ElementAt(i).Value == null) Debug.LogError($"ModularBarrelPrefabsDictionary: Value at index {i} is null!");
            //    else Debug.Log($"ModularBarrelPrefabsDictionary: Value at index {i} is Game Object {ModularBarrelPrefabsDictionary.ElementAt(i).Value.name}!");
            //}

            

            if (!ModularBarrelPrefabsDictionary.ContainsKey(selectedPart))
            {
                OpenScripts2_BepInExPlugin.LogError(fireArm, $"Parts group \"{ModularBarrelAttachmentPoint.ModularPartsGroupID}\" does not contain part with name \"{selectedPart}\"");
                return null;
            }

            GameObject modularBarrelPrefab = UnityEngine.Object.Instantiate(ModularBarrelPrefabsDictionary[selectedPart], ModularBarrelAttachmentPoint.ModularPartPoint.position, ModularBarrelAttachmentPoint.ModularPartPoint.rotation, ModularBarrelAttachmentPoint.ModularPartPoint.parent);

            ModularBarrel oldPart = ModularBarrelAttachmentPoint.ModularPartPoint.GetComponentInChildren<ModularBarrel>();

            Dictionary<string, string> oldSubParts = new();
            Dictionary<string, string> oldSkins = new();

            if (oldPart != null)
            {
                oldPart.DisablePart();

                if (!oldSkins.ContainsKey(ModularBarrelAttachmentPoint.ModularPartsGroupID)) oldSkins.Add(ModularBarrelAttachmentPoint.ModularPartsGroupID, ModularBarrelAttachmentPoint.CurrentSkin);

                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    oldSubParts.Add(point.ModularPartsGroupID, point.SelectedModularWeaponPart);
                    oldSkins.Add(point.ModularPartsGroupID, point.CurrentSkin);

                    RemoveSubAttachmentPoint(point, fireArm, oldSubParts);
                }
                if (oldPart.HasCustomMuzzleEffects)
                {
                    fireArm.DefaultMuzzleEffectSize = OrigMuzzleEffectSize;
                    fireArm.MuzzleEffects = OrigMuzzleEffects;
                }
                if (oldPart.HasCustomMechanicalAccuracy)
                {
                    fireArm.AccuracyClass = OrigMechanicalAccuracyClass;
                    fireArm.m_internalMechanicalMOA = AM.GetFireArmMechanicalSpread(fireArm.AccuracyClass);
                }
                if (oldPart.ChangesFireArmRoundType)
                {
                    fireArm.RoundType = OrigRoundType;
                    foreach (var chamber in fireArm.FChambers)
                    {
                        chamber.RoundType = OrigRoundType;
                    }
                }
                if (oldPart.ChangesRecoilProfile)
                {
                    fireArm.RecoilProfile = OrigRecoilProfile;
                    if (OrigRecoilProfileStocked != null)
                    {
                        fireArm.UsesStockedRecoilProfile = true;
                        fireArm.RecoilProfileStocked = OrigRecoilProfileStocked;
                    }
                }
                if (oldPart.ChangesMagazineMountPoint)
                {
                    fireArm.MagazineMountPos.GoToTransformProxy(OrigMagMountPos);
                    fireArm.MagazineEjectPos.GoToTransformProxy(OrigMagEjectPos);
                }
                if (oldPart.ChangesMagazineType)
                {
                    fireArm.MagazineType = OrigMagazineType;
                }
                if (oldPart.ChangesAudioSet)
                {
                    fireArm.AudioClipSet = OrigAudioSet;
                }
            }

            ModularBarrelAttachmentPoint.SelectedModularWeaponPart = selectedPart;

            ModularBarrel newPart = modularBarrelPrefab.GetComponent<ModularBarrel>();

            fireArm.MuzzlePos.GoToTransformProxy(newPart.MuzzlePosProxy);

            fireArm.DefaultMuzzleState = newPart.DefaultMuzzleState;
            fireArm.DefaultMuzzleDamping = newPart.DefaultMuzzleDamping;

            if (newPart.HasCustomMuzzleEffects)
            {
                if (newPart.KeepRevolverCylinderSmoke)
                {
                    MuzzleEffect cylinderSmoke = fireArm.MuzzleEffects.Single(obj => obj.Entry == MuzzleEffectEntry.Smoke_RevolverCylinder);

                    newPart.CustomMuzzleEffects = newPart.CustomMuzzleEffects.Concat(new MuzzleEffect[] { cylinderSmoke }).ToArray();
                }
                fireArm.DefaultMuzzleEffectSize = newPart.CustomMuzzleEffectSize;
                fireArm.MuzzleEffects = newPart.CustomMuzzleEffects;
            }
            if (newPart.HasCustomMechanicalAccuracy)
            {
                fireArm.AccuracyClass = newPart.CustomMechanicalAccuracy;
                fireArm.m_internalMechanicalMOA = AM.GetFireArmMechanicalSpread(fireArm.AccuracyClass);
            }
            if (newPart.ChangesFireArmRoundType)
            {
                fireArm.RoundType = newPart.CustomRoundType;
                foreach (var chamber in fireArm.FChambers)
                {
                    chamber.RoundType = newPart.CustomRoundType;
                }
            }
            if (newPart.ChangesRecoilProfile)
            {
                fireArm.RecoilProfile = newPart.CustomRecoilProfile;
                if (newPart.CustomRecoilProfileStocked != null)
                {
                    fireArm.UsesStockedRecoilProfile = true;
                    fireArm.RecoilProfileStocked = newPart.CustomRecoilProfileStocked;
                }
                else fireArm.UsesStockedRecoilProfile = false;
            }
            if (newPart.ChangesMagazineMountPoint)
            {
                fireArm.MagazineMountPos.GoToTransformProxy(newPart.MagMountPoint);
                fireArm.MagazineEjectPos.GoToTransformProxy(newPart.MagEjectPoint);
            }
            if (newPart.ChangesMagazineType)
            {
                fireArm.MagazineType = newPart.CustomMagazineType;
            }
            if (newPart.ChangesAudioSet)
            {
                fireArm.AudioClipSet = newPart.CustomAudioSet;
            }

            fireArm.UpdateCurrentMuzzle();

            UpdateFirearm(oldPart, newPart, fireArm);

            UnityEngine.Object.Destroy(ModularBarrelAttachmentPoint.ModularPartPoint.gameObject);
            ModularBarrelAttachmentPoint.ModularPartPoint = modularBarrelPrefab.transform;

            newPart.ConfigurePart();

            ModularBarrelAttachmentPoint.CheckForDefaultSkin();

            if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(ModularBarrelAttachmentPoint.SkinPath, out ModularWorkshopSkinsDefinition definition))
            {
                if (oldSkins.TryGetValue(ModularBarrelAttachmentPoint.ModularPartsGroupID, out string skinName))
                {
                    if (definition.SkinDictionary.ContainsKey(skinName)) ModularBarrelAttachmentPoint.ApplySkin(skinName);
                    else if (ModularBarrelAttachmentPoint.PreviousSkins.TryGetValue(selectedPart, out skinName))
                    {
                        if (definition.SkinDictionary.ContainsKey(skinName)) ModularBarrelAttachmentPoint.ApplySkin(skinName);
                    }
                }
                else if (ModularBarrelAttachmentPoint.PreviousSkins.TryGetValue(selectedPart, out skinName))
                {
                    if (definition.SkinDictionary.ContainsKey(skinName)) ModularBarrelAttachmentPoint.ApplySkin(skinName);
                }
            }

            foreach (var point in newPart.SubAttachmentPoints)
            {
                if (oldSubParts.TryGetValue(point.ModularPartsGroupID, out string oldSelectedPart) && oldSelectedPart != point.SelectedModularWeaponPart) AddSubAttachmentPoint(point, fireArm, oldSubParts, oldSkins, oldSelectedPart);
                else AddSubAttachmentPoint(point, fireArm, oldSubParts, oldSkins, point.SelectedModularWeaponPart);
            }

            return newPart;
        }
        public ModularHandguard ConfigureModularHandguard(string selectedPart, FVRFireArm fireArm)
        {
            if (!ModularHandguardPrefabsDictionary.ContainsKey(selectedPart))
            {
                OpenScripts2_BepInExPlugin.LogError(fireArm, $"Parts group \"{ModularHandguardAttachmentPoint.ModularPartsGroupID}\" does not contain part with name \"{selectedPart}\"");
                return null;
            }

            GameObject modularHandguardPrefab = UnityEngine.Object.Instantiate(ModularHandguardPrefabsDictionary[selectedPart], ModularHandguardAttachmentPoint.ModularPartPoint.position, ModularHandguardAttachmentPoint.ModularPartPoint.rotation, ModularHandguardAttachmentPoint.ModularPartPoint.parent);
            ModularHandguard oldPart = ModularHandguardAttachmentPoint.ModularPartPoint.GetComponentInChildren<ModularHandguard>();

            Dictionary<string, string> oldSubParts = new();
            Dictionary<string, string> oldSkins = new();

            if (oldPart != null)
            {
                oldPart.DisablePart();

                if (!oldSkins.ContainsKey(ModularHandguardAttachmentPoint.ModularPartsGroupID)) oldSkins.Add(ModularHandguardAttachmentPoint.ModularPartsGroupID, ModularHandguardAttachmentPoint.CurrentSkin);

                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    oldSubParts.Add(point.ModularPartsGroupID, point.SelectedModularWeaponPart);
                    oldSkins.Add(point.ModularPartsGroupID, point.CurrentSkin);

                    RemoveSubAttachmentPoint(point, fireArm, oldSubParts);
                }
            }

            ModularHandguardAttachmentPoint.SelectedModularWeaponPart = selectedPart;
            ModularHandguard newPart = modularHandguardPrefab.GetComponent<ModularHandguard>();

            fireArm.Foregrip.gameObject.SetActive(newPart.ActsLikeForeGrip);
            if (newPart.ActsLikeForeGrip)
            {
                Collider grabTrigger = fireArm.Foregrip.GetComponent<Collider>();
                fireArm.Foregrip.transform.GoToTransformProxy(newPart.ForeGripTransformProxy);
                switch (grabTrigger)
                {
                    case SphereCollider c:
                        switch (newPart.ColliderType)
                        {
                            case ModularHandguard.EColliderType.Sphere:
                                c.center = newPart.TriggerCenter;
                                c.radius = newPart.TriggerSize.x;
                                break;
                            case ModularHandguard.EColliderType.Capsule:
                                CapsuleCollider capsuleCollider = grabTrigger.gameObject.AddComponent<CapsuleCollider>();
                                capsuleCollider.center = newPart.TriggerCenter;
                                capsuleCollider.radius = newPart.TriggerSize.x;
                                capsuleCollider.height = newPart.TriggerSize.y;
                                capsuleCollider.direction = newPart.ColliderAxis switch
                                {
                                    OpenScripts2_BasePlugin.Axis.X => 0,
                                    OpenScripts2_BasePlugin.Axis.Y => 1,
                                    OpenScripts2_BasePlugin.Axis.Z => 2,
                                    _ => 0,
                                };
                                capsuleCollider.isTrigger = true;

                                UnityEngine.Object.Destroy(grabTrigger);

                                fireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = fireArm.Foregrip.GetComponentsInChildren<Collider>(true);
                                break;
                            case ModularHandguard.EColliderType.Box:
                                BoxCollider boxCollider = grabTrigger.gameObject.AddComponent<BoxCollider>();
                                boxCollider.center = newPart.TriggerCenter;
                                boxCollider.size = newPart.TriggerSize;
                                boxCollider.isTrigger = true;

                                UnityEngine.Object.Destroy(grabTrigger);

                                fireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = fireArm.Foregrip.GetComponentsInChildren<Collider>(true);
                                break;
                        }
                        break;
                    case CapsuleCollider c:
                        switch (newPart.ColliderType)
                        {
                            case ModularHandguard.EColliderType.Sphere:
                                SphereCollider sphereCollider = grabTrigger.gameObject.AddComponent<SphereCollider>();
                                sphereCollider.center = newPart.TriggerCenter;
                                sphereCollider.radius = newPart.TriggerSize.x;
                                sphereCollider.isTrigger = true;

                                UnityEngine.Object.Destroy(grabTrigger);

                                fireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = fireArm.Foregrip.GetComponentsInChildren<Collider>(true);
                                break;
                            case ModularHandguard.EColliderType.Capsule:
                                c.center = newPart.TriggerCenter;
                                c.radius = newPart.TriggerSize.x;
                                c.height = newPart.TriggerSize.y;
                                c.direction = newPart.ColliderAxis switch
                                {
                                    OpenScripts2_BasePlugin.Axis.X => 0,
                                    OpenScripts2_BasePlugin.Axis.Y => 1,
                                    OpenScripts2_BasePlugin.Axis.Z => 2,
                                    _ => 0,
                                };
                                break;
                            case ModularHandguard.EColliderType.Box:
                                BoxCollider boxCollider = grabTrigger.gameObject.AddComponent<BoxCollider>();
                                boxCollider.center = newPart.TriggerCenter;
                                boxCollider.size = newPart.TriggerSize;
                                boxCollider.isTrigger = true;

                                UnityEngine.Object.Destroy(grabTrigger);

                                fireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = fireArm.Foregrip.GetComponentsInChildren<Collider>(true);
                                break;
                        }
                        break;
                    case BoxCollider c:
                        switch (newPart.ColliderType)
                        {
                            case ModularHandguard.EColliderType.Sphere:
                                SphereCollider sphereCollider = grabTrigger.gameObject.AddComponent<SphereCollider>();
                                sphereCollider.center = newPart.TriggerCenter;
                                sphereCollider.radius = newPart.TriggerSize.x;
                                sphereCollider.isTrigger = true;

                                UnityEngine.Object.Destroy(grabTrigger);

                                fireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = fireArm.Foregrip.GetComponentsInChildren<Collider>(true);
                                break;
                            case ModularHandguard.EColliderType.Capsule:
                                CapsuleCollider capsuleCollider = grabTrigger.gameObject.AddComponent<CapsuleCollider>();
                                capsuleCollider.center = newPart.TriggerCenter;
                                capsuleCollider.radius = newPart.TriggerSize.x;
                                capsuleCollider.height = newPart.TriggerSize.y;
                                capsuleCollider.direction = newPart.ColliderAxis switch
                                {
                                    OpenScripts2_BasePlugin.Axis.X => 0,
                                    OpenScripts2_BasePlugin.Axis.Y => 1,
                                    OpenScripts2_BasePlugin.Axis.Z => 2,
                                    _ => 0,
                                };
                                capsuleCollider.isTrigger = true;

                                UnityEngine.Object.Destroy(grabTrigger);

                                fireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = fireArm.Foregrip.GetComponentsInChildren<Collider>(true);
                                break;
                            case ModularHandguard.EColliderType.Box:
                                c.center = newPart.TriggerCenter;
                                c.size = newPart.TriggerSize;
                                break;
                        }
                        break;
                }
            }

            UpdateFirearm(oldPart, newPart, fireArm);

            UnityEngine.Object.Destroy(ModularHandguardAttachmentPoint.ModularPartPoint.gameObject);
            ModularHandguardAttachmentPoint.ModularPartPoint = modularHandguardPrefab.transform;

            newPart.ConfigurePart();

            ModularHandguardAttachmentPoint.CheckForDefaultSkin();

            if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(ModularHandguardAttachmentPoint.SkinPath, out ModularWorkshopSkinsDefinition definition))
            {
                if (oldSkins.TryGetValue(ModularHandguardAttachmentPoint.ModularPartsGroupID, out string skinName))
                {
                    if (definition.SkinDictionary.ContainsKey(skinName)) ModularHandguardAttachmentPoint.ApplySkin(skinName);
                    else if (ModularHandguardAttachmentPoint.PreviousSkins.TryGetValue(selectedPart, out skinName))
                    {
                        if (definition.SkinDictionary.ContainsKey(skinName)) ModularHandguardAttachmentPoint.ApplySkin(skinName);
                    }
                }
                else if (ModularHandguardAttachmentPoint.PreviousSkins.TryGetValue(selectedPart, out skinName))
                {
                    if (definition.SkinDictionary.ContainsKey(skinName)) ModularHandguardAttachmentPoint.ApplySkin(skinName);
                }
            }

            foreach (var point in newPart.SubAttachmentPoints)
            {
                if (oldSubParts.TryGetValue(point.ModularPartsGroupID, out string oldSelectedPart) && oldSelectedPart != point.SelectedModularWeaponPart) AddSubAttachmentPoint(point, fireArm, oldSubParts, oldSkins, oldSelectedPart);
                else AddSubAttachmentPoint(point, fireArm, oldSubParts, oldSkins, point.SelectedModularWeaponPart);
            }

            return newPart;
        }

        public ModularStock ConfigureModularStock(string selectedPart, FVRFireArm fireArm)
        {
            if (!ModularStockPrefabsDictionary.ContainsKey(selectedPart))
            {
                OpenScripts2_BepInExPlugin.LogError(fireArm, $"Parts group \"{ModularStockAttachmentPoint.ModularPartsGroupID}\" does not contain part with name \"{selectedPart}\"");
                return null;
            }

            GameObject modularStockPrefab = UnityEngine.Object.Instantiate(ModularStockPrefabsDictionary[selectedPart], ModularStockAttachmentPoint.ModularPartPoint.position, ModularStockAttachmentPoint.ModularPartPoint.rotation, ModularStockAttachmentPoint.ModularPartPoint.parent);

            ModularStock oldPart = ModularStockAttachmentPoint.ModularPartPoint.GetComponentInChildren<ModularStock>();

            Dictionary<string, string> oldSubParts = new();
            Dictionary<string, string> oldSkins = new();

            if (oldPart != null)
            {
                oldPart.DisablePart();
                if (!oldSkins.ContainsKey(ModularStockAttachmentPoint.ModularPartsGroupID)) oldSkins.Add(ModularStockAttachmentPoint.ModularPartsGroupID, ModularStockAttachmentPoint.CurrentSkin);

                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    oldSubParts.Add(point.ModularPartsGroupID, point.SelectedModularWeaponPart);
                    oldSkins.Add(point.ModularPartsGroupID, point.CurrentSkin);

                    RemoveSubAttachmentPoint(point, fireArm, oldSubParts);
                }

                if (oldPart.ChangesPosePosition)
                {
                    FVRViveHand hand = GM.CurrentMovementManager.Hands[0];
                    if (hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index)
                    {
                        fireArm.PoseOverride.GoToTransformProxy(OrigPoseOverride_Touch);
                        fireArm.PoseOverride_Touch.GoToTransformProxy(OrigPoseOverride_Touch);
                        fireArm.m_storedLocalPoseOverrideRot = fireArm.PoseOverride.localRotation;
                    }
                    else
                    {
                        fireArm.PoseOverride.GoToTransformProxy(OrigPoseOverride);
                        fireArm.m_storedLocalPoseOverrideRot = fireArm.PoseOverride.localRotation;
                    }
                }
            }
            ModularStockAttachmentPoint.SelectedModularWeaponPart = selectedPart;
            ModularStock newPart = modularStockPrefab.GetComponent<ModularStock>();

            fireArm.HasActiveShoulderStock = newPart.ActsLikeStock;
            fireArm.StockPos = newPart.StockPoint;

            if (newPart.CollapsingStock != null) newPart.CollapsingStock.Firearm = fireArm;
            if (newPart.FoldingStockX != null) newPart.FoldingStockX.FireArm = fireArm;
            if (newPart.FoldingStockY != null) newPart.FoldingStockY.FireArm = fireArm;

            if (newPart.ChangesPosePosition)
            {
                FVRViveHand hand = GM.CurrentMovementManager.Hands[0];
                if (hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index)
                {
                    fireArm.PoseOverride.GoToTransformProxy(newPart.CustomPoseOverride_TouchProxy);
                    fireArm.PoseOverride_Touch.GoToTransformProxy(newPart.CustomPoseOverride_TouchProxy);
                    fireArm.m_storedLocalPoseOverrideRot = fireArm.PoseOverride.localRotation;
                }
                else
                {
                    fireArm.PoseOverride.GoToTransformProxy(newPart.CustomPoseOverrideProxy);
                    fireArm.m_storedLocalPoseOverrideRot = fireArm.PoseOverride.localRotation;
                }
            }

            UpdateFirearm(oldPart, newPart, fireArm);

            UnityEngine.Object.Destroy(ModularStockAttachmentPoint.ModularPartPoint.gameObject);
            ModularStockAttachmentPoint.ModularPartPoint = modularStockPrefab.transform;

            newPart.ConfigurePart();

            ModularStockAttachmentPoint.CheckForDefaultSkin();

            if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(ModularBarrelAttachmentPoint.SkinPath, out ModularWorkshopSkinsDefinition definition))
            {
                if (oldSkins.TryGetValue(ModularStockAttachmentPoint.ModularPartsGroupID, out string skinName))
                {
                    if (definition.SkinDictionary.ContainsKey(skinName)) ModularStockAttachmentPoint.ApplySkin(skinName);
                    else if (ModularStockAttachmentPoint.PreviousSkins.TryGetValue(selectedPart, out skinName))
                    {
                        if (definition.SkinDictionary.ContainsKey(skinName)) ModularStockAttachmentPoint.ApplySkin(skinName);
                    }
                }
                else if (ModularStockAttachmentPoint.PreviousSkins.TryGetValue(selectedPart, out skinName))
                {
                    if (definition.SkinDictionary.ContainsKey(skinName)) ModularStockAttachmentPoint.ApplySkin(skinName);
                }
            }

            foreach (var point in newPart.SubAttachmentPoints)
            {
                if (oldSubParts.TryGetValue(point.ModularPartsGroupID, out string oldSelectedPart) && oldSelectedPart != point.SelectedModularWeaponPart) AddSubAttachmentPoint(point, fireArm, oldSubParts, oldSkins, oldSelectedPart);
                else AddSubAttachmentPoint(point, fireArm, oldSubParts, oldSkins, point.SelectedModularWeaponPart);
            }

            return newPart;
        }

        // Receiver Skins
        public void ApplyReceiverSkin(string skinName)
        {
            CurrentSelectedReceiverSkinID = skinName;
            if (ReceiverSkinsDefinition.SkinDictionary.TryGetValue(skinName, out SkinDefinition skinDefinition))
            {
                try
                {
                    for (int i = 0; i < ReceiverMeshRenderers.Length; i++)
                    {
                        ReceiverMeshRenderers[i].materials = skinDefinition.DifferentSkinnedMeshPieces[i].Materials;
                    }
                }
                catch (Exception)
                {
                    Debug.LogError($"Number of DifferentSkinnedMeshPieces in SkinDefinition {skinDefinition.ModularSkinID} does not match number of meshes on Receiver! ({ReceiverMeshRenderers.Length} vs {skinDefinition.DifferentSkinnedMeshPieces.Length})");
                }
            }
            else Debug.LogError($"Skin with name {skinName} not found in SkinsDefinition {ReceiverSkinsDefinition.name}!");
        }

        public void CheckForDefaultReceiverSkin(FVRFireArm fireArm)
        {
            if (CurrentSelectedReceiverSkinID == "Default" && ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(SkinPath, out ModularWorkshopSkinsDefinition skinsDefinition))
            {
                if (!skinsDefinition.SkinDictionary.ContainsKey("Default"))
                {
                    // Create SkinDefinition for Default Skin
                    SkinDefinition skinDefinition = new()
                    {
                        ModularSkinID = "Default",
                        DisplayName = "Default",
                        Icon = MiscUtilities.CreateEmptySprite()
                    };

                    // Create Array with the length of the MeshRenders on the part and fill it with their materials
                    MeshSkin[] skinPieces = new MeshSkin[ReceiverMeshRenderers.Length];

                    for (int i = 0; i < ReceiverMeshRenderers.Length; i++)
                    {
                        MeshSkin skinPiece = new()
                        {
                            Materials = ReceiverMeshRenderers[i].sharedMaterials
                        };

                        skinPieces[i] = skinPiece;
                    }
                    skinDefinition.DifferentSkinnedMeshPieces = skinPieces;

                    // Add new SkinDefinition to the found SkinsDefinition in the ModularWorkshopManager
                    skinsDefinition.SkinDefinitions.Insert(0, skinDefinition);
                }
            }
            else if (CurrentSelectedReceiverSkinID == "Default")
            {
                // Create SkinDefinition for Default Skin
                SkinDefinition skinDefinition = new()
                {
                    ModularSkinID = "Default",
                    DisplayName = "Default",
                    Icon = MiscUtilities.CreateEmptySprite()
                };

                // Create Array with the length of the MeshRenders on the part and fill it with their materials
                MeshSkin[] skinPieces = new MeshSkin[ReceiverMeshRenderers.Length];

                for (int i = 0; i < ReceiverMeshRenderers.Length; i++)
                {
                    MeshSkin skinPiece = new()
                    {
                        Materials = ReceiverMeshRenderers[i].sharedMaterials
                    };

                    skinPieces[i] = skinPiece;
                }
                skinDefinition.DifferentSkinnedMeshPieces = skinPieces;

                // Create new SkinsDefinition and add it to the ModularWorkshopManager
                skinsDefinition = ScriptableObject.CreateInstance<ModularWorkshopSkinsDefinition>();

                string[] separatedPath = SkinPath.Split('/');

                skinsDefinition.name = separatedPath[0] + "/" + separatedPath[1];
                skinsDefinition.ModularPartsGroupID = separatedPath[0];
                skinsDefinition.PartName = separatedPath[1];
                skinsDefinition.SkinDefinitions = new() { skinDefinition };
                skinsDefinition.AutomaticallyCreated = true;

                ModularWorkshopManager.ModularWorkshopSkinsDictionary.Add(SkinPath, skinsDefinition);
            }
            else if (CurrentSelectedReceiverSkinID != "Default" && !ModularWorkshopManager.ModularWorkshopSkinsDictionary.ContainsKey(SkinPath))
            {
                Debug.LogWarning($"No SkinsDefinition found for receiver skin path {SkinPath}, but part receiver {fireArm.gameObject.name} set to skin name {CurrentSelectedReceiverSkinID}. Naming error?");
            }
        }
        public void GetReceiverMeshRenderers(FVRFireArm fireArm)
        {
            List<MeshRenderer> ignoredMeshRenderers = new();

            foreach (var part in fireArm.GetComponentsInChildren<ModularWeaponPart>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            foreach (var part in fireArm.GetComponentsInChildren<FVRFireArmAttachment>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            foreach (var part in fireArm.GetComponentsInChildren<FVRFireArmChamber>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            foreach (var part in fireArm.GetComponentsInChildren<FVRFirearmMovingProxyRound>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }

            ReceiverMeshRenderers = fireArm.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled).Where(m => !ignoredMeshRenderers.Contains(m)).ToArray();
        }

        // Utility Methods
        public void ConvertTransformsToProxies(FVRFireArm fireArm)
        {
            if (ModularBarrelAttachmentPoint.ModularPartsGroupID != string.Empty)
            {
                ModularBarrelAttachmentPoint.ModularPartUIPointProxy = new(ModularBarrelAttachmentPoint.ModularPartUIPoint, fireArm.transform, true);
            }
            if (ModularHandguardAttachmentPoint.ModularPartsGroupID != string.Empty)
            {
                ModularHandguardAttachmentPoint.ModularPartUIPointProxy = new(ModularHandguardAttachmentPoint.ModularPartUIPoint, fireArm.transform, true);
            }
            if (ModularStockAttachmentPoint.ModularPartsGroupID != string.Empty)
            {
                ModularStockAttachmentPoint.ModularPartUIPointProxy = new(ModularStockAttachmentPoint.ModularPartUIPoint, fireArm.transform, true);
            }

            foreach (var point in ModularWeaponPartsAttachmentPoints)
            {
                point.ModularPartUIPointProxy = new(point.ModularPartUIPoint, fireArm.transform, true);
            }

            if (ReceiverSkinUIPoint != null) ReceiverSkinUIPointProxy = new(ReceiverSkinUIPoint, true);
            else
            {
                GameObject uiPoint = new GameObject("temp");
                uiPoint.transform.SetParent(fireArm.transform);
                uiPoint.transform.position = fireArm.transform.position + -fireArm.transform.right * 0.1f;
                uiPoint.transform.rotation = fireArm.transform.rotation * Quaternion.Euler(new Vector3(0f, 90f, 0f));

                ReceiverSkinUIPointProxy = new(uiPoint.transform, true);
            }
        }

        private void UpdateFirearm(ModularWeaponPart oldPart, ModularWeaponPart newPart, FVRFireArm fireArm)
        {
            IPartFireArmRequirement[] partFireArmRequirements;
            if (oldPart != null)
            {
                foreach (var mount in oldPart.AttachmentMounts)
                {
                    DetachAllAttachmentsFromMount(mount);

                    fireArm.AttachmentMounts.Remove(mount);
                }
                fireArm.Slots = fireArm.Slots.Where(s => !oldPart.SubQuickBeltSlots.Contains(s)).ToArray();

                partFireArmRequirements = oldPart.GetComponents<IPartFireArmRequirement>();
                foreach (var item in partFireArmRequirements)
                {
                    item.FireArm = null;
                }
            }
            fireArm.AttachmentMounts.AddRange(newPart.AttachmentMounts);
            foreach (var mount in newPart.AttachmentMounts)
            {
                mount.Parent = fireArm;
                mount.MyObject = fireArm;
            }

            fireArm.Slots.AddRangeToArray(newPart.SubQuickBeltSlots);

            fireArm.m_colliders = fireArm.GetComponentsInChildren<Collider>(true);

            if (fireArm.m_quickbeltSlot != null) fireArm.SetAllCollidersToLayer(false, "NoCol");

            partFireArmRequirements = newPart.GetComponents<IPartFireArmRequirement>();
            foreach (var item in partFireArmRequirements)
            {
                item.FireArm = fireArm;
            }
        }

        private void DetachAllAttachmentsFromMount(FVRFireArmAttachmentMount mount)
        {
            FVRFireArmAttachment[] attachments = mount.AttachmentsList.ToArray();
            foreach (var attachment in attachments)
            {
                foreach (var subMount in attachment.AttachmentMounts)
                {
                    DetachAllAttachmentsFromMount(subMount);
                }
                attachment.SetAllCollidersToLayer(false, "Default");
                attachment.DetachFromMount();
            }
        }

    }
}