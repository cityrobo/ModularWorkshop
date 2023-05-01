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

        public string SkinPath => Path.Combine(ModularPartsGroupID, SelectedModularWeaponPart);

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

                    // Add new SkinDefinition to the found SkinsDefinition in the ModularWorkshopManager
                    skinsDefinition.SkinDefinitions.Add(skinDefinition);
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

                // Create new SkinsDefinition and add it to the ModularWorkshopManager
                skinsDefinition = new()
                {
                    ModularPartsGroupID = this.ModularPartsGroupID,
                    PartName = SelectedModularWeaponPart,
                    SkinDefinitions = new() { skinDefinition }
                };

                ModularWorkshopManager.ModularWorkshopSkinsDictionary.Add(SkinPath, skinsDefinition);
            }
        }

        public void ConfigureSkin(SkinDefinition skinDefinition)
        {
            ModularWeaponPart part = ModularPartPoint.GetComponent<ModularWeaponPart>();

            part.ApplySkin(skinDefinition);
        }
    }
}