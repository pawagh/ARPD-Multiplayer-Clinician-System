using UnityEngine;
using Fusion.XR.Shared.Grabbing;

public class VRButtonInteraction : MonoBehaviour
{
    private NetworkedButton networkedButton;

    private void Start()
    {
        networkedButton = GetComponent<NetworkedButton>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has a Grabber component
        if (other.GetComponent<Grabber>() != null)
        {
            networkedButton.Press();
        }
    }
}