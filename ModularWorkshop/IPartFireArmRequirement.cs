using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;

namespace ModularWorkshop
{
    public interface IPartFireArmRequirement
    {
        public FVRFireArm FireArm { set; }
    }
}
