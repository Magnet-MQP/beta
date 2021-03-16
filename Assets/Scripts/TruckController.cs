using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckController : ARemoteControllable
{
    [Tooltip("The target end position of the truck")]
    public Vector3 EndPosition;
    [Tooltip("The delay before showingthe cutscene")]
    public float CutsceneDelay = 8f;
    [Tooltip("The cutscene to play when reaching the end")]
    public CutsceneData Cutscene;
    [Tooltip("The audio source for this object")]
    public AudioSource SoundPlayer;
    private bool activated = false;
    private float timer = 14f;
    private bool messaged = false;

    void Start()
    {
        timer = CutsceneDelay + Cutscene.FullDuration;
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            transform.position = Vector3.Lerp(transform.position, EndPosition, Time.deltaTime);
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                if (timer <= Cutscene.FullDuration && !messaged)
                {
                    GameObject player = GameManager.Instance.GetPlayer();
                    if (player != null)
                    {
                        player.GetComponent<PlayerController>().StartCutscene(Cutscene);
                    }
                    messaged = true;
                }
                if (timer <= 0)
                {
                    // advance to next scene
                    GameManager.Instance.nextScene();
                }
            }
        }
    }

    public override void RemoteActivate(IRemoteController controller)
    {
        activated = true;
        SoundPlayer.Play();
    }
}
