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
        public FVRQuickBeltSlot[] SubQuickBeltSlots;

        [HideInInspector]
        public MeshRenderer[] MeshRenderers;

        [Tooltip("Must be left at Default if the part doesn't have multiple skins.")]
        public string SelectedModularWeaponPartSkinID = "Default";

        private Transform[] _childObjects;

        protected List<Transform> _objectsToKeep = new();

        public virtual void Awake()
        {
            MeshRenderers = GetComponentsInChildren<MeshRenderer>();

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

        public void ApplySkin(ModularWorkshopSkinsDefinition.SkinDefinition skinDefinition)
        {
            for (int i = 0; i < MeshRenderers.Length; i++)
            {
                MeshRenderers[i].materials = skinDefinition.DifferentSkinnedMeshPieces[i].Materials;
            }

            SelectedModularWeaponPartSkinID = skinDefinition.ModularSkinID;
        }

        [ContextMenu("Display Mesh Order")]
        public void DisplayMeshOrder()
        {
            MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < meshes.Length; i++)
            {
                Debug.Log($"MeshRenderer on GameObject {meshes[i].gameObject.name} has been assigned with index {i}.");
            }
        }
    }
}
