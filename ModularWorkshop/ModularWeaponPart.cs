﻿using System;
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

        private Transform[] childObjects;

        protected List<Transform>_objectsToKeep = new();

        public virtual void Awake()
        {
            childObjects = this.GetComponentsInDirectChildren<Transform>(true);

            foreach (Transform child in childObjects)
            {
                child.SetParent(transform.parent);
            }
        }

        public virtual void OnDestroy()
        {
            foreach (Transform child in childObjects)
            {
                if (!_objectsToKeep.Contains(child)) Destroy(child.gameObject);
            }
        }
    }
}
