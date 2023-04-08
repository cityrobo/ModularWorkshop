using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    [RequireComponent(typeof(FirearmHeatingEffect))]
    public class HeatingEffectAddon : MonoBehaviour , IPartFireArmRequirement
    {
        public FVRFireArm FireArm 
        { 
            set 
            {
                foreach (var HeatingEffect in GetComponents<FirearmHeatingEffect>())
                {
                    HeatingEffect.FireArm = value;
                    HeatingEffect.Reload();
                }
            } 
        }
    }
}
