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
        [HideInInspector]
        public ReceiverSkinSystem SkinSystem;
        [HideInInspector]
        public ModularFVRPhysicalObject ModularPhysicalObject;

        public Transform AlignmentDirection;

        private IModularWeapon _lastModularWeapon;
        private ReceiverSkinSystem _lastSkinSystem;
        private ModularFVRPhysicalObject _lastModularPhysicalObject;

        private readonly Dictionary<string, GameObject> _UIScreens = new();

        public void Update()
        {
            if (QuickBeltSlot.HeldObject != null)
            {
                ModularWeapon = QuickBeltSlot.HeldObject as IModularWeapon;
                SkinSystem = QuickBeltSlot.HeldObject.GetComponentInChildren<ReceiverSkinSystem>();
                ModularPhysicalObject = QuickBeltSlot.HeldObject.GetComponentInChildren<ModularFVRPhysicalObject>();

                if (ModularWeapon != null && _lastModularWeapon != ModularWeapon)
                {
                    GameObject UIPrefab = ModularWeapon.UIPrefab;
                    GameObject UIObject;
                    ModularWorkshopUI UI;

                    // Receiver Skin UI
                    if (ModularWeapon.GetModularFVRFireArm.ReceiverSkinUIPointProxy != null)
                    {
                        var skinPath = ModularWeapon.GetModularFVRFireArm.SkinPath;
                        if (!_UIScreens.ContainsKey(skinPath))
                        {
                            UIObject = Instantiate(UIPrefab, ModularWeapon.GetModularFVRFireArm.ReceiverSkinUIPointProxy.position, ModularWeapon.GetModularFVRFireArm.ReceiverSkinUIPointProxy.rotation, ModularWeapon.GetModularFVRFireArm.ReceiverSkinUIPointProxy.parent);
                            UIObject.transform.localScale = ModularWeapon.GetModularFVRFireArm.ReceiverSkinUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(skinPath, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.ModularWeapon = ModularWeapon;

                            UI.SetupReceiverSkinOnlyMode();
                            UI.UpdateDisplay();
                        }
                    }
                    // Modular Barrel UI
                    if (ModularWeapon.ModularBarrelPartsID != string.Empty && !ModularWeapon.AllAttachmentPoints[ModularWeapon.ModularBarrelPartsID].IsPointDisabled)
                    {
                        var barrelId = ModularWeapon.ModularBarrelPartsID;
                        if (!_UIScreens.ContainsKey(barrelId))
                        {
                            UIObject = Instantiate(UIPrefab, ModularWeapon.ModularBarrelUIPointProxy.position, ModularWeapon.ModularBarrelUIPointProxy.rotation, ModularWeapon.ModularBarrelUIPointProxy.parent);
                            UIObject.transform.localScale = ModularWeapon.ModularBarrelUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(barrelId, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.PartType = ModularWorkshopUI.EPartType.Barrel;
                            UI.ModularWeapon = ModularWeapon;
                            UI.ModularPartsGroupID = barrelId;

                            UI.InitializeArrays();
                            UI.UpdateDisplay();
                        }
                    }
                    // Modular Handguard UI
                    if (ModularWeapon.ModularHandguardPartsID != string.Empty && !ModularWeapon.AllAttachmentPoints[ModularWeapon.ModularHandguardPartsID].IsPointDisabled)
                    {
                        var handguardId = ModularWeapon.ModularHandguardPartsID;
                        if (!_UIScreens.ContainsKey(handguardId))
                        {
                            UIObject = Instantiate(UIPrefab, ModularWeapon.ModularHandguardUIPointProxy.position, ModularWeapon.ModularHandguardUIPointProxy.rotation, ModularWeapon.ModularHandguardUIPointProxy.parent);
                            UIObject.transform.localScale = ModularWeapon.ModularHandguardUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(handguardId, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.PartType = ModularWorkshopUI.EPartType.Handguard;
                            UI.ModularWeapon = ModularWeapon;
                            UI.ModularPartsGroupID = handguardId;

                            UI.InitializeArrays();
                            UI.UpdateDisplay();
                        }
                    }
                    // Modular Stock UI
                    if (ModularWeapon.ModularStockPartsID != string.Empty && !ModularWeapon.AllAttachmentPoints[ModularWeapon.ModularStockPartsID].IsPointDisabled)
                    {
                        var stockId = ModularWeapon.ModularStockPartsID;
                        if (!_UIScreens.ContainsKey(stockId))
                        {
                            UIObject = Instantiate(UIPrefab, ModularWeapon.ModularStockUIPointProxy.position, ModularWeapon.ModularStockUIPointProxy.rotation, ModularWeapon.ModularStockUIPointProxy.parent);
                            UIObject.transform.localScale = ModularWeapon.ModularStockUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(stockId, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.PartType = ModularWorkshopUI.EPartType.Stock;
                            UI.ModularWeapon = ModularWeapon;
                            UI.ModularPartsGroupID = stockId;

                            UI.InitializeArrays();
                            UI.UpdateDisplay();
                        }
                    }
                    // Modular Parts Points UI
                    foreach (var point in ModularWeapon.ModularWeaponPartsAttachmentPoints)
                    {
                        var groupId = point.ModularPartsGroupID;
                        if (!point.IsPointDisabled && !_UIScreens.ContainsKey(groupId))
                        {
                            UIObject = Instantiate(UIPrefab, point.ModularPartUIPointProxy.position, point.ModularPartUIPointProxy.rotation, point.ModularPartUIPointProxy.parent);
                            UIObject.transform.localScale = point.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(groupId, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.PartType = ModularWorkshopUI.EPartType.MainWeaponGeneralAttachmentPoint;
                            UI.ModularWeapon = ModularWeapon;
                            UI.ModularPartsGroupID = groupId;

                            UI.InitializeArrays();
                            UI.UpdateDisplay();
                        }
                    }
                    // Sub Attachment Points UI
                    foreach (var subPoint in ModularWeapon.SubAttachmentPoints)
                    {
                        var groupId = subPoint.ModularPartsGroupID;
                        if (!subPoint.IsPointDisabled && !_UIScreens.ContainsKey(groupId))
                        {
                            UIObject = Instantiate(UIPrefab, subPoint.ModularPartUIPointProxy.position, subPoint.ModularPartUIPointProxy.rotation, subPoint.ModularPartUIPointProxy.parent);
                            UIObject.transform.localScale = subPoint.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(groupId, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.PartType = ModularWorkshopUI.EPartType.SubAttachmentPoint;
                            UI.ModularWeapon = ModularWeapon;
                            UI.ModularPartsGroupID = groupId;

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
                        var skinPath = SkinSystem.SkinPath;
                        if (!_UIScreens.ContainsKey(skinPath))
                        {
                            UIObject = Instantiate(UIPrefab, SkinSystem.ReceiverSkinUIPointProxy.position, SkinSystem.ReceiverSkinUIPointProxy.rotation, SkinSystem.ReceiverSkinUIPointProxy.parent);
                            UIObject.transform.localScale = SkinSystem.ReceiverSkinUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(skinPath, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.SkinSystem = SkinSystem;

                            UI.SetupReceiverSkinOnlyMode(true);
                            UI.UpdateDisplay();
                        }
                    }
                }
                else if (ModularPhysicalObject != null && _lastModularPhysicalObject != ModularPhysicalObject)
                {
                    GameObject UIPrefab = ModularPhysicalObject.UIPrefab;
                    GameObject UIObject;
                    ModularWorkshopUI UI;

                    // Receiver Skin UI
                    if (ModularPhysicalObject.ReceiverSkinUIPointProxy != null)
                    {
                        var skinPath = ModularPhysicalObject.SkinPath;
                        if (!_UIScreens.ContainsKey(skinPath))
                        {
                            UIObject = Instantiate(UIPrefab, ModularPhysicalObject.ReceiverSkinUIPointProxy.position, ModularPhysicalObject.ReceiverSkinUIPointProxy.rotation, ModularPhysicalObject.ReceiverSkinUIPointProxy.parent);
                            UIObject.transform.localScale = ModularPhysicalObject.ReceiverSkinUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(skinPath, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.ModularPhysicalObject = ModularPhysicalObject;

                            UI.SetupReceiverSkinOnlyMode();
                            UI.UpdateDisplay();
                        }
                    }
                    // Modular Parts Points UI
                    foreach (var point in ModularPhysicalObject.ModularWeaponPartsAttachmentPoints)
                    {
                        var groupId = point.ModularPartsGroupID;
                        if (!point.IsPointDisabled && !_UIScreens.ContainsKey(groupId))
                        {
                            UIObject = Instantiate(UIPrefab, point.ModularPartUIPointProxy.position, point.ModularPartUIPointProxy.rotation, point.ModularPartUIPointProxy.parent);
                            UIObject.transform.localScale = point.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(groupId, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.PartType = ModularWorkshopUI.EPartType.MainWeaponGeneralAttachmentPoint;
                            UI.ModularPhysicalObject = ModularPhysicalObject;
                            UI.ModularPartsGroupID = groupId;

                            UI.InitializeArrays();
                            UI.UpdateDisplay();
                        }
                    }
                    // Sub Attachment Points UI
                    foreach (var subPoint in ModularPhysicalObject.SubAttachmentPoints)
                    {
                        var groupId = subPoint.ModularPartsGroupID;
                        if (!subPoint.IsPointDisabled && !_UIScreens.ContainsKey(groupId))
                        {
                            UIObject = Instantiate(UIPrefab, subPoint.ModularPartUIPointProxy.position, subPoint.ModularPartUIPointProxy.rotation, subPoint.ModularPartUIPointProxy.parent);
                            UIObject.transform.localScale = subPoint.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                            _UIScreens.Add(groupId, UIObject);
                            UI = UIObject.GetComponent<ModularWorkshopUI>();
                            UI.PartType = ModularWorkshopUI.EPartType.SubAttachmentPoint;
                            UI.ModularPhysicalObject = ModularPhysicalObject;
                            UI.ModularPartsGroupID = groupId;

                            UI.InitializeArrays();
                            UI.UpdateDisplay();
                        }
                    }
                    ModularPhysicalObject.WorkshopPlatform = this;
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
            else if (QuickBeltSlot.HeldObject == null && _lastModularPhysicalObject != null)
            {
                for (int i = 0; i < _UIScreens.Count; i++)
                {
                    if (_UIScreens.ElementAt(i).Value != null) Destroy(_UIScreens.ElementAt(i).Value);
                }

                _UIScreens.Clear();
                _lastModularPhysicalObject = null;
                ModularPhysicalObject.WorkshopPlatform = null;
                ModularPhysicalObject = null;
            }

            _lastModularWeapon = ModularWeapon;
            _lastSkinSystem = SkinSystem;
            _lastModularPhysicalObject = ModularPhysicalObject;

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

            if (newRot != QuickBeltSlot.PoseOverride.rotation) QuickBeltSlot.PoseOverride.rotation = newRot;
        }

        public void CreateUIForPoint(ModularWeaponPartsAttachmentPoint point, ModularWorkshopUI.EPartType partType = ModularWorkshopUI.EPartType.SubAttachmentPoint)
        {
            if (!_UIScreens.ContainsKey(point.ModularPartsGroupID) && !point.IsPointDisabled)
            {
                GameObject UIPrefab = null;
                if (ModularWeapon != null) UIPrefab = ModularWeapon.UIPrefab;
                else if (ModularPhysicalObject != null) UIPrefab = ModularPhysicalObject.UIPrefab;
                GameObject UIObject;
                ModularWorkshopUI UI;
                UIObject = Instantiate(UIPrefab, point.ModularPartUIPointProxy.position, point.ModularPartUIPointProxy.rotation, point.ModularPartUIPointProxy.parent);
                UIObject.transform.localScale = point.ModularPartUIPointProxy.localScale.MultiplyComponentWise(UIObject.transform.localScale);
                _UIScreens.Add(point.ModularPartsGroupID, UIObject);
                UI = UIObject.GetComponent<ModularWorkshopUI>();
                UI.PartType = partType;
                UI.ModularPartsGroupID = point.ModularPartsGroupID;
                UI.ModularWeapon = ModularWeapon;
                UI.ModularPhysicalObject = ModularPhysicalObject;

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