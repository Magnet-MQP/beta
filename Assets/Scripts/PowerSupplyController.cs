using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSupplyController : MonoBehaviour, IRemoteController
{
    [Tooltip("The set of sockets attached to the Power Supply")]
    public PowerSocketController[] Sockets;
    [Tooltip("The object to move when powered")]
    public ARemoteControllable ActivationTarget;
    [Tooltip("Whether the power supply is operational")]
    private bool powerActivated = false;
    [Tooltip("The message to display when fully activated")]
    public string ActiveMessage = "";
    [Tooltip("Audio player for activation sound")]
    public AudioSource SoundSource;
    [Tooltip("Completion audio clip")]
    public AudioClip CompletionClip;

    // Update is called once per frame
    void Update()
    {
        //track socket progress
        foreach (PowerSocketController socket in Sockets)
        {
            if (!socket.Activated)
            {
                return; // early exit
            }
        }
        // if all sockets active, proceed
        if (!powerActivated)
        {
            powerActivated = true;
            ActivationTarget.RemoteActivate(this);
            SoundSource.PlayDelayed(1.5f);
            if (ActiveMessage != "")
            {
                SubtitleManager.Instance.QueueSubtitle(new SubtitleData(ActiveMessage, 5000));
            }
        }
    }
}
