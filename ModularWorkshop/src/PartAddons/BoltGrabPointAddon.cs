using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class BoltGrabPointAddon : MonoBehaviour , IPartFireArmRequirement
    {
        public enum EBoltType {Bolt, BoltHandle }

        public EBoltType BoltType;

        public GameObject ColliderReference;
        private TransformProxy _colliderReferenceProxy;

        public bool ReplaceExistingCollider = false;

        private FVRFireArm _firearm;

        private Collider _origCollider;

        private Collider _addedCollider;

        private Vector3 _addedColliderCenter = new();
        private Vector3 _addedColliderSize = new();

        private enum EColliderType
        {
            Sphere,
            Capsule,
            Box
        }

        private EColliderType _addedColliderType = new();
        private OpenScripts2_BasePlugin.Axis _addedColliderAxis = new();

        public void Awake()
        {
            if (ColliderReference != null)
            {
                _colliderReferenceProxy = new(ColliderReference.transform);
                Collider collider = ColliderReference.GetComponent<Collider>();
                switch (collider)
                {
                    case CapsuleCollider c:
                        _addedColliderCenter = c.center;
                        _addedColliderSize = new(c.radius, c.height, 0f);
                        _addedColliderType = EColliderType.Capsule;
                        _addedColliderAxis = c.direction switch
                        {
                            0 => OpenScripts2_BasePlugin.Axis.X,
                            1 => OpenScripts2_BasePlugin.Axis.Y,
                            2 => OpenScripts2_BasePlugin.Axis.Z,
                            _ => OpenScripts2_BasePlugin.Axis.X,
                        };
                        break;
                    case SphereCollider c:
                        _addedColliderCenter = c.center;
                        _addedColliderSize = new(c.radius, 0f, 0f);
                        _addedColliderType = EColliderType.Sphere;
                        break;
                    case BoxCollider c:
                        _addedColliderCenter = c.center;
                        _addedColliderSize = c.size;
                        _addedColliderType = EColliderType.Box;
                        break;
                    case null:
                        ModularWorkshopManager.LogWarning(this, $"ColliderReference {ColliderReference.name} doesn't contain a collider you goofus! Shit's about to break!");
                        break;
                }

                Destroy(ColliderReference);
            }
            else if (ColliderReference == null) ModularWorkshopManager.LogError(this, $"ColliderReference is empty but you want this to be a functional bolt grab point addon you goofus! Shit's about to break!");
        }

        public FVRFireArm FireArm
        { 
            set 
            {
                if (value != null)
                {
                    _firearm = value;

                    Transform bolt = null;

                    switch (BoltType)
                    {
                        case EBoltType.Bolt:
                            bolt = _firearm switch
                            {
                                ClosedBoltWeapon w => w.Bolt.transform,
                                Handgun w => w.Slide.transform,
                                TubeFedShotgun w => w.Bolt.transform,
                                OpenBoltReceiver w => w.Bolt.transform,
                                BoltActionRifle w => w.BoltHandle.transform,
                                _ => null,
                            };
                            break;
                        case EBoltType.BoltHandle:
                            bolt = _firearm switch
                            {
                                ClosedBoltWeapon w => w.Handle.transform,
                                TubeFedShotgun w => w.Handle.transform,
                                OpenBoltReceiver w => (w.GetComponentInChildren<OpenBoltRotatingChargingHandle>() ?? w.GetComponentInChildren<OpenBoltRotatingChargingHandle>()).transform,
                                BoltActionRifle w => w.BoltHandle.transform,
                                _ => null,
                            };
                            break;
                    }

                    Vector3 transformedCenterGlobal = _colliderReferenceProxy.parent.TransformPoint(_colliderReferenceProxy.localPosition + _addedColliderCenter.MultiplyComponentWise(_colliderReferenceProxy.localScale));
                    Vector3 transformedCenterLocal = bolt.InverseTransformPoint(transformedCenterGlobal);

                    if (ReplaceExistingCollider)
                    {
                        _origCollider = bolt.GetComponent<Collider>();
                        _origCollider.enabled = false;
                    }

                    switch (_addedColliderType)
                    {
                        case EColliderType.Sphere:
                            _addedCollider = bolt.gameObject.AddComponent<SphereCollider>();
                            (_addedCollider as SphereCollider).center = transformedCenterLocal;
                            (_addedCollider as SphereCollider).radius = _addedColliderSize.x;
                            break;
                        case EColliderType.Capsule:
                            _addedCollider = bolt.gameObject.AddComponent<CapsuleCollider>();
                            (_addedCollider as CapsuleCollider).center = transformedCenterLocal;
                            (_addedCollider as CapsuleCollider).radius = _addedColliderSize.x;
                            (_addedCollider as CapsuleCollider).height = _addedColliderSize.y;
                            (_addedCollider as CapsuleCollider).direction = (int)_addedColliderAxis;
                            break;
                        case EColliderType.Box:
                            _addedCollider = bolt.gameObject.AddComponent<BoxCollider>();
                            (_addedCollider as BoxCollider).center = transformedCenterLocal;
                            (_addedCollider as BoxCollider).size = _addedColliderSize;
                            break;
                    }
                    _addedCollider.isTrigger = true;
                }
                else if (value == null && _firearm != null)
                {
                    if (ReplaceExistingCollider)
                    {
                        _origCollider.enabled = true;
                    }

                    Destroy(_addedCollider);

                    _firearm = null;
                }
            }
        }
    }
}