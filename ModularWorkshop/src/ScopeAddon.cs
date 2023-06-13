using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class ScopeAddon : MonoBehaviour , IPartFireArmRequirement
    {
        public CustomScopeInterface ScopeInterface;

        public FVRFireArm FireArm
        { 
            set 
            {
                if (value != null)
                {
                    ScopeInterface.FireArm = value;
                    ScopeInterface.IsIntegrated = true;
                    ScopeInterface.ForceInteractable = true;
                    ScopeInterface.Initialize();
                }
            } 
        }
    }
}