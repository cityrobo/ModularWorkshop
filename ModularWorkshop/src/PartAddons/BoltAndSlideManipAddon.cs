using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class BoltAndSlideManipAddon : MonoBehaviour, IPartFireArmRequirement
    {
        public ManipulateTransforms ManipulateTransform;
        
        public FVRFireArm FireArm
        {
            set
            {
                if (value != null)
                {
                    switch (value)
                    {
                        case Handgun _handgun:
                            foreach (var group in ManipulateTransform.TransformGroups)
                            {
                                group.ObservedTransform = _handgun.Slide.transform;
                            }
                            break;
                        case ClosedBoltWeapon w:
                            foreach (var group in ManipulateTransform.TransformGroups)
                            {
                                group.ObservedTransform = w.Bolt.transform;
                            }
                            break;
                        case OpenBoltReceiver w:
                            foreach (var group in ManipulateTransform.TransformGroups)
                            {
                                group.ObservedTransform = w.Bolt.transform;
                            }
                            break;
                        case TubeFedShotgun w:
                            foreach (var group in ManipulateTransform.TransformGroups)
                            {
                                group.ObservedTransform = w.Bolt.transform;
                            }
                            break;
                        case BoltActionRifle w:
                            foreach (var group in ManipulateTransform.TransformGroups)
                            {
                                group.ObservedTransform = w.BoltHandle.transform;
                            }
                            break;
                        default:
                            break;
                    }

                }
                //else if (value == null && _handgun != null)
                //{

                //}
            }
        }
    }
}