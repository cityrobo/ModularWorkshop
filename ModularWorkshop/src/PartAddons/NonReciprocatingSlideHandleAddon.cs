using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using OpenScripts2;
using static ModularWorkshop.BoltGrabPointAddon;
using static RootMotion.BipedNaming;

namespace ModularWorkshop
{
    [RequireComponent(typeof(ModularWeaponPart))]
    public class NonReciprocatingSlideHandleAddon : MonoBehaviour, IPartFireArmRequirement
    {
        public Transform SlideHandle;
        public float ForwardPos;
        public float RearwardPos;
        private HandgunSlide _slide;
        private bool _wasSlideGrabbed = false;
        private float _slideHandleSpeed = 0f;
        private float _curZPos;
        private float _lastSlidePosZ;

        private float GetSlideHandleLerpBetweenRearAndFore() => Mathf.InverseLerp(RearwardPos, ForwardPos, _curZPos);

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
                    switch (value)
                    {
                        case Handgun w:
                            _slide = w.Slide;
                            break;
                        default:
                            break;
                    }
                }

                // Collider Stuff
                if (value != null)
                {
                    _firearm = value;
                    Transform bolt = _firearm switch
                    {
                        Handgun w => w.Slide.transform,
                        _ => null,
                    };

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

        public void Update()
        {
            if (_slide != null)
            {
                float Speed_Forward = _slide.Speed_Forward;
                float SpringStiffness = _slide.SpringStiffness;
                _curZPos = SlideHandle.localPosition.z;

                if (_slide.IsHeld)
                {
                    _wasSlideGrabbed = true;
                    _slideHandleSpeed = 0f;

                    float slideLerp = _slide.GetSlideLerpBetweenRearAndFore();
                    _curZPos = Mathf.Lerp(RearwardPos, ForwardPos, slideLerp);
                    SlideHandle.ModifyLocalPositionAxisValue(OpenScripts2_BasePlugin.Axis.Z, _curZPos);
                }
                else if (!_slide.IsHeld && _wasSlideGrabbed)
                {
                    _slideHandleSpeed = Mathf.MoveTowards(_slideHandleSpeed, Speed_Forward, SpringStiffness * Time.deltaTime);
                    _curZPos += _slideHandleSpeed * Time.deltaTime;
                    _curZPos = Mathf.Clamp(_curZPos, RearwardPos, ForwardPos);

                    SlideHandle.ModifyLocalPositionAxisValue(OpenScripts2_BasePlugin.Axis.Z, _curZPos);
                    float lerp = GetSlideHandleLerpBetweenRearAndFore();
                    if (Mathf.Approximately(lerp, 1f))
                    {
                        _slideHandleSpeed = 0f;
                        _wasSlideGrabbed = false;
                    }

                    Vector3 transformedCenterGlobal = _colliderReferenceProxy.parent.TransformPoint(_colliderReferenceProxy.localPosition + _addedColliderCenter.MultiplyComponentWise(_colliderReferenceProxy.localScale));
                    Vector3 transformedCenterLocal = _slide.transform.InverseTransformPoint(transformedCenterGlobal);
                    switch (_addedColliderType)
                    {
                        case EColliderType.Sphere:
                            (_addedCollider as SphereCollider).center = transformedCenterLocal;
                            break;
                        case EColliderType.Capsule:
                            (_addedCollider as CapsuleCollider).center = transformedCenterLocal;
                            break;
                        case EColliderType.Box:
                            (_addedCollider as BoxCollider).center = transformedCenterLocal;
                            break;
                    }
                }

                float slidePosZ = _slide.m_slideZ_current;
                if (slidePosZ != _lastSlidePosZ)
                {
                    Vector3 transformedCenterGlobal = _colliderReferenceProxy.parent.TransformPoint(_colliderReferenceProxy.localPosition + _addedColliderCenter.MultiplyComponentWise(_colliderReferenceProxy.localScale));
                    Vector3 transformedCenterLocal = _slide.transform.InverseTransformPoint(transformedCenterGlobal);
                    switch (_addedColliderType)
                    {
                        case EColliderType.Sphere:
                            (_addedCollider as SphereCollider).center = transformedCenterLocal;
                            break;
                        case EColliderType.Capsule:
                            (_addedCollider as CapsuleCollider).center = transformedCenterLocal;
                            break;
                        case EColliderType.Box:
                            (_addedCollider as BoxCollider).center = transformedCenterLocal;
                            break;
                    }
                }
                _lastSlidePosZ = slidePosZ;
            }
        }       
    }
}