using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace ModularWorkshop
{
    public class ModularWeaponPart : MonoBehaviour
    {
        public string Name;
        public Sprite Icon;
        public FVRFireArmAttachmentMount[] AttachmentMounts;

        public ModularWeaponPartsAttachmentPoint[] SubAttachmentPoints;

        private Transform[] _childObjects;

        protected List<Transform> _objectsToKeep = new();

        public virtual void Awake()
        {
            _childObjects = this.GetComponentsInDirectChildren<Transform>(true);

            foreach (Transform child in _childObjects)
            {
                child.SetParent(transform.parent);
            }

            foreach (var point in SubAttachmentPoints)
            {
                point.ModularPartUIPointProxy = new(point.ModularPartUIPoint, transform, true);
            }
        }

        public virtual void OnDestroy()
        {
            foreach (Transform child in _childObjects)
            {
                if (child != null && !_objectsToKeep.Contains(child)) Destroy(child.gameObject);
            }
        }

        public virtual void ConfigurePart() { }

        public virtual void DisablePart() { }
    }
}
