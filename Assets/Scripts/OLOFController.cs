using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates to track the player when in range, and plays certain audio clips on cue
/// </summary>

[System.Serializable]
public struct SubtitledAudio {
    public AudioClip Clip;
    public string SubtitleText;
    public int Priority;
}

public class OLOFController : ARemoteControllable
{
    [Tooltip("The joint connecting the head")]
    [Header("References")]
    public Transform HeadJoint;
    [Tooltip("The attached audio source")]
    public AudioSource Speaker;

    [Tooltip("The range at which to detect the player")]
    [Header("Parameters")]
    public float Range = 50f;
    [Tooltip("The factor applied to the body swivel speed")]
    public float SwivelSpeed = 2f;
    [Tooltip("The factor applied to the head tilt speed")]
    public float TiltSpeed = 2f;
    [Tooltip("The distance from the head center to the head joint")]
    public float HeadOffset = 5f;

    [Tooltip("0: Player Enters, 1: Player Lingers, 2: Player Leaves, 3: Player Uses Elevator")]
    [Header("Dialogue")]
    public SubtitledAudio[] AudioClips;
    [Tooltip("Delay before playing Player Lingers audio clip")]
    public float PlayerLingerDelay = 30f;
    [Tooltip("Position to detect player leaving from")]
    public Vector3 PlayerLeaveTriggerPosition;
    [Tooltip("Radius to detect player leaving from")]
    public float PlayerLeaveTriggerRadius;
    private bool playerDetected = false; // whether Player Enters dialogue has been triggered
    private float playerLingerTimer = 0f;
    private bool playerLeft = false; // whether Player Leaves dialogue has been triggered
    private float audioWait = 0f; // delay before a new audio clip can play

    private GameObject player;
    private Vector3 lookAtPosition;

    void Start()
    {
        // get player reference
        player = GameManager.Instance.GetPlayer();

        // start looking forwards
        lookAtPosition = transform.position;
    }

    void Update()
    {
        if (audioWait > 0)
        {
            audioWait -= Time.deltaTime;
        }

        Vector3 alignedPos = transform.position;
        if (player != null)
        {
            Vector3 detectionPosition = alignedPos + Vector3.forward*20f;
            detectionPosition.y = player.transform.position.y;

            // track player while talking or in range
            if (audioWait > 0 || Vector3.Distance(detectionPosition, player.transform.position) <= Range)
            {
                lookAtPosition = player.transform.position;

                // Play Dialogue 0 (Player Enters)
                if (!playerDetected)
                {
                    PlayAudio(0);
                    playerDetected = true;
                }
                // Play Dialogue 1 (Player Lingers)
                else
                {
                    playerLingerTimer += Time.deltaTime;
                    if (playerLingerTimer > PlayerLingerDelay)
                    {
                        PlayAudio(1);
                        playerLingerTimer = 0f;
                    }
                }
            }
        }
        alignedPos.y = lookAtPosition.y;
        if (playerDetected)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookAtPosition-alignedPos, Vector3.up), Time.deltaTime*SwivelSpeed);
            HeadJoint.rotation = Quaternion.Lerp(HeadJoint.rotation, Quaternion.LookRotation(lookAtPosition-(HeadJoint.position-HeadOffset*HeadJoint.right), Vector3.up),  Time.deltaTime*TiltSpeed);
        }
     
        // Play Dialogue 2 (Player Leaves)
        if (!playerLeft && Vector3.Distance(PlayerLeaveTriggerPosition, lookAtPosition) <= PlayerLeaveTriggerRadius)
        {
            lookAtPosition = PlayerLeaveTriggerPosition;
            audioWait = 0f; // overide exisiting audio
            PlayAudio(2);
            playerLeft = true;
        }
    }

    public override void RemoteActivate(IRemoteController controller)
    {
        // Play Dialogue 3 (Player Uses Elevator)
        if (player != null)
        {
            lookAtPosition = player.transform.position;
        }
        audioWait = 0f;
        PlayAudio(3);
    }

    private void PlayAudio(int id)
    {
        // skip if audio already playing
        if (audioWait > 0)
        {
            return;
        }

        // get clip
        SubtitledAudio thisAudio = AudioClips[id];

        // play clip
        Speaker.PlayOneShot(thisAudio.Clip);
        audioWait = thisAudio.Clip.length;

        // show subtitle
        if (player != null)
        {
            player.GetComponent<PlayerController>().DisplaySubtitle(new SubtitleData(thisAudio.SubtitleText, thisAudio.Priority, thisAudio.Clip.length));
        }
    }
}
