using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using System.Linq;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class FireModeAddon : MonoBehaviour , IPartFireArmRequirement
    {
        public enum FireSelectorModeType
        {
            Safe,
            Single,
            Burst,
            FullAuto,
            SuperFastBurst
        }

        [Serializable]
        public class FireSelectorMode
        {
            public float SelectorPosition;
            public FireSelectorModeType ModeType;
            public int BurstAmount = 3;
            [Tooltip("Only works for closed bolt weapons")]
            public bool ARStyleBurst = false;
            public float EngagementDelay;
        }

        public FireSelectorMode[] FireSelectorModes;

        private FVRFireArm _firearm;
        private object[] _originalFireModes;

        public FVRFireArm FireArm
        { 
            set 
            {
                if (value != null)
                {
                    _firearm = value;

                    switch(_firearm)
                    {
                        case ClosedBoltWeapon w:
                            _originalFireModes = w.FireSelector_Modes;
                            ModifyClosedBoltFireModes(w); 
                            break;
                        case OpenBoltReceiver w:
                            _originalFireModes = w.FireSelector_Modes;
                            ModifyOpenBoltFireModes(w);
                            break;
                        case Handgun w:
                            _originalFireModes = w.FireSelectorModes;
                            ModifyHandgunFireModes(w);
                            break;
                    }
                }
                else if (value == null && _firearm != null)
                {
                    _firearm = value;

                    switch (_firearm)
                    {
                        case ClosedBoltWeapon w:
                            w.FireSelector_Modes = (ClosedBoltWeapon.FireSelectorMode[])_originalFireModes;
                            break;
                        case OpenBoltReceiver w:
                            w.FireSelector_Modes = (OpenBoltReceiver.FireSelectorMode[])_originalFireModes;
                            break;
                        case Handgun w:
                            w.FireSelectorModes = (Handgun.FireSelectorMode[])_originalFireModes;
                            break;
                    }
                }
            }
            get => _firearm;
        }

        private void ModifyClosedBoltFireModes(ClosedBoltWeapon w)
        {
            List<ClosedBoltWeapon.FireSelectorMode> fireSelectorModes = new();

            foreach (var fireSelectorMode in FireSelectorModes)
            {
                fireSelectorModes.Add(
                    new ClosedBoltWeapon.FireSelectorMode()
                    {
                        SelectorPosition = fireSelectorMode.SelectorPosition,
                        ModeType = fireSelectorMode.ModeType.ConvertToClosedBolt(),
                        BurstAmount = fireSelectorMode.BurstAmount,
                        ARStyleBurst = fireSelectorMode.ARStyleBurst,
                        EngagementDelay = fireSelectorMode.EngagementDelay
                    }
                );

            }
            w.FireSelector_Modes = fireSelectorModes.ToArray();
            w.FireSelector_Modes2 = fireSelectorModes.ToArray();
        }
        private void ModifyOpenBoltFireModes(OpenBoltReceiver w)
        {
            List<OpenBoltReceiver.FireSelectorMode> fireSelectorModes = new();

            foreach (var fireSelectorMode in FireSelectorModes)
            {
                fireSelectorModes.Add(
                    new OpenBoltReceiver.FireSelectorMode()
                    {
                        SelectorPosition = fireSelectorMode.SelectorPosition,
                        ModeType = fireSelectorMode.ModeType.ConvertToOpenBolt(),
                        BurstAmount = fireSelectorMode.BurstAmount,
                        EngagementDelay = fireSelectorMode.EngagementDelay
                    }
                );
            }
            w.FireSelector_Modes = fireSelectorModes.ToArray();
            w.FireSelector_Modes2 = fireSelectorModes.ToArray();
        }
        private void ModifyHandgunFireModes(Handgun w)
        {
            List<Handgun.FireSelectorMode> fireSelectorModes = new();

            foreach (var fireSelectorMode in FireSelectorModes)
            {
                fireSelectorModes.Add(
                    new Handgun.FireSelectorMode()
                    {
                        SelectorPosition = fireSelectorMode.SelectorPosition,
                        ModeType = fireSelectorMode.ModeType.ConvertToHandgun(),
                        BurstAmount = fireSelectorMode.BurstAmount,
                        EngagementDelay = fireSelectorMode.EngagementDelay
                    }
                );
            }
            w.FireSelectorModes = fireSelectorModes.ToArray();
        }
    }

    public static class FireModeEnumExtensions
    {
        public static ClosedBoltWeapon.FireSelectorModeType ConvertToClosedBolt(this FireModeAddon.FireSelectorModeType fireSelectorMode)
        {
            return fireSelectorMode switch
            {
                FireModeAddon.FireSelectorModeType.Safe => ClosedBoltWeapon.FireSelectorModeType.Safe,
                FireModeAddon.FireSelectorModeType.Single => ClosedBoltWeapon.FireSelectorModeType.Single,
                FireModeAddon.FireSelectorModeType.Burst => ClosedBoltWeapon.FireSelectorModeType.Burst,
                FireModeAddon.FireSelectorModeType.FullAuto => ClosedBoltWeapon.FireSelectorModeType.FullAuto,
                FireModeAddon.FireSelectorModeType.SuperFastBurst => ClosedBoltWeapon.FireSelectorModeType.SuperFastBurst,
                _ => ClosedBoltWeapon.FireSelectorModeType.Safe,
            };
        }
        public static OpenBoltReceiver.FireSelectorModeType ConvertToOpenBolt(this FireModeAddon.FireSelectorModeType fireSelectorMode)
        {
            return fireSelectorMode switch
            {
                FireModeAddon.FireSelectorModeType.Safe => OpenBoltReceiver.FireSelectorModeType.Safe,
                FireModeAddon.FireSelectorModeType.Single => OpenBoltReceiver.FireSelectorModeType.Single,
                FireModeAddon.FireSelectorModeType.Burst => OpenBoltReceiver.FireSelectorModeType.Burst,
                FireModeAddon.FireSelectorModeType.FullAuto => OpenBoltReceiver.FireSelectorModeType.FullAuto,
                FireModeAddon.FireSelectorModeType.SuperFastBurst => OpenBoltReceiver.FireSelectorModeType.SuperFastBurst,
                _ => OpenBoltReceiver.FireSelectorModeType.Safe,
            };
        }
        public static Handgun.FireSelectorModeType ConvertToHandgun(this FireModeAddon.FireSelectorModeType fireSelectorMode)
        {
            return fireSelectorMode switch
            {
                FireModeAddon.FireSelectorModeType.Safe => Handgun.FireSelectorModeType.Safe,
                FireModeAddon.FireSelectorModeType.Single => Handgun.FireSelectorModeType.Single,
                FireModeAddon.FireSelectorModeType.Burst => Handgun.FireSelectorModeType.Burst,
                FireModeAddon.FireSelectorModeType.FullAuto => Handgun.FireSelectorModeType.FullAuto,
                FireModeAddon.FireSelectorModeType.SuperFastBurst => Handgun.FireSelectorModeType.Burst,
                _ => Handgun.FireSelectorModeType.Safe,
            };
        }
    }
}