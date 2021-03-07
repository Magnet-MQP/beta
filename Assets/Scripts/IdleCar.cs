using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleCar : MonoBehaviour
{
    [Tooltip("This object's rigidbody")]
    public Rigidbody RB;
    [Tooltip("The backward extent this car can float")]
    public float ReverseLimit = -10f;
    [Tooltip("The forward extent this car can float")]
    public float ForwardLimit = 10f;
    [Tooltip("The horizontal sway limit for this car")]
    public float HorizontalSwayBound = 5f;
    [Tooltip("The vertical sway limit for this car")]
    public float VerticalSwayBound = 5f;
    private float leftSwayBound;
    private float rightSwayBound;
    private float topSwayBound;
    private float bottomSwayBound;
    private Vector3 initialStartPos;
    private Vector3 moveStartPos;
    private Vector3 moveTargetPos;
    private float moveTime = 0f;
    private float moveTimeLength = 1f;
    private float moveWait = 0f;

    // Start is called before the first frame update
    void Start()
    {
        initialStartPos = transform.position;
        leftSwayBound = transform.position.x - HorizontalSwayBound;
        rightSwayBound = transform.position.x + HorizontalSwayBound;
        bottomSwayBound = transform.position.y - VerticalSwayBound;
        topSwayBound = transform.position.y + VerticalSwayBound;
        StartNewMove();
    }

    // Update is called once per frame
    void Update()
    {
        if (moveTime < moveTimeLength)
        {
            moveTime += Time.deltaTime;
            
            float posShiftFactor = (Mathf.Sin(((moveTime/moveTimeLength)-0.5f)*Mathf.PI)/2)+0.5f;
            RB.position = moveStartPos + (posShiftFactor * (moveTargetPos-moveStartPos));
            
            float rotShiftFactor = Mathf.Sin((moveTime/moveTimeLength)*Mathf.PI);
            float targetRotation = -5f*(moveTargetPos.x-moveStartPos.x);
            RB.MoveRotation(Quaternion.Euler(0,0,targetRotation * rotShiftFactor));
        }
        else if (moveWait > 0)
        {
            moveWait -= Time.deltaTime;
        }
        else
        {
            StartNewMove();
        }
        float currentRotation = gameObject.transform.rotation.eulerAngles.z;
        if (currentRotation >= 0)
        {
            currentRotation = Mathf.Min(currentRotation, 360-currentRotation);
        }
        else
        {
            currentRotation = Mathf.Max(currentRotation, -360-currentRotation);
        }
    }

    private void StartNewMove()
    {
        moveStartPos = transform.position;
        moveTargetPos = new Vector3(Random.Range(leftSwayBound,rightSwayBound), Random.Range(bottomSwayBound,topSwayBound), Random.Range(initialStartPos.z+ReverseLimit, initialStartPos.z+ForwardLimit));
        moveTime = 0f;
        moveTimeLength = Random.Range(2f, 4f);
        moveWait = Random.Range(0f, 1f);
    }
}
