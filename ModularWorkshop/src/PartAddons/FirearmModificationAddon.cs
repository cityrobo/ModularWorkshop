using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using System.Linq;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class FirearmModificationAddon : MonoBehaviour , IPartFireArmRequirement
    {
        public enum E_ModificationMode
        {
            Ignore,
            Off,
            On
        }

        [Header("FireControl Modifications")]
        public E_ModificationMode BoltReleaseButton;
        private bool _origBoltReleaseState;
        public E_ModificationMode MagazineReleaseButton;
        private bool _origMagReleaseState;
        public bool ChangesAdvancedMagGrabTriggerMode;
        public UniversalAdvancedMagazineGrabTrigger.E_InputType RequiredMagGrabInput;
        private UniversalAdvancedMagazineGrabTrigger.E_InputType _origMagGrabInputType;

        //public enum FireSelectorModeType
        //{
        //    Safe,
        //    Single,
        //    Burst,
        //    FullAuto
        //}

        //[Serializable]
        //public class FireSelectorMode
        //{
        //    public float SelectorPosition;
        //    public FireSelectorModeType ModeType;
        //    public int BurstAmount = 3;
        //}

        //public bool ChangesFireSelectorModes;
        //public FireSelectorMode[] FireSelectorModes;

        public bool ChangesTriggerThresholds;
        [Tooltip("Also known as TriggerFiringThreshold/TriggerBreakThreshold.")]
        public float TriggerThreshold;
        private float _origTriggerThreshold;
        [Tooltip("Also known as TriggerResetThreshold.")]
        public float TriggerReset;
        private float _origTriggerReset;

        [Header("Fire Selectors")]
        [Tooltip("Also called Safety on some weapons.")]
        public Transform FireSelector;
        private Transform _origFireSelector;
        [Tooltip("For Handguns this changes the FireSelector object")]
        public Transform FireSelector2;
        private Transform _origFireSelector2;
        [Header("Magazine Release")]
        [Tooltip("Affects the cylinder release lever on revolvers")]
        public Transform MagazineRelease;
        private Transform _origMagazineRelease;
        [Tooltip("For UniversalAdvancedMagazineGrabTrigger")]
        public Transform SecondaryMagazineRelease;
        private Transform _origSecondaryMagazineRelease;

        [Header("Bolt")]
        public Transform BoltRotatingPiece;
        public Vector3 BoltRotatingPartLeftEulers = Vector3.zero;
        public Vector3 BoltRotatingPartNeutralEulers = Vector3.zero;
        public Vector3 BoltRotatingPartRightEulers = Vector3.zero;

        public Transform BoltZRotPiece;
        public AnimationCurve BoltZRotCurve;
        public bool BoltZRotPieceDips;
        public bool BoltZRotPieceLags;

        [Header("Bolt Handle")]
        public Transform BoltHandleRotatingPiece;
        public Vector3 BoltHandleRotatingPartLeftEulers = Vector3.zero;
        public Vector3 BoltHandleRotatingPartNeutralEulers = Vector3.zero;
        public Vector3 BoltHandleRotatingPartRightEulers = Vector3.zero;

        [Header("Bolt Release / Slide Release")]
        public Transform BoltRelease;
        private Transform _origBoltRelease;

        [Header("Handgun Slide")]
        public Transform TiltinBarrel;
        public float BarrelUntilted;
        public float BarrelTilted;

        [Header("Bolt Action Handle Base Rot Offset")]
        public bool AdjustsBoltActionBoltBaseRotOffset;
        public float BaseRotOffset;
        private float _origBaseRotOffset;

        [Header("Trigger")]
        public Transform Trigger;

        [Header("DustCover")]
        [Tooltip("GameObject with DustCover components on it. One for ClosedBolt, the other for OpenBolt weapons! Needs both to work on both weapon types. If that is not required, use one or the other.")]
        public GameObject DustCover;

        [Header("AltGrip/CarryHandle/Bipod")]
        public FVRAlternateGrip AltGrip;
        public CarryHandleWaggle CarryHandle;
        public FVRFireArmBipod Bipod;


        [Header("Fire Rate Adjustments")]
        public bool AdjustsFireRate;
        public float BoltSpeed_Forward;
        private float _origSpeed_Forward;
        public float BoltSpeed_Rearward;
        private float _origSpeed_Rearward;
        public float BoltSpringStiffness;
        private float _origSpringStiffness;

        private FVRFireArm _firearm;
        public FVRFireArm FireArm
        { 
            set 
            {
                if (value != null)
                {
                    _firearm = value;

                    if (BoltReleaseButton != E_ModificationMode.Ignore) ModifyBoltReleaseBehaviour(ModeToBool(BoltReleaseButton));
                    if (MagazineReleaseButton != E_ModificationMode.Ignore) ModifyMagReleaseButton(ModeToBool(MagazineReleaseButton));

                    //if (ChangesFireSelectorModes) ModifyFireControl(true);

                    if (ChangesAdvancedMagGrabTriggerMode) ModifyMagGrabTrigger(true);
                    if (ChangesTriggerThresholds) ModifyTriggerThresholds(true);
                    if (Trigger != null) ModifyTrigger(true);
                    if (FireSelector != null) ModifyFireSelector(true);
                    if (FireSelector2 != null) ModifyFireSelector2(true);
                    if (MagazineRelease != null) ModifyMagazineRelease(true);
                    if (SecondaryMagazineRelease != null) ModifySecondaryMagazineRelease(true);

                    if (BoltRotatingPiece != null) ModifyBoltRotatingPiece(true);
                    if (BoltZRotPiece != null) ModifyBoltZRotPiece(true);

                    if (BoltHandleRotatingPiece != null) ModifyBoltHandleRotatingPiece(true);

                    if (BoltRelease != null) ModifyBoltRelease(true);
                    
                    if (TiltinBarrel != null) ModifyTiltingBarrel(true);

                    if (AdjustsBoltActionBoltBaseRotOffset) ModifyBoltHandleBaseRotOffset(true);

                    if (DustCover != null) ModifyDustCover(true);

                    if (AltGrip != null) AddAltGrip(true);
                    if (CarryHandle != null) AddCarryHandle(true);
                    if (Bipod != null) AddBipod(true);

                    if (AdjustsFireRate) ModifyFireRate(true);
                }
                else if (value == null && _firearm != null)
                {
                    if (BoltReleaseButton != E_ModificationMode.Ignore) UndoBoltRelease();
                    if (MagazineReleaseButton != E_ModificationMode.Ignore) UndoMagReleaseButton();
                    if (ChangesAdvancedMagGrabTriggerMode) ModifyMagGrabTrigger(false);
                    if (ChangesTriggerThresholds) ModifyTriggerThresholds(false);
                    if (Trigger != null) ModifyTrigger(false);
                    if (FireSelector != null) ModifyFireSelector(false);
                    if (FireSelector2 != null) ModifyFireSelector2(false);
                    if (MagazineRelease != null) ModifyMagazineRelease(false);
                    if (SecondaryMagazineRelease != null) ModifySecondaryMagazineRelease(false);

                    if (BoltRotatingPiece != null) ModifyBoltRotatingPiece(false);
                    if (BoltZRotPiece != null) ModifyBoltZRotPiece(false);

                    if (BoltHandleRotatingPiece != null) ModifyBoltHandleRotatingPiece(false);

                    if (BoltRelease != null) ModifyBoltRelease(false);

                    if (TiltinBarrel != null) ModifyTiltingBarrel(false);

                    if (AdjustsBoltActionBoltBaseRotOffset) ModifyBoltHandleBaseRotOffset(false);

                    if (DustCover != null) ModifyDustCover(false);

                    if (AltGrip != null) AddAltGrip(false);
                    if (CarryHandle != null) AddCarryHandle(false);
                    if (Bipod != null) AddBipod(false);

                    if (AdjustsFireRate) ModifyFireRate(false);

                    _firearm = null;
                }
            }
        }

        // Fire Control Group
        //private void ModifyFireControl(bool activate)
        //{
        //    if (activate)
        //    {
        //        switch (_firearm)
        //        {
        //            case ClosedBoltWeapon w:
        //                List<ClosedBoltWeapon.FireSelectorMode> newClosedBoltSelectorModes = new();
        //                foreach (var mode in FireSelectorModes)
        //                {
        //                    ClosedBoltWeapon.FireSelectorMode fireSelectorMode = new()
        //                    {
        //                        SelectorPosition = mode.SelectorPosition,
        //                        ModeType = MiscUtilities.ParseEnum<ClosedBoltWeapon.FireSelectorModeType>(mode.ModeType.ToString()),
        //                        BurstAmount = mode.BurstAmount
        //                    };

        //                    newClosedBoltSelectorModes.Add(fireSelectorMode);
        //                }

        //                w.FireSelector_Modes = newClosedBoltSelectorModes.ToArray();
        //                break;
        //            case Handgun w:
        //                List<Handgun.FireSelectorMode> newHandgunSelectorModes = new();
        //                foreach (var mode in FireSelectorModes)
        //                {
        //                    Handgun.FireSelectorMode fireSelectorMode = new()
        //                    {
        //                        SelectorPosition = mode.SelectorPosition,
        //                        ModeType = MiscUtilities.ParseEnum<Handgun.FireSelectorModeType>(mode.ModeType.ToString()),
        //                        BurstAmount = mode.BurstAmount
        //                    };

        //                    newHandgunSelectorModes.Add(fireSelectorMode);
        //                }

        //                w.FireSelectorModes = newHandgunSelectorModes.ToArray();
        //                break;
        //            case OpenBoltReceiver w:
        //                List<OpenBoltReceiver.FireSelectorMode> newOpenBoltSelectorModes = new();
        //                foreach (var mode in FireSelectorModes)
        //                {
        //                    OpenBoltReceiver.FireSelectorMode fireSelectorMode = new()
        //                    {
        //                        SelectorPosition = mode.SelectorPosition,
        //                        ModeType = MiscUtilities.ParseEnum<OpenBoltReceiver.FireSelectorModeType>(mode.ModeType.ToString()),
        //                        BurstAmount = mode.BurstAmount
        //                    };

        //                    newOpenBoltSelectorModes.Add(fireSelectorMode);
        //                }

        //                w.FireSelector_Modes = newOpenBoltSelectorModes.ToArray();
        //                break;
        //        }
        //    }
        //    else
        //    {

        //    }
        //}


        // Bolt/Slide
        private void ModifyBoltReleaseBehaviour(bool mode)
        {
            switch (_firearm) 
            {
                case ModularClosedBoltWeapon w:
                    if (w.AllowExternalBoltReleaseButtonModification)
                    {
                        _origBoltReleaseState = w.HasBoltReleaseButton;
                        w.HasBoltReleaseButton = mode;
                    }
                    break;
                case Handgun w:
                    _origBoltReleaseState = w.HasSlideRelease;
                    w.HasSlideRelease = mode;
                    break;
                case TubeFedShotgun w:
                    _origBoltReleaseState = w.HasSlideReleaseButton;
                    w.HasSlideReleaseButton = mode;
                    break;
            }
        }

        private void UndoBoltRelease()
        {
            switch (_firearm)
            {
                case ModularClosedBoltWeapon w:
                    if (w.AllowExternalBoltReleaseButtonModification) w.HasBoltReleaseButton = _origBoltReleaseState;
                    break;
                case Handgun w:
                    w.HasSlideRelease = _origBoltReleaseState;
                    break;
                case TubeFedShotgun w:
                    w.HasSlideReleaseButton = _origBoltReleaseState;
                    break;
            }
        }

        // Mag Release
        private void ModifyMagReleaseButton(bool mode)
        {
            switch (_firearm)
            {
                case ClosedBoltWeapon w:
                    _origMagReleaseState = w.HasMagReleaseButton;
                    w.HasMagReleaseButton = mode;
                    break;
                case OpenBoltReceiver w:
                    _origMagReleaseState = w.HasMagReleaseButton;
                    w.HasMagReleaseButton = mode;
                    break;
                case Handgun w:
                    _origMagReleaseState = w.HasMagReleaseButton;
                    w.HasMagReleaseButton = mode;
                    break;
                case BoltActionRifle w:
                    _origMagReleaseState = w.HasMagEjectionButton;
                    w.HasMagEjectionButton = mode;
                    break;
            }
        }

        private void UndoMagReleaseButton()
        {
            switch (_firearm)
            {
                case ClosedBoltWeapon w:
                    w.HasMagReleaseButton = _origMagReleaseState;
                    break;
                case OpenBoltReceiver w:
                    w.HasMagReleaseButton = _origMagReleaseState;
                    break;
                case Handgun w:
                    w.HasMagReleaseButton = _origMagReleaseState;
                    break;
                case BoltActionRifle w:
                    w.HasMagEjectionButton = _origMagReleaseState;
                    break;
            }
        }

        // MagGrabTrigger
        private void ModifyMagGrabTrigger(bool activate)
        {
            UniversalAdvancedMagazineGrabTrigger magGrabTrigger = _firearm.transform.root.GetComponentsInChildren<UniversalAdvancedMagazineGrabTrigger>().Single(g => !g.IsSecondarySlotGrab);
            if (activate)
            {
                _origMagGrabInputType = magGrabTrigger.RequiredInput;
                magGrabTrigger.SetRequiredInput(RequiredMagGrabInput);
            }
            else
            {
                magGrabTrigger.SetRequiredInput(_origMagGrabInputType);
            }
        }

        // Trigger Thresholds
        private void ModifyTriggerThresholds(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        _origTriggerThreshold = w.TriggerFiringThreshold;
                        _origTriggerReset = w.TriggerResetThreshold;

                        w.TriggerFiringThreshold = TriggerThreshold;
                        w.TriggerResetThreshold = TriggerReset;
                        break;
                    case OpenBoltReceiver w:
                        _origTriggerThreshold = w.TriggerFiringThreshold;
                        _origTriggerReset = w.TriggerResetThreshold;

                        w.TriggerFiringThreshold = TriggerThreshold;
                        w.TriggerResetThreshold = TriggerReset;
                        break;
                    case Handgun w:
                        _origTriggerThreshold = w.TriggerBreakThreshold;
                        _origTriggerReset = w.TriggerResetThreshold;

                        w.TriggerBreakThreshold = TriggerThreshold;
                        w.TriggerResetThreshold = TriggerReset;
                        break;
                    case BoltActionRifle w:
                        _origTriggerThreshold = w.TriggerFiringThreshold;
                        _origTriggerReset = w.TriggerResetThreshold;

                        w.TriggerFiringThreshold = TriggerThreshold;
                        w.TriggerResetThreshold = TriggerReset;
                        break;
                    case TubeFedShotgun w:
                        _origTriggerThreshold = w.TriggerBreakThreshold;
                        _origTriggerReset = w.TriggerResetThreshold;

                        w.TriggerBreakThreshold = TriggerThreshold;
                        w.TriggerResetThreshold = TriggerReset;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.TriggerFiringThreshold = _origTriggerThreshold;
                        w.TriggerResetThreshold = _origTriggerReset;
                        break;
                    case OpenBoltReceiver w:
                        w.TriggerFiringThreshold = _origTriggerThreshold;
                        w.TriggerResetThreshold = _origTriggerReset;
                        break;
                    case Handgun w:
                        w.TriggerBreakThreshold = _origTriggerThreshold;
                        w.TriggerResetThreshold = _origTriggerReset;
                        break;
                    case BoltActionRifle w:
                        w.TriggerFiringThreshold = _origTriggerThreshold;
                        w.TriggerResetThreshold = _origTriggerReset;
                        break;
                    case TubeFedShotgun w:
                        w.TriggerBreakThreshold = _origTriggerThreshold;
                        w.TriggerResetThreshold = _origTriggerReset;
                        break;
                }
            }
        }

        // Trigger Object
        private void ModifyTrigger(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.Trigger = Trigger;
                        break;
                    case Handgun w:
                        w.Trigger = Trigger;
                        break;
                    case OpenBoltReceiver w:
                        w.Trigger = Trigger;
                        break;
                    case BoltActionRifle w:
                        w.Trigger_Display = Trigger;
                        break;
                    case TubeFedShotgun w:
                        w.Trigger = Trigger;
                        break;
                    case Revolver w:
                        w.Trigger = Trigger;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.Trigger = null;
                        break;
                    case Handgun w:
                        w.Trigger = null;
                        break;
                    case OpenBoltReceiver w:
                        w.Trigger = null;
                        break;
                    case BoltActionRifle w:
                        w.Trigger_Display = null;
                        break;
                    case TubeFedShotgun w:
                        w.Trigger = null;
                        break;
                    case Revolver w:
                        w.Trigger = null;
                        break;
                }
            }
        }

        // Magazine Release Object
        private void ModifyMagazineRelease(bool activate)
        {
            if (activate) 
            { 
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        _origMagazineRelease = w.MagazineReleaseButton;
                        w.MagazineReleaseButton = MagazineRelease;
                        break;
                    case Handgun w:
                        _origMagazineRelease = w.MagazineReleaseButton;
                        w.MagazineReleaseButton = MagazineRelease;
                        break;
                    case Revolver w:
                        _origMagazineRelease = w.CylinderReleaseButton;
                        w.CylinderReleaseButton = MagazineRelease;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.MagazineReleaseButton = _origMagazineRelease;
                        break;
                    case Handgun w:
                        w.MagazineReleaseButton = _origMagazineRelease;
                        break;
                    case Revolver w:
                        w.CylinderReleaseButton = _origMagazineRelease;
                        break;
                }
            }
        }

        // Magazine Release Object
        private void ModifySecondaryMagazineRelease(bool activate)
        {
            UniversalAdvancedMagazineGrabTrigger grabTrigger = _firearm.GetComponentInChildren<UniversalAdvancedMagazineGrabTrigger>();
            if (activate)
            {
                _origSecondaryMagazineRelease = grabTrigger.SecondaryMagazineRelease;
                grabTrigger.SecondaryMagazineRelease = SecondaryMagazineRelease;
            }
            else
            {
                grabTrigger.SecondaryMagazineRelease = _origSecondaryMagazineRelease;
            }
        }

        // Fire Selector
        private void ModifyFireSelector(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        _origFireSelector = w.FireSelectorSwitch;
                        w.FireSelectorSwitch = FireSelector;
                        break;
                    case Handgun w:
                        _origFireSelector = w.Safety;
                        w.Safety = FireSelector;
                        break;
                    case OpenBoltReceiver w:
                        _origFireSelector = w.FireSelectorSwitch;
                        w.FireSelectorSwitch = FireSelector;
                        break;
                    case BoltActionRifle w:
                        _origFireSelector = w.FireSelector_Display;
                        w.FireSelector_Display = FireSelector;
                        break;
                    case TubeFedShotgun w:
                        _origFireSelector = w.Safety;
                        w.Safety = FireSelector;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.FireSelectorSwitch = _origFireSelector;
                        break;
                    case Handgun w:
                        w.Safety = _origFireSelector;
                        break;
                    case OpenBoltReceiver w:
                        w.FireSelectorSwitch = _origFireSelector;
                        break;
                    case BoltActionRifle w:
                        w.FireSelector_Display = _origFireSelector;
                        break;
                    case TubeFedShotgun w:
                        w.Safety = _origFireSelector;
                        break;
                }
            }
        }

        // Fire Selector 2
        private void ModifyFireSelector2(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        _origFireSelector2 = w.FireSelectorSwitch2;
                        w.FireSelectorSwitch2 = FireSelector2;
                        break;
                    case Handgun w:
                        _origFireSelector2 = w.FireSelector;
                        w.FireSelector = FireSelector2;
                        break;
                    case OpenBoltReceiver w:
                        _origFireSelector2 = w.FireSelectorSwitch2;
                        w.FireSelectorSwitch2 = FireSelector2;
                        break;
                    case BoltActionRifle w:
                        _origFireSelector2 = w.FireSelector_Display_Secondary;
                        w.FireSelector_Display_Secondary = FireSelector2;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.FireSelectorSwitch2 = _origFireSelector2;
                        break;
                    case Handgun w:
                        w.FireSelector = _origFireSelector2;
                        break;
                    case OpenBoltReceiver w:
                        w.FireSelectorSwitch2 = _origFireSelector2;
                        break;
                    case BoltActionRifle w:
                        w.FireSelector_Display_Secondary = _origFireSelector2;
                        break;
                }
            }
        }

        // Bolt Rotating Piece
        private void ModifyBoltRotatingPiece(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.Bolt.HasRotatingPart = true;
                        w.Bolt.RotatingPart = BoltRotatingPiece;
                        w.Bolt.RotatingPartLeftEulers = BoltRotatingPartLeftEulers;
                        w.Bolt.RotatingPartNeutralEulers = BoltRotatingPartNeutralEulers;
                        w.Bolt.RotatingPartRightEulers = BoltRotatingPartRightEulers;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.Bolt.HasRotatingPart = false;
                        w.Bolt.RotatingPart = null;
                        w.Bolt.RotatingPartLeftEulers = Vector3.zero;
                        w.Bolt.RotatingPartNeutralEulers = Vector3.zero;
                        w.Bolt.RotatingPartRightEulers = Vector3.zero;
                        break;
                }
            }
        }

        // Bolt ZRot Piece
        private void ModifyBoltZRotPiece(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.Bolt.HasZRotPart = true;
                        w.Bolt.ZRotPiece = BoltZRotPiece;
                        w.Bolt.ZRotCurve = BoltZRotCurve;
                        w.Bolt.ZRotPieceDips = BoltZRotPieceDips;
                        w.Bolt.ZRotPieceLags = BoltZRotPieceLags;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.Bolt.HasZRotPart = false;
                        w.Bolt.ZRotPiece = null;
                        w.Bolt.ZRotCurve = null;
                        w.Bolt.ZRotPieceDips = false;
                        w.Bolt.ZRotPieceLags = false;
                        break;
                }
            }
        }

        // Bolt Handle Rotating Piece
        private void ModifyBoltHandleRotatingPiece(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.Handle.HasRotatingPart = true;
                        w.Handle.RotatingPart = BoltHandleRotatingPiece;
                        w.Handle.RotatingPartLeftEulers = BoltHandleRotatingPartLeftEulers;
                        w.Handle.RotatingPartNeutralEulers = BoltHandleRotatingPartNeutralEulers;
                        w.Handle.RotatingPartRightEulers = BoltHandleRotatingPartRightEulers;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.Handle.HasRotatingPart = false;
                        w.Handle.RotatingPart = null;
                        w.Handle.RotatingPartLeftEulers = Vector3.zero;
                        w.Handle.RotatingPartNeutralEulers = Vector3.zero;
                        w.Handle.RotatingPartRightEulers = Vector3.zero;
                        break;
                }
            }
        }

        private void ModifyBoltRelease(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        _origBoltRelease = w.BoltCatch;
                        w.BoltCatch = BoltRelease;
                        break;
                    case Handgun w:
                        _origBoltRelease = w.SlideRelease;
                        w.SlideRelease = BoltRelease;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.BoltCatch = _origBoltRelease;
                        break;
                    case Handgun w:
                        w.SlideRelease = _origBoltRelease;
                        break;
                }
            }
        }

        // Tilting Handgun Barrel
        private void ModifyTiltingBarrel(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case Handgun w:
                        w.HasTiltingBarrel = true;
                        w.Barrel = TiltinBarrel;
                        w.BarrelUntilted = BarrelUntilted;
                        w.BarrelTilted = BarrelTilted;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case Handgun w:
                        w.HasTiltingBarrel = false;
                        w.Barrel = null;
                        w.BarrelUntilted = 0f;
                        w.BarrelTilted = 0f;
                        break;
                }
            }
        }

        private void ModifyBoltHandleBaseRotOffset(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case BoltActionRifle w:
                        _origBaseRotOffset = w.BoltHandle.BaseRotOffset;
                        w.BoltHandle.BaseRotOffset = BaseRotOffset;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case BoltActionRifle w:
                        w.BoltHandle.BaseRotOffset = _origBaseRotOffset;
                        break;
                }
            }
        }

        // Dust Cover
        public void ModifyDustCover(bool activate)
        {
            ClosedBoltReceiverDustCoverTrigger closedBoltReceiverDustCoverTrigger = DustCover.GetComponent<ClosedBoltReceiverDustCoverTrigger>();
            OpenBoltDustCover openBoltDustCover = DustCover.GetComponent<OpenBoltDustCover>();

            if (activate)
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        if (openBoltDustCover != null) Destroy(openBoltDustCover);
                        closedBoltReceiverDustCoverTrigger.Bolt = w.Bolt;
                        break;
                    case OpenBoltReceiver w:
                        if (closedBoltReceiverDustCoverTrigger != null) Destroy(closedBoltReceiverDustCoverTrigger);
                        openBoltDustCover.Bolt = w.Bolt;
                        break;
                    default:
                        if (closedBoltReceiverDustCoverTrigger != null) Destroy(closedBoltReceiverDustCoverTrigger);
                        if (openBoltDustCover != null) Destroy(openBoltDustCover);
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:

                        break;
                    case OpenBoltReceiver w:

                        break;
                }
            }
        }

        // Alt Grip / Carry Handle / Bipod
        public void AddAltGrip(bool activate)
        {
            if (activate)
            {
                AltGrip.PrimaryObject = _firearm;
            }
            else
            {

            }
        }

        public void AddCarryHandle(bool activate)
        {
            if (activate)
            {
                CarryHandle.ForceInteractable = true;
                CarryHandle.OverrideFirearm = _firearm;
                CarryHandle.MainForeGrip = _firearm.Foregrip.GetComponent<FVRAlternateGrip>();
            }
            else
            {

            }
        }

        public void AddBipod(bool activate)
        {
            if (activate)
            {
                Bipod.FireArm = _firearm;
                _firearm.Bipod = Bipod;
            }
            else
            {
                _firearm.Bipod = null;
            }
        }

        // Modify Fire Rate
        public void ModifyFireRate(bool activate)
        {
            if (activate)
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        _origSpeed_Forward = w.Bolt.Speed_Forward;
                        w.Bolt.Speed_Forward = BoltSpeed_Forward;
                        _origSpeed_Rearward = w.Bolt.Speed_Rearward;
                        w.Bolt.Speed_Rearward = BoltSpeed_Rearward;
                        _origSpringStiffness = w.Bolt.SpringStiffness;
                        w.Bolt.SpringStiffness = BoltSpringStiffness;
                        break;
                    case Handgun w:
                        _origSpeed_Forward = w.Slide.Speed_Forward;
                        w.Slide.Speed_Forward = BoltSpeed_Forward;
                        _origSpeed_Rearward = w.Slide.Speed_Rearward;
                        w.Slide.Speed_Rearward = BoltSpeed_Rearward;
                        _origSpringStiffness = w.Slide.SpringStiffness;
                        w.Slide.SpringStiffness = BoltSpringStiffness;
                        break;
                    case OpenBoltReceiver w:
                        _origSpeed_Forward = w.Bolt.BoltSpeed_Forward;
                        w.Bolt.BoltSpeed_Forward = BoltSpeed_Forward;
                        _origSpeed_Rearward = w.Bolt.BoltSpeed_Rearward;
                        w.Bolt.BoltSpeed_Rearward = BoltSpeed_Rearward;
                        _origSpringStiffness = w.Bolt.BoltSpringStiffness;
                        w.Bolt.BoltSpringStiffness = BoltSpringStiffness;
                        break;
                    case TubeFedShotgun w:
                        _origSpeed_Forward = w.Bolt.Speed_Forward;
                        w.Bolt.Speed_Forward = BoltSpeed_Forward;
                        _origSpeed_Rearward = w.Bolt.Speed_Rearward;
                        w.Bolt.Speed_Rearward = BoltSpeed_Rearward;
                        _origSpringStiffness = w.Bolt.SpringStiffness;
                        w.Bolt.SpringStiffness = BoltSpringStiffness;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.Bolt.Speed_Forward = _origSpeed_Forward;
                        w.Bolt.Speed_Rearward = _origSpeed_Rearward;
                        w.Bolt.SpringStiffness = _origSpringStiffness;
                        break;
                    case Handgun w:
                        w.Slide.Speed_Forward = _origSpeed_Forward;
                        w.Slide.Speed_Rearward = _origSpeed_Rearward;
                        w.Slide.SpringStiffness = _origSpringStiffness;
                        break;
                    case OpenBoltReceiver w:
                        w.Bolt.BoltSpeed_Forward = _origSpeed_Forward;
                        w.Bolt.BoltSpeed_Rearward = _origSpeed_Rearward;
                        w.Bolt.BoltSpringStiffness = _origSpringStiffness;
                        break;
                    case TubeFedShotgun w:
                        w.Bolt.Speed_Forward = _origSpeed_Forward;
                        w.Bolt.Speed_Rearward = _origSpeed_Rearward;
                        w.Bolt.SpringStiffness = _origSpringStiffness;
                        break;
                }
            }
        }

        private bool ModeToBool(E_ModificationMode mode) => mode switch
        {
            E_ModificationMode.Off => false,
            E_ModificationMode.On => true,
            _ => false,
        };
    }
}