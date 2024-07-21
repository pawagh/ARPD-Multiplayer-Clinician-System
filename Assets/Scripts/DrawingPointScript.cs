using Fusion;
using UnityEngine;

public class DrawingPointScript : NetworkBehaviour
{
    public float PointSize = 0.01f;

    public override void Spawned()
    {
        transform.localScale = Vector3.one * PointSize;
    }
}