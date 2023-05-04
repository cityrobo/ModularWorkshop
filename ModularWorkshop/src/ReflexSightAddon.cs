using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class ReflexSightAddon : MonoBehaviour , IPartFireArmRequirement
    {
        public CustomReflexSightInterface ReflexSightInterface;

        public FVRFireArm FireArm
        { 
            set 
            {
                ReflexSightInterface.FireArm = value;
                ReflexSightInterface.IsIntegrated = true;
                ReflexSightInterface.ForceInteractable = true;
                ReflexSightInterface.OffsetReticle();
            } 
        }
    }
}
