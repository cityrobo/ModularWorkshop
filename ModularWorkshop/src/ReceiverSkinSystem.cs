using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;
using HarmonyLib;
using static ModularWorkshop.ModularWorkshopSkinsDefinition;

namespace ModularWorkshop
{
    public class ReceiverSkinSystem : MonoBehaviour
    {
        public FVRPhysicalObject MainObject;
        public GameObject UIPrefab;

        [Header("Receiver Skins")]
        [Tooltip("This is a combination of ModularPartsGroupID and PartName of a Skins definition, with a \"/\" in between. A requirement of the system. You should choose ModularPartsGroupID and PartName so that it doesn't conflict with anything else. Formatting Example: \"ModularPartsGroupID/PartName\". I would personally recommend something like \"ReceiverName/Receiver\" as a standard.")]
        public string SkinPath;
        public Transform ReceiverSkinUIPoint;
        [HideInInspector]
        public TransformProxy ReceiverSkinUIPointProxy;
        public string CurrentSelectedReceiverSkinID = "Default";

        [Tooltip("Can be populated with the context menu on the gun.")]
        public MeshRenderer[] ReceiverMeshRenderers;

        private static readonly Dictionary<FVRPhysicalObject, ReceiverSkinSystem> _existingReceiverSkinSystems = new();

        public ModularWorkshopSkinsDefinition ReceiverSkinsDefinition 
        { 
            get 
            {
                if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(SkinPath, out ModularWorkshopSkinsDefinition skinsDefinition)) return skinsDefinition;
                else
                {
                    Debug.LogError($"No SkinsDefinition found for {SkinPath}!");
                    return null;
                }
            }
        }
        
        public void Awake()
        {
            if (MainObject != null) _existingReceiverSkinSystems.Add(MainObject, this);
            else _existingReceiverSkinSystems.Add(GetComponent<FVRPhysicalObject>(), this);

            if (ReceiverMeshRenderers == null || ReceiverMeshRenderers.Length == 0) GetReceiverMeshRenderers();
        }

        public void Start()
        {
            if (MainObject.ObjectWrapper == null)
            {
                Destroy(this);
                return;
            }
            if (ReceiverSkinUIPoint != null) ReceiverSkinUIPointProxy = new(ReceiverSkinUIPoint, true);
            else
            {
                GameObject uiPoint = new("temp");
                uiPoint.transform.SetParent(MainObject.transform);
                uiPoint.transform.position = MainObject.transform.position + -MainObject.transform.right * 0.1f;
                uiPoint.transform.rotation = MainObject.transform.rotation * Quaternion.Euler(new Vector3(0f, 90f, 0f));

                ReceiverSkinUIPointProxy = new(uiPoint.transform, true);
            }

            if (SkinPath == null || SkinPath == string.Empty) SkinPath = MainObject.ObjectWrapper.ItemID + "/" + "Receiver";
            CheckForDefaultReceiverSkin(MainObject);

            ApplyReceiverSkin(CurrentSelectedReceiverSkinID);
        }

        public void OnDestroy()
        {
            _existingReceiverSkinSystems.Remove(MainObject);
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

        public void CheckForDefaultReceiverSkin(FVRPhysicalObject fireArm)
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
                Debug.LogWarning($"No SkinsDefinition found for receiver skin path {SkinPath}, but part receiver {fireArm.gameObject.name} set to skin name {CurrentSelectedReceiverSkinID}. Naming error?");
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

            ReceiverMeshRenderers = GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled).Where(m => !ignoredMeshRenderers.Contains(m)).ToArray();
        }

        [ContextMenu("Populate Receiver Mesh Renderer List")]
        public void PopulateReceiverMeshList()
        {
            GetReceiverMeshRenderers();
        }

        // Harmony Patches for vaulting

        [HarmonyPatch(typeof(FVRPhysicalObject), nameof(FVRPhysicalObject.GetFlagDic))]
        [HarmonyPostfix]
        public static void GetFlagDicPatch(FVRPhysicalObject __instance, ref Dictionary<string,string> __result)
        {
            if(_existingReceiverSkinSystems.TryGetValue(__instance,out ReceiverSkinSystem skinSystem))
            {
                __result = skinSystem.GetFlagDic(__result);
            }
        }

        [HarmonyPatch(typeof(FVRPhysicalObject), nameof(FVRPhysicalObject.ConfigureFromFlagDic))]
        [HarmonyPostfix]
        public static void ConfigureFromFlagDicPatch(FVRPhysicalObject __instance, Dictionary<string, string> f)
        {
            if (_existingReceiverSkinSystems.TryGetValue(__instance, out ReceiverSkinSystem skinSystem))
            {
                skinSystem.ConfigureFromFlagDic(f);
            }
        }

        public void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            if (SkinPath == null && MainObject != null && MainObject.ObjectWrapper != null) SkinPath = MainObject.ObjectWrapper.ItemID + "/" + "Receiver";

            if (SkinPath != null && f.TryGetValue(SkinPath, out string selectedSkin)) ApplyReceiverSkin(selectedSkin);
        }

        public Dictionary<string, string> GetFlagDic(Dictionary<string, string> flagDic)
        {
            flagDic.Add(SkinPath, CurrentSelectedReceiverSkinID);

            return flagDic;
        }
    }
}