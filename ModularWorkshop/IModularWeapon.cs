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
        public string ModularBarrelPartsID { get; }
        public Transform ModularBarrelPoint { get; }
        public TransformProxy ModularBarrelUIPointProxy { get; }
        public Dictionary<string,GameObject> ModularBarrelPrefabsDictionary { get; }
        public string ModularHandguardPartsID { get; }
        public Transform ModularHandguardPoint { get; }
        public TransformProxy ModularHandguardUIPointProxy { get; }
        public Dictionary<string, GameObject> ModularHandguardPrefabsDictionary { get; }
        public string ModularStockPartsID { get; }
        public Transform ModularStockPoint { get; }
        public TransformProxy ModularStockUIPointProxy { get; }
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

        public ModularFVRFireArm GetModularFVRFireArm { get; }
    }
}
