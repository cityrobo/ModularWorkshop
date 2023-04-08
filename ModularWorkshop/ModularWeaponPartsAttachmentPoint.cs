using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using Steamworks;
using OpenScripts2;

namespace ModularWorkshop
{
    [Serializable]
    public class ModularWeaponPartsAttachmentPoint
    {
        public string PartID;
        public Transform ModularPartPoint;
        public Transform ModularPartUIPoint;
        [HideInInspector]
        public TransformProxy ModularPartUIPos;
        public string SelectedModularWeaponPart;
    }
}