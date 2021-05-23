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
    public float MaxDelay = 0f;
    private float delay = 0f;
    private bool touchingPlayer = false;
    private GameManager GM;
    //private PlayerInput m_playerInput;

    void Start()
    {
        GM = GameManager.getGameManager();
    }

    void Update()
    {
        if (touchingPlayer)
        {
            if (delay > MaxDelay)
            {
                DisplayMessage();
            }
            else
            {
                delay += Time.deltaTime;
            }
        }
        else
        {
            delay = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            touchingPlayer = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            touchingPlayer = false;
        }
    }

    private void DisplayMessage()
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

