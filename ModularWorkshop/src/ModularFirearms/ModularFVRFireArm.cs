using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;
using HarmonyLib;
using static ModularWorkshop.ModularWorkshopSkinsDefinition;
using System.Net;

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
        [Tooltip("This is a combination of ModularPartsGroupID and PartName of a Skins definition, with a \"/\" in between. A requirement of the system. You should choose ModularPartsGroupID and PartName so that it doesn't conflict with anything else. Formatting Example: \"ModularPartsGroupID/PartName\". I would personally recommend something like \"ItemID/ReceiverName\" as a standard.")]
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
                    OpenScripts2_BepInExPlugin.LogError(FireArm, $"No Receiver SkinsDefinition found for {SkinPath}!");
                    return null;
                }
            }
        }

        [Header("Optional")]
        [Tooltip("Contains all physics colliders of the part. Use for even better performance by flattening out the hierarchy.")]
        public Transform PhysContainer;

        // Dictionaries
        public Dictionary<string, GameObject> ModularBarrelPrefabsDictionary
        {
            get
            {
                if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.TryGetValue(ModularBarrelAttachmentPoint.ModularPartsGroupID, out ModularWorkshopPartsDefinition def))
                {
                    return def.PartsDictionary;
                }
                else 
                { 
                    return null; 
                }
            }
        }

        public Dictionary<string, GameObject> ModularHandguardPrefabsDictionary
        { 
            get
            {
                if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.TryGetValue(ModularHandguardAttachmentPoint.ModularPartsGroupID, out ModularWorkshopPartsDefinition def))
                {
                    return def.PartsDictionary;
                }
                else
                {
                    return null;
                }
            }
        }
        public Dictionary<string, GameObject> ModularStockPrefabsDictionary 
        {
            get
            {
                if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.TryGetValue(ModularStockAttachmentPoint.ModularPartsGroupID, out ModularWorkshopPartsDefinition def))
                {
                    return def.PartsDictionary;
                }
                else
                {
                    return null;
                }
            }
        }

        [HideInInspector]
        public List<ModularWeaponPartsAttachmentPoint> SubAttachmentPoints;
        [HideInInspector]
        public ModularWorkshopPlatform WorkshopPlatform;

        private const string c_modularBarrelKey = "ModulBarrel";
        private const string c_modularHandguardKey = "ModulHandguard";
        private const string c_modularStockKey = "ModulStock";

        private const string c_modularChambering = "ModulChamber";

        [HideInInspector]
        public FVRFireArm FireArm;

        [HideInInspector]
        public event PartAdded PartAdded;

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

        // Situation dependent toggles
        [HideInInspector]
        public bool IsInTakeAndHold = false;
        [HideInInspector]
        public bool WasUnvaulted = false;

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
                        OpenScripts2_BepInExPlugin.LogError(FireArm, $"PartPoint for ModularPartsGroupID {point.ModularPartsGroupID} already in AllAttachmentPoints dictionary!");
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
                        OpenScripts2_BepInExPlugin.LogError(FireArm, $"SubPartPoint for ModularPartsGroupID {subPoint.ModularPartsGroupID} already in AllAttachmentPoints dictionary!");
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

            if (SkinPath == string.Empty) SkinPath = fireArm.ObjectWrapper.ItemID + "/" + "Receiver";
            if (ReceiverMeshRenderers == null || ReceiverMeshRenderers.Length == 0) GetReceiverMeshRenderers(fireArm);
            CheckForDefaultReceiverSkin(fireArm);

            ApplyReceiverSkin(IsInTakeAndHold ? ReceiverSkinsDefinition.GetRandomSkin() : CurrentSelectedReceiverSkinID);

            if (PhysContainer != null)
            {
                while (PhysContainer.childCount != 0)
                {
                    PhysContainer.GetChild(0).SetParent(fireArm.transform);
                }

                UnityEngine.Object.Destroy(PhysContainer.gameObject);
            }

            if (GM.TNH_Manager != null)
            {
                IsInTakeAndHold = ModularWorkshopManager.EnableTNHRandomization.Value;
            }
        }

        public void AddSubAttachmentPoint(ModularWeaponPartsAttachmentPoint subPoint, FVRFireArm fireArm, Dictionary<string,string> oldSubParts, Dictionary<string, string> oldSkins, string selectedPart, bool isRandomized = false)
        {
            SubAttachmentPoints.Add(subPoint);
            ConfigureModularWeaponPart(subPoint, selectedPart, isRandomized, oldSubParts, oldSkins);

            WorkshopPlatform?.CreateUIForPoint(subPoint);
        }

        public void RemoveSubAttachmentPoint(ModularWeaponPartsAttachmentPoint subPoint, FVRFireArm fireArm, Dictionary<string,string> oldSubParts)
        {
            SubAttachmentPoints.Remove(subPoint);

            ModularWeaponPart part = subPoint.ModularPartPoint.GetComponent<ModularWeaponPart>();
            if (part != null)
            {
                part.DisablePart();

                foreach (var mount in part.AttachmentMounts)
                {
                    DetachAllAttachmentsFromMount(mount);

                    fireArm.AttachmentMounts.Remove(mount);
                }

                foreach (var subSubPoint in part.SubAttachmentPoints)
                {
                    oldSubParts.Add(subSubPoint.ModularPartsGroupID, subSubPoint.SelectedModularWeaponPart);

                    RemoveSubAttachmentPoint(subSubPoint, fireArm, oldSubParts);
                }
            }
            WorkshopPlatform?.RemoveUIFromPoint(subPoint);

            UnityEngine.Object.Destroy(subPoint.ModularPartPoint.gameObject);
        }

        public void ConfigureFromFlagDic(Dictionary<string, string> f, FVRFireArm fireArm)
        {
            string selectedPart;
            string selectedSkin;
            FireArm = fireArm;
            if (f.TryGetValue(c_modularBarrelKey, out selectedPart)) ConfigureModularBarrel(selectedPart, false);
            if (f.TryGetValue(c_modularHandguardKey, out selectedPart)) ConfigureModularHandguard(selectedPart, false);
            if (f.TryGetValue(c_modularStockKey, out selectedPart)) ConfigureModularStock(selectedPart, false);

            foreach (var modularWeaponPartsAttachmentPoint in ModularWeaponPartsAttachmentPoints)
            {
                if (!modularWeaponPartsAttachmentPoint.IsPointDisabled && f.TryGetValue("Modul" + modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out selectedPart)) ConfigureModularWeaponPart(modularWeaponPartsAttachmentPoint, selectedPart);
            }
            for (int i = 0; i < SubAttachmentPoints.Count; i++)
            {
                if (!SubAttachmentPoints[i].IsPointDisabled && f.TryGetValue("Modul" + SubAttachmentPoints.ElementAt(i).ModularPartsGroupID, out selectedPart)) ConfigureModularWeaponPart(SubAttachmentPoints.ElementAt(i), selectedPart);
            }

            if (f.TryGetValue(SkinPath, out selectedSkin)) ApplyReceiverSkin(selectedSkin);

            List<ModularWeaponPartsAttachmentPoint> points = AllAttachmentPoints.Values.ToList();
            points.Sort((x, y) => string.Compare(x.ModularPartsGroupID, y.ModularPartsGroupID));
            List<FVRFireArmAttachmentMount> mounts = new();

            foreach (var anyPoint in points)
            {
                string key = anyPoint.ModularPartsGroupID + "/" + anyPoint.SelectedModularWeaponPart;

                if (f.TryGetValue(key, out selectedSkin)) anyPoint.ApplySkin(selectedSkin);

                ModularWeaponPart part = anyPoint.ModularPartPoint.GetComponent<ModularWeaponPart>();
                if (part != null) mounts.AddRange(part.AttachmentMounts);
            }

            FireArm.AttachmentMounts.RemoveAll(mounts.Contains);
            FireArm.AttachmentMounts.AddRange(mounts);

            if (f.TryGetValue(c_modularChambering, out string chamberingString))
            {
                FireArmRoundClass chambering = MiscUtilities.ParseEnum<FireArmRoundClass>(chamberingString);

                foreach (var chamber in fireArm.FChambers)
                {
                    chamber.Autochamber(chambering);
                }
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
                if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out partDefinition) && partDefinition.ModularPrefabs.Count > 0) flagDic.Add("Modul" + modularWeaponPartsAttachmentPoint.ModularPartsGroupID, modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart);
            }
            foreach (var subPoint in SubAttachmentPoints)
            {
                if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.TryGetValue(subPoint.ModularPartsGroupID, out partDefinition) && partDefinition.ModularPrefabs.Count > 0) flagDic.Add("Modul" + subPoint.ModularPartsGroupID, subPoint.SelectedModularWeaponPart);
            }

            flagDic.Add(SkinPath, CurrentSelectedReceiverSkinID);

            List<ModularWeaponPartsAttachmentPoint> points = AllAttachmentPoints.Values.ToList();
            points.Sort((x,y) => string.Compare(x.ModularPartsGroupID,y.ModularPartsGroupID));
            List<FVRFireArmAttachmentMount> mounts = new();

            foreach (var anyPoint in points)
            {
                string key = anyPoint.ModularPartsGroupID + "/" + anyPoint.SelectedModularWeaponPart;
                string value = anyPoint.CurrentSkin;
                flagDic.Add(key, value);

                ModularWeaponPart part = anyPoint.ModularPartPoint.GetComponent<ModularWeaponPart>();
                if (part != null) mounts.AddRange(part.AttachmentMounts);
            }

            FireArm.AttachmentMounts.RemoveAll(mounts.Contains);
            FireArm.AttachmentMounts.AddRange(mounts);

            if (FireArm.FChambers[0].IsFull && FireArm.FChambers[0].RoundType != FireArm.ObjectWrapper.RoundType)
            {
                flagDic.Add(c_modularChambering, FireArm.FChambers[0].GetRound().RoundClass.ToString());
                FireArm.FChambers.ForEach(c => c.SetRound(null));
            }

            return flagDic;
        }

        public ModularWeaponPart ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, string selectedPart, bool isRandomized = false, Dictionary<string, string> oldSubParts = null, Dictionary<string, string> oldSkins = null)
        {
            if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out ModularWorkshopPartsDefinition partsDefinition))
            {
                if (!partsDefinition.PartsDictionary.ContainsKey(selectedPart))
                {
                    OpenScripts2_BepInExPlugin.LogError(FireArm, $"PartsAttachmentPoint Error: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" does not contain part with name \"{selectedPart}\"");
                    return null;
                }
            }
            else if (selectedPart != string.Empty && modularWeaponPartsAttachmentPoint.UsesExternalParts)
            {
                OpenScripts2_BepInExPlugin.Log(FireArm, $"PartsAttachmentPoint Info: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" disabled due to using external parts and no external parts found.");
                modularWeaponPartsAttachmentPoint.IsPointDisabled = true;
                return null;
            }
            else if (selectedPart != string.Empty)
            {
                OpenScripts2_BepInExPlugin.LogError(FireArm, $"PartsAttachmentPoint Error: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary!");
                return null;
            }
            else if (selectedPart == string.Empty || modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart == string.Empty)
            {
                OpenScripts2_BepInExPlugin.LogWarning(FireArm, $"PartsAttachmentPoint Warning: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary, but current part name also empty. Treating as future attachment point!");
                modularWeaponPartsAttachmentPoint.IsPointDisabled = true;
                return null;
            }

            // Old Part Operations
            ModularWeaponPart oldPart = modularWeaponPartsAttachmentPoint.ModularPartPoint.GetComponentInChildren<ModularWeaponPart>();

            if (oldSubParts == null) oldSubParts = new();
            if (oldSkins == null) oldSkins = new();

            if (oldPart != null)
            {
                oldPart.DisablePart();

                RemovePartPointOccupation(oldPart);

                if (!oldSkins.ContainsKey(modularWeaponPartsAttachmentPoint.ModularPartsGroupID)) oldSkins.Add(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, modularWeaponPartsAttachmentPoint.CurrentSkin);

                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    oldSubParts.Add(point.ModularPartsGroupID, point.SelectedModularWeaponPart);
                    oldSkins.Add(point.ModularPartsGroupID, point.CurrentSkin);

                    RemoveSubAttachmentPoint(point, FireArm, oldSubParts);
                }
            }

            // New Part Operations
            GameObject modularWeaponPartPrefab = UnityEngine.Object.Instantiate(partsDefinition.PartsDictionary[selectedPart], modularWeaponPartsAttachmentPoint.ModularPartPoint.position, modularWeaponPartsAttachmentPoint.ModularPartPoint.rotation, modularWeaponPartsAttachmentPoint.ModularPartPoint.parent);
            ModularWeaponPart newPart = modularWeaponPartPrefab.GetComponent<ModularWeaponPart>();
            newPart.AdjustScale(modularWeaponPartsAttachmentPoint);

            modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart = selectedPart;
            UpdateFireArm(oldPart, newPart);

            UnityEngine.Object.Destroy(modularWeaponPartsAttachmentPoint.ModularPartPoint.gameObject);
            modularWeaponPartsAttachmentPoint.ModularPartPoint = newPart.transform;

            newPart.EnablePart();

            ApplyPartPointOccupation(newPart);

            // Finalization
            TryApplyOldSkin(modularWeaponPartsAttachmentPoint, selectedPart, oldSkins, isRandomized);

            ConfigureNewSubParts(newPart, oldSubParts, oldSkins, isRandomized);

            PartAdded?.Invoke(modularWeaponPartsAttachmentPoint, newPart);

            FireArm.ResetClampCOM();

            return newPart;
        }

        public ModularBarrel ConfigureModularBarrel(string selectedPart, bool isRandomized)
        {
            Dictionary<string, GameObject> partsDictionary = ModularBarrelPrefabsDictionary;
            if (partsDictionary != null && !partsDictionary.ContainsKey(selectedPart))
            {
                OpenScripts2_BepInExPlugin.LogError(FireArm, $"Barrel PartsAttachmentPoint Error: Parts group \"{ModularBarrelAttachmentPoint.ModularPartsGroupID}\" does not contain part with name \"{selectedPart}\"");
                return null;
            }
            else if (partsDictionary == null)
            {
                OpenScripts2_BepInExPlugin.LogError(FireArm, $"Barrel PartsAttachmentPoint Error: Parts group \"{ModularBarrelAttachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary!");
                return null;
            }

            GameObject modularBarrelPrefab = UnityEngine.Object.Instantiate(partsDictionary[selectedPart], ModularBarrelAttachmentPoint.ModularPartPoint.position, ModularBarrelAttachmentPoint.ModularPartPoint.rotation, ModularBarrelAttachmentPoint.ModularPartPoint.parent);

            ModularBarrel oldPart = ModularBarrelAttachmentPoint.ModularPartPoint.GetComponentInChildren<ModularBarrel>();

            Dictionary<string, string> oldSubParts = new();
            Dictionary<string, string> oldSkins = new();

            if (oldPart != null)
            {
                oldPart.DisablePart();

                RemovePartPointOccupation(oldPart);

                if (!oldSkins.ContainsKey(ModularBarrelAttachmentPoint.ModularPartsGroupID)) oldSkins.Add(ModularBarrelAttachmentPoint.ModularPartsGroupID, ModularBarrelAttachmentPoint.CurrentSkin);

                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    oldSubParts.Add(point.ModularPartsGroupID, point.SelectedModularWeaponPart);
                    oldSkins.Add(point.ModularPartsGroupID, point.CurrentSkin);

                    RemoveSubAttachmentPoint(point, FireArm, oldSubParts);
                }
                if (oldPart.HasCustomMuzzleEffects)
                {
                    FireArm.DefaultMuzzleEffectSize = OrigMuzzleEffectSize;
                    FireArm.MuzzleEffects = OrigMuzzleEffects;
                }
                if (oldPart.HasCustomMechanicalAccuracy)
                {
                    FireArm.AccuracyClass = OrigMechanicalAccuracyClass;
                    FireArm.m_internalMechanicalMOA = AM.GetFireArmMechanicalSpread(FireArm.AccuracyClass);
                }
                if (oldPart.ChangesFireArmRoundType)
                {
                    FireArm.RoundType = OrigRoundType;
                    foreach (var chamber in FireArm.FChambers)
                    {
                        chamber.RoundType = OrigRoundType;
                    }
                    //FireArm.ObjectWrapper.RoundType = OrigRoundType;
                }
                if (oldPart.ChangesRecoilProfile)
                {
                    FireArm.RecoilProfile = OrigRecoilProfile;
                    if (OrigRecoilProfileStocked != null)
                    {
                        FireArm.UsesStockedRecoilProfile = true;
                        FireArm.RecoilProfileStocked = OrigRecoilProfileStocked;
                    }
                }
                if (oldPart.ChangesMagazineMountPoint)
                {
                    FireArm.MagazineMountPos.GoToTransformProxy(OrigMagMountPos);
                    FireArm.MagazineEjectPos.GoToTransformProxy(OrigMagEjectPos);
                }
                if (oldPart.ChangesMagazineType)
                {
                    FireArm.MagazineType = OrigMagazineType;
                }
                if (oldPart.ChangesAudioSet)
                {
                    FireArm.AudioClipSet = OrigAudioSet;
                }
            }

            ModularBarrelAttachmentPoint.SelectedModularWeaponPart = selectedPart;

            ModularBarrel newPart = modularBarrelPrefab.GetComponent<ModularBarrel>();
            newPart.AdjustScale(ModularBarrelAttachmentPoint);

            FireArm.MuzzlePos.GoToTransformProxy(newPart.MuzzlePosProxy);

            FireArm.DefaultMuzzleState = newPart.DefaultMuzzleState;
            FireArm.DefaultMuzzleDamping = newPart.DefaultMuzzleDamping;

            if (newPart.HasCustomMuzzleEffects)
            {
                if (newPart.KeepRevolverCylinderSmoke)
                {
                    MuzzleEffect cylinderSmoke = FireArm.MuzzleEffects.Single(obj => obj.Entry == MuzzleEffectEntry.Smoke_RevolverCylinder);

                    newPart.CustomMuzzleEffects = newPart.CustomMuzzleEffects.Concat(new MuzzleEffect[] { cylinderSmoke }).ToArray();
                }
                FireArm.DefaultMuzzleEffectSize = newPart.CustomMuzzleEffectSize;
                FireArm.MuzzleEffects = newPart.CustomMuzzleEffects;
            }
            if (newPart.HasCustomMechanicalAccuracy)
            {
                FireArm.AccuracyClass = newPart.CustomMechanicalAccuracy;
                FireArm.m_internalMechanicalMOA = AM.GetFireArmMechanicalSpread(FireArm.AccuracyClass);
            }
            if (newPart.ChangesFireArmRoundType)
            {
                FireArm.RoundType = newPart.CustomRoundType;
                foreach (var chamber in FireArm.FChambers)
                {
                    chamber.RoundType = newPart.CustomRoundType;
                }
                FireArm.ObjectWrapper.RoundType = newPart.CustomRoundType;
            }
            if (newPart.ChangesRecoilProfile)
            {
                FireArm.RecoilProfile = newPart.CustomRecoilProfile;
                if (newPart.CustomRecoilProfileStocked != null)
                {
                    FireArm.UsesStockedRecoilProfile = true;
                    FireArm.RecoilProfileStocked = newPart.CustomRecoilProfileStocked;
                }
                else FireArm.UsesStockedRecoilProfile = false;
            }
            if (newPart.ChangesMagazineMountPoint)
            {
                FireArm.MagazineMountPos.GoToTransformProxy(newPart.MagMountPoint);
                FireArm.MagazineEjectPos.GoToTransformProxy(newPart.MagEjectPoint);
            }
            if (newPart.ChangesMagazineType)
            {
                FireArm.MagazineType = newPart.CustomMagazineType;
            }
            if (newPart.ChangesAudioSet)
            {
                FireArm.AudioClipSet = newPart.CustomAudioSet;
            }

            FireArm.UpdateCurrentMuzzle();

            UpdateFireArm(oldPart, newPart);

            UnityEngine.Object.Destroy(ModularBarrelAttachmentPoint.ModularPartPoint.gameObject);
            ModularBarrelAttachmentPoint.ModularPartPoint = modularBarrelPrefab.transform;

            newPart.EnablePart();

            ApplyPartPointOccupation(newPart);

            TryApplyOldSkin(ModularBarrelAttachmentPoint, selectedPart, oldSkins, isRandomized);

            ConfigureNewSubParts(newPart, oldSubParts, oldSkins, isRandomized);

            PartAdded?.Invoke(ModularBarrelAttachmentPoint, newPart);

            FireArm.ResetClampCOM();

            return newPart;
        }
        public ModularHandguard ConfigureModularHandguard(string selectedPart, bool isRandomized)
        {
            Dictionary<string, GameObject> partsDictionary = ModularHandguardPrefabsDictionary;
            if (partsDictionary != null && !partsDictionary.ContainsKey(selectedPart))
            {
                OpenScripts2_BepInExPlugin.LogError(FireArm, $"Handguard PartsAttachmentPoint Error: Parts group \"{ModularHandguardAttachmentPoint.ModularPartsGroupID}\" does not contain part with name \"{selectedPart}\"");
                return null;
            }
            else if (partsDictionary == null)
            {
                OpenScripts2_BepInExPlugin.LogError(FireArm, $"Handguard PartsAttachmentPoint Error: Parts group \"{ModularHandguardAttachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary!");
                return null;
            }

            GameObject modularHandguardPrefab = UnityEngine.Object.Instantiate(partsDictionary[selectedPart], ModularHandguardAttachmentPoint.ModularPartPoint.position, ModularHandguardAttachmentPoint.ModularPartPoint.rotation, ModularHandguardAttachmentPoint.ModularPartPoint.parent);
            ModularHandguard oldPart = ModularHandguardAttachmentPoint.ModularPartPoint.GetComponentInChildren<ModularHandguard>();

            Dictionary<string, string> oldSubParts = new();
            Dictionary<string, string> oldSkins = new();

            if (oldPart != null)
            {
                oldPart.DisablePart();

                RemovePartPointOccupation(oldPart);

                if (!oldSkins.ContainsKey(ModularHandguardAttachmentPoint.ModularPartsGroupID)) oldSkins.Add(ModularHandguardAttachmentPoint.ModularPartsGroupID, ModularHandguardAttachmentPoint.CurrentSkin);

                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    oldSubParts.Add(point.ModularPartsGroupID, point.SelectedModularWeaponPart);
                    oldSkins.Add(point.ModularPartsGroupID, point.CurrentSkin);

                    RemoveSubAttachmentPoint(point, FireArm, oldSubParts);
                }
            }

            ModularHandguardAttachmentPoint.SelectedModularWeaponPart = selectedPart;
            ModularHandguard newPart = modularHandguardPrefab.GetComponent<ModularHandguard>();
            newPart.AdjustScale(ModularHandguardAttachmentPoint);

            FireArm.Foregrip.gameObject.SetActive(newPart.ActsLikeForeGrip);
            if (newPart.ActsLikeForeGrip)
            {
                Collider grabTrigger = FireArm.Foregrip.GetComponent<Collider>();
                FireArm.Foregrip.transform.GoToTransformProxy(newPart.ForeGripTransformProxy);
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

                                FireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = FireArm.Foregrip.GetComponentsInChildren<Collider>(true);
                                break;
                            case ModularHandguard.EColliderType.Box:
                                BoxCollider boxCollider = grabTrigger.gameObject.AddComponent<BoxCollider>();
                                boxCollider.center = newPart.TriggerCenter;
                                boxCollider.size = newPart.TriggerSize;
                                boxCollider.isTrigger = true;

                                UnityEngine.Object.Destroy(grabTrigger);

                                FireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = FireArm.Foregrip.GetComponentsInChildren<Collider>(true);
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

                                FireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = FireArm.Foregrip.GetComponentsInChildren<Collider>(true);
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

                                FireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = FireArm.Foregrip.GetComponentsInChildren<Collider>(true);
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

                                FireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = FireArm.Foregrip.GetComponentsInChildren<Collider>(true);
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

                                FireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = FireArm.Foregrip.GetComponentsInChildren<Collider>(true);
                                break;
                            case ModularHandguard.EColliderType.Box:
                                c.center = newPart.TriggerCenter;
                                c.size = newPart.TriggerSize;
                                break;
                        }
                        break;
                    case null:
                        switch (newPart.ColliderType)
                        {
                            case ModularHandguard.EColliderType.Sphere:
                                SphereCollider sphereCollider = grabTrigger.gameObject.AddComponent<SphereCollider>();
                                sphereCollider.center = newPart.TriggerCenter;
                                sphereCollider.radius = newPart.TriggerSize.x;
                                sphereCollider.isTrigger = true;

                                FireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = FireArm.Foregrip.GetComponentsInChildren<Collider>(true);
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

                                FireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = FireArm.Foregrip.GetComponentsInChildren<Collider>(true);
                                break;
                            case ModularHandguard.EColliderType.Box:
                                BoxCollider boxCollider = grabTrigger.gameObject.AddComponent<BoxCollider>();
                                boxCollider.center = newPart.TriggerCenter;
                                boxCollider.size = newPart.TriggerSize;
                                boxCollider.isTrigger = true;

                                FireArm.Foregrip.GetComponent<FVRAlternateGrip>().m_colliders = FireArm.Foregrip.GetComponentsInChildren<Collider>(true);
                                break;
                        }
                        break;
                }
            }

            UpdateFireArm(oldPart, newPart);

            UnityEngine.Object.Destroy(ModularHandguardAttachmentPoint.ModularPartPoint.gameObject);
            ModularHandguardAttachmentPoint.ModularPartPoint = modularHandguardPrefab.transform;

            newPart.EnablePart();

            ApplyPartPointOccupation(newPart);

            TryApplyOldSkin(ModularHandguardAttachmentPoint, selectedPart, oldSkins, isRandomized);

            ConfigureNewSubParts(newPart, oldSubParts, oldSkins, isRandomized);

            PartAdded?.Invoke(ModularHandguardAttachmentPoint, newPart);

            FireArm.ResetClampCOM();

            return newPart;
        }

        public ModularStock ConfigureModularStock(string selectedPart, bool isRandomized)
        {
            Dictionary<string, GameObject> partsDictionary = ModularStockPrefabsDictionary;
            if (partsDictionary != null && !partsDictionary.ContainsKey(selectedPart))
            {
                OpenScripts2_BepInExPlugin.LogError(FireArm, $"Stock PartsAttachmentPoint Error: Parts group \"{ModularStockAttachmentPoint.ModularPartsGroupID}\" does not contain part with name \"{selectedPart}\"");
                return null;
            }
            else if (partsDictionary == null)
            {
                OpenScripts2_BepInExPlugin.LogError(FireArm, $"Stock PartsAttachmentPoint Error: Parts group \"{ModularStockAttachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary!");
                return null;
            }

            GameObject modularStockPrefab = UnityEngine.Object.Instantiate(partsDictionary[selectedPart], ModularStockAttachmentPoint.ModularPartPoint.position, ModularStockAttachmentPoint.ModularPartPoint.rotation, ModularStockAttachmentPoint.ModularPartPoint.parent);

            ModularStock oldPart = ModularStockAttachmentPoint.ModularPartPoint.GetComponentInChildren<ModularStock>();

            Dictionary<string, string> oldSubParts = new();
            Dictionary<string, string> oldSkins = new();

            if (oldPart != null)
            {
                oldPart.DisablePart();

                RemovePartPointOccupation(oldPart);

                if (!oldSkins.ContainsKey(ModularStockAttachmentPoint.ModularPartsGroupID)) oldSkins.Add(ModularStockAttachmentPoint.ModularPartsGroupID, ModularStockAttachmentPoint.CurrentSkin);

                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    oldSubParts.Add(point.ModularPartsGroupID, point.SelectedModularWeaponPart);
                    oldSkins.Add(point.ModularPartsGroupID, point.CurrentSkin);

                    RemoveSubAttachmentPoint(point, FireArm, oldSubParts);
                }

                if (oldPart.ChangesPosePosition)
                {
                    FVRViveHand hand = GM.CurrentMovementManager.Hands[0];
                    if (hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index)
                    {
                        FireArm.PoseOverride.GoToTransformProxy(OrigPoseOverride_Touch);
                        FireArm.PoseOverride_Touch.GoToTransformProxy(OrigPoseOverride_Touch);
                        FireArm.m_storedLocalPoseOverrideRot = FireArm.PoseOverride.localRotation;
                    }
                    else
                    {
                        FireArm.PoseOverride.GoToTransformProxy(OrigPoseOverride);
                        FireArm.m_storedLocalPoseOverrideRot = FireArm.PoseOverride.localRotation;
                    }
                }
            }
            ModularStockAttachmentPoint.SelectedModularWeaponPart = selectedPart;
            ModularStock newPart = modularStockPrefab.GetComponent<ModularStock>();
            newPart.AdjustScale(ModularStockAttachmentPoint);

            FireArm.HasActiveShoulderStock = newPart.ActsLikeStock;
            FireArm.StockPos = newPart.StockPoint;

            if (newPart.CollapsingStock != null) newPart.CollapsingStock.Firearm = FireArm;
            if (newPart.FoldingStockX != null) newPart.FoldingStockX.FireArm = FireArm;
            if (newPart.FoldingStockY != null) newPart.FoldingStockY.FireArm = FireArm;

            if (newPart.ChangesPosePosition)
            {
                FVRViveHand hand = GM.CurrentMovementManager.Hands[0];
                if (hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index)
                {
                    FireArm.PoseOverride.GoToTransformProxy(newPart.CustomPoseOverride_TouchProxy);
                    FireArm.PoseOverride_Touch.GoToTransformProxy(newPart.CustomPoseOverride_TouchProxy);
                    FireArm.m_storedLocalPoseOverrideRot = FireArm.PoseOverride.localRotation;
                }
                else
                {
                    FireArm.PoseOverride.GoToTransformProxy(newPart.CustomPoseOverrideProxy);
                    FireArm.m_storedLocalPoseOverrideRot = FireArm.PoseOverride.localRotation;
                }
            }

            UpdateFireArm(oldPart, newPart);

            UnityEngine.Object.Destroy(ModularStockAttachmentPoint.ModularPartPoint.gameObject);
            ModularStockAttachmentPoint.ModularPartPoint = modularStockPrefab.transform;

            newPart.EnablePart();

            ApplyPartPointOccupation(newPart);

            TryApplyOldSkin(ModularStockAttachmentPoint, selectedPart, oldSkins, isRandomized);

            ConfigureNewSubParts(newPart, oldSubParts, oldSkins, isRandomized);

            PartAdded?.Invoke(ModularStockAttachmentPoint, newPart);

            FireArm.ResetClampCOM();

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

        public void CheckForDefaultReceiverSkin(FVRFireArm FireArm)
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
                OpenScripts2_BepInExPlugin.LogWarning(FireArm, $"No SkinsDefinition found for receiver skin path {SkinPath}, but part receiver {FireArm.gameObject.name} set to skin name {CurrentSelectedReceiverSkinID}. Naming error?");
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

        private void UpdateFireArm(ModularWeaponPart oldPart, ModularWeaponPart newPart)
        {
            IPartFireArmRequirement[] partFireArmRequirements;
            if (oldPart != null)
            {
                foreach (var mount in oldPart.AttachmentMounts)
                {
                    DetachAllAttachmentsFromMount(mount);

                    FireArm.AttachmentMounts.Remove(mount);
                }
                FireArm.Slots = FireArm.Slots.Where(s => !oldPart.SubQuickBeltSlots.Contains(s)).ToArray();

                partFireArmRequirements = oldPart.GetComponents<IPartFireArmRequirement>();
                foreach (var item in partFireArmRequirements)
                {
                    item.FireArm = null;
                }
            }
            FireArm.AttachmentMounts.AddRange(newPart.AttachmentMounts);
            foreach (var mount in newPart.AttachmentMounts)
            {
                mount.Parent = FireArm;
                mount.MyObject = FireArm;
            }

            FireArm.Slots.AddRangeToArray(newPart.SubQuickBeltSlots);

            FireArm.m_colliders = FireArm.GetComponentsInChildren<Collider>(true);

            if (FireArm.m_quickbeltSlot != null) FireArm.SetAllCollidersToLayer(false, "NoCol");

            partFireArmRequirements = newPart.GetComponents<IPartFireArmRequirement>();
            foreach (var item in partFireArmRequirements)
            {
                item.FireArm = FireArm;
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

        private void RemovePartPointOccupation(ModularWeaponPart part)
        {
            foreach (var alsoOccupiesPointWithModularPartsGroupID in part.AlsoOccupiesPointWithModularPartsGroupIDs)
            {
                if (AllAttachmentPoints.TryGetValue(alsoOccupiesPointWithModularPartsGroupID, out ModularWeaponPartsAttachmentPoint disabledPoint))
                {
                    disabledPoint.IsPointDisabled = false;
                    ConfigureModularWeaponPart(disabledPoint, disabledPoint.SelectedModularWeaponPart);

                    if (!SubAttachmentPoints.Contains(disabledPoint)) WorkshopPlatform?.CreateUIForPoint(disabledPoint, ModularWorkshopUI.EPartType.MainWeaponGeneralAttachmentPoint);
                    else WorkshopPlatform?.CreateUIForPoint(disabledPoint);
                }
            }
        }

        private void ApplyPartPointOccupation(ModularWeaponPart part)
        {
            foreach (var alsoOccupiesPointWithModularPartsGroupID in part.AlsoOccupiesPointWithModularPartsGroupIDs)
            {
                if (AllAttachmentPoints.TryGetValue(alsoOccupiesPointWithModularPartsGroupID, out ModularWeaponPartsAttachmentPoint pointToDisable))
                {
                    WorkshopPlatform?.RemoveUIFromPoint(pointToDisable);

                    pointToDisable.IsPointDisabled = true;
                    GameObject temp = new(pointToDisable.ModularPartsGroupID + "_TempPoint");
                    temp.transform.position = pointToDisable.ModularPartPoint.position;
                    temp.transform.rotation = pointToDisable.ModularPartPoint.rotation;
                    temp.transform.parent = pointToDisable.ModularPartPoint.parent;

                    if (pointToDisable.ModularPartPoint.TryGetComponent(out ModularWeaponPart partToRemove))
                    {
                        foreach (var mount in partToRemove.AttachmentMounts)
                        {
                            DetachAllAttachmentsFromMount(mount);

                            FireArm.AttachmentMounts.Remove(mount);
                        }
                        FireArm.Slots = FireArm.Slots.Where(s => !partToRemove.SubQuickBeltSlots.Contains(s)).ToArray();

                        IPartFireArmRequirement[] partFireArmRequirements = partToRemove.GetComponents<IPartFireArmRequirement>();
                        foreach (var addon in partFireArmRequirements)
                        {
                            addon.FireArm = null;
                        }
                    }

                    UnityEngine.Object.Destroy(pointToDisable.ModularPartPoint.gameObject);
                    pointToDisable.ModularPartPoint = temp.transform;
                }
            }
        }

        private void TryApplyOldSkin(ModularWeaponPartsAttachmentPoint point, string selectedPart, Dictionary<string, string> oldSkins, bool isRandomized = false)
        {
            point.CheckForDefaultSkin();

            if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(point.SkinPath, out ModularWorkshopSkinsDefinition definition))
            {
                if (!isRandomized)
                {
                    if (oldSkins.TryGetValue(point.ModularPartsGroupID, out string skinName))
                    {
                        if (definition.SkinDictionary.ContainsKey(skinName))
                        {
                            point.ApplySkin(skinName);
                        }
                        else if (point.PreviousSkins.TryGetValue(selectedPart, out skinName))
                        {
                            if (definition.SkinDictionary.ContainsKey(skinName))
                            {
                                point.ApplySkin(skinName);
                            }
                        }
                    }
                    else if (point.PreviousSkins.TryGetValue(selectedPart, out skinName))
                    {
                        if (definition.SkinDictionary.ContainsKey(skinName))
                        {
                            point.ApplySkin(skinName);
                        }
                    }
                }
                else
                {
                    point.ApplySkin(definition.GetRandomSkin());
                }
            }
        }

        private void ConfigureNewSubParts(ModularWeaponPart newPart, Dictionary<string, string> oldSubParts, Dictionary<string, string> oldSkins, bool isRandomized)
        {
            foreach (var subPoint in newPart.SubAttachmentPoints)
            {
                if (!isRandomized && oldSubParts.TryGetValue(subPoint.ModularPartsGroupID, out string oldSelectedPart) && oldSelectedPart != subPoint.SelectedModularWeaponPart) AddSubAttachmentPoint(subPoint, FireArm, oldSubParts, oldSkins, oldSelectedPart);
                else
                {
                    string selectedSubPart = isRandomized ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[subPoint.ModularPartsGroupID].GetRandomPart() : subPoint.SelectedModularWeaponPart;

                    AddSubAttachmentPoint(subPoint, FireArm, oldSubParts, oldSkins, selectedSubPart, isRandomized);
                }
            }
        }

        public void UpdateSelectedParts()
        {
            foreach (var point in AllAttachmentPoints.Values)
            {
                UpdateSelectedPart(point);
            }
        }

        private void UpdateSelectedPart(ModularWeaponPartsAttachmentPoint point)
        {
            if (point.ModularPartPoint.TryGetComponentInChildren(out ModularWeaponPart part))
            {
                point.SelectedModularWeaponPart = part.Name;

                foreach (var subpoint in part.SubAttachmentPoints)
                {
                    UpdateSelectedPart(subpoint);
                }
            }
        }
    }

    public delegate void PartAdded(ModularWeaponPartsAttachmentPoint point, ModularWeaponPart part);
}