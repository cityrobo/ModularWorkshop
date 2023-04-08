using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    public class ModularHandguard : ModularWeaponPart
    {
        public bool ActsLikeForeGrip;
        [Tooltip("If this is an active foregrip: This GameObject defines where the transform of the AltGrip in the FireArm will end up. It should contain a trigger to define what the new handguard interaction zone looks like. Gets removed on load for performance reasons, so don't put anything below it.")]
        public GameObject ForeGripDefinition;

        [HideInInspector]
        public TransformProxy ForeGripTransformProxy;
        [HideInInspector]
        public Vector3 TriggerCenter;
        [HideInInspector]
        public Vector3 TriggerSize;

        public enum EColliderType
        {
            Sphere,
            Capsule,
            Box
        }
        [HideInInspector]
        public EColliderType ColliderType;

        public override void Awake()
        {
            base.Awake();
            if (ActsLikeForeGrip && ForeGripDefinition != null)
            {
                ForeGripTransformProxy = new(ForeGripDefinition.transform);
                Collider collider = ForeGripDefinition.GetComponent<Collider>();
                switch (collider)
                {
                    case CapsuleCollider c:
                        TriggerCenter = c.center;
                        TriggerSize = new(c.radius, c.height, 0f);
                        ColliderType = EColliderType.Capsule;
                        break;
                    case SphereCollider c:
                        TriggerCenter = c.center;
                        TriggerSize = new(c.radius, 0f, 0f);
                        ColliderType = EColliderType.Sphere;
                        break;
                    case BoxCollider c:
                        TriggerCenter = c.center;
                        TriggerSize = c.size;
                        ColliderType = EColliderType.Box;
                        break;
                    case null:
                        OpenScripts2_BepInExPlugin.LogWarning(this, $"ForeGripDefinition {ForeGripDefinition.name} doesn't contain a collider you goofus! Shit's about to break!");
                        break;
                }

                Destroy(ForeGripDefinition);
            }
            else if (ActsLikeForeGrip && ForeGripDefinition == null) OpenScripts2_BepInExPlugin.LogWarning(this, $"ForeGripDefinition is empty but you want this to be a functional foregrip you goofus! Shit's about to break!");
        }
    }
}