using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.Linq;

namespace ModularWorkshop
{
    public class ModularWeaponPart : MonoBehaviour
    {
        public string Name;
        public Sprite Icon;
        public FVRFireArmAttachmentMount[] AttachmentMounts;

        public ModularWeaponPartsAttachmentPoint[] SubAttachmentPoints;
        public FVRQuickBeltSlot[] SubQuickBeltSlots;

        [Tooltip("Fill out with context menu if you wanna know the order of meshes for skins.")]
        public MeshRenderer[] MeshRenderers;

        [Tooltip("Must be left at Default if the part doesn't have multiple skins.")]
        public string SelectedModularWeaponPartSkinID = "Default";

        [Header("Optional")]
        [Tooltip("Contains all physics colliders of the part. Use for even better performance by flattening out the hierarchy.")]
        public Transform PhysContainer;
        public bool ParentToFirearm = false;

        private Transform[] _childObjects;

        protected List<Transform> _objectsToKeep = new();

        public virtual void Awake()
        {
            if (MeshRenderers == null || MeshRenderers.Length == 0) MeshRenderers = GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled).ToArray();

            foreach (var point in SubAttachmentPoints)
            {
                point.ModularPartUIPoint.SetParent(transform);
                point.ModularPartUIPointProxy = new(point.ModularPartUIPoint, true);
            }

            if (PhysContainer != null)
            {
                foreach (Transform child in PhysContainer)
                {
                    child.SetParent(transform);
                }

                Destroy(PhysContainer.gameObject);
            }

            _childObjects = this.GetComponentsInDirectChildren<Transform>(true);
            Transform firearmParent = null;
            if (ParentToFirearm)
            {
                FVRFireArm fireArm = GetComponentInParent<FVRFireArm>();
                if (fireArm != null) firearmParent = fireArm.transform;
            }

            foreach (Transform child in _childObjects)
            {
                if (!ParentToFirearm) child.SetParent(transform.parent);
                else if (ParentToFirearm && firearmParent != null) child.SetParent(firearmParent);
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

        public void ApplySkin(ModularWorkshopSkinsDefinition.SkinDefinition skinDefinition)
        {
            try
            {
                for (int i = 0; i < MeshRenderers.Length; i++)
                {
                    MeshRenderers[i].materials = skinDefinition.DifferentSkinnedMeshPieces[i].Materials;
                }

                SelectedModularWeaponPartSkinID = skinDefinition.ModularSkinID;
            }
            catch (Exception)
            {
                Debug.LogError($"Number of DifferentSkinnedMeshPieces in SkinDefinition {skinDefinition.ModularSkinID} does not match number of meshes on ModularWeaponPart {Name}! ({MeshRenderers.Length} vs {skinDefinition.DifferentSkinnedMeshPieces.Length})");
            }
        }

        [ContextMenu("Display Mesh Order")]
        public void DisplayMeshOrder()
        {
            MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled).ToArray();

            for (int i = 0; i < meshes.Length; i++)
            {
                Debug.Log($"MeshRenderer on GameObject {meshes[i].gameObject.name} has been assigned with index {i}.");
            }
        }

        [ContextMenu("Fill out Mesh Renderers")]
        public void FillMeshRenderes()
        {
            MeshRenderers = GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled).ToArray();
        }
    }
}
