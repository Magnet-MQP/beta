using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A power cell socket
/// needs a trigger collider
/// </summary>

public class PowerSocketController : MonoBehaviour
{
    [Tooltip("The object reference of the displayed attached power cell")]
    public GameObject MockPowerCell;
    [Tooltip("Whether the power socket is activated")]
    public bool Activated = false;
    [Tooltip("Audio player for activation sound")]
    public AudioSource SoundSource;

    void OnTriggerEnter(Collider other)
    {
        // skip if activated
        if (Activated)
        {
            return;
        }

        // destroy the object that was attached to it and display as attached
        Object_PhysicsObject otherPhysics = other.GetComponent<Object_PhysicsObject>();
        if (otherPhysics != null && otherPhysics.Consumable && !otherPhysics.Consumed)
        {
            Activated = true;
            MockPowerCell.SetActive(true);
            SoundSource.Play();
            otherPhysics.DeleteSelf();
        }
    }
}
