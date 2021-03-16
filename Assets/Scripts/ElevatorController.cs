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
    
    [Tooltip("Whether to carry the player with the elevator")]
    [Header("Cutcene")]
    public bool CarryPlayer = false;
    [Tooltip("If assigned, play the cutscene when activated")]
    public CutsceneData StartCutscene = null;
    [Tooltip("If assigned, play the cutscene when reaching the end of its path")]
    public CutsceneData EndCutscene = null;

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
                if (EndCutscene != null)
                {
                    GameObject player = GameManager.Instance.GetPlayer();
                    if (player != null)
                    {
                        player.GetComponent<PlayerController>().StartCutscene(EndCutscene);
                    }
                }
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

                GameObject player = GameManager.Instance.GetPlayer();
                if (player != null)
                {
                    // carry the player
                    if (CarryPlayer)
                    {
                        player.transform.SetParent(transform);
                    }
                    // start the cutscene
                    if (StartCutscene != null)
                    {
                        player.GetComponent<PlayerController>().StartCutscene(StartCutscene);
                    }
                }
            }
        }
    }
}
