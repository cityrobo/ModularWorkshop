using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class HeatingEffectAddon : MonoBehaviour , IPartFireArmRequirement
    {
        [Header("This list is optional, you can also place the Heating effects scripts on this gameobject instead. But please not both!")]
        public FirearmHeatingEffect[] FirearmHeatingEffects;
        public FVRFireArm FireArm
        { 
            set 
            {
                foreach (var HeatingEffect in FirearmHeatingEffects)
                {
                    HeatingEffect.FireArm = value;
                    HeatingEffect.Reload();
                }

                foreach (var HeatingEffect in GetComponents<FirearmHeatingEffect>())
                {
                    HeatingEffect.FireArm = value;
                    HeatingEffect.Reload();
                }
            } 
        }
    }
}
