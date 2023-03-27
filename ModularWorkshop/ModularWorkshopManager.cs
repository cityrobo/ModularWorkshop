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
        private static List<ModularWorkshopPartsDefinition> s_foundModularPartsDefinitions = new();
        public static Dictionary<string, ModularWorkshopPartsDefinition> ModularWorkshopDictionary = new();

        public static ConfigEntry<int> MaximumNumberOfTries;

        private int _lastNumberOfPartsDefinitions = 0;
        private int _numberOfTries = 0;

        public ModularWorkshopManager()
        {
            MaximumNumberOfTries = Config.Bind("Modular Workshop Manager", "Maximum number of tries", 10, "The maximum number of tries (or seconds), the parts manager waits after the last ModularWeaponPartsDefinition has been loaded before it stops loading new definitions.");
        }

        public void Awake()
        {
            StartCoroutine(UpdateDatabase());
        }

        public void OnDestroy()
        {

        }

        private void SettingsChanged(object sender, EventArgs e)
        {

        }

        private IEnumerator UpdateDatabase()
        {
            while (_numberOfTries < MaximumNumberOfTries.Value)
            {
                ModularWorkshopPartsDefinition[] partsDefinitions = Resources.FindObjectsOfTypeAll<ModularWorkshopPartsDefinition>();

                foreach (var partDefinition in partsDefinitions)
                {
                    if (!s_foundModularPartsDefinitions.Contains(partDefinition))
                    {
                        s_foundModularPartsDefinitions.Add(partDefinition);

                        if (!ModularWorkshopDictionary.TryGetValue(partDefinition.WeaponSystemID, out ModularWorkshopPartsDefinition partsDefinitionOld))
                        {
                            ModularWorkshopDictionary.Add(partDefinition.WeaponSystemID, partDefinition);
                        }
                        else
                        {
                            foreach (var prefab in partDefinition.ModularBarrelPrefabs) partsDefinitionOld.ModularBarrelPrefabs.Add(prefab);
                            foreach (var prefab in partDefinition.ModularHandguardPrefabs) partsDefinitionOld.ModularHandguardPrefabs.Add(prefab);
                            foreach (var prefab in partDefinition.ModularStockPrefabs) partsDefinitionOld.ModularStockPrefabs.Add(prefab);
                            foreach (var prefab in partDefinition.ModularWeaponPartsPrefabs)
                            {
                                partsDefinitionOld.ModularWeaponPartsDictionary[prefab.GroupName].AddRange(prefab.Prefabs);
                            }
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

            Logger.LogInfo($"Finishded loading with {_lastNumberOfPartsDefinitions} ModularWorkshopPartsDefinitions in Dictionary.");
        }
    }
}
