using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.XR.Shared.Grabbing;

namespace Fusion.XRShared.GrabbableMagnet
{
    [DefaultExecutionOrder(MagnetPoint.EXECUTION_ORDER)]
    public class MagnetPoint : NetworkBehaviour, IMovableMagnet
    {
        public const int EXECUTION_ORDER = NetworkGrabbable.EXECUTION_ORDER + 5;
        [HideInInspector]
        public NetworkTRSP rootNTRSP;
        NetworkGrabbable networkGrabbable;
        Rigidbody rb;
        public float magnetRadius = 0.1f;
        public string magnetLayer = "Magnets";

        [Header("Snap options")]
        public bool isPlaneMagnet = false;
        [Tooltip("If false, only aligns on y axis")]
        public bool alignOnAllAxis = true;
        public LayerMask compatibleLayers;

        [Header("Snap animation")]
        public bool instantSnap = true;
        public float snapDuration = 1;

        public bool AlignOnAllAxis {
            get => alignOnAllAxis;
            set => alignOnAllAxis = value;
        }

        public float MagnetRadius
        {
            get => magnetRadius;
            set => magnetRadius = value;
        }

        public bool CheckOnUngrab { get; set; } = true;

        MagnetCoordinator _magnetCoordinator;
        public MagnetCoordinator MagnetCoordinator => _magnetCoordinator;


        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (snapRequest != null && networkGrabbable && networkGrabbable.IsGrabbed)
            {
                // Cancel snap
                snapRequest = null;
            }
            if (snapRequest != null)
            {
                DoSnapToMagnet(snapRequest);
            }
        }

        private void Awake()
        {
            _magnetCoordinator = GetComponentInParent<MagnetCoordinator>();

            rootNTRSP = GetComponentInParent<NetworkTRSP>();
            networkGrabbable = GetComponentInParent<NetworkGrabbable>();
            rb = GetComponentInParent<Rigidbody>();
            if(networkGrabbable) networkGrabbable.onDidUngrab.AddListener(OnDidUngrab);
            int layer = LayerMask.NameToLayer(magnetLayer);
            if (layer == -1)
            {
                Debug.LogError($"Please add a {magnetLayer} layer (it will be automatically be set to this object)");
            }
            else
            {
                gameObject.layer = layer;
                foreach (var collider in GetComponentsInChildren<Collider>())
                {
                    collider.gameObject.layer = layer;
                }
            }

            if (gameObject.layer == 0)
            {
                Debug.LogError($"A dedicated magnet layer should be used for better performances ({transform.root.name}/.../{name})");
            }
        }

        private void OnDidUngrab()
        {
            if (CheckOnUngrab)
            {
                CheckMagnetProximity();
            }
        }

        public bool TryFindClosestMagnetInRange(out IMagnet closestMagnet, out float minDistance)
        {
            var layerMask = compatibleLayers | (1 << gameObject.layer);
            var colliders = Physics.OverlapSphere(transform.position, magnetRadius, layerMask: layerMask);
            closestMagnet = null;
            minDistance = float.PositiveInfinity;
            for (int i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                IMagnet magnet = collider.GetComponentInParent<IMagnet>();
                if (magnet == null)
                {
                    Debug.LogError("No magnet");
                    continue;
                }
                if(magnet is IMovableMagnet movableMagnet)
                {
                    if (MagnetCoordinator != null && movableMagnet.MagnetCoordinator == MagnetCoordinator)
                    {
                        continue;
                    }
                }

                var distance = Vector3.Distance(transform.position, magnet.SnapTargetPosition(transform.position));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestMagnet = magnet;
                }
            }
            return closestMagnet != null;
        }

        [ContextMenu("CheckMagnetProximity")]
        public void CheckMagnetProximity()
        {
            if (Object && Object.HasStateAuthority && (networkGrabbable == null || networkGrabbable.IsGrabbed == false))
            {
                if (TryFindClosestMagnetInRange(out var closestMagnet, out _))
                {
                    SnapToMagnet(closestMagnet);
                }
            }
        }

        IMagnet snapRequest = null;
        float snapStart = -1;

        public void SnapToMagnet(IMagnet magnet)
        {
            snapRequest = magnet;
            snapStart = Time.time;
        }

        public void DoSnapToMagnet(IMagnet magnet)
        {
            float progress = 1;
            if (instantSnap)
            {
                snapRequest = null;
            }
            else
            {
                progress = (Time.time - snapStart) / snapDuration;
                if(progress >= 1)
                {
                    progress = 1;
                    snapRequest = null;
                }
            }
            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Rotate the parent NT to match the magnet positions
            Quaternion targetRotation;
            if (magnet.AlignOnAllAxis == false)
            {
                targetRotation = AdaptedRotationOnYAxis(magnet.transform);
            }
            else
            {
                targetRotation = AdaptedRotationOnAllAxis(magnet.transform);
            }
            ApplyRotation(targetRotation, progress);

            // Move the parent NT to match the magnet positions
            var targetPosition = magnet.SnapTargetPosition(transform.position);
            ApplyPosition(targetPosition, progress);
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

        Quaternion AdaptedRotationOnYAxis(Transform targetTransform)
        {
            var projectionPlane = new Plane(targetTransform.up, targetTransform.position);
            var forward = Vector3.ProjectOnPlane(transform.forward, targetTransform.up);
            var up = -targetTransform.up;
            var targetRotation = Quaternion.LookRotation(forward, up);
            return targetRotation;
        }


        Quaternion AdaptedRotationOnAllAxis(Transform targetTransform)
        {
            var upTarget = -targetTransform.up;
            var forwardCandidates = new Vector3[] { targetTransform.right, -targetTransform.right, targetTransform.forward, -targetTransform.forward };
            var forwardTarget = targetTransform.forward;
            var minAngle = float.PositiveInfinity;
            for (int i = 0; i < forwardCandidates.Length; i++)
            {
                var forwardCandidate = forwardCandidates[i];
                var angle = Vector3.Angle(transform.forward, forwardCandidate);
                if (angle < minAngle)
                {
                    minAngle = angle;
                    forwardTarget = forwardCandidate;
                }
            }

            var targetRotation = Quaternion.LookRotation(forwardTarget, upTarget);
            return targetRotation;
        }

        void ApplyRotation(Quaternion targetRotation, float progress)
        {
            var localMagnetRotation = Quaternion.Inverse(rootNTRSP.transform.rotation) * transform.rotation;
            var rotation = targetRotation * Quaternion.Inverse(localMagnetRotation);

            if (progress < 1) rotation = Quaternion.Slerp(rootNTRSP.transform.rotation, rotation, progress);

            if (rb)
            {
                rb.rotation = rotation;
                rootNTRSP.transform.rotation = rotation;
            }
            else
            {
                rootNTRSP.transform.rotation = rotation;
            }
        }

        void ApplyPosition(Vector3 targetPosition, float progress)
        {
            var position = targetPosition - transform.position + rootNTRSP.transform.position;
            if (progress < 1) position = Vector3.Lerp(rootNTRSP.transform.position, position, progress);
            if (rb)
            {
                rb.position = position;
                rootNTRSP.transform.position = position;
            }
            else
            {
                rootNTRSP.transform.position = position;
            }
        }
    }
}


