using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularStockExtension : ModularWeaponPart
    {
        [Header("Stock Behavior")]
        public bool ModifiesStockBehavior;
        public bool ActsLikeStock = true;
        public Transform StockPoint;
        [Header("Pose Position")]
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

            if (CustomPoseOverride != null)
            {
                CustomPoseOverrideProxy = new(CustomPoseOverride, true);
            }
            if (CustomPoseOverride_Touch != null)
            {
                CustomPoseOverride_TouchProxy = new(CustomPoseOverride_Touch, true);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void EnablePart()
        {
            base.EnablePart();

            if (transform.TryGetComponentInParent(out _firearm) && _firearm.TryGetComponentInChildren(out _stock))
            {
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

        public override void DisablePart()
        {
            base.DisablePart();

            if (transform.TryGetComponentInParent(out _firearm) && _firearm.TryGetComponentInChildren(out _stock))
            {
                ModularFVRFireArm modularFireArm = _firearm.GetComponent<IModularWeapon>().GetModularFVRFireArm;
                if (ModifiesStockBehavior)
                {
                    _firearm.HasActiveShoulderStock = _stock.ActsLikeStock;
                    _firearm.StockPos = _stock.StockPoint;
                }

                if (ChangesPosePosition)
                {
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
            }
            else
            {
                if (_firearm == null) OpenScripts2_BepInExPlugin.LogWarning(this, "Firearm not found! ModularStockExtension disabled!");
                if (_stock == null) OpenScripts2_BepInExPlugin.LogWarning(this, "ModularStock not found! ModularStockExtension disabled!");
            }
        }
    }
}