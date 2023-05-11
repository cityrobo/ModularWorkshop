using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class InteractionVisualEffectAddon : MonoBehaviour , IPartFireArmRequirement
    {
        [Header("This list is optional, you can also place the Heating effects scripts on this gameobject instead. But please not both!")]
        public InteractionVisualEffect[] InteractionVisualEffects;
        public FVRFireArm FireArm
        { 
            set 
            {
                foreach (var HeatingEffect in InteractionVisualEffects)
                {
                    HeatingEffect.ObjectToMonitor = value;
                }

                foreach (var HeatingEffect in GetComponents<InteractionVisualEffect>())
                {
                    HeatingEffect.ObjectToMonitor = value;
                }
            } 
        }
    }
}
