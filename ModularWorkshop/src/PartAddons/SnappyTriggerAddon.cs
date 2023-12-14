using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using FistVR;

namespace ModularWorkshop
{
    public class SnappyTriggerAddon : MonoBehaviour, IPartFireArmRequirement
    {
        private static readonly List<FVRFireArm> _existingSnappyTriggers = new();

        private FVRFireArm _firearm;
        public FVRFireArm FireArm
        {
            set
            {
                if (value != null)
                {
                    _firearm = value;
                    switch (_firearm)
                    {
                        case Handgun:
                            _existingSnappyTriggers.Add(_firearm);
                            break;
                        default:
                            break;
                    }
                    
                }
                else if (value == null && _firearm != null)
                {
                    _existingSnappyTriggers.Remove(_firearm);
                }
            }
        }

        [HarmonyPatch(typeof(Handgun), nameof(Handgun.UpdateInputAndAnimate))]
        [HarmonyPrefix]
        private static void Handgun_UpdateInputAndAnimate_Patch(Handgun __instance, FVRViveHand hand)
        {
            if (_existingSnappyTriggers.Contains(__instance))
            {
                if (!__instance.HasTriggerReset && hand.Input.TriggerUp)
                {
                    __instance.HasTriggerReset = true;
                    __instance.m_isSeerReady = true;
                    __instance.PlayAudioEvent(FirearmAudioEventType.TriggerReset, 1f);
                    if (__instance.FireSelectorModes.Length > 0)
                    {
                        __instance.m_CamBurst = __instance.FireSelectorModes[__instance.m_fireSelectorMode].BurstAmount;
                    }
                }

                if (hand.Input.TriggerDown && !__instance.m_isSafetyEngaged && (!__instance.HasMagazineSafety || __instance.Magazine != null))
                {
                    __instance.ReleaseSeer();
                }
            }
        }

        [HarmonyPatch(typeof(ClosedBoltWeapon), nameof(ClosedBoltWeapon.UpdateInputAndAnimate))]
        [HarmonyPrefix]
        private static void ClosedBoltWeapon_UpdateInputAndAnimate_Patch(ClosedBoltWeapon __instance, FVRViveHand hand)
        {
            if (_existingSnappyTriggers.Contains(__instance))
            {
                if (!__instance.m_hasTriggerReset && hand.Input.TriggerUp)
                {
                    __instance.m_hasTriggerReset = true;
                    __instance.PlayAudioEvent(FirearmAudioEventType.TriggerReset, 1f);
                    if (__instance.FireSelector_Modes.Length > 0)
                    {
                        __instance.m_CamBurst = __instance.FireSelector_Modes[__instance.m_fireSelectorMode].BurstAmount;
                    }
                }

                if (hand.Input.TriggerDown && !__instance.IsWeaponOnSafe())
                {
                    __instance.DropHammer();
                }
            }
        }
    }
}