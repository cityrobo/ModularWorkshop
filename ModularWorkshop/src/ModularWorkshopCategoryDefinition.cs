using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModularWorkshop
{
    [CreateAssetMenu(fileName = "New ModularWorkshopCategoryDefinition", menuName = "ModularWorkshop/ModularWorkshopCategoryDefinition", order = 0)]
    public class ModularWorkshopCategoryDefinition : ScriptableObject
    {
        public string ModularPartsGroupID;
        public string CategoryName;
        public Sprite CategoryIcon;
        public List<string> ModularWeaponPartNames;

        public Dictionary<string, GameObject> PartsDictionary
        {
            get
            {
                Dictionary<string, GameObject> keyValuePairs = new();
                foreach (var prefab in ModularWorkshopManager.ModularWorkshopPartsGroupsDictionary[ModularPartsGroupID].ModularPrefabs)
                {
                    string Name = prefab.GetComponent<ModularWeaponPart>().Name;
                    try
                    {
                        keyValuePairs.Add(Name, prefab);
                    }
                    catch (ArgumentException)
                    {
                        Debug.LogWarning($"Part with name {Name} already in parts dictionary! Skipping part!");
                    }
                }
                return keyValuePairs;
            }
        }
    }
}