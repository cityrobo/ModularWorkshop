using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;
using HarmonyLib;
using static ModularWorkshop.ModularWorkshopSkinsDefinition;
using static RootMotion.FinalIK.IKSolver;

namespace ModularWorkshop
{
    public class ModularFVRPhysicalObject : MonoBehaviour
    {
        public FVRPhysicalObject MainObject;
        public GameObject UIPrefab;

        [Header("Receiver Skins")]
        [Tooltip("This is a combination of ModularPartsGroupID and PartName of a Skins definition, with a \"/\" in between. A requirement of the system. You should choose ModularPartsGroupID and PartName so that it doesn't conflict with anything else. Formatting Example: \"ModularPartsGroupID/PartName\". I would personally recommend something like \"ItemID/ReceiverName\" as a standard.")]
        public string SkinPath;
        public Transform ReceiverSkinUIPoint;
        [HideInInspector]
        public TransformProxy ReceiverSkinUIPointProxy;
        public string CurrentSelectedReceiverSkinID = "Default";

        [Tooltip("Can be populated with the context menu on the gun.")]
        public MeshRenderer[] ReceiverMeshRenderers;

        public ModularWeaponPartsAttachmentPoint[] ModularWeaponPartsAttachmentPoints;
        [HideInInspector]
        public List<ModularWeaponPartsAttachmentPoint> SubAttachmentPoints;
        [HideInInspector]
        public ModularWorkshopPlatform WorkshopPlatform;

        [HideInInspector]
        public event PartAdded PartAdded;

        private static readonly Dictionary<FVRPhysicalObject, ModularFVRPhysicalObject> _existingModularPhysicalObjects = new();

        // Situation dependent toggles
        [HideInInspector]
        public bool IsInTakeAndHold = false;
        [HideInInspector]
        public bool WasUnvaulted = false;

        public ModularWorkshopSkinsDefinition ReceiverSkinsDefinition
        {
            get
            {
                if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(SkinPath, out ModularWorkshopSkinsDefinition skinsDefinition)) return skinsDefinition;
                else
                {
                    ModularWorkshopManager.LogError(MainObject, $"No Receiver SkinsDefinition found for {SkinPath}!");
                    return null;
                }
            }
        }

        public Dictionary<string, ModularWeaponPartsAttachmentPoint> AllAttachmentPoints
        {
            get
            {
                Dictionary<string, ModularWeaponPartsAttachmentPoint> keyValuePairs = new();

                foreach (var point in ModularWeaponPartsAttachmentPoints)
                {
                    try
                    {
                        keyValuePairs.Add(point.ModularPartsGroupID, point);
                    }
                    catch (Exception)
                    {
                        ModularWorkshopManager.LogError(MainObject, $"PartPoint for ModularPartsGroupID {point.ModularPartsGroupID} already in AllAttachmentPoints dictionary!");
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
                        ModularWorkshopManager.LogError(MainObject, $"SubPartPoint for ModularPartsGroupID {subPoint.ModularPartsGroupID} already in AllAttachmentPoints dictionary!");
                    }
                }

                return keyValuePairs;
            }
        }

        public void Awake()
        {
            if (MainObject == null) MainObject = GetComponent<FVRPhysicalObject>();
            _existingModularPhysicalObjects.Add(MainObject, this);
            if (SkinPath == null && MainObject != null && MainObject.ObjectWrapper != null) SkinPath = MainObject.ObjectWrapper.ItemID + "/" + "Receiver";
            if (ReceiverMeshRenderers == null || ReceiverMeshRenderers.Length == 0) GetReceiverMeshRenderers();
            CheckForDefaultReceiverSkin(MainObject);

            if (GM.TNH_Manager != null)
            {
                IsInTakeAndHold = ModularWorkshopManager.EnableTNHRandomization.Value;
            }
        }

        public void Start()
        {
            if (MainObject.ObjectWrapper == null)
            {
                Destroy(this);
                return;
            }

            ConvertTransformsToProxies();
            if (!WasUnvaulted)
            {
                ApplyReceiverSkin(IsInTakeAndHold ? ReceiverSkinsDefinition.GetRandomSkin() : CurrentSelectedReceiverSkinID);
                ConfigureAll();
            }
        }

        public void ConfigureAll()
        {
            string selectedPart;
            foreach (ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint in ModularWeaponPartsAttachmentPoints)
            {
                if (modularWeaponPartsAttachmentPoint.IsPointDisabled) continue;

                if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out ModularWorkshopPartsDefinition prefabs) && prefabs.PartsDictionary.Count > 0)
                {
                    selectedPart = IsInTakeAndHold && !WasUnvaulted && !modularWeaponPartsAttachmentPoint.DisallowTakeAndHoldRandomization ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[modularWeaponPartsAttachmentPoint.ModularPartsGroupID].GetRandomPart() : modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart;

                    ConfigureModularWeaponPart(modularWeaponPartsAttachmentPoint, selectedPart, IsInTakeAndHold);
                }
                else if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.ContainsKey(modularWeaponPartsAttachmentPoint.ModularPartsGroupID) && prefabs.PartsDictionary.Count == 0)
                {
                    ModularWorkshopManager.LogError(this, $"PartsAttachmentPoint Error: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" found in ModularWorkshopManager dictionary, but it is empty!");
                    modularWeaponPartsAttachmentPoint.IsPointDisabled = true;
                }
                else if (!ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.ContainsKey(modularWeaponPartsAttachmentPoint.ModularPartsGroupID) && modularWeaponPartsAttachmentPoint.UsesExternalParts)
                {
                    ModularWorkshopManager.Log(this, $"PartsAttachmentPoint Info: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" disabled due to using external parts and no external parts found.");
                    modularWeaponPartsAttachmentPoint.IsPointDisabled = true;
                }
                else if (!ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.ContainsKey(modularWeaponPartsAttachmentPoint.ModularPartsGroupID))
                {
                    ModularWorkshopManager.LogWarning(this, $"PartsAttachmentPoint Warning: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary! Disabling part point!");
                    modularWeaponPartsAttachmentPoint.IsPointDisabled = true;
                }
            }
        }

        public void OnDestroy()
        {
            _existingModularPhysicalObjects.Remove(MainObject);
        }

        // Receiver Skins
        public void ApplyReceiverSkin(string skinName)
        {
            if (ReceiverSkinsDefinition.SkinDictionary.TryGetValue(skinName, out SkinDefinition skinDefinition))
            {
                try
                {
                    for (int i = 0; i < ReceiverMeshRenderers.Length; i++)
                    {
                        ReceiverMeshRenderers[i].materials = skinDefinition.DifferentSkinnedMeshPieces[i].Materials;
                    }

                    CurrentSelectedReceiverSkinID = skinName;
                }
                catch (Exception)
                {
                    ModularWorkshopManager.LogError(this, $"Number of DifferentSkinnedMeshPieces in SkinDefinition \"{skinDefinition.ModularSkinID}\" does not match number of meshes on Receiver! ({ReceiverMeshRenderers.Length} vs {skinDefinition.DifferentSkinnedMeshPieces.Length})");
                }
            }
            else ModularWorkshopManager.LogError(this, $"Skin with name \"{skinName}\" not found in SkinsDefinition \"{ReceiverSkinsDefinition.name}\"!");
        }

        public void CheckForDefaultReceiverSkin(FVRPhysicalObject physicalObject)
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
                ModularWorkshopManager.LogWarning(this, $"No SkinsDefinition found for receiver skin path \"{SkinPath}\", but part receiver \"{physicalObject.gameObject.name}\" set to skin name \"{CurrentSelectedReceiverSkinID}\". Naming error?");
            }
        }

        public void GetReceiverMeshRenderers()
        {
            List<MeshRenderer> ignoredMeshRenderers = new();

            foreach (var part in GetComponentsInChildren<ModularWeaponPart>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            foreach (var part in GetComponentsInChildren<FVRFireArmAttachment>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            foreach (var part in GetComponentsInChildren<FVRFireArmChamber>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            foreach (var part in GetComponentsInChildren<FVRFirearmMovingProxyRound>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            if (MainObject is FVRFireArmMagazine mag)
            {
                ignoredMeshRenderers.AddRange(mag.DisplayRenderers.OfType<MeshRenderer>());
            }

            ReceiverMeshRenderers = GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled).Where(m => !ignoredMeshRenderers.Contains(m)).ToArray();
        }

        [ContextMenu("Populate Receiver Mesh Renderer List")]
        public void PopulateReceiverMeshList()
        {
            GetReceiverMeshRenderers();
        }

        public ModularWeaponPart ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, string selectedPart, bool isRandomized = false, Dictionary<string, string> oldSubParts = null, Dictionary<string, string> oldSkins = null)
        {
            if (ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out ModularWorkshopPartsDefinition partsDefinition))
            {
                if (!partsDefinition.PartsDictionary.ContainsKey(selectedPart))
                {
                    ModularWorkshopManager.LogError(this, $"PartsAttachmentPoint Error: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" does not contain part with name \"{selectedPart}\"");
                    return null;
                }
            }
            else if (selectedPart != string.Empty && modularWeaponPartsAttachmentPoint.UsesExternalParts)
            {
                ModularWorkshopManager.Log(this, $"PartsAttachmentPoint Info: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" disabled due to using external parts and no external parts found.");
                modularWeaponPartsAttachmentPoint.IsPointDisabled = true;
                return null;
            }
            else if (selectedPart != string.Empty)
            {
                ModularWorkshopManager.LogError(this, $"PartsAttachmentPoint Error: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary!");
                return null;
            }
            else if (selectedPart == string.Empty || modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart == string.Empty)
            {
                ModularWorkshopManager.LogWarning(this, $"PartsAttachmentPoint Warning: Parts group \"{modularWeaponPartsAttachmentPoint.ModularPartsGroupID}\" not found in ModularWorkshopManager dictionary, but current part name also empty. Treating as future attachment point!");
                modularWeaponPartsAttachmentPoint.IsPointDisabled = true;
                return null;
            }

            if (oldSubParts == null) oldSubParts = new();
            if (oldSkins == null) oldSkins = new();

            // Old Part Operations
            //ModularWeaponPart oldPart = modularWeaponPartsAttachmentPoint.ModularPartPoint.GetComponentInChildren<ModularWeaponPart>();

            //if (oldPart != null)
            //{
            //    oldPart.DisablePart();

            //    RemovePartPointOccupation(oldPart, oldSubParts, oldSkins);

            //    if (!oldSkins.ContainsKey(modularWeaponPartsAttachmentPoint.ModularPartsGroupID)) oldSkins.Add(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, modularWeaponPartsAttachmentPoint.CurrentSkin);

            //    foreach (var point in oldPart.SubAttachmentPoints)
            //    {
            //        oldSubParts.Add(point.ModularPartsGroupID, point.SelectedModularWeaponPart);
            //        oldSkins.Add(point.ModularPartsGroupID, point.CurrentSkin);

            //        RemoveSubAttachmentPoint(point, MainObject, oldSubParts, oldSkins);
            //    }
            //}

            ModularWeaponPart oldPart = OldPartOperations(modularWeaponPartsAttachmentPoint, oldSubParts, oldSkins);

            // New Part Operations
            //GameObject modularWeaponPartPrefab = UnityEngine.Object.Instantiate(partsDefinition.PartsDictionary[selectedPart], modularWeaponPartsAttachmentPoint.ModularPartPoint.position, modularWeaponPartsAttachmentPoint.ModularPartPoint.rotation, modularWeaponPartsAttachmentPoint.ModularPartPoint.parent);
            //ModularWeaponPart newPart = modularWeaponPartPrefab.GetComponent<ModularWeaponPart>();

            
            //newPart.AdjustScale(modularWeaponPartsAttachmentPoint);

            //modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart = selectedPart;
            //UpdateMainObject(oldPart, newPart);

            //UnityEngine.Object.Destroy(modularWeaponPartsAttachmentPoint.ModularPartPoint.gameObject);
            //modularWeaponPartsAttachmentPoint.ModularPartPoint = newPart.transform;

            //newPart.EnablePart();

            //ApplyPartPointOccupation(newPart, oldSubParts, oldSkins);

            ModularWeaponPart newPart = NewPartOperations(modularWeaponPartsAttachmentPoint, partsDefinition, selectedPart, oldSubParts, oldSkins);

            // Finalization
            TryApplyOldSkin(modularWeaponPartsAttachmentPoint, selectedPart, oldSkins, isRandomized);

            ConfigureNewSubParts(newPart, oldSubParts, oldSkins, isRandomized);
            // Invoke the PartAdded event
            PartAdded?.Invoke(modularWeaponPartsAttachmentPoint, newPart);
            // Finally reset the center of mass on the main object
            MainObject.ResetClampCOM();

            return newPart;
        }

        private ModularWeaponPart OldPartOperations(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, Dictionary<string, string> oldSubParts, Dictionary<string, string> oldSkins)
        {
            ModularWeaponPart oldPart = modularWeaponPartsAttachmentPoint.ModularPartPoint.GetComponentInChildren<ModularWeaponPart>();
            if (oldPart != null)
            {
                oldPart.DisablePart();

                RemovePartPointOccupation(oldPart, oldSubParts, oldSkins);

                if (!oldSkins.ContainsKey(modularWeaponPartsAttachmentPoint.ModularPartsGroupID)) oldSkins.Add(modularWeaponPartsAttachmentPoint.ModularPartsGroupID, modularWeaponPartsAttachmentPoint.CurrentSkin);

                foreach (var point in oldPart.SubAttachmentPoints)
                {
                    oldSubParts.Add(point.ModularPartsGroupID, point.SelectedModularWeaponPart);
                    oldSkins.Add(point.ModularPartsGroupID, point.CurrentSkin);

                    RemoveSubAttachmentPoint(point, MainObject, oldSubParts, oldSkins);
                }

                // PhysicalObject Operations
                foreach (var mount in oldPart.AttachmentMounts)
                {
                    DetachAllAttachmentsFromMount(mount);

                    MainObject.AttachmentMounts.Remove(mount);
                }
                MainObject.Slots = MainObject.Slots.Where(s => !oldPart.SubQuickBeltSlots.Contains(s)).ToArray();

                // Addon Operations
                IPartFireArmRequirement[] partFireArmRequirements = oldPart.GetComponents<IPartFireArmRequirement>();
                foreach (var item in partFireArmRequirements)
                {
                    item.FireArm = null;
                }
                IPartPhysicalObjectRequirement[] partPhysicalObjectRequirement = oldPart.GetComponents<IPartPhysicalObjectRequirement>();
                foreach (var item in partPhysicalObjectRequirement)
                {
                    item.PhysicalObject = null;
                }
            }

            return oldPart;
        }

        private ModularWeaponPart NewPartOperations(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, ModularWorkshopPartsDefinition partsDefinition, string selectedPart, Dictionary<string, string> oldSubParts, Dictionary<string, string> oldSkins)
        {
            GameObject modularWeaponPartPrefab = UnityEngine.Object.Instantiate(partsDefinition.PartsDictionary[selectedPart], modularWeaponPartsAttachmentPoint.ModularPartPoint.position, modularWeaponPartsAttachmentPoint.ModularPartPoint.rotation, modularWeaponPartsAttachmentPoint.ModularPartPoint.parent);
            ModularWeaponPart newPart = modularWeaponPartPrefab.GetComponent<ModularWeaponPart>();

            newPart.AdjustScale(modularWeaponPartsAttachmentPoint);

            modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart = selectedPart;

            UnityEngine.Object.Destroy(modularWeaponPartsAttachmentPoint.ModularPartPoint.gameObject);
            modularWeaponPartsAttachmentPoint.ModularPartPoint = newPart.transform;

            newPart.EnablePart();

            ApplyPartPointOccupation(newPart, oldSubParts, oldSkins);

            MainObject.AttachmentMounts.AddRange(newPart.AttachmentMounts);
            foreach (var mount in newPart.AttachmentMounts)
            {
                mount.Parent = MainObject;
                mount.MyObject = MainObject;
            }

            // PhysicalObject Operations
            MainObject.Slots.AddRangeToArray(newPart.SubQuickBeltSlots);
            MainObject.m_colliders = MainObject.GetComponentsInChildren<Collider>(true);

            if (MainObject.m_quickbeltSlot != null) MainObject.SetAllCollidersToLayer(false, "NoCol");

            // Fix handgun slide colliders
            switch (MainObject)
            {
                case Handgun w:
                    w.m_slideCols.Clear();
                    w.InitSlideCols();
                    break;
                default:
                    break;
            }

            // Addon Operations
            IPartFireArmRequirement[] partFireArmRequirements = newPart.GetComponents<IPartFireArmRequirement>();
            foreach (var item in partFireArmRequirements)
            {
                item.FireArm = MainObject as FVRFireArm;
            }
            IPartPhysicalObjectRequirement[] partPhysicalObjectRequirement = newPart.GetComponents<IPartPhysicalObjectRequirement>();
            foreach (var item in partPhysicalObjectRequirement)
            {
                item.PhysicalObject = MainObject;
            }

            return newPart;
        }

        private void UpdateMainObject(ModularWeaponPart oldPart, ModularWeaponPart newPart)
        {
            IPartFireArmRequirement[] partFireArmRequirements;
            IPartPhysicalObjectRequirement[] partPhysicalObjectRequirement;
            if (oldPart != null)
            {
                foreach (var mount in oldPart.AttachmentMounts)
                {
                    DetachAllAttachmentsFromMount(mount);

                    MainObject.AttachmentMounts.Remove(mount);
                }
                MainObject.Slots = MainObject.Slots.Where(s => !oldPart.SubQuickBeltSlots.Contains(s)).ToArray();

                partFireArmRequirements = oldPart.GetComponents<IPartFireArmRequirement>();
                foreach (var item in partFireArmRequirements)
                {
                    item.FireArm = null;
                }
                partPhysicalObjectRequirement = oldPart.GetComponents<IPartPhysicalObjectRequirement>();
                foreach (var item in partPhysicalObjectRequirement)
                {
                    item.PhysicalObject = null;
                }
            }

            MainObject.AttachmentMounts.AddRange(newPart.AttachmentMounts);
            foreach (var mount in newPart.AttachmentMounts)
            {
                mount.Parent = MainObject;
                mount.MyObject = MainObject;
            }

            MainObject.Slots.AddRangeToArray(newPart.SubQuickBeltSlots);

            MainObject.m_colliders = MainObject.GetComponentsInChildren<Collider>(true);

            if (MainObject.m_quickbeltSlot != null) MainObject.SetAllCollidersToLayer(false, "NoCol");

            switch (MainObject)
            {
                case Handgun w:
                    w.m_slideCols.Clear();
                    w.InitSlideCols();
                    break;
                default:
                    break;
            }

            partFireArmRequirements = newPart.GetComponents<IPartFireArmRequirement>();
            foreach (var item in partFireArmRequirements)
            {
                item.FireArm = MainObject as FVRFireArm;
            }
            partPhysicalObjectRequirement = newPart.GetComponents<IPartPhysicalObjectRequirement>();
            foreach (var item in partPhysicalObjectRequirement)
            {
                item.PhysicalObject = MainObject;
            }
        }

        public void AddSubAttachmentPoint(ModularWeaponPartsAttachmentPoint subPoint, FVRPhysicalObject fireArm, Dictionary<string, string> oldSubParts, Dictionary<string, string> oldSkins, string selectedPart)
        {
            SubAttachmentPoints.Add(subPoint);
            ConfigureModularWeaponPart(subPoint, selectedPart, fireArm, oldSubParts, oldSkins);

            WorkshopPlatform?.CreateUIForPoint(subPoint);
        }

        public void RemoveSubAttachmentPoint(ModularWeaponPartsAttachmentPoint subPoint, FVRPhysicalObject fireArm, Dictionary<string, string> oldSubParts, Dictionary<string, string> oldSkins)
        {
            SubAttachmentPoints.Remove(subPoint);

            ModularWeaponPart part = subPoint.ModularPartPoint.GetComponent<ModularWeaponPart>();
            if (part != null)
            {
                IPartFireArmRequirement[] partFireArmRequirement = part.GetComponents<IPartFireArmRequirement>();
                foreach (var addon in partFireArmRequirement)
                {
                    addon.FireArm = null;
                }
                IPartPhysicalObjectRequirement[] partPhysicalObjectRequirement = part.GetComponents<IPartPhysicalObjectRequirement>();
                foreach (var addon in partPhysicalObjectRequirement)
                {
                    addon.PhysicalObject = null;
                }

                part.DisablePart();

                foreach (var mount in part.AttachmentMounts)
                {
                    DetachAllAttachmentsFromMount(mount);

                    fireArm.AttachmentMounts.Remove(mount);
                }

                foreach (var subSubPoint in part.SubAttachmentPoints)
                {
                    oldSubParts.Add(subSubPoint.ModularPartsGroupID, subSubPoint.SelectedModularWeaponPart);
                    oldSkins.AddOrReplace(subPoint.ModularPartsGroupID, subPoint.CurrentSkin);

                    RemoveSubAttachmentPoint(subSubPoint, fireArm, oldSubParts, oldSkins);
                }
            }
            WorkshopPlatform?.RemoveUIFromPoint(subPoint);

            UnityEngine.Object.Destroy(subPoint.ModularPartPoint.gameObject);
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

        private void RemovePartPointOccupation(ModularWeaponPart part, Dictionary<string, string> oldSubParts, Dictionary<string, string> oldSkins)
        {
            foreach (var alsoOccupiesPointWithModularPartsGroupID in part.AlsoOccupiesPointWithModularPartsGroupIDs)
            {
                if (AllAttachmentPoints.TryGetValue(alsoOccupiesPointWithModularPartsGroupID, out ModularWeaponPartsAttachmentPoint disabledPoint))
                {
                    disabledPoint.IsPointDisabled = false;
                    ConfigureModularWeaponPart(disabledPoint, disabledPoint.SelectedModularWeaponPart, false, oldSubParts, oldSkins);

                    if (!SubAttachmentPoints.Contains(disabledPoint)) WorkshopPlatform?.CreateUIForPoint(disabledPoint, ModularWorkshopUI.EPartType.MainWeaponGeneralAttachmentPoint);
                    else WorkshopPlatform?.CreateUIForPoint(disabledPoint);
                }
            }
        }

        private void ApplyPartPointOccupation(ModularWeaponPart part, Dictionary<string, string> oldSubParts, Dictionary<string, string> oldSkins)
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

                            MainObject.AttachmentMounts.Remove(mount);
                        }
                        MainObject.Slots = MainObject.Slots.Where(s => !partToRemove.SubQuickBeltSlots.Contains(s)).ToArray();

                        IPartFireArmRequirement[] partFireArmRequirements = partToRemove.GetComponents<IPartFireArmRequirement>();
                        foreach (var addon in partFireArmRequirements)
                        {
                            addon.FireArm = null;
                        }
                        IPartPhysicalObjectRequirement[] partPhysicalObjectRequirement = partToRemove.GetComponents<IPartPhysicalObjectRequirement>();
                        foreach (var addon in partPhysicalObjectRequirement)
                        {
                            addon.PhysicalObject = null;
                        }

                        ModularWeaponPartsAttachmentPoint[] subPoints = partToRemove.SubAttachmentPoints;
                        foreach (var subPoint in subPoints)
                        {
                            oldSubParts.AddOrReplace(subPoint.ModularPartsGroupID, subPoint.SelectedModularWeaponPart);
                            oldSkins.AddOrReplace(subPoint.ModularPartsGroupID, subPoint.CurrentSkin);

                            RemoveSubAttachmentPoint(subPoint, MainObject, oldSubParts, oldSkins);
                        }
                    }

                    Destroy(pointToDisable.ModularPartPoint.gameObject);
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

        public void ApplySkin(string ModularPartsGroupID, string SkinName)
        {
            AllAttachmentPoints[ModularPartsGroupID].ApplySkin(SkinName);
        }

        private void ConfigureNewSubParts(ModularWeaponPart newPart, Dictionary<string, string> oldSubParts, Dictionary<string, string> oldSkins, bool isRandomized)
        {
            foreach (var subPoint in newPart.SubAttachmentPoints)
            {
                if (!isRandomized && oldSubParts.TryGetValue(subPoint.ModularPartsGroupID, out string oldSelectedPart) && oldSelectedPart != subPoint.SelectedModularWeaponPart) AddSubAttachmentPoint(subPoint, MainObject, oldSubParts, oldSkins, oldSelectedPart);
                else
                {
                    string selectedSubPart = isRandomized ? ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[subPoint.ModularPartsGroupID].GetRandomPart() : subPoint.SelectedModularWeaponPart;

                    AddSubAttachmentPoint(subPoint, MainObject, oldSubParts, oldSkins, selectedSubPart);
                }
            }
        }

        public void ConvertTransformsToProxies()
        {
            foreach (var point in ModularWeaponPartsAttachmentPoints)
            {
                point.ModularPartUIPointProxy = new(point.ModularPartUIPoint, MainObject.transform, true);
            }

            if (ReceiverSkinUIPoint != null) ReceiverSkinUIPointProxy = new(ReceiverSkinUIPoint, true);
            else
            {
                GameObject uiPoint = new("UIPoint");
                uiPoint.transform.SetParent(MainObject.transform);
                uiPoint.transform.position = MainObject.transform.position + -MainObject.transform.right * 0.1f;
                uiPoint.transform.rotation = MainObject.transform.rotation * Quaternion.Euler(new Vector3(0f, 90f, 0f));

                ReceiverSkinUIPointProxy = new(uiPoint.transform, true);
            }
        }

        // Harmony Patches for vaulting

        [HarmonyPatch(typeof(FVRPhysicalObject), nameof(FVRPhysicalObject.GetFlagDic))]
        [HarmonyPostfix]
        public static void GetFlagDicPatch(FVRPhysicalObject __instance, ref Dictionary<string, string> __result)
        {
            if (_existingModularPhysicalObjects.TryGetValue(__instance, out ModularFVRPhysicalObject modularPhysicalObject))
            {
                __result = modularPhysicalObject.GetFlagDic(__result);
            }
        }

        [HarmonyPatch(typeof(FVRPhysicalObject), nameof(FVRPhysicalObject.ConfigureFromFlagDic))]
        [HarmonyPostfix]
        public static void ConfigureFromFlagDicPatch(FVRPhysicalObject __instance, Dictionary<string, string> f)
        {
            if (_existingModularPhysicalObjects.TryGetValue(__instance, out ModularFVRPhysicalObject modularPhysicalObject))
            {
                modularPhysicalObject.ConfigureFromFlagDic(f);
            }
        }

        //// TEMPORARY WHILE ANTON FIXES HIS SHITTTT
        //[HarmonyPatch(typeof(FVRFireArmAttachment), nameof(FVRFireArmAttachment.ConfigureFromFlagDic))]
        //[HarmonyPostfix]
        //public static void ConfigureFromFlagDicPatch_Attachment(FVRPhysicalObject __instance, Dictionary<string, string> f)
        //{
        //    if (_existingModularPhysicalObjects.TryGetValue(__instance, out ModularFVRPhysicalObject modularPhysicalObject))
        //    {
        //        modularPhysicalObject.ConfigureFromFlagDic(f);
        //    }
        //}

        [HarmonyPatch(typeof(FVRPhysicalObject), nameof(FVRPhysicalObject.DuplicateFromSpawnLock))]
        [HarmonyPostfix]
        public static void DuplicateFromSpawnLockPatch(FVRPhysicalObject __instance, ref GameObject __result)
        {
            if (_existingModularPhysicalObjects.TryGetValue(__instance, out ModularFVRPhysicalObject modularPhysicalObject))
            {
                __result = modularPhysicalObject.DuplicateFromSpawnLock(__result);
            }
        }

        public void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            string selectedPart;
            string selectedSkin;

            WasUnvaulted = true;

            foreach (var modularWeaponPartsAttachmentPoint in ModularWeaponPartsAttachmentPoints)
            {
                if (!modularWeaponPartsAttachmentPoint.IsPointDisabled && f.TryGetValue("Modul" + modularWeaponPartsAttachmentPoint.ModularPartsGroupID, out selectedPart)) ConfigureModularWeaponPart(modularWeaponPartsAttachmentPoint, selectedPart);
            }
            for (int i = 0; i < SubAttachmentPoints.Count; i++)
            {
                if (!SubAttachmentPoints[i].IsPointDisabled && f.TryGetValue("Modul" + SubAttachmentPoints.ElementAt(i).ModularPartsGroupID, out selectedPart)) ConfigureModularWeaponPart(SubAttachmentPoints.ElementAt(i), selectedPart);
            }

            if (SkinPath != null && f.TryGetValue(SkinPath, out selectedSkin))
            {
                ApplyReceiverSkin(selectedSkin);
            }

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

            MainObject.AttachmentMounts.RemoveAll(mounts.Contains);
            MainObject.AttachmentMounts.AddRange(mounts);
        }

        public Dictionary<string, string> GetFlagDic(Dictionary<string, string> flagDic)
        {
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
            points.Sort((x, y) => string.Compare(x.ModularPartsGroupID, y.ModularPartsGroupID));
            List<FVRFireArmAttachmentMount> mounts = new();

            foreach (var anyPoint in points)
            {
                string key = anyPoint.ModularPartsGroupID + "/" + anyPoint.SelectedModularWeaponPart;
                string value = anyPoint.CurrentSkin;
                flagDic.Add(key, value);

                ModularWeaponPart part = anyPoint.ModularPartPoint.GetComponent<ModularWeaponPart>();
                if (part != null) mounts.AddRange(part.AttachmentMounts);
            }

            MainObject.AttachmentMounts.RemoveAll(mounts.Contains);
            MainObject.AttachmentMounts.AddRange(mounts);

            return flagDic;
        }

        public GameObject DuplicateFromSpawnLock(GameObject copy)
        {
            ModularFVRPhysicalObject modularPhysicalObject = copy.GetComponentInChildren<ModularFVRPhysicalObject>();
            modularPhysicalObject.WasUnvaulted = true;

            foreach (var partsPoint in AllAttachmentPoints)
            {
                string partsGroupID = partsPoint.Key;
                ModularWeaponPartsAttachmentPoint point = modularPhysicalObject.AllAttachmentPoints[partsGroupID];
                string selectedPart = partsPoint.Value.SelectedModularWeaponPart;
                string selectedSkin = partsPoint.Value.CurrentSkin;

                if (!point.IsPointDisabled)
                {
                    modularPhysicalObject.ConfigureModularWeaponPart(point, selectedPart);
                    modularPhysicalObject.ApplySkin(partsGroupID, selectedSkin);
                }
            }

            modularPhysicalObject.ApplyReceiverSkin(CurrentSelectedReceiverSkinID);

            //if (MainObject is FVRFireArmMagazine mag)
            //{
            //    FVRFireArmMagazine copyMag = copy.GetComponent<FVRFireArmMagazine>();
            //    for (int i = 0; i < Mathf.Min(mag.LoadedRounds.Length, copyMag.LoadedRounds.Length); i++)
            //    {
            //        if (copyMag.LoadedRounds[i] != null) copyMag.LoadedRounds[i] = new();
            //    }
            //}
            return copy;
        }
    }
}