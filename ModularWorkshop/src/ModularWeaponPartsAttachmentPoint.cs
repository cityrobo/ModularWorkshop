using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using static ModularWorkshop.ModularWorkshopSkinsDefinition;

namespace ModularWorkshop
{
    [Serializable]
    public class ModularWeaponPartsAttachmentPoint
    {
        public string ModularPartsGroupID;
        public Transform ModularPartPoint;
        [HideInInspector]
        public Vector3 PartPointStartScale = Vector3.zero;
        public Transform ModularPartUIPoint;
        [HideInInspector]
        public TransformProxy ModularPartUIPointProxy;
        public string SelectedModularWeaponPart;

        [Header("Take & Hold")]
        public bool DisallowTakeAndHoldRandomization = false;
        //[Header("Parts Blacklist/Whitelist")]
        //public bool BlacklistActsLikeWhitelistInstead = false;
        //public List<string> PartsBlacklist = new();

        [Tooltip("Check this box if this point is intended to use parts from a different mod only, aka you don't include any parts for this point yourself in your package.")]
        public bool UsesExternalParts = false;

        [HideInInspector]
        public bool IsPointDisabled = false;

        public string SkinPath => ModularPartsGroupID + "/" + SelectedModularWeaponPart;

        public string CurrentSkin 
        {
            get 
            {
                ModularWeaponPart part = ModularPartPoint.GetComponentInChildren<ModularWeaponPart>();
                if (part != null) return part.SelectedModularWeaponPartSkinID;
                else return string.Empty;
            }
        }

        public readonly Dictionary<string, string> PreviousSkins = new();

        public void CheckForDefaultSkin()
        {
            ModularWeaponPart part = ModularPartPoint.GetComponentInChildren<ModularWeaponPart>();

            if (part != null && part.SelectedModularWeaponPartSkinID == "Default" && ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(SkinPath, out ModularWorkshopSkinsDefinition skinsDefinition))
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
                    MeshSkin[] skinPieces = new MeshSkin[part.MeshRenderers.Length];

                    for (int i = 0; i < part.MeshRenderers.Length; i++)
                    {
                        MeshSkin skinPiece = new()
                        {
                            Materials = part.MeshRenderers[i].sharedMaterials
                        };

                        skinPieces[i] = skinPiece;
                    }
                    skinDefinition.DifferentSkinnedMeshPieces = skinPieces;

                    // Add new SkinDefinition to the found SkinsDefinition in the ModularWorkshopManager
                    skinsDefinition.SkinDefinitions.Insert(0, skinDefinition);
                }
            }
            else if (part != null && part.SelectedModularWeaponPartSkinID == "Default")
            {
                // Create SkinDefinition for Default Skin
                SkinDefinition skinDefinition = new()
                {
                    ModularSkinID = "Default",
                    DisplayName = "Default",
                    Icon = MiscUtilities.CreateEmptySprite()
                };

                // Create Array with the length of the MeshRenders on the part and fill it with their materials
                MeshSkin[] skinPieces = new MeshSkin[part.MeshRenderers.Length];

                for (int i = 0; i < part.MeshRenderers.Length; i++)
                {
                    MeshSkin skinPiece = new()
                    {
                        Materials = part.MeshRenderers[i].sharedMaterials
                    };

                    skinPieces[i] = skinPiece;
                }
                skinDefinition.DifferentSkinnedMeshPieces = skinPieces;

                // Create new SkinsDefinition and add it to the ModularWorkshopManager
                skinsDefinition = ScriptableObject.CreateInstance<ModularWorkshopSkinsDefinition>();

                skinsDefinition.name = ModularPartsGroupID + "/" + SelectedModularWeaponPart;
                skinsDefinition.ModularPartsGroupID = ModularPartsGroupID;
                skinsDefinition.PartName = SelectedModularWeaponPart;
                skinsDefinition.SkinDefinitions = new() { skinDefinition };
                skinsDefinition.AutomaticallyCreated = true;

                ModularWorkshopManager.ModularWorkshopSkinsDictionary.Add(SkinPath, skinsDefinition);
            }
            else if (part != null && part.SelectedModularWeaponPartSkinID != "Default" && !ModularWorkshopManager.ModularWorkshopSkinsDictionary.ContainsKey(SkinPath))
            {
                ModularWorkshopManager.LogWarning($"ModularWeaponPartsAttachmentPoint.CheckForDefaultSkin(): No SkinsDefinition found for skin path \"{SkinPath}\", but part \"{part.Name}\" set to skin name \"{part.SelectedModularWeaponPartSkinID}\". Naming error?");
            }
        }

        public void ApplySkin(string skinName)
        {
            if (ModularPartPoint != null)
            {
                ModularWeaponPart part = ModularPartPoint.GetComponent<ModularWeaponPart>();
                if (part != null)
                {
                    if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(SkinPath, out ModularWorkshopSkinsDefinition definition))
                    {
                        if (definition.SkinDictionary.TryGetValue(skinName, out SkinDefinition skinDefinition))
                        {
                            part.ApplySkin(skinDefinition);

                            if (PreviousSkins.ContainsKey(part.Name)) PreviousSkins[part.Name] = skinName;
                            else PreviousSkins.Add(part.Name, skinName);
                        }
                        else ModularWorkshopManager.LogError($"ModularWeaponPartsAttachmentPoint.ApplySkin({skinName}): Skin with name \"{skinName}\" not found in SkinsDefinition \"{definition.name}\"!");
                    }
                    else ModularWorkshopManager.LogError($"ModularWeaponPartsAttachmentPoint.ApplySkin({skinName}): No SkinsDefinition found for \"{SkinPath}\"!");
                }
                else ModularWorkshopManager.LogWarning($"ModularWeaponPartsAttachmentPoint.ApplySkin({skinName}): Parts point for ID \"{ModularPartsGroupID}\" does not have a part on its point to apply a skin to!");
            }
            else ModularWorkshopManager.LogWarning($"ModularWeaponPartsAttachmentPoint.ApplySkin({skinName}): Parts point for ID \"{ModularPartsGroupID}\" does not have a point assigned!");
        }
    }
}