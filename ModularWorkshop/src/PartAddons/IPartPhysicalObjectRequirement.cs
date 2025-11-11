using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;

namespace ModularWorkshop
{
    public interface IPartPhysicalObjectRequirement
    {
        public FVRPhysicalObject PhysicalObject { set; }
    }
}
