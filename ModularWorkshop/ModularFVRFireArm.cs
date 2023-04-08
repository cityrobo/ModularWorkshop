using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;

namespace ModularWorkshop
{
    [Serializable]
    public class ModularFVRFireArm
    {
        public GameObject UIPrefab;
        public string ModularBarrelPartID;
        public Transform ModularBarrelPosition;
        public Transform ModularBarrelUIPosition;
        [HideInInspector]
        public TransformProxy ModularBarrelUIPositionProxy;
        public string ModularHandguardPartID;
        public Transform ModularHandguardPosition;
        public Transform ModularHandguardUIPosition;
        [HideInInspector]
        public TransformProxy ModularHandguardUIPositionProxy;
        public string ModularStockPartID;
        public Transform ModularStockPosition;
        public Transform ModularStockUIPosition;
        [HideInInspector]
        public TransformProxy ModularStockUIPositionProxy;

        public string SelectedModularBarrel;

        public string SelectedModularHandguard;

        public string SelectedModularStock;

        public ModularWeaponPartsAttachmentPoint[] ModularWeaponPartsAttachmentPoints;
        public ModularWeaponPartsAttachmentPoint[] GetModularWeaponPartsAttachmentPoints => ModularWeaponPartsAttachmentPoints;
        public Dictionary<string, GameObject> ModularBarrelPrefabsDictionary => ModularWorkshopManager.ModularWorkshopDictionary[ModularBarrelPartID].PartsDictionary;
        public Dictionary<string, GameObject> ModularHandguardPrefabsDictionary => ModularWorkshopManager.ModularWorkshopDictionary[ModularHandguardPartID].PartsDictionary;
        public Dictionary<string, GameObject> ModularStockPrefabsDictionary => ModularWorkshopManager.ModularWorkshopDictionary[ModularStockPartID].PartsDictionary;

        [HideInInspector]
        public List<ModularWeaponPartsAttachmentPoint> SubAttachmentPoints;
        [HideInInspector]
        public ModularWorkshopPlatform WorkshopPlatform;

        private const string c_modularBarrelKey = "ModulBarrel";
        private const string c_modularHandguardKey = "ModulHandguard";
        private const string c_modularStockKey = "ModulStock";

        // Original Barrel Parameters
        private FVRFireArmMechanicalAccuracyClass _origMechanicalAccuracyClass;

        private MuzzleEffectSize _origMuzzleEffectSize;
        private MuzzleEffect[] _origMuzzleEffects;

        private FireArmRoundType _origRoundType;
        private TransformProxy _origMagMountPos;
        private TransformProxy _origMagEjectPos;
        private FireArmMagazineType _origMagazineType;

        // Original Stock Parameters
        private TransformProxy _origPoseOverride;
        private TransformProxy _origPoseOverride_Touch;

        public void Awake(FVRFireArm fireArm)
        {
            _origMuzzleEffectSize = fireArm.DefaultMuzzleEffectSize;
            _origMuzzleEffects = fireArm.MuzzleEffects;
            _origMechanicalAccuracyClass = fireArm.AccuracyClass;

            _origRoundType = fireArm.RoundType;
            if (fireArm.MagazineMountPos != null) _origMagMountPos = new(fireArm.MagazineMountPos);
            if (fireArm.MagazineEjectPos != null) _origMagEjectPos = new(fireArm.MagazineEjectPos);
            _origMagazineType = fireArm.MagazineType;

            _origPoseOverride = new(fireArm.PoseOverride);
            _origPoseOverride_Touch = new(fireArm.PoseOverride_Touch);
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
                if (f.TryGetValue("Modul" + modularWeaponPartsAttachmentPoint.PartID, out selectedPart)) ConfigureModularWeaponPart(modularWeaponPartsAttachmentPoint, selectedPart, fireArm);
            }
            foreach (var subPoint in SubAttachmentPoints)
            {
                if (f.TryGetValue("Modul" + subPoint.PartID, out selectedPart)) ConfigureModularWeaponPart(subPoint, selectedPart, fireArm);
            }
        }

        public Dictionary<string, string> GetFlagDic(Dictionary<string, string> flagDic)
        {
            if (ModularBarrelPartID != string.Empty) flagDic.Add(c_modularBarrelKey, SelectedModularBarrel);
            if (ModularHandguardPartID != string.Empty) flagDic.Add(c_modularHandguardKey, SelectedModularHandguard);
            if (ModularStockPartID != string.Empty) flagDic.Add(c_modularStockKey, SelectedModularStock);
            ModularWorkshopPartsDefinition partDefinition;
            foreach (var modularWeaponPartsAttachmentPoint in ModularWeaponPartsAttachmentPoints)
            {
                if (ModularWorkshopManager.ModularWorkshopDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.PartID, out partDefinition) && partDefinition.ModularPrefabs.Count > 0) flagDic.Add("Modul" + modularWeaponPartsAttachmentPoint.PartID, modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart);
            }
            foreach (var subPoint in SubAttachmentPoints)
            {
                if (ModularWorkshopManager.ModularWorkshopDictionary.TryGetValue(subPoint.PartID, out partDefinition) && partDefinition.ModularPrefabs.Count > 0) flagDic.Add("Modul" + subPoint.PartID, subPoint.SelectedModularWeaponPart);
            }

            return flagDic;
        }

        public ModularWeaponPart ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, string selectedPart, FVRFireArm fireArm)
        {
            modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart = selectedPart;
            ModularWorkshopManager.ModularWorkshopDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.PartID, out ModularWorkshopPartsDefinition prefabs);
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
            SelectedModularBarrel = selectedPart;

            GameObject modularBarrelPrefab = UnityEngine.Object.Instantiate(ModularBarrelPrefabsDictionary[selectedPart], ModularBarrelPosition.position, ModularBarrelPosition.rotation, ModularBarrelPosition.parent);

            ModularBarrel oldPart = ModularBarrelPosition.GetComponent<ModularBarrel>();
            ModularBarrel newPart = modularBarrelPrefab.GetComponent<ModularBarrel>();

            fireArm.MuzzlePos.position = newPart.MuzzlePosition.position;
            fireArm.MuzzlePos.rotation = newPart.MuzzlePosition.rotation;
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
                    fireArm.DefaultMuzzleEffectSize = _origMuzzleEffectSize;
                    fireArm.MuzzleEffects = _origMuzzleEffects;
                }
                if (oldPart.HasCustomMechanicalAccuracy)
                {
                    fireArm.AccuracyClass = _origMechanicalAccuracyClass;
                    fireArm.m_internalMechanicalMOA = AM.GetFireArmMechanicalSpread(fireArm.AccuracyClass);
                }
                if (oldPart.ChangesFireArmRoundType)
                {
                    fireArm.RoundType = _origRoundType;
                    foreach (var chamber in fireArm.FChambers)
                    {
                        chamber.RoundType = _origRoundType;
                    }
                }
                if (oldPart.ChangesMagazineMountPoint)
                {
                    fireArm.MagazineMountPos.GoToTransformProxy(_origMagMountPos);
                    fireArm.MagazineEjectPos.GoToTransformProxy(_origMagEjectPos);
                }
                if (oldPart.ChangesMagazineType)
                {
                    fireArm.MagazineType = _origMagazineType;
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

            UnityEngine.Object.Destroy(ModularBarrelPosition.gameObject);
            ModularBarrelPosition = modularBarrelPrefab.transform;
            return newPart;
        }
        public ModularHandguard ConfigureModularHandguard(string selectedPart, FVRFireArm fireArm)
        {
            SelectedModularHandguard = selectedPart;

            GameObject modularHandguardPrefab = UnityEngine.Object.Instantiate(ModularHandguardPrefabsDictionary[selectedPart], ModularHandguardPosition.position, ModularHandguardPosition.rotation, ModularHandguardPosition.parent);
            ModularHandguard oldPart = ModularHandguardPosition.GetComponent<ModularHandguard>();

            if (oldPart != null)
            {
                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    RemoveSubAttachmentPoint(point, fireArm);
                }
            }

            ModularHandguard newPart = modularHandguardPrefab.GetComponent<ModularHandguard>();

            fireArm.Foregrip.gameObject.SetActive(newPart.ActsLikeForeGrip);
            Collider grabTrigger = fireArm.AltGrip.GetComponent<Collider>();
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


            foreach (var point in newPart.SubAttachmentPoints)
            {
                AddSubAttachmentPoint(point, fireArm);
            }

            UpdateFirearm(oldPart, newPart, fireArm);

            UnityEngine.Object.Destroy(ModularHandguardPosition.gameObject);
            ModularHandguardPosition = modularHandguardPrefab.transform;

            return newPart;
        }

        public ModularStock ConfigureModularStock(string selectedPart, FVRFireArm fireArm)
        {
            SelectedModularStock = selectedPart;

            GameObject modularStockPrefab = UnityEngine.Object.Instantiate(ModularStockPrefabsDictionary[selectedPart], ModularStockPosition.position, ModularStockPosition.rotation, ModularStockPosition.parent);

            ModularStock oldPart = ModularStockPosition.GetComponent<ModularStock>();

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
                        fireArm.PoseOverride.GoToTransformProxy(_origPoseOverride_Touch);
                        fireArm.PoseOverride_Touch.GoToTransformProxy(_origPoseOverride_Touch);
                        fireArm.m_storedLocalPoseOverrideRot = fireArm.PoseOverride.localRotation;
                    }
                    else
                    {
                        fireArm.PoseOverride.GoToTransformProxy(_origPoseOverride);
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

            UnityEngine.Object.Destroy(ModularStockPosition.gameObject);
            ModularStockPosition = modularStockPrefab.transform;

            return newPart;
        }

        public void ConvertTransformsToProxies(FVRFireArm fireArm)
        {
            if (ModularBarrelPartID != string.Empty)
            {
                ModularBarrelUIPositionProxy = new(ModularBarrelUIPosition, fireArm.transform);
                UnityEngine.Object.Destroy(ModularBarrelUIPosition.gameObject);
            }
            if (ModularHandguardPartID != string.Empty)
            {
                ModularHandguardUIPositionProxy = new(ModularHandguardUIPosition, fireArm.transform);
                UnityEngine.Object.Destroy(ModularHandguardUIPosition.gameObject);
            }
            if (ModularStockPartID != string.Empty)
            {
                ModularStockUIPositionProxy = new(ModularStockUIPosition, fireArm.transform);
                UnityEngine.Object.Destroy(ModularStockUIPosition.gameObject);
            }

            foreach (var point in ModularWeaponPartsAttachmentPoints)
            {
                point.ModularPartUIPos = new(point.ModularPartUIPoint, fireArm.transform);
                UnityEngine.Object.Destroy(point.ModularPartUIPoint.gameObject);
            }
        }

        private void UpdateFirearm(ModularWeaponPart oldPart, ModularWeaponPart newPart, FVRFireArm fireArm)
        {
            if (oldPart != null)
            {
                FVRFireArmAttachment[] attachments;
                foreach (var mount in oldPart.AttachmentMounts)
                {
                    attachments = mount.AttachmentsList.ToArray();
                    foreach (var attachment in attachments)
                    {
                        attachment.SetAllCollidersToLayer(false, "Default");
                        attachment.DetachFromMount();
                    }

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
    }
}
