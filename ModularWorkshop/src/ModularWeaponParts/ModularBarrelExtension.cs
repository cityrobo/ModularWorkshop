using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using System.Linq;

namespace ModularWorkshop
{
    public class ModularBarrelExtension : ModularWeaponPart
    {
        [Header("Muzzle Position")]
        public bool ChangesMuzzlePosition = false;
        public Transform MuzzlePosition;
        [HideInInspector]
        public TransformProxy MuzzlePosProxy;
        [Header("Muzzle Damping")]
        public bool ChangesDefaultMuzzleStandAndDamping = false;
        public FVRFireArm.MuzzleState DefaultMuzzleState;
        public FVRFireArmMechanicalAccuracyClass DefaultMuzzleDamping;
        [Header("Accuracy")]
        public bool HasCustomMechanicalAccuracy = false;
        public FVRFireArmMechanicalAccuracyClass CustomMechanicalAccuracy;
        [Header("Muzzle Effects")]
        public bool HasCustomMuzzleEffects = false;
        public MuzzleEffectSize CustomDefaultMuzzleEffectSize;
        public MuzzleEffect[] CustomMuzzleEffects;
        public bool KeepRevolverCylinderSmoke = false;
        [Header("Round Type")]
        public bool ChangesFireArmRoundType = false;
        public FireArmRoundType CustomRoundType;
        [Header("Recoil Profile")]
        public bool ChangesRecoilProfile = false;
        public FVRFireArmRecoilProfile CustomRecoilProfile;
        public FVRFireArmRecoilProfile CustomRecoilProfileStocked;
        [Header("Magazine Mounting")]
        public bool ChangesMagazineMountPoint = false;
        public Transform CustomMagMountPoint;
        public Transform CustomMagEjectPoint;
        [Header("Magazine Type")]
        public bool ChangesMagazineType = false;
        public FireArmMagazineType CustomMagazineType;
        [Header("Audio Set")]
        public bool ChangesAudioSet = false;
        public FVRFirearmAudioSet CustomAudioSet;

        [HideInInspector]
        public TransformProxy MagMountPoint;
        [HideInInspector]
        public TransformProxy MagEjectPoint;

        protected ModularBarrel _barrel;
        protected FVRFireArm _firearm;

        public override void Awake()
        {
            base.Awake();

            if (ChangesMuzzlePosition)
            {
                MuzzlePosProxy = new(MuzzlePosition, true);
            }

            if (ChangesMagazineMountPoint)
            {
                MagMountPoint = new(CustomMagMountPoint, true);
                MagEjectPoint = new(CustomMagEjectPoint, true);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void EnablePart()
        {
            base.EnablePart();

            if (transform.TryGetComponentInParent(out _firearm) && _firearm.TryGetComponentInChildren(out _barrel))
            {
                if (ChangesMuzzlePosition)
                {
                    _firearm.MuzzlePos.GoToTransformProxy(MuzzlePosProxy);

                    switch (_firearm)
                    {
                        case BreakActionWeapon w:
                            foreach (var barrel in w.Barrels)
                            {
                                Vector3 transformedMuzzle = barrel.Muzzle.parent.InverseTransformPoint(MuzzlePosProxy.position);
                                barrel.Muzzle.ModifyLocalPositionAxisValue(OpenScripts2_BasePlugin.Axis.Z, transformedMuzzle.z);
                            }
                            break;
                    }
                }

                if (ChangesDefaultMuzzleStandAndDamping)
                {
                    _firearm.DefaultMuzzleState = DefaultMuzzleState;
                    _firearm.DefaultMuzzleDamping = DefaultMuzzleDamping;
                }

                if (HasCustomMuzzleEffects)
                {
                    if (KeepRevolverCylinderSmoke)
                    {
                        MuzzleEffect cylinderSmoke = _firearm.MuzzleEffects.Single(obj => obj.Entry == MuzzleEffectEntry.Smoke_RevolverCylinder);

                        CustomMuzzleEffects = CustomMuzzleEffects.Concat(new MuzzleEffect[] { cylinderSmoke }).ToArray();
                    }

                    _firearm.DefaultMuzzleEffectSize = CustomDefaultMuzzleEffectSize;
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
                if (ChangesAudioSet)
                {
                    _firearm.AudioClipSet = CustomAudioSet;
                }

                if (ChangesMuzzlePosition || HasCustomMuzzleEffects) _firearm.UpdateCurrentMuzzle();
            }
            else
            {
                if (_firearm == null) OpenScripts2_BepInExPlugin.LogWarning(this, "Firearm not found! ModularBarrelExtension disabled!");
                if (_barrel == null) OpenScripts2_BepInExPlugin.LogWarning(this, "ModularBarrel not found! ModularBarrelExtension disabled!");
            }
        }

        public override void DisablePart()
        {
            base.DisablePart();

            if (transform.TryGetComponentInParent(out _firearm) && _firearm.TryGetComponentInChildren(out _barrel))
            {
                ModularFVRFireArm modularFireArm = _firearm.GetComponent<IModularWeapon>().GetModularFVRFireArm;
                if (ChangesMuzzlePosition)
                {
                    _firearm.MuzzlePos.GoToTransformProxy(_barrel.MuzzlePosProxy);

                    switch (_firearm)
                    {
                        case BreakActionWeapon w:
                            foreach (var barrel in w.Barrels)
                            {
                                Vector3 transformedMuzzle = barrel.Muzzle.parent.InverseTransformPoint(_barrel.MuzzlePosProxy.position);
                                barrel.Muzzle.ModifyLocalPositionAxisValue(OpenScripts2_BasePlugin.Axis.Z, transformedMuzzle.z);
                            }
                            break;
                    }
                }

                if (ChangesDefaultMuzzleStandAndDamping)
                {
                    _firearm.DefaultMuzzleState = _barrel.DefaultMuzzleState;
                    _firearm.DefaultMuzzleDamping = _barrel.DefaultMuzzleDamping;
                }
                if (HasCustomMuzzleEffects)
                {
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
                }
                if (HasCustomMechanicalAccuracy)
                {
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
                }
                if (ChangesFireArmRoundType)
                {
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
                }
                if (ChangesRecoilProfile)
                {
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
                }
                if (ChangesMagazineMountPoint)
                {
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
                }
                if (ChangesMagazineType)
                {
                    if (_barrel.ChangesMagazineType)
                    {
                        _firearm.MagazineType = _barrel.CustomMagazineType;
                    }
                    else
                    {
                        _firearm.MagazineType = modularFireArm.OrigMagazineType;
                    }
                }
                if (ChangesAudioSet)
                {
                    if (_barrel.ChangesAudioSet)
                    {
                        _firearm.AudioClipSet = _barrel.CustomAudioSet;
                    }
                    else
                    {
                        _firearm.AudioClipSet = modularFireArm.OrigAudioSet;
                    }
                }
                if (ChangesMuzzlePosition || HasCustomMuzzleEffects)
                {
                    _firearm.UpdateCurrentMuzzle();
                }
            }
            else
            {
                if (_firearm == null) OpenScripts2_BepInExPlugin.LogWarning(this, "Firearm not found! ModularBarrelExtension couldn't be disabled!");
                if (_barrel == null) OpenScripts2_BepInExPlugin.LogWarning(this, "ModularBarrel not found! ModularBarrelExtension couldn't be disabled!");
            }
        }
    }
}