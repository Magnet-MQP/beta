using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to assign a polarity to an object.
/// </summary>

public class CompassController : MonoBehaviour
{
    void Update()
    {
        // Stay oriented correctly
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
    }
}
