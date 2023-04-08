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
        public string PartsID;
        public List<GameObject> ModularPrefabs;

        public Dictionary<string, GameObject> PartsDictionary
        {
            get
            {
                Dictionary<string, GameObject> keyValuePairs = new();
                foreach (var item in ModularPrefabs)
                {
                    string Name = item.GetComponent<ModularWeaponPart>().Name;
                    keyValuePairs.Add(Name, item);
                }
                return keyValuePairs;
            }
        }
    }
}