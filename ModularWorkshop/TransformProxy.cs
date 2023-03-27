using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using Steamworks;

namespace ModularWorkshop
{
    [Serializable]
    public class TransformProxy
    {
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;

        public TransformProxy(Transform transform)
        {
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
        }

        public TransformProxy(Transform transform, Transform parent)
        {
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
            this.parent = parent;
        }

        public Vector3 position
        {
            get { return parent.TransformPoint(localPosition); }
        }
        public Quaternion rotation
        {
            get { return parent.TransformRotation(localRotation); }
        }

        public Vector3 GetGlobalPosition(Transform parent)
        {
            return parent.TransformPoint(localPosition);
        }
        public Quaternion GetGlobalRotation(Transform parent)
        {
            return parent.TransformRotation(localRotation);
        }
    }
}