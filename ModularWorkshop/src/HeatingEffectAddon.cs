using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    /// <summary>
    /// This script is an addon that can be attached to a modular weapon part to provide an interface for heating effects on a modular workshop firearm.
    /// </summary>

    [RequireComponent(typeof(ModularWeaponPart))]
    public class HeatingEffectAddon : MonoBehaviour, IPartFireArmRequirement
    {
        [Header("This list is optional, you can also place the Heating effects scripts on this game object instead. But please not both!")]
        public FirearmHeatingEffect[] FirearmHeatingEffects;

        // The FireArm property allows setting the firearm associated with the heating effects.
        public FVRFireArm FireArm
        {
            set
            {
                // Loop through the specified FirearmHeatingEffects and configure them.
                foreach (var HeatingEffect in FirearmHeatingEffects)
                {
                    HeatingEffect.FireArm = value;
                    HeatingEffect.Reload();
                }

                // Also, configure any FirearmHeatingEffects attached to this game object.
                foreach (var HeatingEffect in GetComponents<FirearmHeatingEffect>())
                {
                    HeatingEffect.FireArm = value;
                    HeatingEffect.Reload();
                }
            }
        }
    }
}
