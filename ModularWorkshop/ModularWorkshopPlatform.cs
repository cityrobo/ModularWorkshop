using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularWorkshopPlatform : MonoBehaviour
    {
        public FVRQuickBeltSlot QuickBeltSlot;
        [HideInInspector]
        public IModularWeapon ModularWeapon;

        private IModularWeapon _lastModularWeapon;

        private List<GameObject> _UIScreens = new();
        private List<GameObject> _subUIScreens = new();

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
                    if (ModularWeapon.ModularBarrelPartID != string.Empty)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.ModularBarrelUIPosition.position, ModularWeapon.ModularBarrelUIPosition.rotation, ModularWeapon.ModularBarrelUIPosition.parent);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Barrel;
                        UI.ModularWeapon = ModularWeapon;
                        UI.PartID = ModularWeapon.ModularBarrelPartID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    if (ModularWeapon.ModularHandguardPartID != string.Empty)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.ModularHandguardUIPosition.position, ModularWeapon.ModularHandguardUIPosition.rotation, ModularWeapon.ModularHandguardUIPosition.parent);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Handguard;
                        UI.ModularWeapon = ModularWeapon;
                        UI.PartID = ModularWeapon.ModularHandguardPartID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    if (ModularWeapon.ModularStockPartID != string.Empty)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.ModularStockUIPosition.position, ModularWeapon.ModularStockUIPosition.rotation, ModularWeapon.ModularStockUIPosition.parent);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Stock;
                        UI.ModularWeapon = ModularWeapon;
                        UI.PartID = ModularWeapon.ModularStockPartID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }

                    foreach (var point in ModularWeapon.ModularWeaponPartsAttachmentPoints)
                    {
                        UIObject = Instantiate(UIPrefab, point.ModularPartUIPos.position, point.ModularPartUIPos.rotation, point.ModularPartUIPos.parent);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.MainWeaponGeneralAttachmentPoint;
                        UI.ModularWeapon = ModularWeapon;
                        UI.PartID = point.PartID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }

                    foreach (var subPoint in ModularWeapon.SubAttachmentPoints)
                    {
                        UIObject = Instantiate(UIPrefab, subPoint.ModularPartUIPos.position, subPoint.ModularPartUIPos.rotation, subPoint.ModularPartUIPos.parent);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.SubAttachmentPoint;
                        UI.ModularWeapon = ModularWeapon;
                        UI.PartID = subPoint.PartID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    ModularWeapon.WorkshopPlatform = this;
                }
            }
            else if (QuickBeltSlot.HeldObject == null && _lastModularWeapon != null)
            {
                for (int i = 0; i < _UIScreens.Count; i++)
                {
                    Destroy(_UIScreens[i]);
                }
                for (int i = 0; i < _subUIScreens.Count; i++)
                {
                    if (_subUIScreens[i] != null) Destroy(_subUIScreens[i]);
                }

                _UIScreens.Clear();
                _lastModularWeapon = null;
                ModularWeapon.WorkshopPlatform = null;
                ModularWeapon = null;
            }

            _lastModularWeapon = ModularWeapon;
        }

        public void CreateUIForPoint(ModularWeaponPartsAttachmentPoint point)
        {
            GameObject UIPrefab = ModularWeapon.UIPrefab;
            GameObject UIObject;
            ModularWorkshopUI UI;
            UIObject = Instantiate(UIPrefab, point.ModularPartUIPos.position, point.ModularPartUIPos.rotation, point.ModularPartUIPos.parent);
            _subUIScreens.Add(UIObject);
            UI = UIObject.GetComponent<ModularWorkshopUI>();
            UI.PartType = ModularWorkshopUI.EPartType.SubAttachmentPoint;
            UI.PartID = point.PartID;
            UI.ModularWeapon = ModularWeapon;

            UI.InitializeArrays();
            UI.UpdateDisplay();
        }
    }
}