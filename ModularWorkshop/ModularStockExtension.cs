using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularStockExtension : ModularWeaponPart
    {
        public bool ModifiesStockBehavior;
        public bool ActsLikeStock = true;
        public Transform StockPoint;

        public bool ChangesPosePosition = false;
        public Transform CustomPoseOverride;
        [HideInInspector]
        public TransformProxy CustomPoseOverrideProxy;

        public Transform CustomPoseOverride_Touch;
        [HideInInspector]
        public TransformProxy CustomPoseOverride_TouchProxy;

        private ModularStock _stock;
        private FVRFireArm _firearm;

        public override void Awake()
        {
            base.Awake();

            Transform current = transform;
            do
            {
                if (_stock == null) _stock = current.GetComponentInChildren<ModularStock>();
                if (_firearm == null) _firearm = current.GetComponentInChildren<FVRFireArm>();
                current = current.parent;
            } while ((_stock == null || _firearm == null) && current != null);

            if (_firearm != null && _stock != null)
            {
                if (CustomPoseOverride != null)
                {
                    CustomPoseOverrideProxy = new(CustomPoseOverride);
                    Destroy(CustomPoseOverride.gameObject);
                }
                if (CustomPoseOverride_Touch != null)
                {
                    CustomPoseOverride_TouchProxy = new(CustomPoseOverride_Touch);
                    Destroy(CustomPoseOverride_Touch.gameObject);
                }

                if (ModifiesStockBehavior)
                {
                    _firearm.HasActiveShoulderStock = ActsLikeStock;
                    _firearm.StockPos = StockPoint;
                }
                if (ChangesPosePosition)
                {
                    FVRViveHand hand = GM.CurrentMovementManager.Hands[0];
                    if (hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index)
                    {
                        _firearm.PoseOverride.GoToTransformProxy(CustomPoseOverride_TouchProxy);
                        _firearm.PoseOverride_Touch.GoToTransformProxy(CustomPoseOverride_TouchProxy);
                        _firearm.m_storedLocalPoseOverrideRot = _firearm.PoseOverride.localRotation;
                    }
                    else
                    {
                        _firearm.PoseOverride.GoToTransformProxy(CustomPoseOverrideProxy);
                        _firearm.m_storedLocalPoseOverrideRot = _firearm.PoseOverride.localRotation;
                    }
                }
            }
            else 
            {
                if (_firearm == null) OpenScripts2_BepInExPlugin.LogWarning(this, "Firearm not found! ModularStockExtension disabled!");
                if (_stock == null) OpenScripts2_BepInExPlugin.LogWarning(this, "ModularStock not found! ModularStockExtension disabled!");
            }
        }

        public override void OnDestroy()
        {
            if (_firearm != null && _stock != null)
            {
                ModularFVRFireArm modularFireArm = _firearm.GetComponent<IModularWeapon>().GetModularFVRFireArm;

                _firearm.HasActiveShoulderStock = _stock.ActsLikeStock;
                _firearm.StockPos = _stock.StockPoint;

                if (_stock.ChangesPosePosition)
                {
                    FVRViveHand hand = GM.CurrentMovementManager.Hands[0];
                    if (hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index)
                    {
                        _firearm.PoseOverride.GoToTransformProxy(_stock.CustomPoseOverride_TouchProxy);
                        _firearm.PoseOverride_Touch.GoToTransformProxy(_stock.CustomPoseOverride_TouchProxy);
                        _firearm.m_storedLocalPoseOverrideRot = _firearm.PoseOverride.localRotation;
                    }
                    else
                    {
                        _firearm.PoseOverride.GoToTransformProxy(_stock.CustomPoseOverrideProxy);
                        _firearm.m_storedLocalPoseOverrideRot = _firearm.PoseOverride.localRotation;
                    }
                }
                else
                {
                    FVRViveHand hand = GM.CurrentMovementManager.Hands[0];
                    if (hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index)
                    {
                        _firearm.PoseOverride.GoToTransformProxy(modularFireArm.OrigPoseOverride_Touch);
                        _firearm.PoseOverride_Touch.GoToTransformProxy(modularFireArm.OrigPoseOverride_Touch);
                        _firearm.m_storedLocalPoseOverrideRot = _firearm.PoseOverride.localRotation;
                    }
                    else
                    {
                        _firearm.PoseOverride.GoToTransformProxy(modularFireArm.OrigPoseOverride);
                        _firearm.m_storedLocalPoseOverrideRot = _firearm.PoseOverride.localRotation;
                    }
                }
            }

            base.OnDestroy();
        }
    }
}