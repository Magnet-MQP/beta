using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls city building objects to simulate them moving
/// </summary>

public class CityScroll : MonoBehaviour
{
    public float MoveSpeed = 4f;
    public Rigidbody RB;
    public Vector3 MinSize = new Vector3(150, 200, 150);
    public Vector3 MaxSize = new Vector3(200, 500, 200);
    public float LeftBound;
    public float RightBound;

    // Resize the building and reposition accordingly
    public void SetNewScale()
    {
        transform.localScale = new Vector3(Random.Range(MinSize.x, MaxSize.x),
                                        Random.Range(MinSize.y, MaxSize.y),
                                        Random.Range(MinSize.z, MaxSize.z));
        float width = transform.localScale.x;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, LeftBound + width, RightBound - width),
                                        transform.localScale.y/2,
                                        transform.position.z);
    }

    public void Respawn(float distance)
    {
        transform.position += Vector3.forward * distance;
        SetNewScale();
    }

    void FixedUpdate()
    {
        RB.MovePosition(RB.position - Vector3.forward * MoveSpeed);
    }

}
