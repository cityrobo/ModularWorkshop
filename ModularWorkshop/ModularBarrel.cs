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
        public Transform MuzzlePosition;
        [HideInInspector]
        public TransformProxy MuzzlePosProxy;
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

        public override void Awake()
        {
            base.Awake();
            MuzzlePosProxy = new(MuzzlePosition);
            Destroy(MuzzlePosition.gameObject);

            if (ChangesMagazineMountPoint)
            {
                MagMountPoint = new(CustomMagMountPoint);
                MagEjectPoint = new(CustomMagEjectPoint);

                Destroy(CustomMagMountPoint.gameObject);
                Destroy(CustomMagEjectPoint.gameObject);
            }
        }
    }
}