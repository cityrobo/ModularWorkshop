using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class RecoilModificationAddon : MonoBehaviour , IPartFireArmRequirement
    {
        public float VerticalRecoilMultiplier = 1.0f;

        [Tooltip("This option will use the below multipliers to\naffect the recoil profile directly.\nUse a brain. Don't also change the recoil profile with another part!")]
        public bool UseAdvancedRecoilMultipliers = false;
        public OpenScripts2_BasePlugin.RecoilMultipliers RecoilMultipliers;

        private FVRFireArm _firearm;

        public FVRFireArm FireArm
        { 
            set 
            {
                if (value != null)
                {
                    _firearm = value;

                    RecoilModificationCore recoilCore = _firearm.GetComponent<RecoilModificationCore>() ?? _firearm.gameObject.AddComponent<RecoilModificationCore>();

                    recoilCore.VerticalRecoilMultipliers.Add(VerticalRecoilMultiplier);

                    if (UseAdvancedRecoilMultipliers)
                    {
                        recoilCore.RecoilMultipliersList.Add(RecoilMultipliers);
                        recoilCore.UpdateRecoilProfile();
                    }
                }
                else if (value == null && _firearm != null)
                {
                    RecoilModificationCore recoilCore = _firearm.GetComponent<RecoilModificationCore>();

                    recoilCore?.VerticalRecoilMultipliers.Remove(VerticalRecoilMultiplier);

                    if (UseAdvancedRecoilMultipliers)
                    {
                        recoilCore?.RecoilMultipliersList.Remove(RecoilMultipliers);
                        recoilCore?.UpdateRecoilProfile();
                    }

                    _firearm = null;
                }
            }
        }
    }
}