using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using OpenScripts2;
using static RootMotion.FinalIK.IKSolver;

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

                    // Receiver Skin UI
                    if (ModularWeapon.GetModularFVRFireArm.ReceiverSkinUIPointProxy != null)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.GetModularFVRFireArm.ReceiverSkinUIPointProxy.position, ModularWeapon.GetModularFVRFireArm.ReceiverSkinUIPointProxy.rotation, ModularWeapon.GetModularFVRFireArm.ReceiverSkinUIPointProxy.parent);
                        UIObject.transform.localScale = ModularWeapon.GetModularFVRFireArm.ReceiverSkinUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.ModularWeapon = ModularWeapon;

                        UI.SetupReceiverSkinOnlyMode();
                        UI.UpdateDisplay();
                    }

                    // Modular Barrel UI
                    if (ModularWeapon.ModularBarrelPartsID != string.Empty)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.ModularBarrelUIPointProxy.position, ModularWeapon.ModularBarrelUIPointProxy.rotation, ModularWeapon.ModularBarrelUIPointProxy.parent);
                        UIObject.transform.localScale = ModularWeapon.ModularBarrelUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Barrel;
                        UI.ModularWeapon = ModularWeapon;
                        UI.ModularPartsGroupID = ModularWeapon.ModularBarrelPartsID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    // Modular Handguard UI
                    if (ModularWeapon.ModularHandguardPartsID != string.Empty)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.ModularHandguardUIPointProxy.position, ModularWeapon.ModularHandguardUIPointProxy.rotation, ModularWeapon.ModularHandguardUIPointProxy.parent);
                        UIObject.transform.localScale = ModularWeapon.ModularHandguardUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Handguard;
                        UI.ModularWeapon = ModularWeapon;
                        UI.ModularPartsGroupID = ModularWeapon.ModularHandguardPartsID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    // Modular Stock UI
                    if (ModularWeapon.ModularStockPartsID != string.Empty)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.ModularStockUIPointProxy.position, ModularWeapon.ModularStockUIPointProxy.rotation, ModularWeapon.ModularStockUIPointProxy.parent);
                        UIObject.transform.localScale = ModularWeapon.ModularStockUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Stock;
                        UI.ModularWeapon = ModularWeapon;
                        UI.ModularPartsGroupID = ModularWeapon.ModularStockPartsID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    // Modular Parts Points UI
                    foreach (var point in ModularWeapon.ModularWeaponPartsAttachmentPoints)
                    {
                        UIObject = Instantiate(UIPrefab, point.ModularPartUIPointProxy.position, point.ModularPartUIPointProxy.rotation, point.ModularPartUIPointProxy.parent);
                        UIObject.transform.localScale = point.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.MainWeaponGeneralAttachmentPoint;
                        UI.ModularWeapon = ModularWeapon;
                        UI.ModularPartsGroupID = point.ModularPartsGroupID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    // Sub Attachment Points UI
                    foreach (var subPoint in ModularWeapon.SubAttachmentPoints)
                    {
                        UIObject = Instantiate(UIPrefab, subPoint.ModularPartUIPointProxy.position, subPoint.ModularPartUIPointProxy.rotation, subPoint.ModularPartUIPointProxy.parent);
                        UIObject.transform.localScale = subPoint.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                        _UIScreens.Add(UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.SubAttachmentPoint;
                        UI.ModularWeapon = ModularWeapon;
                        UI.ModularPartsGroupID = subPoint.ModularPartsGroupID;

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
            UIObject = Instantiate(UIPrefab, point.ModularPartUIPointProxy.position, point.ModularPartUIPointProxy.rotation, point.ModularPartUIPointProxy.parent);
            UIObject.transform.localScale = point.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
            _subUIScreens.Add(UIObject);
            UI = UIObject.GetComponent<ModularWorkshopUI>();
            UI.PartType = ModularWorkshopUI.EPartType.SubAttachmentPoint;
            UI.ModularPartsGroupID = point.ModularPartsGroupID;
            UI.ModularWeapon = ModularWeapon;

            UI.InitializeArrays();
            UI.UpdateDisplay();
        }
    }
}