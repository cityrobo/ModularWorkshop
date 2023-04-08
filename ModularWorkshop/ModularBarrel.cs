using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularBarrel : ModularWeaponPart
    {
        public Transform MuzzlePosition;
        public FVRFireArm.MuzzleState DefaultMuzzleState;
        public FVRFireArmMechanicalAccuracyClass DefaultMuzzleDamping;

        public bool HasCustomMechanicalAccuracy = false;
        public FVRFireArmMechanicalAccuracyClass CustomMechanicalAccuracy;

        public bool HasCustomMuzzleEffects = false;
        public MuzzleEffectSize CustomMuzzleEffectSize;
        public MuzzleEffect[] CustomMuzzleEffects;

        public bool ChangesFireArmRoundType = false;
        public FireArmRoundType CustomRoundType;

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