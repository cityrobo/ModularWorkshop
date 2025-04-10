using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using UnityEngine;
using Valve.VR;
using HarmonyLib;
using System.Linq;
using BepInEx.Logging;

namespace ModularWorkshop
{
    [BepInPlugin("h3vr.cityrobo.ModularWorkshopManager", "Modular Workshop Manager", "1.2.0")]
    [BepInDependency("h3vr.OpenScripts2")]
    public class ModularWorkshopManager : BaseUnityPlugin
    {
        public static ConfigEntry<bool> EnableTNHRandomization;

        private static readonly List<ModularWorkshopPartsDefinition> s_foundModularPartsDefinitions = new();
        public static readonly Dictionary<string, ModularWorkshopPartsDefinition> ModularWorkshopPartsGroupsDictionary = new();

        private static readonly List<ModularWorkshopSkinsDefinition> s_foundModularSkinsDefinitions = new();
        public static readonly Dictionary<string, ModularWorkshopSkinsDefinition> ModularWorkshopSkinsDictionary = new();

        private static readonly List<ModularWorkshopCategoryDefinition> s_foundModularCategoryDefinitions = new();
        public static readonly Dictionary<string, ModularWorkshopCategoryDefinition> ModularWorkshopCategoryDictionary = new();

        public static ConfigEntry<int> MaximumNumberOfTries;
        public static ConfigEntry<bool> ReloadDatabase;

        public static ModularWorkshopManager Instance;

        public ManualLogSource Logging => Logger;

        private int _lastNumberOfPartsDefinitions = 0;
        private int _lastNumberOfSkinsDefinitions = 0;
        private int _lastNumberOfCategoryDefinitions = 0;
        private int _numberOfPartsTries = 0;
        private int _numberOfSkinsTries = 0;
        private int _numberOfCategoryTries = 0;
        private bool _loadingPartsDatabase = false;
        private bool _loadingSkinsDatabase = false;
        private bool _loadingCategoriesDatabase = false;

        public ModularWorkshopManager()
        {
            MaximumNumberOfTries = Config.Bind("Modular Workshop Manager", "Maximum number of tries", 10, "The maximum number of tries (or seconds), the parts manager waits after the last ModularWeaponPartsDefinition has been loaded before it stops loading new definitions.");
            ReloadDatabase = Config.Bind("Modular Workshop Manager", "Reload Database", false, "You can use this from inside the game to reload the database, if needed. Shouldn't be, but hey, I give you the option!");
            EnableTNHRandomization = Config.Bind("Modular Workshop Manager", "Enable TnH Randomization", true, "You can disable Take & Hold randomization of modular objects here.");
        }

        public void Awake()
        {
            if (!_loadingPartsDatabase && !_loadingSkinsDatabase)
            {
                StartCoroutine(UpdatePartsDatabase());
                StartCoroutine(UpdateSkinDatabase());
            }

            ReloadDatabase.SettingChanged += SettingsChanged;

            Harmony.CreateAndPatchAll(typeof(ReceiverSkinSystem));
            Harmony.CreateAndPatchAll(typeof(ModularFVRPhysicalObject));
            Harmony.CreateAndPatchAll(typeof(ModularMagazineExtension));
            Harmony.CreateAndPatchAll(typeof(SnappyTriggerAddon));
            Harmony.CreateAndPatchAll(typeof(MagazineCompatibilityAddon));

            Instance = this;
        }

        public void OnDestroy()
        {
            ReloadDatabase.SettingChanged -= SettingsChanged;
        }

        private void SettingsChanged(object sender, EventArgs e)
        {
            if (ReloadDatabase.Value)
            {
                if (!_loadingPartsDatabase && !_loadingSkinsDatabase)
                {
                    StartCoroutine(UpdatePartsDatabase());
                    StartCoroutine(UpdateSkinDatabase());
                }

                ReloadDatabase.Value = false;
            }
        }

        private IEnumerator UpdatePartsDatabase()
        {
            _loadingPartsDatabase = true;
            s_foundModularPartsDefinitions.Clear();
            ModularWorkshopPartsGroupsDictionary.Clear();
            while (_numberOfPartsTries < MaximumNumberOfTries.Value)
            {
                ModularWorkshopPartsDefinition[] partsDefinitions = Resources.FindObjectsOfTypeAll<ModularWorkshopPartsDefinition>();

                foreach (var partDefinition in partsDefinitions)
                {
                    if (!s_foundModularPartsDefinitions.Contains(partDefinition))
                    {
                        s_foundModularPartsDefinitions.Add(partDefinition);

                        //if (partDefinition.ModularPartsGroupID == null) Logger.LogError($"{partDefinition.name} has null ModularPartsGroupID field!");

                        if (!ModularWorkshopPartsGroupsDictionary.TryGetValue(partDefinition.ModularPartsGroupID, out ModularWorkshopPartsDefinition partsDefinitionOld))
                        {
                            if (partDefinition.DisplayName == string.Empty) partDefinition.DisplayName = partDefinition.ModularPartsGroupID;
                            ModularWorkshopPartsGroupsDictionary.Add(partDefinition.ModularPartsGroupID, partDefinition);
                            Logger.LogInfo($"Loaded ModularWorkshopPartsDefinition \"{partDefinition.name}\" with ModularPartsGroupID \"{partDefinition.ModularPartsGroupID}\".");
                        }
                        else
                        {
                            partsDefinitionOld.ModularPrefabs.AddRange(partDefinition.ModularPrefabs);
                            Logger.LogInfo($"Added more parts from ModularWorkshopPartsDefinition \"{partDefinition.name}\" to ModularPartsGroupID \"{partDefinition.ModularPartsGroupID}\".");
                        }
                    }
                }

                if (_lastNumberOfPartsDefinitions != partsDefinitions.Length)
                {
                    _lastNumberOfPartsDefinitions = partsDefinitions.Length;
                    _numberOfPartsTries = 0;
                    Logger.LogInfo($"Loaded {_lastNumberOfPartsDefinitions} ModularWorkshopPartsDefinitions.");
                }
                else _numberOfPartsTries++;

                yield return new WaitForSeconds(1f);
            }

            _loadingPartsDatabase = false;
            Logger.LogInfo($"Finished loading with {_lastNumberOfPartsDefinitions} ModularWorkshopPartsDefinitions total and {ModularWorkshopPartsGroupsDictionary.Count} in Dictionary.");
        }

        private IEnumerator UpdateSkinDatabase()
        {
            _loadingSkinsDatabase = true;
            s_foundModularPartsDefinitions.Clear();
            ModularWorkshopSkinsDictionary.Clear();
            while (_numberOfSkinsTries < MaximumNumberOfTries.Value)
            {
                ModularWorkshopSkinsDefinition[] skinsDefinitions = Resources.FindObjectsOfTypeAll<ModularWorkshopSkinsDefinition>().Where(d => !d.AutomaticallyCreated).ToArray();

                foreach (var skinDefinition in skinsDefinitions)
                {
                    if (!s_foundModularSkinsDefinitions.Contains(skinDefinition))
                    {
                        s_foundModularSkinsDefinitions.Add(skinDefinition);

                        //if (partDefinition.ModularPartsGroupID == null) Logger.LogError($"{partDefinition.name} has null ModularPartsGroupID field!");
                        string skinPath = skinDefinition.ModularPartsGroupID + "/" + skinDefinition.PartName;

                        if (ModularWorkshopSkinsDictionary.TryGetValue(skinPath, out ModularWorkshopSkinsDefinition skinsDefinitionOld))
                        {
                            skinsDefinitionOld.SkinDefinitions.AddRange(skinDefinition.SkinDefinitions);
                            Logger.LogInfo($"Added more skins from ModularWorkshopSkinsDefinition \"{skinDefinition.name}\" to ModularSkin \"{skinPath}\".");
                        }
                        else
                        {
                            foreach (var skin in skinDefinition.SkinDefinitions)
                            {
                                if (skin.DisplayName == string.Empty) skin.DisplayName = skin.ModularSkinID;
                            }

                            ModularWorkshopSkinsDictionary.Add(skinPath, skinDefinition);
                            Logger.LogInfo($"Loaded ModularWorkshopSkinsDefinition \"{skinDefinition.name}\" with ModularSkin \"{skinPath}\".");
                        }
                    }
                }

                if (_lastNumberOfSkinsDefinitions != skinsDefinitions.Length)
                {
                    _lastNumberOfSkinsDefinitions = skinsDefinitions.Length;
                    _numberOfSkinsTries = 0;
                    Logger.LogInfo($"Loaded {_lastNumberOfSkinsDefinitions} ModularWorkshopSkinsDefinition.");
                }
                else _numberOfSkinsTries++;

                yield return new WaitForSeconds(1f);
            }

            _loadingSkinsDatabase = false;
            Logger.LogInfo($"Finished loading with {_lastNumberOfSkinsDefinitions} ModularWorkshopSkinsDefinition total and {ModularWorkshopSkinsDictionary.Values.Where(d=>!d.AutomaticallyCreated).Count()} in Dictionary.");
        }

        //private IEnumerator UpdateCategoryDatabase()
        //{
        //    _loadingCategoryDatabase = true;
        //    s_foundModularCategoryDefinitions.Clear();
        //    ModularWorkshopCategoryDictionary.Clear();
        //    while (_numberOfSkinTries < MaximumNumberOfTries.Value)
        //    {
        //        //ModularWorkshopCategoryDefinition[] categoriesDefinitions = Resources.FindObjectsOfTypeAll<ModularWorkshopCategoryDefinition>().Where(d => !d.AutomaticallyCreated).ToArray();
        //        ModularWorkshopCategoryDefinition[] categoryDefinitions = Resources.FindObjectsOfTypeAll<ModularWorkshopCategoryDefinition>();

        //        foreach (var categoryDefinition in categoryDefinitions)
        //        {
        //            if (!s_foundModularCategoryDefinitions.Contains(categoryDefinition))
        //            {
        //                s_foundModularCategoryDefinitions.Add(categoryDefinition);

        //                //if (partDefinition.ModularPartsGroupID == null) Logger.LogError($"{partDefinition.name} has null ModularPartsGroupID field!");
        //                string skinPath = categoryDefinition.ModularPartsGroupID + "/" + categoryDefinition.PartName;

        //                if (ModularWorkshopCategoriesDictionary.TryGetValue(skinPath, out ModularWorkshopCategoriesDefinition categoriesDefinitionOld))
        //                {
        //                    categoriesDefinitionOld.SkinDefinitions.AddRange(categoryDefinition.SkinDefinitions);
        //                    Logger.LogInfo($"Added more categories from ModularWorkshopCategoriesDefinition {categoryDefinition.name} to ModularSkin {skinPath}.");
        //                }
        //                else
        //                {
        //                    foreach (var skin in categoryDefinition.SkinDefinitions)
        //                    {
        //                        if (skin.DisplayName == string.Empty) skin.DisplayName = skin.ModularSkinID;
        //                    }

        //                    ModularWorkshopCategoriesDictionary.Add(skinPath, categoryDefinition);
        //                    Logger.LogInfo($"Loaded ModularWorkshopCategoriesDefinition {categoryDefinition.name} with ModularSkin {skinPath}.");
        //                }
        //            }
        //        }

        //        if (_lastNumberOfCategoriesDefinitions != categoryDefinitions.Length)
        //        {
        //            _lastNumberOfCategoriesDefinitions = categoryDefinitions.Length;
        //            _numberOfSkinTries = 0;
        //            Logger.LogInfo($"Loaded {_lastNumberOfCategoriesDefinitions} ModularWorkshopCategoriesDefinition.");
        //        }
        //        else _numberOfSkinTries++;

        //        yield return new WaitForSeconds(1f);
        //    }

        //    _loadingCategoriesDatabase = false;
        //    Logger.LogInfo($"Finished loading with {_lastNumberOfCategoriesDefinitions} ModularWorkshopCategoriesDefinition total and {ModularWorkshopCategoriesDictionary.Values.Where(d => !d.AutomaticallyCreated).Count()} in Dictionary.");
        //}

        public static void Log(MonoBehaviour plugin, string message)
        {
            Instance.Logging.LogMessage($"{plugin}: {message}");
        }
        public static void Log(string message)
        {
            Instance.Logging.LogMessage(message);
        }
        public static void LogWarning(MonoBehaviour plugin, string message)
        {
            Instance.Logging.LogWarning($"{plugin}: {message}");
        }
        public static void LogWarning(string message)
        {
            Instance.Logging.LogWarning(message);
        }
        public static void LogError(MonoBehaviour plugin, string message)
        {
            Instance.Logging.LogError($"{plugin}: {message}");
        }
        public static void LogError(string message)
        {
            Instance.Logging.LogError(message);
        }
        public static void LogException(MonoBehaviour plugin, Exception e)
        {
            Instance.Logging.LogError($"{plugin}: {e.Message}");
        }
    }
}
