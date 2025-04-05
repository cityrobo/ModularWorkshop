using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using HarmonyLib;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class MagazineCompatibilityAddon : MonoBehaviour , IPartFireArmRequirement
    {
        public List<string> CompatibleMagazines = new();
        public bool RejectListedMagazinesInstead = false;

        public AudioEvent IncompatibleAudioEvent;

        private FVRFireArm _firearm;

        private static readonly Dictionary<FVRFireArm, MagazineCompatibilityAddon> _existingMagazineCompatibilityAddons = new();

        public FVRFireArm FireArm
        { 
            set 
            {
                if (value != null)
                {
                    _firearm = value;

                    _existingMagazineCompatibilityAddons.Add(_firearm, this);
                }
                else if (value == null && _firearm != null)
                {
                    _existingMagazineCompatibilityAddons.Remove(_firearm);

                    _firearm = null;
                }
            }
        }

        [HarmonyPatch(typeof(FVRFireArmReloadTriggerMag),nameof(FVRFireArmReloadTriggerMag.OnTriggerEnter))]
        [HarmonyPrefix]
        public static bool FVRFireArmReloadTriggerMag_OnTriggerEnter_Patch(FVRFireArmReloadTriggerMag __instance, Collider collider)
        {
            if (__instance.Magazine != null && __instance.Magazine.FireArm == null && __instance.Magazine.QuickbeltSlot == null)
            {
                if (collider.gameObject.tag == "FVRFireArmReloadTriggerWell")
                {
                    FVRFireArmReloadTriggerWell reloadTriggerWell = collider.gameObject.GetComponent<FVRFireArmReloadTriggerWell>();
                    if (reloadTriggerWell != null && _existingMagazineCompatibilityAddons.TryGetValue(reloadTriggerWell.FireArm, out MagazineCompatibilityAddon magazineCompatibilityAddon))
                    {
                        if (reloadTriggerWell.IsAttachableWell && reloadTriggerWell.AFireArm != null && reloadTriggerWell.AFireArm.Magazine == null)
                        {
                            FireArmMagazineType fireArmMagazineType = reloadTriggerWell.AFireArm.MagazineType;
                            if (reloadTriggerWell.UsesTypeOverride)
                            {
                                fireArmMagazineType = reloadTriggerWell.TypeOverride;
                            }
                            if (fireArmMagazineType == __instance.Magazine.MagazineType && (reloadTriggerWell.AFireArm.EjectDelay <= 0f || __instance.Magazine != reloadTriggerWell.AFireArm.LastEjectedMag) && reloadTriggerWell.AFireArm.Magazine == null)
                            {
                                if (!magazineCompatibilityAddon.RejectListedMagazinesInstead ? magazineCompatibilityAddon.CompatibleMagazines.Contains(__instance.Magazine.ObjectWrapper.ItemID)
                                                                                             : !magazineCompatibilityAddon.CompatibleMagazines.Contains(__instance.Magazine.ObjectWrapper.ItemID))
                                {
                                    __instance.Magazine.Load(reloadTriggerWell.AFireArm);
                                }
                                else
                                {
                                    SM.PlayGenericSound(magazineCompatibilityAddon.IncompatibleAudioEvent,__instance.transform.position);
                                }
                            }
                        }
                        else if (reloadTriggerWell.UsesSecondaryMagSlots && reloadTriggerWell.FireArm != null && reloadTriggerWell.FireArm.SecondaryMagazineSlots[reloadTriggerWell.SecondaryMagSlotIndex].Magazine == null)
                        {
                            FireArmMagazineType fireArmMagazineType2 = reloadTriggerWell.FireArm.MagazineType;
                            if (reloadTriggerWell.UsesTypeOverride)
                            {
                                fireArmMagazineType2 = reloadTriggerWell.TypeOverride;
                            }
                            if (fireArmMagazineType2 == __instance.Magazine.MagazineType && (reloadTriggerWell.FireArm.SecondaryMagazineSlots[reloadTriggerWell.SecondaryMagSlotIndex].m_ejectDelay <= 0f || __instance.Magazine != reloadTriggerWell.FireArm.SecondaryMagazineSlots[reloadTriggerWell.SecondaryMagSlotIndex].m_lastEjectedMag) && reloadTriggerWell.FireArm.SecondaryMagazineSlots[reloadTriggerWell.SecondaryMagSlotIndex].Magazine == null)
                            {
                                if (!magazineCompatibilityAddon.RejectListedMagazinesInstead ? magazineCompatibilityAddon.CompatibleMagazines.Contains(__instance.Magazine.ObjectWrapper.ItemID)
                                                                                             : !magazineCompatibilityAddon.CompatibleMagazines.Contains(__instance.Magazine.ObjectWrapper.ItemID))
                                {
                                    __instance.Magazine.LoadIntoSecondary(reloadTriggerWell.FireArm, reloadTriggerWell.SecondaryMagSlotIndex);
                                }
                                else
                                {
                                    SM.PlayGenericSound(magazineCompatibilityAddon.IncompatibleAudioEvent, __instance.transform.position);
                                }
                            }
                        }
                        else
                        {
                            bool alreadyHasBelt = false;
                            if (!__instance.Magazine.IsBeltBox && reloadTriggerWell.FireArm != null && reloadTriggerWell.FireArm.HasBelt)
                            {
                                alreadyHasBelt = true;
                            }
                            if (reloadTriggerWell.IsBeltBox == __instance.Magazine.IsBeltBox && reloadTriggerWell.FireArm != null && reloadTriggerWell.FireArm.Magazine == null && !alreadyHasBelt)
                            {
                                FireArmMagazineType fireArmMagazineType3 = reloadTriggerWell.FireArm.MagazineType;
                                if (reloadTriggerWell.UsesTypeOverride)
                                {
                                    fireArmMagazineType3 = reloadTriggerWell.TypeOverride;
                                }
                                if (fireArmMagazineType3 == __instance.Magazine.MagazineType && (reloadTriggerWell.FireArm.EjectDelay <= 0f || __instance.Magazine != reloadTriggerWell.FireArm.LastEjectedMag) && reloadTriggerWell.FireArm.Magazine == null)
                                {
                                    if (!magazineCompatibilityAddon.RejectListedMagazinesInstead ? magazineCompatibilityAddon.CompatibleMagazines.Contains(__instance.Magazine.ObjectWrapper.ItemID)
                                                                                                 : !magazineCompatibilityAddon.CompatibleMagazines.Contains(__instance.Magazine.ObjectWrapper.ItemID))
                                    {
                                        __instance.Magazine.Load(reloadTriggerWell.FireArm);
                                    }
                                    else
                                    {
                                        SM.PlayGenericSound(magazineCompatibilityAddon.IncompatibleAudioEvent, __instance.transform.position);
                                    }
                                }
                            }
                        }
                        return false;
                    }
                }
            }

            return true;
        }
    }
}