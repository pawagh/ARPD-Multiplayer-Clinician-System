using Fusion;
using UnityEngine;
using UnityEngine.XR;

public class VRPlayerScript : NetworkBehaviour  //THIS IS FOR THE 3D DRAWING
{
    public Transform LeftHand;
    public Transform RightHand;

    private DrawingManagerScript _drawingManager;
    private bool _wasDrawing = false;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            var xrRig = FindObjectOfType<UnityEngine.XR.Management.XRManagerSettings>().activeLoader.GetLoadedSubsystem<XRInputSubsystem>();
            xrRig.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);

            _drawingManager = GetComponent<DrawingManagerScript>();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasInputAuthority)
        {
            UpdateHandPositions();
            CheckDrawingInput();
        }
    }

    private void UpdateHandPositions()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPosition);
        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRotation);
        LeftHand.SetPositionAndRotation(leftPosition, leftRotation);

        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPosition);
        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRotation);
        RightHand.SetPositionAndRotation(rightPosition, rightRotation);
    }

    private void CheckDrawingInput()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed);

        if (triggerPressed && !_wasDrawing)
        {
            _drawingManager.RpcStartDrawing(RightHand.position);
            _wasDrawing = true;
        }
        else if (triggerPressed && _wasDrawing)
        {
            _drawingManager.RpcUpdateDrawing(RightHand.position);
        }
        else if (!triggerPressed && _wasDrawing)
        {
            _drawingManager.RpcStopDrawing();
            _wasDrawing = false;
        }
    }
}