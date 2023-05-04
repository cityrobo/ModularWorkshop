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
        public string ModularPartsGroupID;
        [Tooltip("Optional Display name. If left empty, the ModularPartsGroupID will be used as a display name instead.")]
        public string PartName;
        public List<SkinDefinition> SkinDefinitions;

        [Serializable]
        public class SkinDefinition
        {
            public string ModularSkinID;
            [Tooltip("Optional Display name. If left empty, the ModularSkinID will be used as a display name instead.")]
            public string DisplayName;
            public Sprite Icon;
            [Tooltip("These are in order of the mesh renderes how they get found by the \"Display Mesh Order\" context menu option on the Modular Weapon Part.")]
            public MeshSkin[] DifferentSkinnedMeshPieces;
        }

        [Serializable]
        public class MeshSkin
        {
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
                        Debug.LogWarning($"Skin with name {SkinName} already in Skin dictionary! Skipping skin!");
                    }
                }
                return keyValuePairs;
            }
        }

        public SkinDefinition GetRandomSkin()
        {
            int randomIndex = UnityEngine.Random.Range(0, SkinDefinitions.Count);

            return SkinDefinitions[randomIndex];
        }
    }
}