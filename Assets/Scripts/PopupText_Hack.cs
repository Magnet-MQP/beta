using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

/// <summary>
/// Used to make a one-time text pop-up at a location
/// </summary>

public class PopupText_Hack : MonoBehaviour
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
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DisplayMessage();
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

