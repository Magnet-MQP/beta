using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyExitController : ARemoteControllable
{
    /// <summary>
    /// Activate the door
    /// (dummy implementation: disable self)
    /// </summary>
    public override void RemoteActivate(IRemoteController controller)
    {
        gameObject.SetActive(false);
    }
}
