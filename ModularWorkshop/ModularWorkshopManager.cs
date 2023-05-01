using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using UnityEngine;
using Valve.VR;

namespace ModularWorkshop
{
    [BepInPlugin("h3vr.cityrobo.ModularWorkshopManager", "Modular Workshop Manager", "1.0.0")]
    [BepInDependency("h3vr.OpenScripts2")]
    public class ModularWorkshopManager : BaseUnityPlugin
    {
        private static readonly List<ModularWorkshopPartsDefinition> s_foundModularPartsDefinitions = new();
        public static readonly Dictionary<string, ModularWorkshopPartsDefinition> ModularWorkshopDictionary = new();

        private static readonly List<ModularWorkshopSkinsDefinition> s_foundModularSkinsDefinitions = new();
        public static readonly Dictionary<string, ModularWorkshopSkinsDefinition> ModularWorkshopSkinsDictionary = new();

        public static ConfigEntry<int> MaximumNumberOfTries;
        public static ConfigEntry<bool> ReloadDatabase;

        private int _lastNumberOfPartsDefinitions = 0;
        private int _lastNumberOfSkinsDefinitions = 0;
        private int _numberOfPartsTries = 0;
        private int _numberOfSkinTries = 0;
        private bool _loadingDatabase = false;

        public ModularWorkshopManager()
        {
            MaximumNumberOfTries = Config.Bind("Modular Workshop Manager", "Maximum number of tries", 10, "The maximum number of tries (or seconds), the parts manager waits after the last ModularWeaponPartsDefinition has been loaded before it stops loading new definitions.");
            ReloadDatabase = Config.Bind("Modular Workshop Manager", "Reload Database", false, "You can use this from inside the game to reload the database, if needed. Shouldn't be, but hey, I give you the option!");
        }

        public void Awake()
        {
            if (!_loadingDatabase)
            {
                StartCoroutine(UpdatePartsDatabase());
                StartCoroutine(UpdateSkinDatabase());
            }

            ReloadDatabase.SettingChanged += SettingsChanged;
        }

        public void OnDestroy()
        {

        }

        private void SettingsChanged(object sender, EventArgs e)
        {
            if (ReloadDatabase.Value)
            {
                if (!_loadingDatabase)
                {
                    StartCoroutine(UpdatePartsDatabase());
                    StartCoroutine(UpdateSkinDatabase());
                }

                ReloadDatabase.Value = false;
            }
        }

        private IEnumerator UpdatePartsDatabase()
        {
            _loadingDatabase = true;
            s_foundModularPartsDefinitions.Clear();
            ModularWorkshopDictionary.Clear();
            while (_numberOfPartsTries < MaximumNumberOfTries.Value)
            {
                ModularWorkshopPartsDefinition[] partsDefinitions = Resources.FindObjectsOfTypeAll<ModularWorkshopPartsDefinition>();

                foreach (var partDefinition in partsDefinitions)
                {
                    if (!s_foundModularPartsDefinitions.Contains(partDefinition))
                    {
                        s_foundModularPartsDefinitions.Add(partDefinition);

                        //if (partDefinition.ModularPartsGroupID == null) Logger.LogError($"{partDefinition.name} has null ModularPartsGroupID field!");

                        if (!ModularWorkshopDictionary.TryGetValue(partDefinition.ModularPartsGroupID, out ModularWorkshopPartsDefinition partsDefinitionOld))
                        {
                            if (partDefinition.DisplayName == string.Empty) partDefinition.DisplayName = partDefinition.ModularPartsGroupID;
                            ModularWorkshopDictionary.Add(partDefinition.ModularPartsGroupID, partDefinition);
                            Logger.LogInfo($"Loaded ModularWorkshopPartsDefinition {partDefinition.name} with ModularPartsGroupID {partDefinition.ModularPartsGroupID}.");
                        }
                        else
                        {
                            partsDefinitionOld.ModularPrefabs.AddRange(partDefinition.ModularPrefabs);
                            Logger.LogInfo($"Added more parts from ModularWorkshopPartsDefinition {partDefinition.name} to ModularPartsGroupID {partDefinition.ModularPartsGroupID}.");
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

            _loadingDatabase = false;
            Logger.LogInfo($"Finishded loading with {_lastNumberOfPartsDefinitions} ModularWorkshopPartsDefinitions total and {ModularWorkshopDictionary.Count} in Dictionary.");
        }
        private IEnumerator UpdateSkinDatabase()
        {
            _loadingDatabase = true;
            s_foundModularPartsDefinitions.Clear();
            ModularWorkshopDictionary.Clear();
            while (_numberOfSkinTries < MaximumNumberOfTries.Value)
            {
                ModularWorkshopSkinsDefinition[] skinsDefinitions = Resources.FindObjectsOfTypeAll<ModularWorkshopSkinsDefinition>();

                foreach (var skinDefinition in skinsDefinitions)
                {
                    if (!s_foundModularSkinsDefinitions.Contains(skinDefinition))
                    {
                        s_foundModularSkinsDefinitions.Add(skinDefinition);

                        //if (partDefinition.ModularPartsGroupID == null) Logger.LogError($"{partDefinition.name} has null ModularPartsGroupID field!");
                        string skinPath = Path.Combine(skinDefinition.ModularPartsGroupID, skinDefinition.PartName);

                        if (!ModularWorkshopSkinsDictionary.TryGetValue(skinPath, out ModularWorkshopSkinsDefinition skinsDefinitionOld))
                        {
                            foreach (var skin in skinDefinition.SkinDefinitions)
                            {
                                if (skin.DisplayName == string.Empty) skin.DisplayName = skin.ModularSkinID;
                            }

                            ModularWorkshopSkinsDictionary.Add(skinDefinition.ModularPartsGroupID, skinDefinition);
                            Logger.LogInfo($"Loaded ModularWorkshopSkinsDefinition {skinDefinition.name} with ModularSkin {skinPath}.");
                        }
                        else
                        {
                            skinsDefinitionOld.SkinDefinitions.AddRange(skinDefinition.SkinDefinitions);
                            Logger.LogInfo($"Added more parts from ModularWorkshopSkinsDefinition {skinDefinition.name} to ModularSkin {skinPath}.");
                        }
                    }
                }

                if (_lastNumberOfSkinsDefinitions != skinsDefinitions.Length)
                {
                    _lastNumberOfSkinsDefinitions = skinsDefinitions.Length;
                    _numberOfSkinTries = 0;
                    Logger.LogInfo($"Loaded {_lastNumberOfSkinsDefinitions} ModularWorkshopPartsDefinitions.");
                }
                else _numberOfSkinTries++;

                yield return new WaitForSeconds(1f);
            }

            _loadingDatabase = false;
            Logger.LogInfo($"Finishded loading with {_lastNumberOfSkinsDefinitions} ModularWorkshopPartsDefinitions total and {ModularWorkshopSkinsDictionary.Count} in Dictionary.");
        }
    }
}
