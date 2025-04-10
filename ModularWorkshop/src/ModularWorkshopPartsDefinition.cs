using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModularWorkshop
{
    [CreateAssetMenu(fileName = "New ModularWorkshopPartsDefinition", menuName = "ModularWorkshop/ModularWorkshopPartsDefinition", order = 0)]
    public class ModularWorkshopPartsDefinition : ScriptableObject
    {
        public string ModularPartsGroupID;
        [Tooltip("Optional Display name. If left empty, the ModularPartsGroupID will be used as a display name instead.")]
        public string DisplayName;
        public List<GameObject> ModularPrefabs;

        [HideInInspector]
        public bool _usesCategories = false;
        [HideInInspector]
        public List<ModularWorkshopCategoryDefinition> _categoryDefinitions = new();

        public Dictionary<string, GameObject> PartsDictionary
        {
            get
            {
                Dictionary<string, GameObject> keyValuePairs = new();
                foreach (var prefab in ModularPrefabs)
                {
                    if (prefab == null)
                    {
                        ModularWorkshopManager.LogWarning($"Null entry in ModularPrefabs list in ModularPartsGroupID \"{ModularPartsGroupID}\"! Remove empty space or make sure the prefab is not also in the Otherloader Build Item!");
                        continue;
                    }
                    string Name = prefab.GetComponent<ModularWeaponPart>().Name;
                    try
                    {
                        keyValuePairs.Add(Name, prefab);
                    }
                    catch (ArgumentException)
                    {
                        ModularWorkshopManager.LogWarning($"Part with name {Name} already in parts dictionary \"{name}\" with ModularPartsGroupID \"{ModularPartsGroupID}\"! Skipping duplicate part!");
                    }
                }
                return keyValuePairs;
            }
        }

        public string GetRandomPart()
        {
            List<string> spawnpool = TakeAndHoldSpawnPool;

            int randomIndex = UnityEngine.Random.Range(0, spawnpool.Count);

            return spawnpool[randomIndex];
        }

        public List<string> TakeAndHoldSpawnPool
        {
            get
            {
                List<string> spawnpool = new();
                foreach (var prefab in ModularPrefabs)
                {
                    ModularWeaponPart part = prefab.GetComponent<ModularWeaponPart>();
                    if (part.RemovedFromSpawnTable) continue;
                    string Name = part.Name;

                    spawnpool.Add(Name);

                    for (int i = 0; i < part.AdditionalSpawnTableEntries; i++)
                    {
                        spawnpool.Add(Name);
                    }
                }
                return spawnpool;
            }
        }
    }
}