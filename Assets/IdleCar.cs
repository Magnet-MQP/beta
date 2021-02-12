using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleCar : MonoBehaviour
{
    public Rigidbody RB;
    public float ReverseLimit = -10f;
    public float ForwardLimit = 10f;
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
        StartNewMove();
    }

    // Update is called once per frame
    void Update()
    {
        if (moveTime < moveTimeLength)
        {
            moveTime += Time.deltaTime;
            RB.position = moveStartPos + ((Mathf.Sin(((moveTime/moveTimeLength)-0.5f)*Mathf.PI)/2)+0.5f) * (moveTargetPos-moveStartPos);
            //RB.position = moveStartPos + (moveTime/moveTimeLength) * (moveTargetPos-moveStartPos);
        }
        else if (moveWait > 0)
        {
            moveWait -= Time.deltaTime;
        }
        else
        {
            StartNewMove();
        }
    }

    private void StartNewMove()
    {
        moveStartPos = transform.position;
        moveTargetPos = new Vector3(moveStartPos.x, moveStartPos.y, Random.Range(initialStartPos.z+ReverseLimit, initialStartPos.z+ForwardLimit));
        moveTime = 0f;
        moveTimeLength = Random.Range(2f, 4f);
        moveWait = Random.Range(0f, 1f);
    }
}
