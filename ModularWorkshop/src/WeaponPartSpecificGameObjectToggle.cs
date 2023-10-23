using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using OpenScripts2;
using UnityEngine;

namespace ModularWorkshop
{
    public class WeaponPartSpecificGameObjectToggle : MonoBehaviour, IPartFireArmRequirement
    {
        [Tooltip("Use this when you want to toggle something on the gun itself, leave empty when toggling something on a part. This component needs to be put next to the ModularWeaponPart component in that case.")]
        public GameObject ModularWeapon;
        private IModularWeapon _modularWeapon;

        public string ModularPartsGroupID;
        public string[] SpecificParts;

        public GameObject ObjectToToggle;
        
        public enum E_Mode
        {
            EnableGameObject,
            DisableGameObject
        }

        public E_Mode Mode;

        public FVRFireArm FireArm 
        { 
            set
            {
                if (value != null)
                {
                    _modularWeapon = value.GetComponent<IModularWeapon>();

                    _modularWeapon.GetModularFVRFireArm.PartAdded -= OnPartAttached;
                    _modularWeapon.GetModularFVRFireArm.PartAdded += OnPartAttached;
                }
                else
                {
                    if (_modularWeapon != null)
                    {
                        _modularWeapon.GetModularFVRFireArm.PartAdded -= OnPartAttached;
                    }
                }
            }
        }

        public void Start()
        {
            if (ModularWeapon != null)
            {
                _modularWeapon = ModularWeapon.GetComponent<IModularWeapon>();

                _modularWeapon.GetModularFVRFireArm.PartAdded -= OnPartAttached;
                _modularWeapon.GetModularFVRFireArm.PartAdded += OnPartAttached;
            }

            if (_modularWeapon != null)
            {
                if (_modularWeapon.AllAttachmentPoints.TryGetValue(ModularPartsGroupID, out ModularWeaponPartsAttachmentPoint point))
                {
                    if (SpecificParts.Contains(point.SelectedModularWeaponPart))
                    {
                        switch (Mode)
                        {
                            case E_Mode.EnableGameObject:
                                ObjectToToggle.SetActive(true);
                                break;
                            case E_Mode.DisableGameObject:
                                ObjectToToggle.SetActive(false);
                                break;
                        }
                    }
                    else
                    {
                        switch (Mode)
                        {
                            case E_Mode.EnableGameObject:
                                ObjectToToggle.SetActive(false);
                                break;
                            case E_Mode.DisableGameObject:
                                ObjectToToggle.SetActive(true);
                                break;
                        }
                    }
                }
                else
                {
                    switch (Mode)
                    {
                        case E_Mode.EnableGameObject:
                            ObjectToToggle.SetActive(false);
                            break;
                        case E_Mode.DisableGameObject:
                            ObjectToToggle.SetActive(true);
                            break;
                    }
                }
            }
        }

        public void OnDestroy()
        {
            if (_modularWeapon != null)
            {
                _modularWeapon.GetModularFVRFireArm.PartAdded -= OnPartAttached;

                switch (Mode)
                {
                    case E_Mode.EnableGameObject:
                        ObjectToToggle.SetActive(false);
                        break;
                    case E_Mode.DisableGameObject:
                        ObjectToToggle.SetActive(true);
                        break;
                }
            }
        }

        private void OnPartAttached(ModularWeaponPartsAttachmentPoint _, ModularWeaponPart part)
        {
            if (_modularWeapon.AllAttachmentPoints.TryGetValue(ModularPartsGroupID, out ModularWeaponPartsAttachmentPoint point))
            {
                if (SpecificParts.Contains(point.SelectedModularWeaponPart))
                {
                    switch (Mode)
                    {
                        case E_Mode.EnableGameObject:
                            ObjectToToggle.SetActive(true);
                            break;
                        case E_Mode.DisableGameObject:
                            ObjectToToggle.SetActive(false);
                            break;
                    }
                }
                else
                {
                    switch (Mode)
                    {
                        case E_Mode.EnableGameObject:
                            ObjectToToggle.SetActive(false);
                            break;
                        case E_Mode.DisableGameObject:
                            ObjectToToggle.SetActive(true);
                            break;
                    }
                }
            }
            else
            {
                switch (Mode)
                {
                    case E_Mode.EnableGameObject:
                        ObjectToToggle.SetActive(false);
                        break;
                    case E_Mode.DisableGameObject:
                        ObjectToToggle.SetActive(true);
                        break;
                }
            }
        }
    }
}