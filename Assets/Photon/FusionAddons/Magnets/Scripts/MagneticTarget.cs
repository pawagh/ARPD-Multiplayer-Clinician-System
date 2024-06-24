using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XRShared.GrabbableMagnet
{
    public class MagneticTarget : MonoBehaviour, IMagnet
    {
        public string magnetLayer = "Magnets";
        [Header("Snap options")]
        public bool isPlaneMagnet = false;
        public bool alignOnAllAxis = true;

        private void Awake()
        {
            int layer = LayerMask.NameToLayer(magnetLayer);
            if (layer == -1)
            {
                Debug.LogError($"Please add a {magnetLayer} layer (it will be automatically be set to this object)");
            }
            else
            {
                gameObject.layer = layer;
                foreach(var collider in GetComponentsInChildren<Collider>())
                {
                    collider.gameObject.layer = layer;
                }
            }
        }

        #region IMagnet
        public bool AlignOnAllAxis
        {
            get => alignOnAllAxis;
            set => alignOnAllAxis = value;
        }

        public Vector3 SnapTargetPosition(Vector3 position)
        {
            if (isPlaneMagnet)
            {
                var projectionPlane = new Plane(transform.up, transform.position);
                // Project position on plane
                return projectionPlane.ClosestPointOnPlane(position);
            }
            else
            {
                return transform.position;
            }
        }
        #endregion
    }
}
