using UnityEngine;

public class FollowSmoother
{
    [System.Serializable]
    public struct SmoothingConfig
    {
        public float menuMoveSpeed;//  1
        public float menuRotationSpeed;//  180
        public float noMoveDistance;// 0.05f
        public float noMoveRotation;// 5f
        public float targetReachedDistance;// 0.001f
        public float targetReachedAngle;// 0.5f
        public float teleportDistance;// 0.5f

    }
    Pose lastPose;
    float lastMove = -1;

    bool lastTargetPositionReached = false;
    bool lastTargetRotationReached = false;

    public SmoothingConfig smoothingConfig;

    public Pose SmoothPoseChangeRequest(Pose pose, bool forceMove)
    {
        Pose smoothedPose;
        if(smoothingConfig.teleportDistance != 0 && lastMove != -1 && Vector3.Distance(lastPose.position, pose.position) > smoothingConfig.teleportDistance)
        {
            forceMove = true;
        }
        if (forceMove || lastMove == -1)
        {
            smoothedPose = pose;
            lastTargetPositionReached = true;
            lastTargetRotationReached = true;
        }
        else
        {
            if (lastTargetPositionReached && ((lastPose.position - pose.position).magnitude < smoothingConfig.noMoveDistance))
            {
                // No need to move (we are already at a position previously requested, and did not moved a lot)
                smoothedPose.position = lastPose.position;
            }
            else
            {
                lastTargetPositionReached = false;
                float delta = Time.time - lastMove;
                smoothedPose.position = Vector3.Lerp(lastPose.position, pose.position, smoothingConfig.menuMoveSpeed * delta);
                if ((smoothedPose.position - pose.position).magnitude < smoothingConfig.targetReachedDistance)
                {
                    lastTargetPositionReached = true;
                }
            }

            if (lastTargetRotationReached && Quaternion.Angle(lastPose.rotation, pose.rotation) < smoothingConfig.noMoveRotation)
            {
                // No need to move (we are already at a position previously requested, and did not moved a lot)
                smoothedPose.rotation = lastPose.rotation;
            }
            else
            {
                lastTargetRotationReached = false;
                float delta = Time.time - lastMove;
                smoothedPose.rotation = Quaternion.Slerp(lastPose.rotation, pose.rotation, smoothingConfig.menuRotationSpeed * delta);
                if (Quaternion.Angle(smoothedPose.rotation, pose.rotation) < smoothingConfig.targetReachedAngle)
                {
                    lastTargetRotationReached = true;
                }
            }
        }
        lastMove = Time.time;
        lastPose = smoothedPose;
        /*
        var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.transform.localScale = 0.01f * Vector3.one;
        c.transform.position = smoothedPose.position;
        */
        return smoothedPose;
    }
}
