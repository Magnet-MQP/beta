using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchPlayer : MonoBehaviour
{
    private GameObject player;

    void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    void Update()
    {
        if (player != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position-player.transform.position, Vector3.up);
        }
    }
}
