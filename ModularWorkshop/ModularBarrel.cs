using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using JetBrains.Annotations;

namespace ModularWorkshop
{
    public class ModularBarrel : ModularWeaponPart
    {
        [Header("Default Barrel Parameters")]
        public Transform MuzzlePosition;
        [HideInInspector]
        public TransformProxy MuzzlePosProxy;
        public FVRFireArm.MuzzleState DefaultMuzzleState;
        public FVRFireArmMechanicalAccuracyClass DefaultMuzzleDamping;
        [Header("Accuracy")]
        public bool HasCustomMechanicalAccuracy = false;
        public FVRFireArmMechanicalAccuracyClass CustomMechanicalAccuracy;
        [Header("Muzzle Effects")]
        public bool HasCustomMuzzleEffects = false;
        public MuzzleEffectSize CustomMuzzleEffectSize;
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

        public override void Awake()
        {
            base.Awake();
            MuzzlePosProxy = new(MuzzlePosition, true);

            if (ChangesMagazineMountPoint)
            {
                MagMountPoint = new(CustomMagMountPoint, true);
                MagEjectPoint = new(CustomMagEjectPoint, true);
            }
        }
    }
}