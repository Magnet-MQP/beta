using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Advances to the next scene when touched by the player
public class AdvanceScene : MonoBehaviour
{
    public CutsceneData TransitionCutscene;
    private bool activated = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !activated)
        {
            other.GetComponent<PlayerController>().StartCutscene(TransitionCutscene);
            activated = true;
        }
    }
}
