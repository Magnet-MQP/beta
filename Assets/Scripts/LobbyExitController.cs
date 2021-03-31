using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyExitController : ARemoteControllable
{
    private bool active = false;
    [Tooltip("The left door")]
    public GameObject DoorL;
    [Tooltip("The right door")]
    public GameObject DoorR;
    [Tooltip("The duration of the opening animation")]
    public float OpenDuration = 2f;
    private float openTimer = 0f;
    private Vector3 leftDoorPos1;
    private Vector3 leftDoorPos2;
    private Vector3 rightDoorPos1;
    private Vector3 rightDoorPos2;

    void Start()
    {
        leftDoorPos1 = DoorL.transform.position;
        leftDoorPos2 = leftDoorPos1 + Vector3.left*DoorL.transform.localScale.x;
        rightDoorPos1 = DoorR.transform.position;
        rightDoorPos2 = rightDoorPos1 - Vector3.left*DoorR.transform.localScale.x;
    }

    void Update()
    {
        if (active)
        {
            if (openTimer < OpenDuration)
            {
                openTimer = Mathf.Min(OpenDuration, openTimer + Time.deltaTime);
                float progress = openTimer/OpenDuration;
                DoorL.transform.position = leftDoorPos1*(1-progress) + leftDoorPos2*progress;
                DoorR.transform.position = rightDoorPos1*(1-progress) + rightDoorPos2*progress;
            }
            else
            {
                active = false;
            }
        }
    }

    /// <summary>
    /// Activate the door
    /// </summary>
    public override void RemoteActivate(IRemoteController controller)
    {
        active = true;
    }
}
