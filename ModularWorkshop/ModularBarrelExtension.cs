using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularBarrelExtension : ModularWeaponPart
    {
        public bool ChangesMuzzlePosition = false;
        public Transform MuzzlePosition;
        [HideInInspector]
        public TransformProxy MuzzlePosProxy;

        public bool ChangesDefaultMuzzleStandAndDamping = false;
        public FVRFireArm.MuzzleState DefaultMuzzleState;
        public FVRFireArmMechanicalAccuracyClass DefaultMuzzleDamping;

        public bool HasCustomMechanicalAccuracy = false;
        public FVRFireArmMechanicalAccuracyClass CustomMechanicalAccuracy;

        public bool HasCustomMuzzleEffects = false;
        public MuzzleEffectSize CustomMuzzleEffectSize;
        public MuzzleEffect[] CustomMuzzleEffects;

        public bool ChangesFireArmRoundType = false;
        public FireArmRoundType CustomRoundType;

        public bool ChangesRecoilProfile = false;
        public FVRFireArmRecoilProfile CustomRecoilProfile;
        public FVRFireArmRecoilProfile CustomRecoilProfileStocked;

        public bool ChangesMagazineMountPoint = false;
        public Transform CustomMagMountPoint;
        public Transform CustomMagEjectPoint;

        public bool ChangesMagazineType = false;
        public FireArmMagazineType CustomMagazineType;

        [HideInInspector]
        public TransformProxy MagMountPoint;
        [HideInInspector]
        public TransformProxy MagEjectPoint;

        private ModularBarrel _barrel;
        private FVRFireArm _firearm;

        public override void Awake()
        {
            base.Awake();

            if (ChangesMagazineMountPoint)
            {
                MagMountPoint = new(CustomMagMountPoint);
                MagEjectPoint = new(CustomMagEjectPoint);

                Destroy(CustomMagMountPoint.gameObject);
                Destroy(CustomMagEjectPoint.gameObject);
            }

            Transform current = transform;
            do
            {
                if (_barrel == null) _barrel = current.GetComponentInChildren<ModularBarrel>();
                if (_firearm == null) _firearm = current.GetComponentInChildren<FVRFireArm>();
                current = current.parent;
            } while ((_barrel == null || _firearm == null) && current != null);

            if (_firearm != null && _barrel != null)
            {
                if (ChangesMagazineMountPoint)
                _firearm.MuzzlePos.GoToTransformProxy(MuzzlePosProxy);
                if (ChangesDefaultMuzzleStandAndDamping)
                {
                    _firearm.DefaultMuzzleState = DefaultMuzzleState;
                    _firearm.DefaultMuzzleDamping = DefaultMuzzleDamping;
                }

                if (HasCustomMuzzleEffects)
                {
                    _firearm.DefaultMuzzleEffectSize = CustomMuzzleEffectSize;
                    _firearm.MuzzleEffects = CustomMuzzleEffects;
                }
                if (HasCustomMechanicalAccuracy)
                {
                    _firearm.AccuracyClass = CustomMechanicalAccuracy;
                    _firearm.m_internalMechanicalMOA = AM.GetFireArmMechanicalSpread(_firearm.AccuracyClass);
                }
                if (ChangesFireArmRoundType)
                {
                    _firearm.RoundType = CustomRoundType;
                    foreach (var chamber in _firearm.FChambers)
                    {
                        chamber.RoundType = CustomRoundType;
                    }
                }
                if (ChangesRecoilProfile)
                {
                    _firearm.RecoilProfile = CustomRecoilProfile;
                    if (CustomRecoilProfileStocked != null)
                    {
                        _firearm.UsesStockedRecoilProfile = true;
                        _firearm.RecoilProfileStocked = CustomRecoilProfileStocked;
                    }
                    else _firearm.UsesStockedRecoilProfile = false;
                }
                if (ChangesMagazineMountPoint)
                {
                    _firearm.MagazineMountPos.GoToTransformProxy(MagMountPoint);
                    _firearm.MagazineEjectPos.GoToTransformProxy(MagEjectPoint);
                }
                if (ChangesMagazineType)
                {
                    _firearm.MagazineType = CustomMagazineType;
                }
            }
            else 
            {
                if (_firearm == null) OpenScripts2_BepInExPlugin.LogWarning(this, "Firearm not found! ModularStockExtension disabled!");
                if (_barrel == null) OpenScripts2_BepInExPlugin.LogWarning(this, "ModularBarrel not found! ModularStockExtension disabled!");
            }
        }

        public override void OnDestroy()
        {
            if (_firearm != null && _barrel != null)
            {
                ModularFVRFireArm modularFireArm = _firearm.GetComponent<IModularWeapon>().GetModularFVRFireArm;
                _firearm.MuzzlePos.GoToTransformProxy(_barrel.MuzzlePosProxy);

                _firearm.DefaultMuzzleState = _barrel.DefaultMuzzleState;
                _firearm.DefaultMuzzleDamping = _barrel.DefaultMuzzleDamping;

                if (_barrel.HasCustomMuzzleEffects)
                {
                    _firearm.DefaultMuzzleEffectSize = _barrel.CustomMuzzleEffectSize;
                    _firearm.MuzzleEffects = _barrel.CustomMuzzleEffects;
                }
                else
                {
                    _firearm.DefaultMuzzleEffectSize = modularFireArm.OrigMuzzleEffectSize;
                    _firearm.MuzzleEffects = modularFireArm.OrigMuzzleEffects;
                }
                if (_barrel.HasCustomMechanicalAccuracy)
                {
                    _firearm.AccuracyClass = _barrel.CustomMechanicalAccuracy;
                    _firearm.m_internalMechanicalMOA = AM.GetFireArmMechanicalSpread(_firearm.AccuracyClass);
                }
                else
                {
                    _firearm.AccuracyClass = modularFireArm.OrigMechanicalAccuracyClass;
                    _firearm.m_internalMechanicalMOA = AM.GetFireArmMechanicalSpread(_firearm.AccuracyClass);
                }
                if (_barrel.ChangesFireArmRoundType)
                {
                    _firearm.RoundType = _barrel.CustomRoundType;
                    foreach (var chamber in _firearm.FChambers)
                    {
                        chamber.RoundType = _barrel.CustomRoundType;
                    }
                }
                else
                {
                    _firearm.RoundType = modularFireArm.OrigRoundType;
                    foreach (var chamber in _firearm.FChambers)
                    {
                        chamber.RoundType = modularFireArm.OrigRoundType;
                    }
                }
                if (_barrel.ChangesRecoilProfile)
                {
                    _firearm.RecoilProfile = _barrel.CustomRecoilProfile;
                    if (_barrel.CustomRecoilProfileStocked != null)
                    {
                        _firearm.UsesStockedRecoilProfile = true;
                        _firearm.RecoilProfileStocked = _barrel.CustomRecoilProfileStocked;
                    }
                    else _firearm.UsesStockedRecoilProfile = false;
                }
                else
                {
                    _firearm.RecoilProfile = modularFireArm.OrigRecoilProfile;
                    if (modularFireArm.OrigRecoilProfileStocked != null)
                    {
                        _firearm.UsesStockedRecoilProfile = true;
                        _firearm.RecoilProfileStocked = modularFireArm.OrigRecoilProfileStocked;
                    }
                    else
                        _firearm.UsesStockedRecoilProfile = false;
                }
                if (_barrel.ChangesMagazineMountPoint)
                {
                    _firearm.MagazineMountPos.GoToTransformProxy(_barrel.MagMountPoint);
                    _firearm.MagazineEjectPos.GoToTransformProxy(_barrel.MagEjectPoint);
                }
                else
                {
                    _firearm.MagazineMountPos.GoToTransformProxy(modularFireArm.OrigMagMountPos);
                    _firearm.MagazineEjectPos.GoToTransformProxy(modularFireArm.OrigMagEjectPos);
                }
                if (_barrel.ChangesMagazineType)
                {
                    _firearm.MagazineType = _barrel.CustomMagazineType;
                }
                else
                {
                    _firearm.MagazineType = modularFireArm.OrigMagazineType;
                }
            }

            base.OnDestroy();
        }
    }
}