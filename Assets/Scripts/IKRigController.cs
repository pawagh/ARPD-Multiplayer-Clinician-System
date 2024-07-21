using UnityEngine;
using Fusion;

public class IKRigController : NetworkBehaviour
{
    [SerializeField] private Transform[] bodyJoints;
    [SerializeField] private string[] jointNames;
    [SerializeField] private JSONLoader jsonLoader;

    [Networked] private int CurrentFrame { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CurrentFrame = 0;
            UpdateSkeleton(CurrentFrame);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            UpdateSkeleton(CurrentFrame);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateSkeleton(int frame)
    {
        CurrentFrame = frame;
        Vector3[] jointPositions = jsonLoader.GetFrameSafe(frame);
        ApplyJointPositions(jointPositions);
    }

    public void UpdateSkeleton(int frame)
    {
        if (Object.HasStateAuthority)
        {
            RPC_UpdateSkeleton(frame);
        }
    }

    private void ApplyJointPositions(Vector3[] jointPositions)
    {
        if (jointPositions == null || bodyJoints.Length != jointPositions.Length)
        {
            Debug.LogError($"Invalid joint data. Joints: {bodyJoints.Length}, Positions: {(jointPositions != null ? jointPositions.Length : 0)}");
            return;
        }

        for (int i = 0; i < bodyJoints.Length; i++)
        {
            if (bodyJoints[i] != null)
            {
                bodyJoints[i].localPosition = jointPositions[i];
            }
        }
    }

    public int GetTotalFrames()
    {
        return jsonLoader != null ? jsonLoader.GetFrameCount() : 0;
    }

    public int GetCurrentFrame()
    {
        return CurrentFrame;
    }
}