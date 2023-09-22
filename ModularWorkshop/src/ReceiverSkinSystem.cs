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
    /// <summary>
    /// This component allows a FVRPhysicalObject to use the ModularWorkshop skin system.
    /// </summary>
    public class ReceiverSkinSystem : MonoBehaviour
    {
        // Reference to the main physical object.
        public FVRPhysicalObject MainObject;

        // Prefab for the user interface (UI).
        public GameObject UIPrefab;

        [Header("Receiver Skins")]
        [Tooltip("This is a combination of ModularPartsGroupID and PartName of a Skins definition, with a \"/\" in between. A requirement of the system. You should choose ModularPartsGroupID and PartName so that it doesn't conflict with anything else. Formatting Example: \"ModularPartsGroupID/PartName\". I would personally recommend something like \"ItemID/ItemName\" as a standard.")]
        public string SkinPath;

        // Transform where the receiver skin UI is attached.
        public Transform ReceiverSkinUIPoint;

        [HideInInspector]
        public TransformProxy ReceiverSkinUIPointProxy;

        // The currently selected receiver skin ID.
        public string CurrentSelectedReceiverSkinID = "Default";

        // Mesh renderers for the receiver.
        [Tooltip("Can be populated with the context menu on the gun.")]
        public MeshRenderer[] ReceiverMeshRenderers;

        // Collection of existing ReceiverSkinSystem instances.
        private static readonly Dictionary<FVRPhysicalObject, ReceiverSkinSystem> _existingReceiverSkinSystems = new();

        // Take & Hold randomization flag
        [HideInInspector]
        public bool IsInTakeAndHold = false;

        // Unvaulting toggle
        [HideInInspector]
        public bool WasUnvaulted = false;

        // Property to get the receiver skins definition.
        public ModularWorkshopSkinsDefinition ReceiverSkinsDefinition
        {
            get
            {
                if (ModularWorkshopManager.ModularWorkshopSkinsDictionary.TryGetValue(SkinPath, out ModularWorkshopSkinsDefinition skinsDefinition)) return skinsDefinition;
                else
                {
                    OpenScripts2_BepInExPlugin.LogError(MainObject, $"No Receiver SkinsDefinition found for {SkinPath}!");
                    return null;
                }
            }
        }

        /// <summary>
        /// Awake method called when the script instance is being loaded.
        /// </summary>
        public void Awake()
        {
            if (MainObject == null) MainObject = GetComponent<FVRPhysicalObject>();
            _existingReceiverSkinSystems.Add(MainObject, this);
            if (ReceiverMeshRenderers == null || ReceiverMeshRenderers.Length == 0) GetReceiverMeshRenderers();

            // Check for TnH
            if (GM.TNH_Manager != null)
            {
                IsInTakeAndHold = ModularWorkshopManager.EnableTNHRandomization.Value;
            }
        }

        /// <summary>
        /// Start method called when the script starts running.
        /// </summary>
        public void Start()
        {
            // If there's no ObjectWrapper assigned (required for vaulting) delete this component.
            if (MainObject.ObjectWrapper == null)
            {
                Destroy(this);
                return;
            }
            if (ReceiverSkinUIPoint != null) ReceiverSkinUIPointProxy = new(ReceiverSkinUIPoint, true);
            else
            {
                // Create a temporary UI point if none is provided.
                GameObject uiPoint = new("temp");
                uiPoint.transform.SetParent(MainObject.transform);
                uiPoint.transform.position = MainObject.transform.position + -MainObject.transform.right * 0.1f;
                uiPoint.transform.rotation = MainObject.transform.rotation * Quaternion.Euler(new Vector3(0f, 90f, 0f));

                ReceiverSkinUIPointProxy = new(uiPoint.transform, true);
            }

            // Set SkinPath if non is provided
            if (SkinPath == null || SkinPath == string.Empty) SkinPath = MainObject.ObjectWrapper.ItemID + "/" + "Receiver";
            // Create a new default skin if a default skin does not exist
            CheckForDefaultReceiverSkin(MainObject);
            // Apply currently selected skin
            if (!WasUnvaulted) ApplyReceiverSkin(IsInTakeAndHold ? ReceiverSkinsDefinition.GetRandomSkin() : CurrentSelectedReceiverSkinID);
        }

        /// <summary>
        /// OnDestroy method called when the script is being destroyed.
        /// </summary>
        public void OnDestroy()
        {
            _existingReceiverSkinSystems.Remove(MainObject);
        }

        // Receiver Skins

        /// <summary>
        /// Applies the selected receiver skin.
        /// </summary>
        /// <param name="skinName">The name of the skin to apply.</param>
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

        /// <summary>
        /// Checks for the default receiver skin and creates it if not found.
        /// </summary>
        /// <param name="fireArm">The FVRPhysicalObject representing the firearm.</param>
        public void CheckForDefaultReceiverSkin(FVRPhysicalObject fireArm)
        {
            // Check if the default skin is selected and create it if not found.
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

        /// <summary>
        /// Retrieves and populates the list of receiver mesh renderers.
        /// </summary>
        public void GetReceiverMeshRenderers()
        {
            List<MeshRenderer> ignoredMeshRenderers = new();

            // Collect mesh renderers to ignore from various component types.
            foreach (var part in MainObject.GetComponentsInChildren<ModularWeaponPart>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            foreach (var part in MainObject.GetComponentsInChildren<FVRFireArmAttachment>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            foreach (var part in MainObject.GetComponentsInChildren<FVRFireArmChamber>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            foreach (var part in MainObject.GetComponentsInChildren<FVRFirearmMovingProxyRound>())
            {
                ignoredMeshRenderers.AddRange(part.GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled));
            }
            if (MainObject is FVRFireArmMagazine mag)
            {
                ignoredMeshRenderers.AddRange(mag.DisplayRenderers.OfType<MeshRenderer>());
            }

            ReceiverMeshRenderers = GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled).Where(m => !ignoredMeshRenderers.Contains(m)).ToArray();
        }

        /// <summary>
        /// Populates the list of receiver mesh renderers using a context menu.
        /// </summary>
        [ContextMenu("Populate Receiver Mesh Renderer List")]
        public void PopulateReceiverMeshList()
        {
            GetReceiverMeshRenderers();
        }

        // Harmony Patches for vaulting

        /// <summary>
        /// Harmony patch for the GetFlagDic method of FVRPhysicalObject.
        /// </summary>
        [HarmonyPatch(typeof(FVRPhysicalObject), nameof(FVRPhysicalObject.GetFlagDic))]
        [HarmonyPostfix]
        public static void GetFlagDicPatch(FVRPhysicalObject __instance, ref Dictionary<string, string> __result)
        {
            if (_existingReceiverSkinSystems.TryGetValue(__instance, out ReceiverSkinSystem skinSystem))
            {
                __result = skinSystem.GetFlagDic(__result);
            }
        }

        /// <summary>
        /// Harmony patch for the ConfigureFromFlagDic method of FVRPhysicalObject.
        /// </summary>
        [HarmonyPatch(typeof(FVRPhysicalObject), nameof(FVRPhysicalObject.ConfigureFromFlagDic))]
        [HarmonyPostfix]
        public static void ConfigureFromFlagDicPatch(FVRPhysicalObject __instance, Dictionary<string, string> f)
        {
            if (_existingReceiverSkinSystems.TryGetValue(__instance, out ReceiverSkinSystem skinSystem))
            {
                skinSystem.ConfigureFromFlagDic(f);
            }
        }

        /// <summary>
        /// Harmony patch for the DuplicateFromSpawnLock method of FVRPhysicalObject.
        /// </summary>
        [HarmonyPatch(typeof(FVRPhysicalObject), nameof(FVRPhysicalObject.DuplicateFromSpawnLock))]
        [HarmonyPostfix]
        public static void DuplicateFromSpawnLockPatch(FVRPhysicalObject __instance, ref GameObject __result)
        {
            if (_existingReceiverSkinSystems.TryGetValue(__instance, out ReceiverSkinSystem skinSystem))
            {
                __result = skinSystem.DuplicateFromSpawnLock(__result);
            }
        }

        /// <summary>
        /// Configures the receiver skin from a flag dictionary.
        /// </summary>
        /// <param name="f">The flag dictionary containing skin information.</param>
        public void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            if (SkinPath == null && MainObject != null && MainObject.ObjectWrapper != null) SkinPath = MainObject.ObjectWrapper.ItemID + "/" + "Receiver";

            if (SkinPath != null && f.TryGetValue(SkinPath, out string selectedSkin)) ApplyReceiverSkin(selectedSkin);

            WasUnvaulted = true;
        }

        /// <summary>
        /// Gets the flag dictionary for the receiver skin.
        /// </summary>
        /// <param name="flagDic">The existing flag dictionary.</param>
        /// <returns>The updated flag dictionary.</returns>
        public Dictionary<string, string> GetFlagDic(Dictionary<string, string> flagDic)
        {
            flagDic.Add(SkinPath, CurrentSelectedReceiverSkinID);

            return flagDic;
        }

        /// <summary>
        /// Duplicates the object from a spawn lock, applying the selected receiver skin.
        /// </summary>
        /// <param name="copy">The duplicated GameObject.</param>
        /// <returns>The duplicated GameObject with the receiver skin applied.</returns>
        public GameObject DuplicateFromSpawnLock(GameObject copy)
        {
            ReceiverSkinSystem receiverSkinSystem = copy.GetComponentInChildren<ReceiverSkinSystem>();

            receiverSkinSystem.ApplyReceiverSkin(CurrentSelectedReceiverSkinID);

            WasUnvaulted = true;
            return copy;
        }
    }
}