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
        public string WeaponSystemID;
        public GameObject UIPrefab;
        public List<GameObject> ModularBarrelPrefabs;
        public List<GameObject> ModularHandguardPrefabs;
        public List<GameObject> ModularStockPrefabs;
        public List<ModularWeaponPartsPrefabs> ModularWeaponPartsPrefabs;

        public Dictionary<string, List<GameObject>> ModularWeaponPartsDictionary 
        { 
            get 
            {
                Dictionary<string, List<GameObject>> keyValuePairs = new();
                foreach (var item in ModularWeaponPartsPrefabs)
                {
                    keyValuePairs.Add(item.GroupName, item.Prefabs);
                }
                return keyValuePairs;
            } 
        }
    }
}