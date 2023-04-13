using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using FistVR;
using Steamworks;
using OpenScripts2;

namespace ModularWorkshop
{
    [Serializable]
    public class ModularWeaponPartsAttachmentPoint
    {
        [FormerlySerializedAs("PartID")]
        public string ModularPartsGroupID;
        public Transform ModularPartPoint;
        public Transform ModularPartUIPoint;
        [HideInInspector]
        public TransformProxy ModularPartUIPointProxy;
        public string SelectedModularWeaponPart;
    }
}