using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Prevents objects in the player's hands from getting moved below their feet
/// </summary>

public class PlayerHandController : MonoBehaviour
{
    public Transform PlayerTransform;
    public Transform CameraTransform;
    public float LowerLimitAngle = -60f;

    // Update is called once per frame
    void Update()
    {
        // angle self if player looking too far down
        float angleOffset = -Vector3.SignedAngle(PlayerTransform.forward, CameraTransform.forward, PlayerTransform.right);
        if (angleOffset < LowerLimitAngle)
        {
            transform.localRotation = Quaternion.Euler(angleOffset-LowerLimitAngle,0,0);
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }
}
