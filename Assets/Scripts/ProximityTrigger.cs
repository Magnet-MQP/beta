using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a group of elevator lights to activate them when the player is in proximity
/// </summary>
public class ProximityTrigger : MonoBehaviour
{
    [Tooltip("The set of of objects in the group. Calling them lights is a silly legacy thing")]
    public GameObject[] Lights;

    // Start is called before the first frame update
    void Start()
    {
       ActivateObjects(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            ActivateObjects(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            ActivateObjects(false);
        }
    }

    private void ActivateObjects(bool state)
    {
        foreach (GameObject light in Lights)
        {
            light.SetActive(state);
        }
    }
}
