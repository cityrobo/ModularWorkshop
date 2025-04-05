using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using System.Linq;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class MagazineWellAddon : MonoBehaviour , IPartFireArmRequirement
    {
        public UniversalMagGrabTrigger MagGrabTrigger;
        public UniversalAdvancedMagazineGrabTrigger AdvancedMagazineGrabTrigger;
        public FVRFireArmReloadTriggerWell ReloadTriggerWell;

        public FVRFireArm.SecondaryMagazineSlot SecondaryMagazineSlot;
        private FVRFireArm _firearm;

        public FVRFireArm FireArm
        { 
            set 
            {
                if (value != null)
                {
                    _firearm = value;
                    if (MagGrabTrigger != null) MagGrabTrigger.FireArm = _firearm;
                    if (AdvancedMagazineGrabTrigger != null) AdvancedMagazineGrabTrigger.FireArm = _firearm;
                    if (ReloadTriggerWell != null) ReloadTriggerWell.FireArm = _firearm;

                    List<FVRFireArm.SecondaryMagazineSlot> secondaryMagazineSlotsList = _firearm.SecondaryMagazineSlots.ToList();
                    secondaryMagazineSlotsList.Add(SecondaryMagazineSlot);
                    _firearm.SecondaryMagazineSlots = secondaryMagazineSlotsList.ToArray();
                }
                else
                {
                    List<FVRFireArm.SecondaryMagazineSlot> secondaryMagazineSlotsList = _firearm.SecondaryMagazineSlots.ToList();
                    int index = secondaryMagazineSlotsList.IndexOf(SecondaryMagazineSlot);
                    _firearm.EjectSecondaryMagFromSlot(index);
                    secondaryMagazineSlotsList.Remove(SecondaryMagazineSlot);
                    _firearm.SecondaryMagazineSlots = secondaryMagazineSlotsList.ToArray();
                }
            } 
        }
    }
}