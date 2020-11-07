using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckController : ARemoteControllable
{
    [Tooltip("The target end position of the truck")]
    public Vector3 EndPosition;
    [Tooltip("The audio source for this object")]
    public AudioSource SoundPlayer;
    private bool activated = false;
    private float timer = 14f;
    private bool messaged = false;

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            transform.position = Vector3.Lerp(transform.position, EndPosition, Time.deltaTime);
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                if (timer <= 6f && !messaged)
                {
                    SubtitleManager.Instance.QueueSubtitle(new SubtitleData("END OF DEMO", 5000, 6f));
                    messaged = true;
                }
                if (timer <= 0)
                {
                    // return to menu
                    GameManager.Instance.mainMenu();
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
