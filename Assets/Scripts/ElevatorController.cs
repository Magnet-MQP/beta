using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the elevator in the first level
/// </summary>

public class ElevatorController : ARemoteControllable
{
    [Tooltip("The set of positions the elevator moves between")]
    public Vector3[] Positions;
    [Tooltip("The position the elevator is currently targetting")]
    private int targetPositionID = 0;
    [Tooltip("The set of switches that have activated the elevator")]
    private List<IRemoteController> usedSwitches = new List<IRemoteController>();
    [Tooltip("The speed at which the elevator will move")]
    public float MoveSpeed;
    [Tooltip("The audio source on this object")]
    public AudioSource SoundPlayer;

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = Positions[targetPositionID];
        if (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, MoveSpeed*Time.deltaTime);
            if (transform.position == targetPosition)
            {
                SoundPlayer.Stop();
            }
        }
    }

    // When activated, record the switch used and advance to the next position if possible
    public override void RemoteActivate(IRemoteController controller)
    {
        if (!usedSwitches.Contains(controller))
        {
            usedSwitches.Add(controller);
            int activationCount = usedSwitches.Count;
            if (activationCount < Positions.Length)
            {
                targetPositionID = activationCount;
                SoundPlayer.Play();
            }
        }
    }
}
