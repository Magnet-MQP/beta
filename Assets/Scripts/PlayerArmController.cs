using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player arm movement
/// </summary>

public class PlayerArmController : MonoBehaviour
{
    [Tooltip("The arm model itself")]
    public GameObject Arm;
    [Tooltip("The endpoint of the arm")]
    public Transform ArmEnd;
    [Tooltip("The arm material")]
    public Renderer ArmRenderer;
    private Vector3 shoulderStartPos; // starting position of the shoulder
    private Vector3 shoulderStartAngle; // starting angle of the shoulder
    private Vector3 armStartPos; // starting position of the arm
    private float armLength; // the length of the arm
    private Vector3 shoulderPositionOffset; // the position offset of the arm
    private Vector3 shoulderAngleOffset; // the angle offset of the arm
    private float armSnapSpeed = 6f;

    // Start is called before the first frame update
    void Start()
    {
        // store shoulder local transform
        shoulderStartPos = gameObject.transform.localPosition;
        shoulderStartAngle = gameObject.transform.localEulerAngles;
        // store arm local transform
        armStartPos = Arm.transform.localPosition;
        // calculate default arm length
        armLength = Vector3.Distance(gameObject.transform.position, ArmEnd.position);
        // initialize arm and shoulder offsets
        shoulderPositionOffset = Vector3.zero;
        shoulderAngleOffset = Vector3.zero;

        // set enable emissive color changes
        ArmRenderer.material.EnableKeyword("_EMISSION");
    }

    // Update is called once per frame
    void Update()
    {
        // set orientation rotation
        gameObject.transform.localPosition = shoulderStartPos + shoulderPositionOffset;
        gameObject.transform.localEulerAngles = shoulderStartAngle + shoulderAngleOffset;

        // account for collision
        Vector3 startPos = gameObject.transform.position;
        float offset = 0;
        RaycastHit armHit;
        if (Physics.SphereCast(startPos, 0.2f, ArmEnd.position-startPos, out armHit, armLength, LayerMask.GetMask("Wall","Interactable"), QueryTriggerInteraction.Ignore))
        {
            offset = armLength-armHit.distance;
        }

        // set arm position
        Vector3 armTarget = armStartPos + new Vector3(0,offset,0.3f*offset);
        Arm.transform.localPosition += (armTarget-Arm.transform.localPosition)*armSnapSpeed*Time.deltaTime;
    }


    /// <summary>
    /// Updates the arm emissive
    /// </summary>
    public void SetArmEmissive(Color newColor)
    {
        ArmRenderer.material.SetColor("_EmissiveColor", newColor);
    }

     /// <summary>
    /// Sets the arm position offset
    /// </summary>
    public void SetArmPositionOffset(Vector3 offset)
    {
        shoulderPositionOffset = offset;
    }

    /// <summary>
    /// Sets the arm angle offset (roating about the shoulder)
    /// </summary>
    public void SetArmAngleOffset(Vector3 angles)
    {
        shoulderAngleOffset = angles;
    }
}
