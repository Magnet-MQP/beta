using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Advances to the next scene when touched by the player
public class AdvanceScene : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameManager.Instance.nextScene();
        }
    }
}
