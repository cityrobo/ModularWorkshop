using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using System.Linq;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class AltGripAddon : MonoBehaviour , IPartFireArmRequirement
    {
        public FVRAlternateGrip AltGrip;
        private FVRFireArm _firearm;
        private GameObject _origForeGrip;

        public FVRFireArm FireArm
        { 
            set 
            {
                if (value != null)
                {
                    _firearm = value;
                    AltGrip.PrimaryObject = _firearm;
                    _origForeGrip = _firearm.Foregrip;
                    _firearm.Foregrip = AltGrip.gameObject;
                }
                else
                {
                    _firearm.Foregrip = _origForeGrip;
                }
            } 
        }
    }
}