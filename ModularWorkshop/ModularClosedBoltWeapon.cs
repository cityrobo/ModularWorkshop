using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularClosedBoltWeapon : ClosedBoltWeapon , IModularWeapon
    {
        [Header("Modular Configuration")]
        public string WeaponSystemID;

        public GameObject UIPrefab => ModularWorkshopManager.ModularWorkshopDictionary[WeaponSystemID].UIPrefab;
        public Transform ModularBarrelPosition;
        public Transform ModularBarrelUIPosition;
        private TransformProxy _modularBarrelUIPosition;
        public Transform GetModularBarrelPosition => ModularBarrelPosition;
        public TransformProxy GetModularBarrelUIPosition => _modularBarrelUIPosition;
        public GameObject[] ModularBarrelPrefabs => ModularWorkshopManager.ModularWorkshopDictionary[WeaponSystemID].ModularBarrelPrefabs.ToArray();
        public Transform ModularHandguardPosition;
        public Transform ModularHandguardUIPosition;
        private TransformProxy _modularHandguardUIPosition;
        public Transform GetModularHandguardPosition => ModularHandguardPosition;
        public TransformProxy GetModularHandguardUIPosition => _modularHandguardUIPosition;
        public GameObject[] ModularHandguardPrefabs => ModularWorkshopManager.ModularWorkshopDictionary[WeaponSystemID].ModularHandguardPrefabs.ToArray();
        public Transform ModularStockPosition;
        public Transform ModularStockUIPosition;
        private TransformProxy _modularStockUIPosition;
        public Transform GetModularStockPosition => ModularStockPosition;
        public TransformProxy GetModularStockUIPosition => _modularHandguardUIPosition;
        public GameObject[] ModularStockPrefabs => ModularWorkshopManager.ModularWorkshopDictionary[WeaponSystemID].ModularStockPrefabs.ToArray();

        public int SelectedModularBarrel = 0;
        public int GetSelectedModularBarrel => SelectedModularBarrel;
        public int SelectedModularHandguard = 0;
        public int GetSelectedModularHandguard => SelectedModularHandguard;
        public int SelectedModularStock = 0;
        public int GetSelectedModularStock => SelectedModularStock;

        public ModularWeaponPartsAttachmentPoint[] ModularWeaponPartsAttachmentPoints;
        public ModularWeaponPartsAttachmentPoint[] GetModularWeaponPartsAttachmentPoints => ModularWeaponPartsAttachmentPoints;

        public Dictionary<string, List<GameObject>> GetModularWeaponPartsDictionary => ModularWorkshopManager.ModularWorkshopDictionary[WeaponSystemID].ModularWeaponPartsDictionary;

        private const string c_modularBarrelKey = "ModulBarrel";
        private const string c_modularHandguardKey = "ModulHandguard";
        private const string c_modularStockKey = "ModulStock";

        public override void Awake()
        {
            base.Awake();

            ConfigureAll();

            ConvertTransformsToProxies();
        }

        public override void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            base.ConfigureFromFlagDic(f);

            string indexString;
            if (f.TryGetValue(c_modularBarrelKey, out indexString)) ConfigureModularBarrel(int.Parse(indexString));
            if (f.TryGetValue(c_modularHandguardKey, out indexString)) ConfigureModularHandguard(int.Parse(indexString));
            if (f.TryGetValue(c_modularStockKey, out indexString)) ConfigureModularStock(int.Parse(indexString));

            foreach (var modularWeaponPartsAttachmentPoint in ModularWeaponPartsAttachmentPoints)
            {
                if (f.TryGetValue("Modul" + modularWeaponPartsAttachmentPoint.GroupName, out indexString)) ConfigureModularWeaponPart(modularWeaponPartsAttachmentPoint, int.Parse(indexString));
            }
        }

        public override Dictionary<string, string> GetFlagDic()
        {
            Dictionary<string, string> flagDic = base.GetFlagDic();

            if (ModularBarrelPrefabs.Length > 0) flagDic.Add(c_modularBarrelKey, SelectedModularBarrel.ToString());
            if (ModularHandguardPrefabs.Length > 0) flagDic.Add(c_modularHandguardKey, SelectedModularHandguard.ToString());
            if (ModularStockPrefabs.Length > 0) flagDic.Add(c_modularStockKey, SelectedModularStock.ToString());

            foreach (var modularWeaponPartsAttachmentPoint in ModularWeaponPartsAttachmentPoints)
            {
                List<GameObject> prefabs;

                if (GetModularWeaponPartsDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.GroupName, out prefabs) && prefabs.Count > 0) flagDic.Add("Modul" + modularWeaponPartsAttachmentPoint.GroupName, modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart.ToString());
            }

            return flagDic;
        }

        public void ConfigureModularWeaponPart(ModularWeaponPartsAttachmentPoint modularWeaponPartsAttachmentPoint, int index)
        {
            modularWeaponPartsAttachmentPoint.SelectedModularWeaponPart = index;
            GetModularWeaponPartsDictionary.TryGetValue(modularWeaponPartsAttachmentPoint.GroupName, out List<GameObject> prefabs);
            GameObject modularWeaponPartPrefab = Instantiate(prefabs[index], modularWeaponPartsAttachmentPoint.ModularPartPoint.position, modularWeaponPartsAttachmentPoint.ModularPartPoint.rotation, modularWeaponPartsAttachmentPoint.ModularPartPoint.parent);
            ModularWeaponPart modularWeaponPartOld = modularWeaponPartsAttachmentPoint.ModularPartPoint.GetComponent<ModularWeaponPart>();
            ModularWeaponPart modularWeaponPartNew = modularWeaponPartPrefab.GetComponent<ModularWeaponPart>();

            UpdateAttachmentMounts(modularWeaponPartOld, modularWeaponPartNew);

            Destroy(modularWeaponPartsAttachmentPoint.ModularPartPoint.gameObject);
            modularWeaponPartsAttachmentPoint.ModularPartPoint = modularWeaponPartNew.transform;
        }

        public void ConfigureModularBarrel(int index)
        {
            SelectedModularBarrel = index;

            GameObject modularBarrelPrefab = Instantiate(ModularBarrelPrefabs[index], ModularBarrelPosition.position, ModularBarrelPosition.rotation, ModularBarrelPosition.parent);

            ModularBarrel modularBarrelOld = ModularBarrelPosition.GetComponent<ModularBarrel>();
            ModularBarrel modularBarrelNew = modularBarrelPrefab.GetComponent<ModularBarrel>();

            MuzzlePos.position = modularBarrelNew.MuzzlePosition.position;
            MuzzlePos.rotation = modularBarrelNew.MuzzlePosition.rotation;
            DefaultMuzzleState = modularBarrelNew.DefaultMuzzleState;

            UpdateAttachmentMounts(modularBarrelOld, modularBarrelNew);

            Destroy(ModularBarrelPosition.gameObject);
            ModularBarrelPosition = modularBarrelPrefab.transform;
        }
        public void ConfigureModularHandguard(int index)
        {
            SelectedModularHandguard = index;

            GameObject modularHandguardPrefab = Instantiate(ModularHandguardPrefabs[index], ModularHandguardPosition.position, ModularHandguardPosition.rotation, ModularHandguardPosition.parent);
            ModularHandguard modularHandguardOld = ModularHandguardPosition.GetComponent<ModularHandguard>();
            ModularHandguard modularHandguardNew = modularHandguardPrefab.GetComponent<ModularHandguard>();

            AltGrip.gameObject.SetActive(modularHandguardNew.ActsLikeForeGrip);
            Collider grabTrigger = AltGrip.GetComponent<Collider>();
            switch (grabTrigger)
            {
                case BoxCollider c:
                    if (modularHandguardNew.IsTriggerComponentPosition)
                    {
                        c.center = modularHandguardNew.AltGripTriggerGameObjectPosition;
                    }
                    else
                    {
                        AltGrip.transform.localPosition = modularHandguardNew.AltGripTriggerGameObjectPosition;
                    }

                    if (modularHandguardNew.IsTriggerComponentSize)
                    {
                        c.size = modularHandguardNew.AltGripTriggerGameObjectScale;
                    }
                    else
                    {
                        AltGrip.transform.localScale = modularHandguardNew.AltGripTriggerGameObjectScale;
                    }
                    break;
                case CapsuleCollider c:
                    if (modularHandguardNew.IsTriggerComponentPosition)
                    {
                        c.center = modularHandguardNew.AltGripTriggerGameObjectPosition;
                    }
                    else
                    {
                        AltGrip.transform.localPosition = modularHandguardNew.AltGripTriggerGameObjectPosition;
                    }

                    if (modularHandguardNew.IsTriggerComponentSize)
                    {
                        c.radius = modularHandguardNew.AltGripTriggerGameObjectScale.x;
                        c.height = modularHandguardNew.AltGripTriggerGameObjectScale.y;
                    }
                    else
                    {
                        AltGrip.transform.localScale = modularHandguardNew.AltGripTriggerGameObjectScale;
                    }
                    break;
            }
            AltGrip.transform.localRotation = Quaternion.Euler(modularHandguardNew.AltGripTriggerGameObjectRotation);

            UpdateAttachmentMounts(modularHandguardOld, modularHandguardNew);

            Destroy(ModularHandguardPosition.gameObject);
            ModularHandguardPosition = modularHandguardPrefab.transform;
        }
        public void ConfigureModularStock(int index)
        {
            SelectedModularStock = index;

            GameObject modularStockPrefab = Instantiate(ModularStockPrefabs[index], ModularStockPosition.position, ModularStockPosition.rotation, ModularStockPosition.parent);

            ModularStock modularStockOld = ModularStockPosition.GetComponent<ModularStock>();
            ModularStock modularStockNew = modularStockPrefab.GetComponent<ModularStock>();

            HasActiveShoulderStock = modularStockNew.ActsLikeStock;
            StockPos.position = modularStockNew.StockPoint.position;
            StockPos.rotation = modularStockNew.StockPoint.rotation;

            if (modularStockNew.CollapsingStock != null) modularStockNew.CollapsingStock.Firearm = this;
            if (modularStockNew.FoldingStockX != null) modularStockNew.FoldingStockX.FireArm = this;
            if (modularStockNew.FoldingStockY != null) modularStockNew.FoldingStockY.FireArm = this;

            UpdateAttachmentMounts(modularStockOld, modularStockNew);

            Destroy(ModularStockPosition.gameObject);
            ModularStockPosition = modularStockPrefab.transform;
        }

        public void ConfigureAll()
        {
            if (ModularBarrelPrefabs.Length > 0) ConfigureModularBarrel(SelectedModularBarrel);
            if (ModularHandguardPrefabs.Length > 0) ConfigureModularHandguard(SelectedModularHandguard);
            if (ModularStockPrefabs.Length > 0) ConfigureModularStock(SelectedModularStock);
            foreach (ModularWeaponPartsAttachmentPoint attachmentPoint in ModularWeaponPartsAttachmentPoints)
            {
                if (GetModularWeaponPartsDictionary.TryGetValue(attachmentPoint.GroupName, out List<GameObject> prefabs) && prefabs.Count > 0) ConfigureModularWeaponPart(attachmentPoint, attachmentPoint.SelectedModularWeaponPart);
            }
        }

        public void ConvertTransformsToProxies()
        {
            if (ModularBarrelPrefabs.Length > 0)
            {
                _modularBarrelUIPosition = new(ModularBarrelUIPosition, transform);
                Destroy(ModularBarrelUIPosition.gameObject);
            }
            if (ModularHandguardPrefabs.Length > 0)
            {
                _modularHandguardUIPosition = new(ModularHandguardUIPosition, transform);
                Destroy(ModularHandguardUIPosition.gameObject);
            }
            if (ModularStockPrefabs.Length > 0)
            {
                _modularStockUIPosition = new(ModularStockUIPosition, transform);
                Destroy(ModularStockUIPosition.gameObject);
            }

            foreach (var point in ModularWeaponPartsAttachmentPoints)
            {
                point.ModularPartUIPos = new(point.ModularPartUIPoint);
                Destroy(point.ModularPartUIPoint.gameObject);
            }
        }

        private void UpdateAttachmentMounts(ModularWeaponPart oldPart, ModularWeaponPart newPart)
        {
            FVRFireArmAttachment[] attachments;
            foreach (var mount in oldPart.AttachmentMounts)
            {
                attachments = mount.AttachmentsList.ToArray();
                foreach (var attachment in attachments) attachment.DetachFromMount();
                AttachmentMounts.Remove(mount);
            }
            AttachmentMounts.AddRange(newPart.AttachmentMounts);
            foreach (var mount in newPart.AttachmentMounts)
            {
                mount.Parent = this;
                mount.MyObject = this;
            }
        }

        [ContextMenu("Copy Existing Firearm Component")]
        public void CopyFirearm()
        {
            ClosedBoltWeapon[] attachments = GetComponents<ClosedBoltWeapon>();
            ClosedBoltWeapon toCopy = attachments.Single(c => c != this);
            toCopy.Bolt.Weapon = this;

            this.CopyComponent(toCopy);
        }
    }
}
