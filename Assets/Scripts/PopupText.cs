using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to make a one-time text pop-up at a location
/// </summary>

public class PopupText : MonoBehaviour
{
    public SubtitleData sd;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SubtitleManager.Instance.QueueSubtitle(sd);
            //other.gameObject.GetComponent<PlayerController>().DisplaySubtitle(sd);
            Destroy(this.gameObject);
        }
    }
}

