using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using Steamworks;

namespace ModularWorkshop
{
    [Serializable]
    public class ModularWeaponPartsAttachmentPoint
    {
        public string GroupName;
        public Transform ModularPartPoint;
        public Transform ModularPartUIPoint;
        [HideInInspector]
        public TransformProxy ModularPartUIPos;
        public int SelectedModularWeaponPart;
    }
}