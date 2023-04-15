using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;
using System.Net.Mail;

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
        public ModularWeaponPartsAttachmentPoint[] GetModularWeaponPartsAttachmentPoints => ModularWeaponPartsAttachmentPoints;
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

                keyValuePairs.Add(ModularBarrelAttachmentPoint.ModularPartsGroupID, ModularBarrelAttachmentPoint);
                keyValuePairs.Add(ModularHandguardAttachmentPoint.ModularPartsGroupID, ModularHandguardAttachmentPoint);
                keyValuePairs.Add(ModularStockAttachmentPoint.ModularPartsGroupID, ModularStockAttachmentPoint);

                foreach (var point in ModularWeaponPartsAttachmentPoints)
                {
                    keyValuePairs.Add(point.ModularPartsGroupID, point);
                }
                foreach (var subPoint in SubAttachmentPoints)
                {
                    keyValuePairs.Add(subPoint.ModularPartsGroupID, subPoint);
                }

                return keyValuePairs;
            }
        }

        public void Awake(FVRFireArm fireArm)
        {
            OrigMuzzleEffectSize = fireArm.DefaultMuzzleEffectSize;
            OrigMuzzleEffects = fireArm.MuzzleEffects;
            OrigMechanicalAccuracyClass = fireArm.AccuracyClass;

            OrigRoundType = fireArm.RoundType;

            OrigRecoilProfile = fireArm.RecoilProfile;
            OrigRecoilProfileStocked = fireArm.RecoilProfileStocked;

            if (fireArm.MagazineMountPos != null) OrigMagMountPos = new(fireArm.MagazineMountPos);
            if (fireArm.MagazineEjectPos != null) OrigMagEjectPos = new(fireArm.MagazineEjectPos);
            OrigMagazineType = fireArm.MagazineType;

            OrigPoseOverride = new(fireArm.PoseOverride);
            OrigPoseOverride_Touch = new(fireArm.PoseOverride_Touch);
        }

        public void AddSubAttachmentPoint(ModularWeaponPartsAttachmentPoint subPoint, FVRFireArm fireArm)
        {
            SubAttachmentPoints.Add(subPoint);
            ConfigureModularWeaponPart(subPoint, subPoint.SelectedModularWeaponPart, fireArm);

            WorkshopPlatform?.CreateUIForPoint(subPoint);
        }

        public void RemoveSubAttachmentPoint(ModularWeaponPartsAttachmentPoint subPoint, FVRFireArm fireArm)
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
                    RemoveSubAttachmentPoint(subSubPoint, fireArm);
                }
            }

            UnityEngine.Object.Destroy(subPoint.ModularPartPoint.gameObject);
        }

        public void ConfigureFromFlagDic(Dictionary<string, string> f, FVRFireArm fireArm)
        {
            string selectedPart;
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

            return flagDic;
        }

        public ModularWeaponPart ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, string selectedPart, FVRFireArm fireArm)
        {
            modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart = selectedPart;
            ModularWorkshopManager.ModularWorkshopDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out ModularWorkshopPartsDefinition prefabs);
            GameObject modularWeaponPartPrefab = UnityEngine.Object.Instantiate(prefabs.PartsDictionary[selectedPart], modularWeaponPartsAttachmentPoint.ModularPartPoint.position, modularWeaponPartsAttachmentPoint.ModularPartPoint.rotation, modularWeaponPartsAttachmentPoint.ModularPartPoint.parent);
            ModularWeaponPart oldPart = modularWeaponPartsAttachmentPoint.ModularPartPoint.GetComponent<ModularWeaponPart>();

            if (oldPart != null)
            {
                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    RemoveSubAttachmentPoint(point, fireArm);
                }
            }

            ModularWeaponPart newPart = modularWeaponPartPrefab.GetComponent<ModularWeaponPart>();

            UpdateFirearm(oldPart, newPart, fireArm);

            UnityEngine.Object.Destroy(modularWeaponPartsAttachmentPoint.ModularPartPoint.gameObject);
            modularWeaponPartsAttachmentPoint.ModularPartPoint = newPart.transform;

            foreach (var point in newPart.SubAttachmentPoints)
            {
                AddSubAttachmentPoint(point, fireArm);
            }

            return newPart;
        }

        public ModularBarrel ConfigureModularBarrel(string selectedPart, FVRFireArm fireArm)
        {
            Debug.Log($"Selected Barrel Name: {selectedPart}");

            for (int i = 0; i < ModularBarrelPrefabsDictionary.Count; i++)
            {
                if (ModularBarrelPrefabsDictionary.ElementAt(i).Key == null) Debug.LogError($"ModularBarrelPrefabsDictionary: Key at index {i} is null!");
                else if (ModularBarrelPrefabsDictionary.ElementAt(i).Key == selectedPart) Debug.Log($"ModularBarrelPrefabsDictionary: Key at index {i} is Selected part with name {ModularBarrelPrefabsDictionary.ElementAt(i).Key}!");
                else Debug.Log($"ModularBarrelPrefabsDictionary: Key at index {i} is part name {ModularBarrelPrefabsDictionary.ElementAt(i).Key}!");
                if (ModularBarrelPrefabsDictionary.ElementAt(i).Value == null) Debug.LogError($"ModularBarrelPrefabsDictionary: Value at index {i} is null!");
                else Debug.LogError($"ModularBarrelPrefabsDictionary: Value at index {i} is Game Object {ModularBarrelPrefabsDictionary.ElementAt(i).Value.name}!");
            }

            ModularBarrelAttachmentPoint.SelectedModularWeaponPart = selectedPart;
            GameObject modularBarrelPrefab = UnityEngine.Object.Instantiate(ModularBarrelPrefabsDictionary[selectedPart], ModularBarrelAttachmentPoint.ModularPartPoint.position, ModularBarrelAttachmentPoint.ModularPartPoint.rotation, ModularBarrelAttachmentPoint.ModularPartPoint.parent);

            ModularBarrel oldPart = ModularBarrelAttachmentPoint.ModularPartPoint.GetComponent<ModularBarrel>();
            ModularBarrel newPart = modularBarrelPrefab.GetComponent<ModularBarrel>();

            fireArm.MuzzlePos.GoToTransformProxy(newPart.MuzzlePosProxy);

            fireArm.DefaultMuzzleState = newPart.DefaultMuzzleState;
            fireArm.DefaultMuzzleDamping = newPart.DefaultMuzzleDamping;

            if (oldPart != null)
            {
                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    RemoveSubAttachmentPoint(point, fireArm);
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
            }
            if (newPart.HasCustomMuzzleEffects)
            {
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

            foreach (var point in newPart.SubAttachmentPoints)
            {
                AddSubAttachmentPoint(point, fireArm);
            }

            fireArm.UpdateCurrentMuzzle();

            UpdateFirearm(oldPart, newPart, fireArm);

            UnityEngine.Object.Destroy(ModularBarrelAttachmentPoint.ModularPartPoint.gameObject);
            ModularBarrelAttachmentPoint.ModularPartPoint = modularBarrelPrefab.transform;
            return newPart;
        }
        public ModularHandguard ConfigureModularHandguard(string selectedPart, FVRFireArm fireArm)
        {
            ModularHandguardAttachmentPoint.SelectedModularWeaponPart = selectedPart;

            GameObject modularHandguardPrefab = UnityEngine.Object.Instantiate(ModularHandguardPrefabsDictionary[selectedPart], ModularHandguardAttachmentPoint.ModularPartPoint.position, ModularHandguardAttachmentPoint.ModularPartPoint.rotation, ModularHandguardAttachmentPoint.ModularPartPoint.parent);
            ModularHandguard oldPart = ModularHandguardAttachmentPoint.ModularPartPoint.GetComponent<ModularHandguard>();

            if (oldPart != null)
            {
                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    RemoveSubAttachmentPoint(point, fireArm);
                }
            }

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

            foreach (var point in newPart.SubAttachmentPoints)
            {
                AddSubAttachmentPoint(point, fireArm);
            }

            UpdateFirearm(oldPart, newPart, fireArm);

            UnityEngine.Object.Destroy(ModularHandguardAttachmentPoint.ModularPartPoint.gameObject);
            ModularHandguardAttachmentPoint.ModularPartPoint = modularHandguardPrefab.transform;

            return newPart;
        }

        public ModularStock ConfigureModularStock(string selectedPart, FVRFireArm fireArm)
        {
            ModularStockAttachmentPoint.SelectedModularWeaponPart = selectedPart;

            GameObject modularStockPrefab = UnityEngine.Object.Instantiate(ModularStockPrefabsDictionary[selectedPart], ModularStockAttachmentPoint.ModularPartPoint.position, ModularStockAttachmentPoint.ModularPartPoint.rotation, ModularStockAttachmentPoint.ModularPartPoint.parent);

            ModularStock oldPart = ModularStockAttachmentPoint.ModularPartPoint.GetComponent<ModularStock>();

            if (oldPart != null)
            {
                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    RemoveSubAttachmentPoint(point, fireArm);
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

            foreach (var point in newPart.SubAttachmentPoints)
            {
                AddSubAttachmentPoint(point, fireArm);
            }

            UpdateFirearm(oldPart, newPart, fireArm);

            UnityEngine.Object.Destroy(ModularStockAttachmentPoint.ModularPartPoint.gameObject);
            ModularStockAttachmentPoint.ModularPartPoint = modularStockPrefab.transform;

            return newPart;
        }

        public void ConvertTransformsToProxies(FVRFireArm fireArm)
        {
            if (ModularBarrelAttachmentPoint.ModularPartsGroupID != string.Empty)
            {
                ModularBarrelAttachmentPoint.ModularPartUIPointProxy = new(ModularBarrelAttachmentPoint.ModularPartUIPoint, fireArm.transform);
                UnityEngine.Object.Destroy(ModularBarrelAttachmentPoint.ModularPartUIPoint.gameObject);
            }
            if (ModularHandguardAttachmentPoint.ModularPartsGroupID != string.Empty)
            {
                ModularHandguardAttachmentPoint.ModularPartUIPointProxy = new(ModularHandguardAttachmentPoint.ModularPartUIPoint, fireArm.transform);
                UnityEngine.Object.Destroy(ModularHandguardAttachmentPoint.ModularPartUIPoint.gameObject);
            }
            if (ModularStockAttachmentPoint.ModularPartsGroupID != string.Empty)
            {
                ModularStockAttachmentPoint.ModularPartUIPointProxy = new(ModularStockAttachmentPoint.ModularPartUIPoint, fireArm.transform);
                UnityEngine.Object.Destroy(ModularStockAttachmentPoint.ModularPartUIPoint.gameObject);
            }

            foreach (var point in ModularWeaponPartsAttachmentPoints)
            {
                point.ModularPartUIPointProxy = new(point.ModularPartUIPoint, fireArm.transform);
                UnityEngine.Object.Destroy(point.ModularPartUIPoint.gameObject);
            }
        }

        private void UpdateFirearm(ModularWeaponPart oldPart, ModularWeaponPart newPart, FVRFireArm fireArm)
        {
            if (oldPart != null)
            {
                
                foreach (var mount in oldPart.AttachmentMounts)
                {
                    DetachAllAttachments(mount);

                    fireArm.AttachmentMounts.Remove(mount);
                }
            }
            fireArm.AttachmentMounts.AddRange(newPart.AttachmentMounts);
            foreach (var mount in newPart.AttachmentMounts)
            {
                mount.Parent = fireArm;
                mount.MyObject = fireArm;
            }

            fireArm.m_colliders = fireArm.GetComponentsInChildren<Collider>(true);

            if (fireArm.m_quickbeltSlot != null) fireArm.SetAllCollidersToLayer(false, "NoCol");

            IPartFireArmRequirement[] partFireArmRequirements = newPart.GetComponents<IPartFireArmRequirement>();
            foreach (var item in partFireArmRequirements)
            {
                item.FireArm = fireArm;
            }
        }

        private void DetachAllAttachments(FVRFireArmAttachmentMount mount)
        {
            FVRFireArmAttachment[] attachments = mount.AttachmentsList.ToArray();
            foreach (var attachment in attachments)
            {
                foreach (var subMount in attachment.AttachmentMounts)
                {
                    DetachAllAttachments(subMount);
                }
                attachment.SetAllCollidersToLayer(false, "Default");
                attachment.DetachFromMount();
            }
        }

    }
}
