using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using OpenScripts2;
using UnityEngine;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class PicatinnyForwardStopAddon : MonoBehaviour, IPartPhysicalObjectRequirement
    {
        private FVRFireArmAttachment _attachment;

        public AttachmentPicatinnyRailForwardStop ForwardStop;

        public FVRPhysicalObject PhysicalObject
        { 
            set
            {
                if (value != null)
                {
                    _attachment = value as FVRFireArmAttachment;
                    if ( _attachment != null)
                    {
                        ForwardStop.Attachment = _attachment;
                        ForwardStop.ExternalActivation();
                    }
                    else if (_attachment == null)
                    {
                        ModularWorkshopManager.LogWarning($"PicatinnyForwardStopAddon on {gameObject.name} requires the main modular object to be an FVRFireArmAttachment of some kind, but it found a {value.GetType()}!");
                    }
                }
                else
                {
                    if (_attachment != null)
                    {
                        ForwardStop.ExternalDeactivation();
                    }
                }
            }
        }
    }
}