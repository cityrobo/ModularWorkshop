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
            FullAuto
        }

        [Serializable]
        public class FireSelectorMode
        {
            public float SelectorPosition;

            public FireSelectorModeType ModeType;

            public int BurstAmount = 3;
        }

        public FireSelectorMode[] FireSelectorModes;

        private FVRFireArm _firearm;
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
                            ModifyClosedBoltFireModes(w); 
                            break;
                        case OpenBoltReceiver w:
                            ModifyOpenBoltFireModes(w);
                            break;
                        case Handgun w:
                            ModifyHandgunFireModes(w);
                            break;
                    }
                }
                else if (value == null && _firearm != null)
                {

                }
            }
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
                        //BurstAmount = fireSelectorMode.BurstAmount,
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
                _ => ClosedBoltWeapon.FireSelectorModeType.Safe,
            };
        }
        public static OpenBoltReceiver.FireSelectorModeType ConvertToOpenBolt(this FireModeAddon.FireSelectorModeType fireSelectorMode)
        {
            return fireSelectorMode switch
            {
                FireModeAddon.FireSelectorModeType.Safe => OpenBoltReceiver.FireSelectorModeType.Safe,
                FireModeAddon.FireSelectorModeType.Single => OpenBoltReceiver.FireSelectorModeType.Single,
                //FireModeAddon.FireSelectorModeType.Burst => OpenBoltReceiver.FireSelectorModeType.Burst,
                FireModeAddon.FireSelectorModeType.FullAuto => OpenBoltReceiver.FireSelectorModeType.FullAuto,
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
                _ => Handgun.FireSelectorModeType.Safe,
            };
        }
    }
}