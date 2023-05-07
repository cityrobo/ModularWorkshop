﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using FistVR;
using Steamworks;
using OpenScripts2;
using System.IO;
using static ModularWorkshop.ModularWorkshopSkinsDefinition;

namespace ModularWorkshop
{
    [Serializable]
    public class ModularWeaponPartsAttachmentPoint
    {
        public string ModularPartsGroupID;
        public Transform ModularPartPoint;
        public Transform ModularPartUIPoint;
        [HideInInspector]
        public TransformProxy ModularPartUIPointProxy;
        public string SelectedModularWeaponPart;

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

                ModularWorkshopManager.ModularWorkshopSkinsDictionary.Add(SkinPath, skinsDefinition);
            }
            else if (part != null && part.SelectedModularWeaponPartSkinID != "Default" && !ModularWorkshopManager.ModularWorkshopSkinsDictionary.ContainsKey(SkinPath))
            {
                Debug.LogWarning($"No SkinsDefinition found for skin path {SkinPath}, but part {part.Name} set to skin name {part.SelectedModularWeaponPartSkinID}. Naming error?");
            }
        }

        public void ApplySkin(string skinName)
        {
            ModularWeaponPart part = ModularPartPoint.GetComponent<ModularWeaponPart>();
            if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(SkinPath, out ModularWorkshopSkinsDefinition definition)) {
                if (definition.SkinDictionary.TryGetValue(skinName, out SkinDefinition skinDefinition)) part.ApplySkin(skinDefinition);
                else Debug.LogError($"Skin with name {skinName} not found in SkinsDefinition {definition.name}!");
            }
            else Debug.LogError($"No SkinsDefinition found for {SkinPath}!");
        }
    }
}