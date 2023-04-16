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
    [CreateAssetMenu(fileName = "New ModularWorkshopPartsDefinition", menuName = "ModularWorkshop/ModularWorkshopPartsDefinition", order = 0)]
    public class ModularWorkshopPartsDefinition : ScriptableObject
    {
        public string ModularPartsGroupID;
        [Tooltip("Optional Display name. If left empty, the ModularPartsGroupID will be used as a display name instead.")]
        public string DisplayName;
        public List<GameObject> ModularPrefabs;

        public Dictionary<string, GameObject> PartsDictionary
        {
            get
            {
                Dictionary<string, GameObject> keyValuePairs = new();
                foreach (var prefab in ModularPrefabs)
                {
                    string Name = prefab.GetComponent<ModularWeaponPart>().Name;
                    keyValuePairs.Add(Name, prefab);
                }
                return keyValuePairs;
            }
        }

        public GameObject GetRandomPart()
        {
            int randomIndex = UnityEngine.Random.Range(0, ModularPrefabs.Count);

            return ModularPrefabs[randomIndex];
        }
    }
}