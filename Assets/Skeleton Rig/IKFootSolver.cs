using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Created by Ashley Neall – followed Unity's Procedural Walking Tutorial
// This script enables us to control the target (foot position) so that each foot (Right and Left) are always procedurally placed based on a mesh surface (e.g. Passthrough Plane)
// to add this script as a component, we must select BOTH the right and left leg TARGETs (bc we're using both legs!)

// Stretch Goal 1: make contact w a MR (quest passthrough) object in a room and identify this as the mesh surface

// Stretch Goal 2 (completed): offset the initial position of each foot so that they don't move at the same time
//  wasn't needed, but useful (we just need it standing correctly for an MVP)

public class IKFootSolver : MonoBehaviour
{
    // Global Variables:
        // Changed initial stepDistance, Length, and Height bc of the skeleton humanoid's scale
    [SerializeField] LayerMask terrainLayer = default;
    [SerializeField] Transform body = default;
    [SerializeField] IKFootSolver otherFoot = default;
    [SerializeField] float speed = 1;
    [SerializeField] float stepDistance = .5f;
    [SerializeField] float stepLength = .4f;
    [SerializeField] float stepHeight = .2f;
    [SerializeField] Vector3 footOffset = default;

    float footSpacing;
    Vector3 oldPosition, currentPosition, newPosition;
    Vector3 oldNormal, currentNormal, newNormal;
    public Vector3 footRotOffset; // to rotate the feet correctly
    float lerp;

    // Start is called before the first frame update
    void Start()
    {
        footSpacing = transform.localPosition.x;

        currentPosition = newPosition = oldPosition = transform.position;
        currentNormal = newNormal = oldNormal = transform.up;

        lerp = 1;
    } // end Start()

    // Update is called once per frame
    void Update()
    {
        transform.position = currentPosition;
        transform.up = currentNormal;
        transform.localRotation = Quaternion.Euler(footRotOffset); // this enables us to rotate the feet (local to the skeleton model)

        // casting a ray downward, toward the terrain / surface:
        Ray ray = new Ray(body.position + (body.right * footSpacing), Vector3.down);

        // now to make the feet procedurally move, we have to find the next 3D placement on the terrain / surface for the feet:
        if (Physics.Raycast(ray, out RaycastHit info, 10, terrainLayer.value))
        {

            // if the new position found on the terrain / surface is greater than the step distance, the feet move forward (to the info point):
            if (Vector3.Distance(newPosition, info.point) > stepDistance && !otherFoot.IsMoving() && lerp >= 1) // ensuring only EITHER the left or right foot is on the ground
            {
                lerp = 0;
                int direction = body.InverseTransformPoint(info.point).z > body.InverseTransformPoint(newPosition).z ? 1 : -1;
                newPosition = info.point + (body.forward * stepLength * direction) + footOffset;
                newNormal = info.normal;
            }
        }

        // using linear interpolation to move the target to the new position (info point):
        if (lerp < 1)
        {
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            tempPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight; // using Mathf.sin curve so that there's an 'arc' to the movement of the foot position

            currentPosition = tempPosition;
            currentNormal = Vector3.Lerp(oldNormal, newNormal, lerp);
            lerp += Time.deltaTime * speed;
        }
        else
        {
            oldPosition = newPosition; // updating foot's position
            oldNormal = newNormal; // updating foot's normal, which is useful for succeeding procedural steps!
        }
    } // end Update()

    public bool IsMoving()
    {
        return lerp < 1;
    }
}