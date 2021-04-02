using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the trash compactor walls in the opening
/// </summary>

public class CompactorController : MonoBehaviour
{
    [Tooltip("Whether the compactor is active")]
    public bool StartUp = true; // Set to false to disable auto-run
    [Tooltip("The end position the compactor is moving towards")]
    public Vector3 EndOffset;
    private Vector3 endPosition;
    [Tooltip("The move speed of the compactor")]
    public float MoveSpeed = 1;
    [Tooltip("The delay before the compactor starts moving")]
    public float MoveWait = 2f;
    [Tooltip("The audio source on this object")]
    public AudioSource SoundPlayer;

    void Start()
    {
        endPosition = transform.position + EndOffset;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Count down timer
        if (MoveWait > 0)
        {
            MoveWait -= Time.deltaTime;
            if (MoveWait <= 0)
            {
                SoundPlayer.Play();
            }
            return;
        }

        // Move if active and able
        if (StartUp && transform.position != endPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPosition, MoveSpeed*Time.deltaTime);
        }
    }

    /// <summary>
    /// Starts the compacing animation remotely (if not already started)
    /// <summary>
    public void StartCompacting()
    {
        StartUp = true;
        SoundPlayer.Play();
    }
}
