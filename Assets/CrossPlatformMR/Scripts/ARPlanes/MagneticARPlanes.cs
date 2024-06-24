using Fusion.Addons.VisionOsHelpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MagneticARPlanes : MonoBehaviour
{
    MeshRenderer meshRenderer;
    ARPlane plane;
    LineMesh lineMesh;
    [SerializeField]
    float boundaryWidth = 0.02f;
    [SerializeField]
    Color boundaryColor = new Color(51, 153, 255);
    [SerializeField]
    bool addFirstPointToBoundaryEnd = true;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        plane = GetComponentInParent<ARPlane>();
        lineMesh = GetComponentInChildren<LineMesh>();

    }

    void OnEnable()
    {
        plane.boundaryChanged += OnBoundaryChanged;
        OnBoundaryChanged(default(ARPlaneBoundaryChangedEventArgs));
    }

    void OnDisable()
    {
        plane.boundaryChanged += OnBoundaryChanged;
    }

    private void OnBoundaryChanged(ARPlaneBoundaryChangedEventArgs args)
    {
        if (lineMesh == null || plane == null) return;
        Debug.Log("Updating boundary: "+ plane.boundary.Length);
        var boundary = plane.boundary;
        lineMesh.points.Clear();
        lineMesh.ResetMesh();

        while (boundary.Length > lineMesh.points.Count)
        {
            var i = lineMesh.points.Count;
            var point = new LineMesh.Point();
            var boundaryPoint = boundary[i];

            point.relativePosition = new Vector3(boundaryPoint.x, 0, boundaryPoint.y);
            point.color = boundaryColor;
            point.pressure = boundaryWidth;
            lineMesh.points.Add(point);
            lineMesh.UpdateMesh();
        }
        if (boundary.Length > 0 && addFirstPointToBoundaryEnd) {
            var point = new LineMesh.Point();
            var boundaryPoint = boundary[0];
            point.relativePosition = new Vector3(boundaryPoint.x, 0, boundaryPoint.y);
            point.color = boundaryColor;
            point.pressure = boundaryWidth;
            lineMesh.points.Add(point);
            lineMesh.UpdateMesh();
        }
    }

    private void Update()
    {
        var isNotGround = Mathf.Abs(transform.position.y) > 0.15f;
        if (isNotGround != meshRenderer.enabled)
        {
            //Debug.Log($"Changing planes visibility: isNotGround:{isNotGround} y:{transform.position.y}");
        }
        meshRenderer.enabled = isNotGround;
    }
}
