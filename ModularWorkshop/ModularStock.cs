using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace ModularWorkshop
{
    public class ModularStock : ModularWeaponPart
    {
        public bool ActsLikeStock = true;
        public Transform StockPoint;
        public ShotgunMoveableStock CollapsingStock;
        public FVRFoldingStockXAxis FoldingStockX;
        public FVRFoldingStockYAxis FoldingStockY;
    }
}