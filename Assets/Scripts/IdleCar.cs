using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the animation of cars
/// </summary>

public class IdleCar : MonoBehaviour
{
    [Tooltip("This object's rigidbody")]
    [Header("Core References")]
    private Rigidbody RB;
    
    [Tooltip("The backward extent this car can float")]
    [Header("Idle Motion")]
    public float ReverseLimit = -10f;
    [Tooltip("The forward extent this car can float")]
    public float ForwardLimit = 10f;
    [Tooltip("The horizontal sway limit for this car")]
    public float HorizontalSwayBound = 5f;
    [Tooltip("The vertical sway limit for this car")]
    public float VerticalSwayBound = 5f;
    private Vector3 idlePosition;
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
    
    [Tooltip("Whether or not to react to player proximity")]
    [Header("Player Proximity")]
    public bool UsePlayerProximity = false;
    [Tooltip("The position the behicle settles to when approached by the player")]
    public Vector3 SettlePosition;
    [Tooltip("The proximity at which to start settling into a fixed position")]
    public float SettleStartDistance = 40f;
    [Tooltip("The proximity at which to finish settling into a fixed position")]
    public float SettleEndDistance = 20f;
    [SerializeField]
    private float proximityFactor = 0f;
    private GameObject playerReference = null;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();

        // get position and bounds
        initialStartPos = transform.position;
        leftSwayBound = transform.position.x - HorizontalSwayBound;
        rightSwayBound = transform.position.x + HorizontalSwayBound;
        bottomSwayBound = transform.position.y - VerticalSwayBound;
        topSwayBound = transform.position.y + VerticalSwayBound;
        
        // setup player proximity
        idlePosition = initialStartPos;
        if (UsePlayerProximity)
        {
            playerReference = GameManager.Instance.GetPlayer();
        }
        else
        {
            SettlePosition = initialStartPos;
        }
        
        StartNewMove();
    }

    // Update is called once per frame
    void Update()
    {
        // get the player's proximity
        if (playerReference != null)
        {
            float proximity = Vector3.Distance(SettlePosition, playerReference.transform.position);
            if (proximity < SettleEndDistance)
            {
                proximityFactor = 1;
            }
            else if (proximity < SettleStartDistance)
            {
                proximityFactor = Mathf.Max(0,(proximity-SettleStartDistance)/(SettleEndDistance-SettleStartDistance));
            }
            else
            {
                proximityFactor = 0;
            }
        }

        // animate move
        if (moveTime < moveTimeLength)
        {
            moveTime += Time.deltaTime;
            
            float posShiftFactor = (Mathf.Sin(((moveTime/moveTimeLength)-0.5f)*Mathf.PI)/2)+0.5f;
            idlePosition = moveStartPos + (posShiftFactor * (moveTargetPos-moveStartPos));
            
            float rotShiftFactor = Mathf.Sin((moveTime/moveTimeLength)*Mathf.PI);
            float targetRotation = -5f*(moveTargetPos.x-moveStartPos.x);
            RB.MoveRotation(Quaternion.Euler(0,0,targetRotation * rotShiftFactor * (1-proximityFactor)));
        }
        else if (moveWait > 0)
        {
            moveWait -= Time.deltaTime;
        }
        else
        {
            StartNewMove();
        }
        // apply movement
        RB.position = SettlePosition*proximityFactor + idlePosition*(1-proximityFactor);
        // constrain rotation
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
        moveStartPos = idlePosition;
        moveTargetPos = new Vector3(Random.Range(leftSwayBound,rightSwayBound), Random.Range(bottomSwayBound,topSwayBound), Random.Range(initialStartPos.z+ReverseLimit, initialStartPos.z+ForwardLimit));
        moveTime = 0f;
        moveTimeLength = Random.Range(2f, 4f);
        moveWait = Random.Range(0f, 1f);
    }
}
