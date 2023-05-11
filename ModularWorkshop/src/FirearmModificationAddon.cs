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

        public bool ChangesTriggerThresholds;
        public float TriggerThreshold;
        private float _origTriggerThreshold;
        public float TriggerReset;
        private float _origTriggerReset;

        [Header("Fire Selectors")]
        [Tooltip("Also called Safety on some weapons.")]
        public Transform FireSelector;
        [Tooltip("For Handguns this changes the FireSelector object")]
        public Transform FireSelector2;
        [Header("Magazine Release")]
        public Transform MagazineRelease;

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

        [Header("Trigger")]
        public Transform Trigger;

        [Header("DustCover")]
        [Tooltip("GameObject with DustCover components on it. One for ClosedBolt, the other for OpenBolt weapons! Needs both to work on both weapon types. If that is not required, use one or the other.")]
        public GameObject DustCover;

        [Header("AltGrip/CarryHandle")]
        public FVRAlternateGrip AltGrip;
        public CarryHandleWaggle CarryHandle;

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

                    if (BoltReleaseButton != E_ModificationMode.Ignore) ModifyBoltRelease(ModeToBool(BoltReleaseButton));
                    if (MagazineReleaseButton != E_ModificationMode.Ignore) ModifyMagReleaseButton(ModeToBool(MagazineReleaseButton));
                    if (ChangesAdvancedMagGrabTriggerMode) ModifyMagGrabTrigger(true);
                    if (ChangesTriggerThresholds) ModifyTriggerThresholds(true);
                    if (Trigger != null) ModifyTrigger(true);
                    if (FireSelector != null) ModifyFireSelector(true);
                    if (FireSelector2 != null) ModifyFireSelector2(true);
                    if (MagazineRelease != null) ModifyMagazineRelease(true);

                    if (BoltRotatingPiece != null) ModifyBoltRotatingPiece(true);
                    if (BoltZRotPiece != null) ModifyBoltZRotPiece(true);

                    if (BoltHandleRotatingPiece != null) ModifyBoltHandleRotatingPiece(true);

                    if (DustCover != null) ModifyDustCover(true);

                    if (AltGrip != null) AddAltGrip(true);

                    if (CarryHandle != null) AddCarryHandle(true);

                    if (AdjustsFireRate) ModifyFireRate(true);
                }
                else
                {
                    if (BoltReleaseButton != E_ModificationMode.Ignore) UndoBoltRelease();
                    if (MagazineReleaseButton != E_ModificationMode.Ignore) UndoMagReleaseButton();
                    if (ChangesAdvancedMagGrabTriggerMode) ModifyMagGrabTrigger(false);
                    if (ChangesTriggerThresholds) ModifyTriggerThresholds(false);
                    if (Trigger != null) ModifyTrigger(false);
                    if (FireSelector != null) ModifyFireSelector(false);
                    if (FireSelector2 != null) ModifyFireSelector2(false);
                    if (MagazineRelease != null) ModifyMagazineRelease(false);

                    if (BoltRotatingPiece != null) ModifyBoltRotatingPiece(false);
                    if (BoltZRotPiece != null) ModifyBoltZRotPiece(false);

                    if (BoltHandleRotatingPiece != null) ModifyBoltHandleRotatingPiece(false);

                    if (DustCover != null) ModifyDustCover(false);

                    if (AltGrip != null) AddAltGrip(false);

                    if (CarryHandle != null) AddCarryHandle(false);

                    if (AdjustsFireRate) ModifyFireRate(false);
                }
            }
        }

        //public void OnDestroy()
        //{
        //}

        // Bolt/Slide
        private void ModifyBoltRelease(bool mode)
        {
            switch (_firearm) 
            {
                case ClosedBoltWeapon w:
                    _origBoltReleaseState = w.HasBoltReleaseButton;
                    w.HasBoltReleaseButton = mode;
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
                case ClosedBoltWeapon w:
                    w.HasBoltReleaseButton = _origBoltReleaseState;
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
                magGrabTrigger.RequiredInput = RequiredMagGrabInput;
            }
            else
            {
                magGrabTrigger.RequiredInput = _origMagGrabInputType;
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
                        w.MagazineReleaseButton = MagazineRelease;
                        break;
                    case Handgun w:
                        w.MagazineReleaseButton = MagazineRelease;
                        break;
                    case Revolver w:
                        w.CylinderReleaseButton = MagazineRelease;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.MagazineReleaseButton = null;
                        break;
                    case Handgun w:
                        w.MagazineReleaseButton = null;
                        break;
                    case Revolver w:
                        w.CylinderReleaseButton = null;
                        break;
                }
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
                        w.FireSelectorSwitch = FireSelector;
                        break;
                    case Handgun w:
                        w.Safety = FireSelector;
                        break;
                    case OpenBoltReceiver w:
                        w.FireSelectorSwitch = FireSelector;
                        break;
                    case BoltActionRifle w:
                        w.FireSelector_Display = FireSelector;
                        break;
                    case TubeFedShotgun w:
                        w.Safety = FireSelector;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.FireSelectorSwitch = null;
                        break;
                    case Handgun w:
                        w.Safety = null;
                        break;
                    case OpenBoltReceiver w:
                        w.FireSelectorSwitch = null;
                        break;
                    case BoltActionRifle w:
                        w.FireSelector_Display = null;
                        break;
                    case TubeFedShotgun w:
                        w.Safety = FireSelector;
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
                        w.FireSelectorSwitch2 = FireSelector2;
                        break;
                    case Handgun w:
                        w.FireSelector = FireSelector2;
                        break;
                    case OpenBoltReceiver w:
                        w.FireSelectorSwitch2 = FireSelector2;
                        break;
                    case BoltActionRifle w:
                        w.FireSelector_Display_Secondary = FireSelector2;
                        break;
                }
            }
            else
            {
                switch (_firearm)
                {
                    case ClosedBoltWeapon w:
                        w.FireSelectorSwitch2 = null;
                        break;
                    case Handgun w:
                        w.FireSelector = null;
                        break;
                    case OpenBoltReceiver w:
                        w.FireSelectorSwitch2 = null;
                        break;
                    case BoltActionRifle w:
                        w.FireSelector_Display_Secondary = null;
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

        // Alt Grip / Carry Handle
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