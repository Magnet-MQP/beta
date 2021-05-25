using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An interactable switch that controls specific IRemoteControlled object(s)
/// </summary>

public class SwitchController : ARemoteControllable, IRemoteController
{
    [Tooltip("The set of control targets to activate when used")]
    public ARemoteControllable[] ControlTargets;
    [Tooltip("The musiccontroller associated. Can be null.")]
    public MusicController musicController;
    [Tooltip("The audio source for this object")]
    public AudioSource SoundPlayer;
    [Tooltip("Tracks whether the switch has been used")]
    private bool used = false;
    [Tooltip("Tracks whether the switch has power")]
    public bool HasPower = true;

    [Tooltip("The switch's button")]
    [Header("Visuals")]
    public GameObject Button;
    [Tooltip("The hinge for the switch's lever")]
    public GameObject LeverHinge;
    [Tooltip("The material to use when powered")]
    public Material MatReady;
    [Tooltip("The material to use when pressed or de-powered")]
    public Material MatInactive;

    [Tooltip("The text to show when active")]
    public string ActiveMessage;
    private TextProperty myText;

    void Start()
    {
        myText = GetComponent<TextProperty>();

        if (HasPower)
        {
            Button.GetComponent<Renderer>().material = MatReady;
        }
        else
        {
            Button.GetComponent<Renderer>().material = MatInactive;
            myText.Text = "This switch needs power";
        }
    }

    // Activate all targets when used
    public void UseSwitch()
    {
        if (!used && HasPower)
        {
            foreach (ARemoteControllable rc in ControlTargets)
            {
                rc.RemoteActivate(this);
            }
            used = true;
            Button.GetComponent<Renderer>().material = MatInactive;
            LeverHinge.transform.Rotate(new Vector3(100,0,0));
            myText.Text = "";
            SoundPlayer.Play();
            if(musicController != null){
                musicController.PlayNextTrack();
            }
        }
    }

    public override void RemoteActivate(IRemoteController controller)
    {
        HasPower = true;
        if (!used)
        {
            Button.GetComponent<Renderer>().material = MatReady;
            myText.Text = ActiveMessage;
        }
    }
}
