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
    public class AttachmentMountTypeChangeAddon : MonoBehaviour, IPartPhysicalObjectRequirement
    {
        private FVRFireArmAttachment _attachment;

        public FVRFireArmAttachementMountType AttachmentMountType;
        private FVRFireArmAttachementMountType _origAttachmentMountType;

        public FVRPhysicalObject PhysicalObject
        { 
            set
            {
                if (value != null)
                {
                    _attachment = value as FVRFireArmAttachment;
                    if (_attachment != null)
                    {
                        _origAttachmentMountType = _attachment.Type;
                        _attachment.Type = AttachmentMountType;
                    }
                    else
                    {
                        ModularWorkshopManager.LogWarning($"AttachmentMountTypeChangeAddon on {gameObject.name} requires the main modular object to be an FVRFireArmAttachment of some kind, but it found a {value.GetType()}!");
                    }
                }
                else
                {
                    if (_attachment != null)
                    {
                        _attachment.Type = _origAttachmentMountType;
                    }
                }
            }
        }
    }
}