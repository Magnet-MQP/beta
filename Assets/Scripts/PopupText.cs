using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

/// <summary>
/// Used to make a one-time text pop-up at a location
/// </summary>

public class PopupText : MonoBehaviour
{
    public SubtitleData sd;
    public string actionName;
    private GameManager GM;
    //private PlayerInput m_playerInput;

    void start()
    {
        GM = GameManager.getGameManager();
    }

    void OnTriggerEnter(Collider other)
    {
        GM = GameManager.getGameManager();
        if (other.CompareTag("Player"))
        {
            string newText = sd.message;
            if (actionName != "")
            {
                string actionText = GM.getActionName(actionName);
                newText = newText.Replace("_", actionText);
            }
            sd.message = newText;
            SubtitleManager.Instance.QueueSubtitle(sd);
            //other.gameObject.GetComponent<PlayerController>().DisplaySubtitle(sd);
            //Destroy(this.gameObject);
        }
    }
}

