using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resets the room when the player falls out of bounds
/// </summary>

public class KillzoneController : MonoBehaviour
{
    [Tooltip("The cutscene to show when respawning the player")]
    public CutsceneData RespawnCutscene;

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().StartCutscene(RespawnCutscene);
        }
    }
}
