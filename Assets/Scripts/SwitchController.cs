using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An interactable switch that controls specific IRemoteControlled object(s)
/// </summary>

public class SwitchController : MonoBehaviour, IRemoteController
{
    [Tooltip("The set of control targets to activate when used")]
    public ARemoteControllable[] ControlTargets;
    [Tooltip("The audio source for this object")]
    public AudioSource SoundPlayer;
    [Tooltip("Tracks whether the switch has been used")]
    private bool used = false;

    // Activate all targets when used
    public void UseSwitch()
    {
        if (!used)
        {
            foreach (ARemoteControllable rc in ControlTargets)
            {
                rc.RemoteActivate(this);
            }
            used = true;
            SoundPlayer.Play();
        }
    }
}
