using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Handles all of the player's movement
/// </summary>

public enum PlayerState
{
    Neutral, // Full regular input and movement. Transitions to: Pulling
    Pulling, // Pulling self to a surface. Move along coordinate path with collisions disabled. Transitions to: Attached
    Attached, // Attached to a surface. Bounded movement. Transitions to: Neutral, Pulling
}


public class PlayerController : MonoBehaviour
{

    /*
    TODO
    - player can sometimes clip through 
    - fix collision issues
    -> stuttery descent down sloped wall
    -> fix floor clipping issues (to replicate: target adjacent wall & switch/disable polarity mid-flight)

    NOTES FROM ADVISORS
    - Explore mixing first and third person (maybe can choose one or the other)
    - look into asset store & miximo for 3rd person animations/resources
    - Think about how we want the object pickup/push to feel
        + play with actual magnets and emulate the feel
    
    GAMES TO LOOK AT
    - Destroy All Humans (ranged physical manipulation effects)
    */

    // Main References
    [Tooltip("The player's Rigidbody component")]
    [Header("Main References")] // this has to be below the tooltip to draw correctly :/
    public Rigidbody RB;
    [Tooltip("The player's sphere collider")]
    public SphereCollider Collider;
    [Tooltip("The camera attached to the player, acting as their head")]
    public Camera MainCamera;
    public SubtitleManager SM;

    // UI References
    [Tooltip("The top part of the crosshair, indicating glove polarity")]
    [Header("UI References")]
    public Image CrosshairTop;
    [Tooltip("The marks at the top part of the crosshair, indicating a glove target")]
    public Image CrosshairTopMarks;
    [Tooltip("The bottom part of the crosshair, indicating boot polarity")]
    public Image CrosshairBottom;
    [Tooltip("The marks at the top part of the crosshair, indicating a glove target")]
    public Image CrosshairBottomMarks;
    [Tooltip("The marks at the side of the crosshair, indicating an interactable target (ex. a button)")]
    public Image CrosshairSideMarks;
    [Tooltip("X overlayed on the crosshair, indicating an unreachable target")]
    public Image CrosshairError;
    [Tooltip("The UI image indicating the player's glove polarity")]
    public Image GlovePolarityReadout;
    [Tooltip("The UI image indicating the player's boot polarity")]
    public Sprite[] GlovePolarityIcons; 
    [Tooltip("The UI icon set used to show the player's boot polarity")]
    public Image BootPolarityReadout;
    [Tooltip("The UI icon set used to show the player's glove polarity")]
    public Sprite[] BootPolarityIcons;
    [Tooltip("The glow effect at the bottom of the screen indicating your boot polarity")]
    public Image BootPolarityGlow;
    [Tooltip("The set of colors to use for the boot polarity glow effect")]
    public Color[] PolarityColors;
    /// <summary>
    /// Access the correct color for the current glove polarity
    /// </summary>
    public Color GlovePolarityColor { get {return PolarityColors[1 + (int) GlovePolarity];} }
    /// <summary>
    /// Access the correct color for the current boot polarity
    /// </summary>
    public Color BootPolarityColor { get {return PolarityColors[1 + (int) BootPolarity];} }

    // Movement and Orientation
    [Header("Movement and Orientation")] // this has to be below the first tooltip for dumb reasons
    [SerializeField]
    private PlayerState m_CurrentPlayerState = PlayerState.Neutral;
    [Tooltip("The player's movement speed")]
    public float MoveSpeed = 50f;
    [Tooltip("The amount of acceleration to apply")]
    public float GravityIntensity = 1f;
    [Tooltip("The player's maximum falling speed")]
    public float TerminalVelocity = 30f;
    [Tooltip("The speed at which the player is falling")]
    public float FallSpeed = 0f;
    [Tooltip("The angle (in degrees) at which point you slide down a surface")]
    public float SlopeTolerance = 30f;
    [Tooltip("The direction the player percieves as up (should match the normal of the surface they're standing on or falling towards)")]
    private Vector3 targetUpDirection = Vector3.up;
    private Vector3 nextTargetUpDirection = Vector3.up;
    [Tooltip("The path the player pulls themselves along when using boots")]
    [SerializeField]
    private List<Vector3> PullPath = new List<Vector3>();
    [Tooltip("Offset used to ensure player doesn't clip through objects")]
    private const float COLLISION_OFFSET = 0.01f;
    /// <summary>
    /// Get and Set the player's state, automatically changing the appropriate collision settings
    /// </summary>
    public PlayerState CurrentPlayerState {
        // TODO: Add and apply PLAYER and MAGNETFIELD layers!!!
        get {return m_CurrentPlayerState;}
        set {
            // determine collision settings based on new state
            bool disableFieldCollision = true;
            bool enableCollider = true;
            switch (value)
            {
                case PlayerState.Neutral:
                    targetUpDirection = Vector3.up;
                    break;
                case PlayerState.Pulling:
                    enableCollider = false;
                    break;
                case PlayerState.Attached:
                    disableFieldCollision = false;
                    break;
            }
            // update whether collider is active
            Collider.enabled = enableCollider;
            // update physics collision with Magnet Fields
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"),LayerMask.NameToLayer("MagnetField"), disableFieldCollision);
            // apply new state
            m_CurrentPlayerState = value;
            } 
        }
    
    // Camera
    [Tooltip("The upper bound of the player's vertical visual range")]
    [Header("Camera")]
    public float LookUpLimit = 89f;
    [Tooltip("The lower bound of the player's vertical visual range")]
    public float LookDownLimit = -89f;
    [Tooltip("Tracks the current angle the player is looking at, since the actual rotation is unreliable")]
    private float lookAngle = 0;
    [Tooltip("The speed at which the player looks")]
    public float LookSpeed = 1f;

    // Magnetism
    [Tooltip("The current polarity of the player's boots (-1, 0, or 1)")]
    [Header("Magnetism")]
    public Charge BootPolarity = Charge.Neutral;
    private Charge nextBootPolarity = Charge.Neutral;
    [Tooltip("The current polarity of the player's gloves (-1, 0, or 1)")]
    public Charge GlovePolarity = Charge.Neutral;
    [Tooltip("The intensity of the boots' magnet effect")]
    public float MagnetBootIntensity = 1f;
    [Tooltip("The intensity of the gloves' magnet effect")]
    public float MagnetGloveIntensity = 25f;
    [Tooltip("The effectiveness of gloves at longer range. Increase to increase range.")]
    public float MagnetGloveFalloff = 5f;
    [Tooltip("The distance at which the raycast will pick up the object.")]
    public const float PICKUP_RANGE = 5f;
    [Tooltip("The distance at which the raycast will attract/repel the object.")]
    public const float PULL_RANGE = 30f;
    [Tooltip("The distance at which the player can pull themselves to a target.")]
    public const float SELF_PULL_RANGE = 45f;
    private float bootResetTimer = 0;
    private float bootResetTimerMax = 0.2f;

    // Targetting
    [Tooltip("The object the player currently is aiming at")]
    public GameObject ReticleTarget;
    [Tooltip("The position the surface the reticle is aimed at")]
    public Vector3 ReticleHitPosition = Vector3.zero;
    [Tooltip("The normal of the surface the reticle is aimed at")]
    public Vector3 ReticleHitNormal = Vector3.zero;
    [Tooltip("The magnetic surface the player currently has selected")]
    public GameObject BootMagnetTarget;
    private GameObject nextBootMagnetTarget;
    [Tooltip("The position the player is trying to pull themselves too")]
    public Vector3 BootTargetPosition;
    private Vector3 nextBootTargetPosition;
    [Tooltip("Tracks whether the boot target has changed")]
    private bool bootTargetChanged = false;
    [Tooltip("Tracks whether the player has landed on a surface")]
    public bool Landed = false;
    [Tooltip("The distance at which you can interact with an object (ex. a button)")]
    public const float INTERACT_RANGE = 10f;
    
    // RFDOLAN PICKUP CODE
    [Tooltip("Code for attracting and repelling objects")]
    [Header("Gloves")]
    [SerializeField] private Transform m_CameraTransform = null;
    public Transform m_HandTransform = null;
    public float m_ThrowForce = 200f;

    private RaycastHit m_RaycastFocus;
    private bool m_CanInteract = false;
    private bool m_CanInteract_Far = false;
    public bool holding = false;
    //private bool GlovesOn = false;

    private GameManager GM;
    private InputController controls;
    private InputControlScheme gamepad;
    [Tooltip("Foward Vector2 of player movement")]
    private float dForward = 0;
    [Tooltip("Right Vector2 of player movement")]
    private float dRight = 0;
    [Tooltip("Distance to target")]
    private float targetDistance = 0;
    private PlayerInput m_PlayerInput;
    // Audio
    public AudioSource AudioSourceBoots;
    public AudioClip AudioBootsOn;
    public AudioClip AudioBootsOff;
    public AudioSource AudioSourceGloves;
    public AudioClip AudioGlovesNeg;
    public AudioClip AudioGlovesPos;
    private float gloveTargetVolume = 0;
    private float gloveChangeTimer = 0;
    private float gloveChangeTimerMax = 0.2f;
    // For opening sequence
    private float openingTimer = 17f;
    private float openingFreePoint = 4f; // how much time should be left on the clock when the player gains input

    void Awake() 
    {
        m_PlayerInput = GetComponent<PlayerInput>();
    }

<<<<<<< HEAD
=======
    void ScaleLook() 
    {
        if ( m_PlayerInput.currentControlScheme == "Gamepad")
        {
            gamepadFactor = 15;
        }
        else {
            gamepadFactor = 1;
        }
    }

>>>>>>> main
    void Boots() 
    {
        // skip if paused, in intro, or pulling
        if (GM.isPaused || openingTimer > 0 || CurrentPlayerState == PlayerState.Pulling) 
        {
            return;
        }
        Charge targetCharge = GetPolarity(ReticleTarget);
        //
        Charge newCharge = Charge.Neutral;
        // - choose correct polarity and set target
        if (targetCharge != Charge.Neutral && ReticleTarget != null && ReticleTarget.CompareTag("MagnetTarget") && targetDistance < SELF_PULL_RANGE)
        {
            if (targetCharge == Charge.Negative)
            {
                newCharge = Charge.Positive;
            }
            else
            {
                newCharge = Charge.Negative;
            }
            nextBootPolarity = newCharge;
            nextBootMagnetTarget = ReticleTarget;
            nextBootTargetPosition = ReticleHitPosition;
            nextTargetUpDirection = ReticleHitNormal;
            bootTargetChanged = true;
        }
        // - disable boots if without target
        else if (BootPolarity != Charge.Neutral || CurrentPlayerState == PlayerState.Attached)
        {
            nextBootPolarity = Charge.Neutral;
            nextBootMagnetTarget = null;
            nextBootTargetPosition = transform.position;
            nextTargetUpDirection = targetUpDirection;   // unchanged
            bootTargetChanged = true;
            CurrentPlayerState = PlayerState.Neutral;
            Debug.Log("Disabled boots");
        }
        
        
        //*/
        /* DEACTIVATED - OLD VERSION WITH MULTIPLE BOOT POLARITY BUTTONS
        if (value == 1)
        {
            // Negative (Cyan)
            // - always pull to opposite polarity
            if (targetCharge == Charge.Positive && ReticleTarget != null && 
                ReticleTarget.CompareTag("MagnetTarget") && targetDistance < SELF_PULL_RANGE)
            {
                nextBootPolarity = Charge.Negative;
                nextBootMagnetTarget = ReticleTarget;
                nextBootTargetPosition = ReticleHitPosition;
                nextTargetUpDirection = ReticleHitNormal;
            }
            // - disable boots if on negative without target
            else if (BootPolarity == Charge.Negative)
            {
                nextBootPolarity = Charge.Neutral;
                nextBootMagnetTarget = null;
                nextBootTargetPosition = transform.position;
                nextTargetUpDirection = targetUpDirection;   // unchanged
            }
            // - switch to negative
            else 
            {
                nextBootPolarity = Charge.Negative;
                nextBootMagnetTarget = BootMagnetTarget;     // unchanged
                nextBootTargetPosition = BootTargetPosition; // unchanged
                nextTargetUpDirection = targetUpDirection;   // unchanged
            }
            bootTargetChanged = true;
        }
        if (value == -1)
        {
            // Positive (Red)
            // - always pull to opposite polarity
            if (targetCharge == Charge.Negative && ReticleTarget != null && 
                ReticleTarget.CompareTag("MagnetTarget") && targetDistance < SELF_PULL_RANGE)
            {
                nextBootPolarity = Charge.Positive;
                nextBootMagnetTarget = ReticleTarget;
                nextBootTargetPosition = ReticleHitPosition;
                nextTargetUpDirection = ReticleHitNormal;
            }
            // - disable boots if on positive without target
            else if (BootPolarity == Charge.Positive)
            {
                nextBootPolarity = Charge.Neutral;
                nextBootMagnetTarget = null;
                nextBootTargetPosition = transform.position;
                nextTargetUpDirection = targetUpDirection; // unchanged
            }
            // - switch to positive
            else
            {
                nextBootPolarity = Charge.Positive;
                nextBootMagnetTarget = BootMagnetTarget; // unchanged
                nextBootTargetPosition = BootTargetPosition; // unchanged
                nextTargetUpDirection = targetUpDirection; // unchanged
            }
            bootTargetChanged = true;
        }
        */
    }

    void Gloves(float value) 
    {
        if (GM.isPaused || openingTimer > 0 || CurrentPlayerState == PlayerState.Pulling) 
        {
            return;
        }
        // left click --> -1 right click --> 1
        //DEBUG - Use Left Mouse and Right Mouse to change glove polarity
        if (value == -1)
        {
            // Negative (Cyan)
            if (GlovePolarity == Charge.Negative)
            {
                GlovePolarity = Charge.Neutral;
                gloveTargetVolume = 0;
            }
            else
            {
                GlovePolarity = Charge.Negative;
                AudioSourceGloves.clip = AudioGlovesNeg;
                gloveTargetVolume = 1;
                AudioSourceGloves.Play();
            }
            gloveChangeTimer = 0;
        }
        if (value == 1)
        {
            // Positive (Red)
            if (GlovePolarity == Charge.Positive)
            {
                GlovePolarity = Charge.Neutral;
                GlovePolarity = Charge.Neutral;
                gloveTargetVolume = 0;
            }
            else
            {
                GlovePolarity = Charge.Positive;
                AudioSourceGloves.clip = AudioGlovesPos;
                gloveTargetVolume = 1;
                AudioSourceGloves.Play();
            }
            gloveChangeTimer = 0;
        }
    }

    void Interact() 
    {
        // DEBUG - Use F to flip a switch
        SwitchController sc = ReticleTarget.GetComponent<SwitchController>();
        if (sc != null  && targetDistance < INTERACT_RANGE)
        {
            sc.UseSwitch();
        }
    }

    /// <summary>
    /// When colliding with a magnet field while Attached, orient to its normal
    /// </summary> 
    void OnCollisionEnter(Collision col)
    {
        if (CurrentPlayerState == PlayerState.Attached && col.gameObject.layer == LayerMask.NameToLayer("MagnetField"))
        {
            targetUpDirection = col.gameObject.transform.up;
        }
        Debug.Log("Hit!");
    }

    // Start is called before the first frame update
    void Start()
    {
        m_CameraTransform = GetComponentInChildren<Camera>().transform;
        RB = GetComponent<Rigidbody>();
        
        GM = GameManager.getGameManager();
        GM.setPlayerInput(m_PlayerInput);

        // opening sequence (hard-coded for now)
        SM.QueueSubtitle(new SubtitleData("[ INITIALIZING SYSTEMS... ]", 5000, 4f));
        SM.QueueSubtitle(new SubtitleData("[ UNIT ID: FyED-OR ]", 4999, 8f));
        SM.QueueSubtitle(new SubtitleData("[ ELECTROMAGNETS: ACTIVE ]", 4998, 12f));
        SM.QueueSubtitle(new SubtitleData("[ OCULAR CAMERA: ACTIVE ]", 4997, 16f));
    }

    void Update()
    {
        var move = m_PlayerInput.actions["move"].ReadValue<Vector2>();
        var look = m_PlayerInput.actions["camera"].ReadValue<Vector2>();
<<<<<<< HEAD

        if (m_PlayerInput.actions["boots"].triggered) 
        {
=======
        ScaleLook();
        if (m_PlayerInput.actions["boots"].triggered) {
>>>>>>> main
            Boots();
        }
        if (m_PlayerInput.actions["gloves"].triggered) 
        {
            var gloves = m_PlayerInput.actions["gloves"].ReadValue<float>();
            Gloves(gloves);
        }
        if (m_PlayerInput.actions["interact"].triggered) 
        {
            Interact();
        }

        // opening sequence (hard-coded for now)
        if (openingTimer > 0) 
        {
            openingTimer -= Time.deltaTime;
            if (openingTimer <= 0)
            {
                SM.QueueSubtitle(new SubtitleData("FyED-OR: \"Wait... where am I now?\"", 4996, 4f));
                SM.QueueSubtitle(new SubtitleData("FyED-OR: \"THE TRASH COMPACTOR!?\"", 4995, 8f));
                SM.QueueSubtitle(new SubtitleData("FyED-OR: \"I need to find a way out!\"", 4994, 12f));
            }
            if (openingTimer > openingFreePoint)
            {
                return;
            }
        }

        // fade in/out gloves audio
        if (gloveChangeTimer < gloveChangeTimerMax)
        {
            gloveChangeTimer = Mathf.Min(gloveChangeTimer + Time.deltaTime, gloveChangeTimerMax);
            AudioSourceGloves.volume = Mathf.Lerp(AudioSourceGloves.volume, gloveTargetVolume, gloveChangeTimer/gloveChangeTimerMax);
        }

        if (!GM.isPaused) 
        {
            dForward = move.y;
            dRight = move.x;

            // potentially have different look speed values for mouse and controller?
            float dLookRight = 0.0f;
            float dLookUp = 0.0f;
            if ( m_PlayerInput.currentControlScheme == "Gamepad")
            {
                dLookRight = look.x * LookSpeed * 30; 
                dLookUp = look.y * LookSpeed * 30; 
            }
            else 
            {
                dLookRight = look.x * LookSpeed; 
                dLookUp = look.y * LookSpeed; 
            }


            //Look and change facing direction
            transform.RotateAround(transform.position, transform.up, dLookRight);
            float xAngle = lookAngle + dLookUp;
            if (xAngle > LookUpLimit)
            {
                dLookUp = LookUpLimit - lookAngle;
            }
            else if (xAngle < LookDownLimit)
            {
                dLookUp = LookDownLimit - lookAngle;
            }
            MainCamera.transform.Rotate(-dLookUp, 0, 0, Space.Self);
            lookAngle += dLookUp;
        }
        else 
        {
            dForward = 0;
            dRight = 0;
        }

        GlovePolarityReadout.sprite = GlovePolarityIcons[1 + (int) GlovePolarity];
        CrosshairTop.color = GlovePolarityColor;
        CrosshairTop.enabled = GlovePolarity != Charge.Neutral;

        BootPolarityReadout.sprite = BootPolarityIcons[1 + (int) BootPolarity];
        CrosshairBottom.color = BootPolarityColor;
        CrosshairBottom.enabled = BootPolarity != Charge.Neutral;
        BootPolarityGlow.color = BootPolarityColor;

        // Calculate distance to target and show error
        if (ReticleTarget != null)
        {
            targetDistance = Vector3.Distance(transform.position, ReticleTarget.transform.position);

            if (ReticleTarget.CompareTag("MagnetTarget"))
            {
                // check if magnet target too far to pull from
                CrosshairError.enabled = (targetDistance > SELF_PULL_RANGE);
            }
            else if (ReticleTarget.CompareTag("Interactable"))
            {
                if (ReticleTarget.GetComponent<ChargeProperty>())
                {
                    // check if interactable magnet object too far to interact with
                    CrosshairError.enabled = (targetDistance > PULL_RANGE);
                }
                else
                {
                    // check if physically-interactable object too far to reach
                    CrosshairError.enabled = (targetDistance > INTERACT_RANGE);
                }
            }
            else
            {
                CrosshairError.enabled = false;
            }
        }
        else
        {
            CrosshairError.enabled = false;
        }
    }

    /// <summary>
    /// Handle physics and movement
    /// </summary>
    void FixedUpdate()
    {
        bool sliding = false;

        // Update boot target if landed
        if ((CurrentPlayerState == PlayerState.Neutral || CurrentPlayerState == PlayerState.Attached) && bootTargetChanged)
        {
            BootPolarity = nextBootPolarity;
            BootMagnetTarget = nextBootMagnetTarget;
            BootTargetPosition = nextBootTargetPosition;
            targetUpDirection = nextTargetUpDirection;
            bootTargetChanged = false;
            if (BootMagnetTarget != null && CanStickTo(BootMagnetTarget))
            {
                // Change state to Pulling
                CurrentPlayerState = PlayerState.Pulling;
                // Generate movement path
                PullPath.Clear();
                // - end 1 unit above target surface
                PullPath.Add(BootTargetPosition + targetUpDirection.normalized*Collider.radius);
                // TODO - GENERATE FULL PATH
//TODO!
                // Apply launch force
                RB.AddForce(transform.up*200f, ForceMode.Force);
                // Play boots activate sound
                AudioSourceBoots.clip = AudioBootsOn;
                AudioSourceBoots.Play();
            }
            else
            {
                // Detach from surfaces
                CurrentPlayerState = PlayerState.Neutral;
                // Play boots disable sound
                AudioSourceBoots.clip = AudioBootsOff;
                AudioSourceBoots.Play();
            }
        }

        // Orient local up vector towards target up vector (i.e. rotate body to match boot pull direction)
        if (transform.up.normalized != targetUpDirection.normalized)
        {
            Vector3 facingDirection = Vector3.ProjectOnPlane(MainCamera.transform.forward, targetUpDirection);
            Quaternion quatToRotate = Quaternion.LookRotation(facingDirection, targetUpDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, quatToRotate, 5 * Time.deltaTime);
        }

        // Apply Physics
        Vector3 gravityDirection = -transform.up;
        // - move (if Neutral or Attached to surface)
        if (CurrentPlayerState == PlayerState.Neutral || CurrentPlayerState == PlayerState.Attached)
        {
            if (dForward != 0 || dRight != 0)
            {
                // speed up
                Vector3 newForce = (transform.forward*dForward + transform.right*dRight).normalized * MoveSpeed;
                RB.AddForce(newForce, ForceMode.Force);
            }
            else
            {
                // slow down
                RB.velocity = RB.velocity - Vector3.ProjectOnPlane(RB.velocity/2, -gravityDirection);
            }
        }
        // - apply gravity OR pull self along target path
        if (CurrentPlayerState == PlayerState.Pulling)
        {
            // advance to next path node if at end
            if (PullPath.Count > 0 && Vector3.Distance(transform.position, PullPath[0]) < Collider.radius)
            {
                PullPath.RemoveAt(0); 
            }
            // if further nodes still present, pull towards next
            if (PullPath.Count > 0)
            {
                gravityDirection = PullPath[0] - transform.position;
                FallSpeed = Mathf.Min(FallSpeed + MagnetBootIntensity, TerminalVelocity);
            }
            // otherwise, attach to surface
            else
            {
                CurrentPlayerState = PlayerState.Attached;
            }
        }
        else
        {
            // fall from gravity
            FallSpeed = Mathf.Min(FallSpeed + GravityIntensity, TerminalVelocity);
        }
        // - detect floor
        //   detects Wall and Interactable objects - you must set them to the appropriate layer first!
        Vector3 fallOffset = gravityDirection.normalized * FallSpeed * Time.deltaTime;
        if (CurrentPlayerState == PlayerState.Neutral || CurrentPlayerState == PlayerState.Attached)
        {
            RaycastHit floorHit;
            Physics.SphereCast(transform.position, Collider.radius, fallOffset*2, out floorHit, fallOffset.magnitude, LayerMask.GetMask("Wall","Interactable"), QueryTriggerInteraction.Ignore);
            if (floorHit.collider)
            {
                /*
                // if floor can't be stuck to, switch back to normal gravity
                if (targetUpDirection.normalized != Vector3.up && BootMagnetTarget == null && !CanStickTo(floorHit.collider.gameObject))
                {
                    targetUpDirection = Vector3.up;
                    FallSpeed = 0f;
                    fallOffset = Vector3.zero;
                }
                else
                */

                // if floor is metal or opposite polarity and within slope tolerance, stick to it
                if (Vector3.Angle(transform.up, floorHit.normal) < SlopeTolerance)
                {
                    // floor detected
                    if (BootMagnetTarget == null)
                    {
                        Landed = true;
                        FallSpeed = 0f;
                    }
                    fallOffset = fallOffset.normalized * (floorHit.distance-COLLISION_OFFSET);
                    // stop targetting landing point once reached
                    if (floorHit.collider.gameObject == BootMagnetTarget)
                    {
                        BootMagnetTarget = null;
                        Landed = true;
                        FallSpeed = 0f;
                    }
                    else
                    {
                        // force gravity reset timer to still continue
                        sliding = true;
                    }
                }
                // fall or slide if no floor detected
                else
                {
                    // no floor, slide instead
                    RB.AddForce(Vector3.ProjectOnPlane(floorHit.normal * FallSpeed * Time.deltaTime, -gravityDirection), ForceMode.VelocityChange);
                    Landed = false;
                    sliding = true;
                }
            }
            else
            {
                // falling
                Landed = false;
                // disable boots if falling without a target
                if (BootMagnetTarget == null)
                {
                    BootPolarity = Charge.Neutral;
                }
            }
        }
        // - update position (with extra check to prevent clipping)
        RaycastHit fallHit;
        if (CurrentPlayerState == PlayerState.Neutral || CurrentPlayerState == PlayerState.Attached)
        {
        Physics.SphereCast(transform.position, Collider.radius, fallOffset, out fallHit, fallOffset.magnitude, LayerMask.GetMask("Wall","Interactable"), QueryTriggerInteraction.Ignore);
            if (fallHit.collider)
            {
                fallOffset = fallOffset.normalized * (fallHit.distance-COLLISION_OFFSET);
            }
        }
        RB.MovePosition(transform.position + fallOffset);

        // Increment timer to reset boot target if stuck to invalid surface
        if ((Landed || sliding) && BootMagnetTarget != null)
        {
            bootResetTimer += Time.deltaTime;
            if (bootResetTimer >= bootResetTimerMax)
            {
                BootMagnetTarget = null;

                // Play boots disable sound
                AudioSourceBoots.clip = AudioBootsOff;
                AudioSourceBoots.Play();
            }
        }
        else
        {
            bootResetTimer = 0;
        }

        // Raycast Crosshair
        // detects Wall and Interactable objects - must set the appropriate layer first!)
        float castDistance = 500f;
        RaycastHit hit;
        Physics.Raycast(MainCamera.transform.position, MainCamera.transform.forward, out hit, castDistance, LayerMask.GetMask("Wall","Interactable"), QueryTriggerInteraction.Ignore);
        if (hit.collider)
        {
            ReticleTarget = hit.collider.gameObject;
            ReticleHitNormal = hit.normal;
            ReticleHitPosition = hit.point;
            Color reticleColor = Color.white;

            // Case 1: The target is a magnetic surface you can target with your boots
            if (ReticleTarget.CompareTag("MagnetTarget"))
            {
                // indicate surface as valid target
                CrosshairBottomMarks.enabled = true;
                CrosshairTopMarks.enabled = false;
                CrosshairSideMarks.enabled = false;

                // DEBUG - show the normal of the surface with a magenta line
                Debug.DrawRay(ReticleHitPosition, hit.normal*2f, Color.magenta,0f);
            }

            // Case 2: The target is a moveable magnetic object
            else if (ReticleTarget.CompareTag("Interactable"))
            {
                CrosshairBottomMarks.enabled = false;
                if (ReticleTarget.GetComponent<ChargeProperty>())
                {
                    CrosshairTopMarks.enabled = true;
                    CrosshairSideMarks.enabled = false;
                }
                else
                {
                    CrosshairTopMarks.enabled = false;
                    CrosshairSideMarks.enabled = true;
                }
            }

            // Case 3: Target is not polarized
            else
            {
                CrosshairBottomMarks.enabled = false;
                CrosshairTopMarks.enabled = false;
                CrosshairSideMarks.enabled = false;
            }

            // Get and display target's subtitle data if present and within reading distance
            // Negative distance = infinite
            TextProperty tp = ReticleTarget.GetComponent<TextProperty>();
            if (tp && (tp.ReadingDistance < 0 || tp.ReadingDistance >= targetDistance))
            {
                SM.QueueSubtitle(new SubtitleData(tp.Text, 0));
            }

            // DEBUG - draw info at crosshair hit position
            // - red line points to global up
            // - white lines are aligned to other global axes (they appear green instead if the target position is a magnet boots target)
            // - cyan line points to player's local up direction
            // - blue line points to player's target up direction
            //(- magenta line, if present, shows the normal of the surface)
            float hitscale = 0.2f;
            // target up
            Debug.DrawRay(ReticleHitPosition,targetUpDirection*hitscale*7f, Color.blue, 0f);
            // player up
            Debug.DrawRay(ReticleHitPosition,transform.up*hitscale*5f, Color.cyan, 0f);
            Debug.DrawLine(ReticleHitPosition+Vector3.left*hitscale, ReticleHitPosition+Vector3.right*hitscale, reticleColor, 0f);
            Debug.DrawLine(ReticleHitPosition+Vector3.forward*hitscale, ReticleHitPosition+Vector3.back*hitscale, reticleColor, 0f);
            Debug.DrawRay(ReticleHitPosition,Vector3.down*hitscale, reticleColor, 0f);
            Debug.DrawRay(ReticleHitPosition,Vector3.up*hitscale, Color.red, 0f);
        }
        else
        {
            // no target found
            ReticleTarget = null;
            ReticleHitNormal = Vector3.zero;
            ReticleHitPosition = transform.position;
            CrosshairBottomMarks.enabled = false;
            CrosshairTopMarks.enabled = false;
        }

        // RFDOLAN PICKUP CODE

        Ray ray = new Ray(m_CameraTransform.position, m_CameraTransform.forward);

        if (Physics.Raycast(ray, out m_RaycastFocus, PICKUP_RANGE, LayerMask.GetMask("Interactable")) && m_RaycastFocus.collider.transform.tag == "Interactable") {
        //TODO add different options for pulling from far vs picking up.
            m_CanInteract = true;
            m_CanInteract_Far = false;
        }

        else if (Physics.Raycast(ray, out m_RaycastFocus, PULL_RANGE, LayerMask.GetMask("Interactable")) && m_RaycastFocus.collider.transform.tag == "Interactable") {
            m_CanInteract_Far = true;
            m_CanInteract = false;
            
        }
        // Is interactable object detected in front of player?
        else {
            m_CanInteract = false;
            m_CanInteract_Far = false;
        }

        // Has interact button been pressed whilst interactable object is in front of player?
        if (m_CanInteract) {
            IInteractable interactComponent = m_RaycastFocus.collider.transform.GetComponent<IInteractable>();

            if (interactComponent != null) {
                // Perform object's interaction
                interactComponent.Interact(this);
            }
        }
        // Wasn't right in front of the player, but is it within pull range?
        else if ((GlovePolarity != Charge.Neutral) && m_CanInteract_Far) {
            IInteractable interactComponent = m_RaycastFocus.collider.transform.GetComponent<IInteractable>();
            if (interactComponent != null) {
                // Perform object's interaction
                interactComponent.InteractFar(this);
            }

        }
 
        /*
         // Has action button been pressed whilst interactable object is in front of player?
         if (Input.GetButtonDown("Fire3") && m_CanInteract == true) {
             IInteractable interactComponent = m_RaycastFocus.collider.transform.GetComponent<IInteractable>();
 
             if (interactComponent != null) {
                 // Perform object's action
                 interactComponent.Action(this);
             }
         }
         */
    }

    /// <summary>
    /// Determines whether the player can stick to a given object
    /// </summary>
    /// <returns>Boolean indicating if the surface can be stuck to</returns>
    bool CanStickTo(GameObject target)
    {
        // Can't stick to anything if boots not charged
        if (BootPolarity == Charge.Neutral)
        {
            return false;
        }
        // Can always stick to metal if boots are charged
        else if (target.CompareTag("Metal"))
        {
            return true;
        }
        // Can stick to charged objects with opposite polarity
        else
        {
            ChargeProperty targetCharge = target.GetComponent<ChargeProperty>();
            // If object has charge property, stick if polarity is opposite
            if (targetCharge)
            {
                return ((int) BootPolarity + (int) targetCharge.Polarity) == 0;
            }
            // If object lacks charge property, cannot stick
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Returns the polarity of the given object. Objects without one assigned (and null objects) are treated as neutral.
    /// </summary>
    Charge GetPolarity(GameObject target)
    {   
        if (target)
        {
            ChargeProperty targetCharge = target.GetComponent<ChargeProperty>();
            if (targetCharge)
            {
                return targetCharge.Polarity;
            }
        }
        return Charge.Neutral;
    }

    /// <summary>
    /// Display a subtitle (other objects can call this)
    /// </summary>
    public void DisplaySubtitle(SubtitleData sd)
    {
        SM.QueueSubtitle(sd);
    }
}
