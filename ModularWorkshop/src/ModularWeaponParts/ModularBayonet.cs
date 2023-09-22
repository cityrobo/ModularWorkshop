using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using System.Linq;

namespace ModularWorkshop
{
    public class ModularBayonet : ModularWeaponPart
    {
        [Tooltip("HandPoint will be automatically set to the old hand point.")]
        public FVRPhysicalObject.MeleeParams MeleeParams;
        private FVRPhysicalObject.MeleeParams _origMeleeParams;

        protected FVRPhysicalObject _physicalObject;

        public override void EnablePart()
        {
            base.EnablePart();

            _physicalObject = transform.GetComponentInParent<FVRPhysicalObject>();

            if (_physicalObject != null)
            {
                _origMeleeParams = _physicalObject.MP;

                MeleeParams.HandPoint = _origMeleeParams.HandPoint;
                _physicalObject.MP = MeleeParams;
            }
            else
            {
                if (_physicalObject == null) OpenScripts2_BepInExPlugin.LogWarning(this, "Firearm not found! ModularBayonet disabled!");
            }
        }

        public override void DisablePart()
        {
            base.DisablePart();

            _physicalObject = transform.GetComponentInParent<FVRPhysicalObject>();

            if (_physicalObject != null)
            {
                _physicalObject.MP = _origMeleeParams;
            }
        }
    }
}