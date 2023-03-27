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
    public class ModularWorkshopPlatform : MonoBehaviour
    {
        public FVRQuickBeltSlot QuickBeltSlot;
        [HideInInspector]
        public IModularWeapon ModularWeapon;

        private IModularWeapon _lastModularWeapon;

        private List<GameObject> _UIScreens = new();

        public void Update()
        {
            if (QuickBeltSlot.HeldObject != null)
            {
                ModularWeapon = QuickBeltSlot.HeldObject as IModularWeapon;

                if (ModularWeapon != null && _lastModularWeapon != ModularWeapon)
                {
                    GameObject UIPrefab = ModularWeapon.UIPrefab;
                    GameObject UIObject;
                    ModularWorkshopUI UI;
                    if (ModularWeapon.ModularBarrelPrefabs.Length > 0)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.GetModularBarrelUIPosition.position, ModularWeapon.GetModularBarrelUIPosition.rotation, ModularWeapon.GetModularBarrelUIPosition.parent);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Barrel;
                        UI.ModularWeapon = ModularWeapon;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    if (ModularWeapon.ModularHandguardPrefabs.Length > 0)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.GetModularHandguardUIPosition.position, ModularWeapon.GetModularHandguardUIPosition.rotation, ModularWeapon.GetModularHandguardUIPosition.parent);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Handguard;
                        UI.ModularWeapon = ModularWeapon;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    if (ModularWeapon.ModularStockPrefabs.Length > 0)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.GetModularStockUIPosition.position, ModularWeapon.GetModularStockUIPosition.rotation, ModularWeapon.GetModularStockUIPosition.parent);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Stock;
                        UI.ModularWeapon = ModularWeapon;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }

                    foreach (var item in ModularWeapon.GetModularWeaponPartsAttachmentPoints)
                    {
                        UIObject = Instantiate(UIPrefab, item.ModularPartUIPos.position, item.ModularPartUIPos.rotation, item.ModularPartUIPos.parent);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Custom;
                        UI.CustomPartTypeGroupName = item.GroupName;
                        UI.ModularWeapon = ModularWeapon;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                }
            }
            else if (QuickBeltSlot.HeldObject == null && _lastModularWeapon != null)
            {
                for (int i = 0; i < _UIScreens.Count; i++)
                {
                    Destroy(_UIScreens[i]);
                }
                _UIScreens.Clear();
                _lastModularWeapon = null;
                ModularWeapon = null;
            }

            _lastModularWeapon = ModularWeapon;
        }
    }
}