using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ModularWorkshop
{
    [CreateAssetMenu(fileName = "New ModularWorkshopSkinsDefinition", menuName = "ModularWorkshop/ModularWorkshopSkinsDefinition", order = 0)]
    public class ModularWorkshopSkinsDefinition : ScriptableObject
    {
        [Tooltip("ModularPartsGroupID the skinned part is under.")]
        public string ModularPartsGroupID;
        [Tooltip("Name of the Part the SkinsDefinition is meant for.")]
        public string PartName;
        [Tooltip("List of the different skins of the part.")]
        public List<SkinDefinition> SkinDefinitions;
        [HideInInspector]
        public bool AutomaticallyCreated = false;

        [Serializable]
        public class SkinDefinition
        {
            [Tooltip("Internal ID of the skin. If used across multiple parts in the same ModularPartsGroupID allows for the next selected part to take on the same skin as the one before.")]
            public string ModularSkinID;
            [Tooltip("Optional display name. If left empty, the ModularSkinID will be used as a display name instead.")]
            public string DisplayName;
            [Tooltip("Icon of the skin that can be shown next to the skin button on the UI panel.")]
            public Sprite Icon;
            [Tooltip("These are in order of the mesh renderes how they get found by the \"Display Mesh Order\" or \"Fill Mesh Renderers\" context menu option on the Modular Weapon Part, or the \"Populate Receiver Mesh Renderer List\" option on the firearm if you wanna skin a receiver. You need one for each mesh on the part!")]
            public MeshSkin[] DifferentSkinnedMeshPieces;
        }

        [Serializable]
        public class MeshSkin
        {
            [Tooltip("All of the materials used on the one Mesh Renderer piece.")]
            public Material[] Materials;
        }

        public Dictionary<string, SkinDefinition> SkinDictionary
        {
            get
            {
                Dictionary<string, SkinDefinition> keyValuePairs = new();
                foreach (var skinDefinition in SkinDefinitions)
                {
                    string SkinName = skinDefinition.ModularSkinID;
                    try
                    {
                        keyValuePairs.Add(SkinName, skinDefinition);
                    }
                    catch (ArgumentException)
                    {
                        Debug.LogWarning($"Skin with name {SkinName} already in skin dictionary \"{name}\" with SkinPath {ModularPartsGroupID}/{PartName}! Skipping duplicate skin!");
                    }
                }
                return keyValuePairs;
            }
        }

        public string GetRandomSkin()
        {
            int randomIndex = UnityEngine.Random.Range(0, SkinDefinitions.Count);

            return SkinDefinitions[randomIndex].ModularSkinID;
        }
    }
}