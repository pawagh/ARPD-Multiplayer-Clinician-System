using System.Collections.Generic;
using Fusion;
using Fusion.XR.Shared.Rig;
using TMPro;
using UnityEngine;

public class LogPanel : UpdateConnectionStatus
{
    [Header("Headset following")]
    [SerializeField]
    float distanceFromHead = 0.6f;
    [SerializeField]
    GameObject menuGameObject;
    [SerializeField]
    GameObject progressBar;
    [SerializeField]
    float nonErrorMessageDuration = 5;
    [SerializeField]
    bool moveOnlyOnActivation = false;
    [SerializeField]
    bool inverseHeadRotation = false;
    [SerializeField]
    FollowSmoother.SmoothingConfig smoothingConfig = new FollowSmoother.SmoothingConfig
    {
        menuMoveSpeed = 1,
        menuRotationSpeed = 2,
        noMoveDistance = 0.3f,
        noMoveRotation = 30f,
        targetReachedDistance = 0.05f,
        targetReachedAngle = 2f,
        teleportDistance = 1f
    };
    FollowSmoother followSmoother;
    List<HardwareRig> hardwareRigs = new List<HardwareRig>();
    bool firstPositioningDone = false;
    float endDisplayTime = -1;

    public TextMeshPro status;


    protected override void FindRunner()
    {
        // Find the associated runner, if not defined
        if (runner == null) runner = FindObjectOfType<NetworkRunner>(true);
        if (runner == null)
        {
            Debug.LogError("NetworkRunner not found");
            return;
        }
    }

    protected override void DebugLog(string debug, bool permanentError)
    {
        if(status)
            status.text = debug;

        if (permanentError)
        {
            Debug.LogError(debug);
        }
        else
        {
            Debug.Log(debug);
        }


        if (menuGameObject.activeSelf == false) firstPositioningDone = false;
        menuGameObject.SetActive(true);
        if (permanentError || nonErrorMessageDuration == 0)
        {
            endDisplayTime = -1;
        }
        else
        {
            endDisplayTime = Time.time + nonErrorMessageDuration;
        }
        MoveMenu();
    }

    protected override void Start()
    {
        base.Start();
        if(menuGameObject == null)
        {
            menuGameObject = gameObject;
        }
        followSmoother = new FollowSmoother();
        followSmoother.smoothingConfig = smoothingConfig;
    }

    bool TryFindActiveRig(out HardwareRig activeRig)
    {
        if(hardwareRigs.Count == 0){
            hardwareRigs = new List<HardwareRig>(FindObjectsOfType<HardwareRig>(true));
        }
        foreach(var rig in hardwareRigs)
        {
            if (rig.gameObject.activeSelf)
            {
                activeRig = rig;
                return true;
            }
        }
        activeRig = null;
        return false;
    }

    private void Update()
    {
        if (menuGameObject.activeSelf)
        {
            if (endDisplayTime != -1 && Time.time > endDisplayTime)
            {
                menuGameObject.SetActive(false);
            }
            else if (progressBar)
            {
                if (endDisplayTime == -1)
                {
                    progressBar.SetActive(false);
                }
                else
                {
                    progressBar.SetActive(true);
                    float progress = Mathf.Clamp01((endDisplayTime - Time.time) / nonErrorMessageDuration);
                    progressBar.transform.localScale = new Vector3(progress, 1, 1);
                }
            }
        }
        if(moveOnlyOnActivation == false)
        {
            MoveMenu();
        }
    }

    protected void MoveMenu()
    { 
        if(menuGameObject.activeSelf && TryFindActiveRig(out var rig))
        {
            followSmoother.smoothingConfig = smoothingConfig;


            Pose menuPose;
            menuPose.position = rig.headset.transform.position + rig.headset.transform.forward * distanceFromHead;


            // Look at the user
            var direction = rig.headset.transform.position - menuPose.position;
            if (inverseHeadRotation)
            {
                direction = -direction;
            }
            menuPose.rotation = Quaternion.LookRotation(direction);

            bool forceMove = firstPositioningDone == false;
            firstPositioningDone = true;
            var smoothedPose = followSmoother.SmoothPoseChangeRequest(menuPose, forceMove);
            menuGameObject.transform.position = smoothedPose.position;
            menuGameObject.transform.rotation = smoothedPose.rotation;
        }
    }
}
