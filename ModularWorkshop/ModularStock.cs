using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularStock : ModularWeaponPart
    {
        public bool ActsLikeStock = true;
        public Transform StockPoint;
        public ShotgunMoveableStock CollapsingStock;
        public FVRFoldingStockXAxis FoldingStockX;
        public FVRFoldingStockYAxis FoldingStockY;

        public bool ChangesPosePosition = false;
        public Transform CustomPoseOverride;
        [HideInInspector]
        public TransformProxy CustomPoseOverrideProxy;

        public Transform CustomPoseOverride_Touch;
        [HideInInspector]
        public TransformProxy CustomPoseOverride_TouchProxy;

        public override void Awake()
        {
            base.Awake();

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
        }
    }
}