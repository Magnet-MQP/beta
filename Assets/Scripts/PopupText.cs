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
    private GameObject touchingPlayer;
    private float breakRadius;
    private GameManager GM;
    //private PlayerInput m_playerInput;

    void Start()
    {
        GM = GameManager.getGameManager();
        breakRadius = Mathf.Max(transform.localScale.x, Mathf.Max(transform.localScale.y, transform.localScale.z));
    }

    void Update()
    {
        if (touchingPlayer != null)
        {
            // break contact if player is too far
            if (Vector3.Distance(transform.position, touchingPlayer.transform.position) > 1.5f)
            {
                touchingPlayer = null;
            }
            else
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
            touchingPlayer = other.gameObject;
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
    }
}

