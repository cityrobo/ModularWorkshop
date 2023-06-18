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
        [HideInInspector]
        public ReceiverSkinSystem SkinSystem;

        public Transform AlignmentDirection;

        private IModularWeapon _lastModularWeapon;
        private ReceiverSkinSystem _lastSkinSystem;

        private readonly Dictionary<string, GameObject> _UIScreens = new();

        public void Update()
        {
            if (QuickBeltSlot.HeldObject != null)
            {
                ModularWeapon = QuickBeltSlot.HeldObject as IModularWeapon;
                SkinSystem = QuickBeltSlot.HeldObject.GetComponent<ReceiverSkinSystem>();

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
                        _UIScreens.Add(ModularWeapon.GetModularFVRFireArm.SkinPath, UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.ModularWeapon = ModularWeapon;

                        UI.SetupReceiverSkinOnlyMode();
                        UI.UpdateDisplay();
                    }

                    // Modular Barrel UI
                    if (ModularWeapon.ModularBarrelPartsID != string.Empty && !ModularWeapon.AllAttachmentPoints[ModularWeapon.ModularBarrelPartsID].IsPointDisabled)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.ModularBarrelUIPointProxy.position, ModularWeapon.ModularBarrelUIPointProxy.rotation, ModularWeapon.ModularBarrelUIPointProxy.parent);
                        UIObject.transform.localScale = ModularWeapon.ModularBarrelUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                        _UIScreens.Add(ModularWeapon.ModularBarrelPartsID, UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Barrel;
                        UI.ModularWeapon = ModularWeapon;
                        UI.ModularPartsGroupID = ModularWeapon.ModularBarrelPartsID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    // Modular Handguard UI
                    if (ModularWeapon.ModularHandguardPartsID != string.Empty && !ModularWeapon.AllAttachmentPoints[ModularWeapon.ModularHandguardPartsID].IsPointDisabled)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.ModularHandguardUIPointProxy.position, ModularWeapon.ModularHandguardUIPointProxy.rotation, ModularWeapon.ModularHandguardUIPointProxy.parent);
                        UIObject.transform.localScale = ModularWeapon.ModularHandguardUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                        _UIScreens.Add(ModularWeapon.ModularHandguardPartsID, UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.PartType = ModularWorkshopUI.EPartType.Handguard;
                        UI.ModularWeapon = ModularWeapon;
                        UI.ModularPartsGroupID = ModularWeapon.ModularHandguardPartsID;

                        UI.InitializeArrays();
                        UI.UpdateDisplay();
                    }
                    // Modular Stock UI
                    if (ModularWeapon.ModularStockPartsID != string.Empty && !ModularWeapon.AllAttachmentPoints[ModularWeapon.ModularStockPartsID].IsPointDisabled)
                    {
                        UIObject = Instantiate(UIPrefab, ModularWeapon.ModularStockUIPointProxy.position, ModularWeapon.ModularStockUIPointProxy.rotation, ModularWeapon.ModularStockUIPointProxy.parent);
                        UIObject.transform.localScale = ModularWeapon.ModularStockUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                        _UIScreens.Add(ModularWeapon.ModularStockPartsID, UIObject);
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
                        if (!point.IsPointDisabled)
                        {
                            UIObject = Instantiate(UIPrefab, point.ModularPartUIPointProxy.position, point.ModularPartUIPointProxy.rotation, point.ModularPartUIPointProxy.parent);
                            UIObject.transform.localScale = point.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(point.ModularPartsGroupID, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.PartType = ModularWorkshopUI.EPartType.MainWeaponGeneralAttachmentPoint;
                            UI.ModularWeapon = ModularWeapon;
                            UI.ModularPartsGroupID = point.ModularPartsGroupID;

                            UI.InitializeArrays();
                            UI.UpdateDisplay();
                        }
                    }
                    // Sub Attachment Points UI
                    foreach (var subPoint in ModularWeapon.SubAttachmentPoints)
                    {
                        if (!subPoint.IsPointDisabled)
                        {
                            UIObject = Instantiate(UIPrefab, subPoint.ModularPartUIPointProxy.position, subPoint.ModularPartUIPointProxy.rotation, subPoint.ModularPartUIPointProxy.parent);
                            UIObject.transform.localScale = subPoint.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(subPoint.ModularPartsGroupID, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.PartType = ModularWorkshopUI.EPartType.SubAttachmentPoint;
                            UI.ModularWeapon = ModularWeapon;
                            UI.ModularPartsGroupID = subPoint.ModularPartsGroupID;

                            UI.InitializeArrays();
                            UI.UpdateDisplay();
                        }
                    }
                    ModularWeapon.WorkshopPlatform = this;
                }
                else if (SkinSystem != null && _lastSkinSystem != SkinSystem)
                {
                    GameObject UIPrefab = SkinSystem.UIPrefab;
                    GameObject UIObject;
                    ModularWorkshopUI UI;

                    // Receiver Skin System UI
                    if (SkinSystem.ReceiverSkinUIPointProxy != null)
                    {
                        UIObject = Instantiate(UIPrefab, SkinSystem.ReceiverSkinUIPointProxy.position, SkinSystem.ReceiverSkinUIPointProxy.rotation, SkinSystem.ReceiverSkinUIPointProxy.parent);
                        UIObject.transform.localScale = SkinSystem.ReceiverSkinUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                        _UIScreens.Add(SkinSystem.SkinPath, UIObject);
                        UI = UIObject.GetComponent<ModularWorkshopUI>();
                        UI.SkinSystem = SkinSystem;

                        UI.SetupReceiverSkinOnlyMode(true);
                        UI.UpdateDisplay();
                    }
                }
            }
            else if (QuickBeltSlot.HeldObject == null && _lastModularWeapon != null)
            {
                for (int i = 0; i < _UIScreens.Count; i++)
                {
                    if (_UIScreens.ElementAt(i).Value != null) Destroy(_UIScreens.ElementAt(i).Value);
                }

                _UIScreens.Clear();
                _lastModularWeapon = null;
                ModularWeapon.WorkshopPlatform = null;
                ModularWeapon = null;
            }
            else if (QuickBeltSlot.HeldObject == null && _lastSkinSystem != null)
            {
                for (int i = 0; i < _UIScreens.Count; i++)
                {
                    if (_UIScreens.ElementAt(i).Value != null) Destroy(_UIScreens.ElementAt(i).Value);
                }

                _UIScreens.Clear();
                _lastSkinSystem = null;
                SkinSystem = null;
            }

            _lastModularWeapon = ModularWeapon;
            _lastSkinSystem = SkinSystem;

            if (QuickBeltSlot.CurObject != null && AlignmentDirection != null) AlignQBSlot();
        }

        private void AlignQBSlot()
        {
            Quaternion newRot;
            if (QuickBeltSlot.CurObject.QBPoseOverride != null)
            {
                newRot = AlignmentDirection.rotation * QuickBeltSlot.CurObject.QBPoseOverride.localRotation;
            }
            else
            {
                newRot = AlignmentDirection.rotation;
            }

            if ( newRot != QuickBeltSlot.PoseOverride.rotation) QuickBeltSlot.PoseOverride.rotation = newRot;
        }

        public void CreateUIForPoint(ModularWeaponPartsAttachmentPoint point, ModularWorkshopUI.EPartType partType = ModularWorkshopUI.EPartType.SubAttachmentPoint)
        {
            if (!_UIScreens.ContainsKey(point.ModularPartsGroupID) && !point.IsPointDisabled)
            {
                GameObject UIPrefab = ModularWeapon.UIPrefab;
                GameObject UIObject;
                ModularWorkshopUI UI;
                UIObject = Instantiate(UIPrefab, point.ModularPartUIPointProxy.position, point.ModularPartUIPointProxy.rotation, point.ModularPartUIPointProxy.parent);
                UIObject.transform.localScale = point.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                _UIScreens.Add(point.ModularPartsGroupID, UIObject);
                UI = UIObject.GetComponent<ModularWorkshopUI>();
                UI.PartType = partType;
                UI.ModularPartsGroupID = point.ModularPartsGroupID;
                UI.ModularWeapon = ModularWeapon;

                UI.InitializeArrays();
                UI.UpdateDisplay();
            }
        }

        public void RemoveUIFromPoint(ModularWeaponPartsAttachmentPoint point)
        {
            if (_UIScreens.TryGetValue(point.ModularPartsGroupID, out GameObject screen))
            {
                Destroy(screen);
                _UIScreens.Remove(point.ModularPartsGroupID);
            }
        }
    }
}