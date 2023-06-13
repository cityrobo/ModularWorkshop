using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using static FistVR.Derringer;

namespace ModularWorkshop
{
    public class ModularHandguardExtension : ModularWeaponPart
    {
        [Tooltip("This GameObject defines where the extension of the AltGrip trigger will end up. It should contain a trigger to define what the new handguard interaction zone looks like. Gets removed on load for performance reasons, so don't put anything below it.")]
        public GameObject ForeGripExtensionDefinition;

        [HideInInspector]
        public TransformProxy ForeGripExtensionTransformProxy;
        [HideInInspector]
        public Vector3 TriggerCenter;
        [HideInInspector]
        public Vector3 TriggerSize;
        [HideInInspector]
        public OpenScripts2_BasePlugin.Axis ColliderAxis;

        public enum EColliderType
        {
            Sphere,
            Capsule,
            Box
        }
        [HideInInspector]
        public EColliderType ColliderType;

        private FVRFireArm _firearm;
        private Collider _addedCollider;

        public override void Awake()
        {
            base.Awake();
            if (ForeGripExtensionDefinition != null)
            {
                ForeGripExtensionTransformProxy = new(ForeGripExtensionDefinition.transform);
                Collider collider = ForeGripExtensionDefinition.GetComponent<Collider>();
                switch (collider)
                {
                    case CapsuleCollider c:
                        TriggerCenter = c.center;
                        TriggerSize = new(c.radius, c.height, 0f);
                        ColliderType = EColliderType.Capsule;

                        ColliderAxis = c.direction switch
                        {
                            0 => OpenScripts2_BasePlugin.Axis.X,
                            1 => OpenScripts2_BasePlugin.Axis.Y,
                            2 => OpenScripts2_BasePlugin.Axis.Z,
                            _ => OpenScripts2_BasePlugin.Axis.X,
                        };
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
                        OpenScripts2_BepInExPlugin.LogWarning(this, $"ForeGripDefinition {ForeGripExtensionDefinition.name} doesn't contain a collider you goofus! Shit's about to break!");
                        break;
                }

                Destroy(ForeGripExtensionDefinition);
            }
            else if (ForeGripExtensionDefinition == null) OpenScripts2_BepInExPlugin.LogError(this, $"ForeGripExtensionDefinition is empty but you want this to be a functional foregrip extension you goofus! Shit's about to break!");
        }

        public override void EnablePart()
        {
            base.EnablePart();

            _firearm = transform.root.GetComponentInChildren<FVRFireArm>();

            if (_firearm != null)
            {
                Vector3 transformedCenterGlobal = ForeGripExtensionTransformProxy.parent.TransformPoint(ForeGripExtensionTransformProxy.localPosition + TriggerCenter.MultiplyComponentWise(ForeGripExtensionTransformProxy.localScale));
                Vector3 transformedCenterLocal = _firearm.Foregrip.transform.InverseTransformPoint(transformedCenterGlobal);

                switch (ColliderType)
                {
                    case EColliderType.Sphere:
                        _addedCollider = _firearm.Foregrip.AddComponent<SphereCollider>();
                        (_addedCollider as SphereCollider).center = transformedCenterLocal;
                        (_addedCollider as SphereCollider).radius = TriggerSize.x;
                        break;
                    case EColliderType.Capsule:
                        _addedCollider = _firearm.Foregrip.AddComponent<CapsuleCollider>();
                        (_addedCollider as CapsuleCollider).center = transformedCenterLocal;
                        (_addedCollider as CapsuleCollider).radius = TriggerSize.x;
                        (_addedCollider as CapsuleCollider).height = TriggerSize.y;
                        (_addedCollider as CapsuleCollider).direction = (int) ColliderAxis;
                        break;
                    case EColliderType.Box:
                        _addedCollider = _firearm.Foregrip.AddComponent<BoxCollider>();
                        (_addedCollider as BoxCollider).center = transformedCenterLocal;
                        (_addedCollider as BoxCollider).size = TriggerSize;
                        break;
                }
                _addedCollider.isTrigger = true;
            }
            else
            {
                if (_firearm == null) OpenScripts2_BepInExPlugin.LogWarning(this, "Firearm not found! ModularHandguardExtension disabled!");
            }
        }

        public override void DisablePart()
        {
            base.DisablePart();

            _firearm = transform.root.GetComponentInChildren<FVRFireArm>();

            if (_firearm != null && _addedCollider != null)
            {
                Destroy(_addedCollider);
            }
        }
    }
}