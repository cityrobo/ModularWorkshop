using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;

namespace ModularWorkshop
{
    public interface IModularWeapon
    {
        public GameObject UIPrefab { get; }
        public Transform GetModularBarrelPosition { get; }
        public TransformProxy GetModularBarrelUIPosition { get; }
        public GameObject[] ModularBarrelPrefabs { get; }
        public Transform GetModularHandguardPosition { get; }
        public TransformProxy GetModularHandguardUIPosition { get; }
        public GameObject[] ModularHandguardPrefabs { get; }
        public Transform GetModularStockPosition { get; }
        public TransformProxy GetModularStockUIPosition { get; }
        public GameObject[] ModularStockPrefabs { get; }

        public int GetSelectedModularBarrel { get; }
        public int GetSelectedModularHandguard { get; }
        public int GetSelectedModularStock { get; }

        public ModularWeaponPartsAttachmentPoint[] GetModularWeaponPartsAttachmentPoints { get; }
        public Dictionary<string, List<GameObject>> GetModularWeaponPartsDictionary { get; }

        public void ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, int index);

        public void ConfigureModularBarrel(int index);
        public void ConfigureModularHandguard(int index);

        public void ConfigureModularStock(int index);
    }
}
