using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class BoltAndSlideInteractionVisualEffectAddon : MonoBehaviour, IPartFireArmRequirement
    {
        public InteractionVisualEffect InteractionVisualEffect;
        
        public FVRFireArm FireArm
        {
            set
            {
                if (value != null)
                {
                    switch (value)
                    {
                        case Handgun w:
                            InteractionVisualEffect.ObjectToMonitor = w.Slide;
                            break;
                        case ClosedBoltWeapon w:
                            InteractionVisualEffect.ObjectToMonitor = w.Bolt;
                            break;
                        case OpenBoltReceiver w:
                            InteractionVisualEffect.ObjectToMonitor = w.Bolt;
                            break;
                        case TubeFedShotgun w:
                            InteractionVisualEffect.ObjectToMonitor = w.Bolt;
                            break;
                        case BoltActionRifle w:
                            InteractionVisualEffect.ObjectToMonitor = w.BoltHandle;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}