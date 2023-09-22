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
        [Tooltip("This list is optional, you can also place the Interaction Visual Effect scripts on this gameobject instead.")]
        public InteractionVisualEffect[] InteractionVisualEffects;
        public FVRFireArm FireArm
        { 
            set
            {
                if (value != null)
                {
                    foreach (var VisualEffect in InteractionVisualEffects)
                    {
                        VisualEffect.ObjectToMonitor = value;
                    }

                    foreach (var VisualEffect in GetComponents<InteractionVisualEffect>())
                    {
                        VisualEffect.ObjectToMonitor = value;
                    }
                }
            } 
        }
    }
}