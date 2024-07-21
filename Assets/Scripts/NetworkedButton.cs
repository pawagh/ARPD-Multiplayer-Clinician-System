using Fusion;
using UnityEngine;
using UnityEngine.Events;

public class NetworkedButton : NetworkBehaviour
{
    public UnityEvent onPress;

    [Networked] public NetworkBool pressed { get; set; }

    public void Press()
    {
        if (Object.HasInputAuthority)
        {
            RPC_PressButton();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_PressButton()
    {
        pressed = true;
        onPress.Invoke();
        pressed = false;
    }
}