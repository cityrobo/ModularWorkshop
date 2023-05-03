using System;
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

        public string CurrentSkin => ModularPartPoint.GetComponent<ModularWeaponPart>().SelectedModularWeaponPartSkinID;

        public void CheckForDefaultSkin()
        {
            ModularWeaponPart part = ModularPartPoint.GetComponent<ModularWeaponPart>();

            if (part.SelectedModularWeaponPartSkinID == "Default" && ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(SkinPath, out ModularWorkshopSkinsDefinition skinsDefinition))
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
            else if (part.SelectedModularWeaponPartSkinID == "Default")
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
                skinsDefinition = new()
                {
                    name = this.ModularPartsGroupID + "/" + SelectedModularWeaponPart,
                    ModularPartsGroupID = this.ModularPartsGroupID,
                    PartName = SelectedModularWeaponPart,
                    SkinDefinitions = new() { skinDefinition }
                };

                ModularWorkshopManager.ModularWorkshopSkinsDictionary.Add(SkinPath, skinsDefinition);
            }
        }

        public void ApplySkin(string skinName)
        {
            ModularWeaponPart part = ModularPartPoint.GetComponent<ModularWeaponPart>();
            if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(SkinPath, out ModularWorkshopSkinsDefinition definition)) {
                if (definition.SkinDictionary.TryGetValue(skinName, out SkinDefinition skinDefinition)) part.ApplySkin(skinDefinition);
                else Debug.LogError($"Skin with name {skinName} not found in SkinDefinition {definition.name}!");
            }
            else Debug.LogError($"No SkinsDefinion found for {SkinPath}!");
        }
    }
}