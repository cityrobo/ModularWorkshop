using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;

namespace ModularWorkshop
{
    public interface IModularWeapon
    {
        public GameObject UIPrefab { get; }
        public string ModularBarrelPartID { get; }
        public Transform ModularBarrelPosition { get; }
        public TransformProxy ModularBarrelUIPosition { get; }
        public Dictionary<string,GameObject> ModularBarrelPrefabsDictionary { get; }
        public string ModularHandguardPartID { get; }
        public Transform ModularHandguardPosition { get; }
        public TransformProxy ModularHandguardUIPosition { get; }
        public Dictionary<string, GameObject> ModularHandguardPrefabsDictionary { get; }
        public string ModularStockPartID { get; }
        public Transform ModularStockPosition { get; }
        public TransformProxy ModularStockUIPosition { get; }
        public Dictionary<string, GameObject> ModularStockPrefabsDictionary { get; }

        public string SelectedModularBarrel { get;}
        public string SelectedModularHandguard { get;}
        public string SelectedModularStock { get;}

        public ModularWeaponPartsAttachmentPoint[] ModularWeaponPartsAttachmentPoints { get; }

        public ModularWeaponPart ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, string partName);

        public ModularBarrel ConfigureModularBarrel(string partName);
        public ModularHandguard ConfigureModularHandguard(string partName);

        public ModularStock ConfigureModularStock(string partName);

        public ModularWorkshopPlatform WorkshopPlatform { get; set; }
        public List<ModularWeaponPartsAttachmentPoint> SubAttachmentPoints { get; }
    }
}
