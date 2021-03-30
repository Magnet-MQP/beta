using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a group of elevator lights to activate them when the player is in proximity
/// </summary>
public class ProximityLight : MonoBehaviour
{
    [Tooltip("The set of of lights in the light group")]
    public GameObject[] Lights;

    // Start is called before the first frame update
    void Start()
    {
       ActivateLights(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            ActivateLights(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            ActivateLights(false);
        }
    }

    private void ActivateLights(bool state)
    {
        foreach (GameObject light in Lights)
        {
            light.SetActive(state);
        }
    }
}
