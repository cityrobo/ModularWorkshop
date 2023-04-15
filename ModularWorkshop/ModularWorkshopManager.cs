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

        public static ConfigEntry<int> MaximumNumberOfTries;
        public static ConfigEntry<bool> ReloadDatabase;

        private int _lastNumberOfPartsDefinitions = 0;
        private int _numberOfTries = 0;
        private bool _loadingDatabase = false;

        public ModularWorkshopManager()
        {
            MaximumNumberOfTries = Config.Bind("Modular Workshop Manager", "Maximum number of tries", 10, "The maximum number of tries (or seconds), the parts manager waits after the last ModularWeaponPartsDefinition has been loaded before it stops loading new definitions.");
            ReloadDatabase = Config.Bind("Modular Workshop Manager", "Reload Database", false, "You can use this from inside the game to reload the database, if needed. Shouldn't be, but hey, I give you the option!");
        }

        public void Awake()
        {
            if (!_loadingDatabase) StartCoroutine(UpdateDatabase());

            ReloadDatabase.SettingChanged += SettingsChanged;
        }

        public void OnDestroy()
        {

        }

        private void SettingsChanged(object sender, EventArgs e)
        {
            if (ReloadDatabase.Value)
            {
                if (!_loadingDatabase) StartCoroutine(UpdateDatabase());
                ReloadDatabase.Value = false;
            }
        }

        private IEnumerator UpdateDatabase()
        {
            _loadingDatabase = true;
            s_foundModularPartsDefinitions.Clear();
            ModularWorkshopDictionary.Clear();
            while (_numberOfTries < MaximumNumberOfTries.Value)
            {
                ModularWorkshopPartsDefinition[] partsDefinitions = Resources.FindObjectsOfTypeAll<ModularWorkshopPartsDefinition>();

                foreach (var partDefinition in partsDefinitions)
                {
                    if (!s_foundModularPartsDefinitions.Contains(partDefinition))
                    {
                        s_foundModularPartsDefinitions.Add(partDefinition);

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
                    _numberOfTries = 0;
                    Logger.LogInfo($"Loaded {_lastNumberOfPartsDefinitions} ModularWorkshopPartsDefinitions.");
                }
                else _numberOfTries++;

                yield return new WaitForSeconds(1f);
            }

            _loadingDatabase = false;
            Logger.LogInfo($"Finishded loading with {_lastNumberOfPartsDefinitions} ModularWorkshopPartsDefinitions total and {ModularWorkshopDictionary.Count} in Dictionary.");
        }
    }
}
