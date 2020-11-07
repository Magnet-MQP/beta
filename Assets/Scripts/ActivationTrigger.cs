using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers an event on collision with the player when overlapped
/// </summary>

public class ActivationTrigger : MonoBehaviour, IRemoteController
{
    [Tooltip("The set of control targets to activate when used")]
    public ARemoteControllable[] ControlTargets;

    // Activate all targets when touched, then destroys self
    void  OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (ARemoteControllable rc in ControlTargets)
            {
                rc.RemoteActivate(this);
            }
            Destroy(this.gameObject);
        }
    }
}
