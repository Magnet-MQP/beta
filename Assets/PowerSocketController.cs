using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A power cell socket
/// needs a trigger collider
/// </summary>

public class PowerSocketController : MonoBehaviour
{
    public GameObject MockPowerCell;
    public bool Activated = false;

    void OnTriggerEnter(Collider other)
    {
        // skip if activated
        if (Activated)
        {
            return;
        }

        // destroy the object that was attached to it and display as attached
        Object_PhysicsObject otherPhysics = other.GetComponent<Object_PhysicsObject>();
        if (otherPhysics != null)
        {
            Activated = true;
            MockPowerCell.SetActive(true);
            otherPhysics.DeleteSelf();
        }
    }
}
