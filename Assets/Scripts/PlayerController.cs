using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    - player can sometimes clip through edges of magnet field
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
    [Tooltip("The Subtitle Manager for the game")]
    public SubtitleManager SM;
    [Tooltip("The Bootup Controller, which handles the bootup animation.")]
    public BootupController BC;

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
    public Image CrosshairInteract;
    [Tooltip("X overlayed on the crosshair, indicating an unreachable target")]
    public Image CrosshairError;
    /*
    [Tooltip("The UI image indicating the player's glove polarity")]
    public Image GlovePolarityReadout; // CONSIDER DELETING
    [Tooltip("The UI image indicating the player's boot polarity")]
    public Sprite[] GlovePolarityIcons; // CONSIDER DELETING
    [Tooltip("The UI icon set used to show the player's boot polarity")]
    public Image BootPolarityReadout; // CONSIDER DELETING
    [Tooltip("The UI icon set used to show the player's glove polarity")]
    public Sprite[] BootPolarityIcons; // CONSIDER DELETING
    [Tooltip("The glow effect at the bottom of the screen indicating your boot polarity")]
    public Image BootPolarityGlow; // CONSIDER DELETING
    */
    [Tooltip("The image used for the fadeout/in boot pull animation")]
    public Image BootFadeOverlay;
    [Tooltip("The set of colors to use for the boot polarity glow effect")]
    public Color[] PolarityColors;
    [Tooltip("The color used to indicate that boots are active")]
    public Color BootActiveColor;
    [Tooltip("The set of colors to use for polarity emissives on the player")]
    public Color[] EmissivePolarityColors;
    /// <summary>
    /// Access the correct color for the current glove polarity
    /// </summary>
    public Color GlovePolarityColor { get {return PolarityColors[1 + (int) GlovePolarity];} }
    /// <summary>
    /// Access the correct color for the current glove glow
    /// </summary>
    public Color EmissivePolarityColor { get {return EmissivePolarityColors[1 + (int) GlovePolarity];} }
    /// <summary>
    /// Access the correct color for the current boot polarity
    /// </summary>
    public Color BootPolarityColor {
        get {
            if (BootPolarity == Charge.Neutral && CurrentPlayerState == PlayerState.Neutral)
            {
                return new Color(0,0,0,0);
            }
            else
            {
                return BootActiveColor;
            }
        }
    }

    // Arm Model References
    [Tooltip("The player's left arm")]
    [Header("Arm Models")]
    public PlayerArmController ArmL;
    [Tooltip("The player's right arm")]
    public PlayerArmController ArmR;
    private float armCyclePos = 0f; // used to animate walking hand gesture
    private float armCycleSpeed = 0.002f; // used to animate walking hand gesture
    private float armSwing = 0.1f; // amount of arm movement
    private float armGripPos = 0f; // used to animate magnet hang gesture
    private float armGripSpeed = 7f; // rate at which arms move into magnet position when in use
    private bool armInteractAnim = false; // whether to play the arm interact animation
    private float armInteractPos = 0f; // used to animate object use gesture
    private float armInteractSpeed = 5f; // rate at which the arm inteact animation plays
    private float armRestSpeed = 4f; // rate at which arms settle back into place when using magnets or not moving
    private float armLookX = 0f; // amount the arms lead the camera in the x direction
    private float armLookY = 0f; // amount the arms lead the camera in the y direction
    private float armLookFactor = 0.06f; // ratio of look movement to arm leading
    private float armLookSpeed = 2f; // rate at which arms settle back from leading

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
    [Tooltip("The detection radius of the player's targetting reticle")]
    public float ReticleRadius = 1f;

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
    [Tooltip("Distance to target")]
    private float targetDistance = 0;

    //private bool GlovesOn = false;

    [Header("Managers")]
    private GameManager GM;
    [Tooltip("Foward Vector2 of player movement")]
    private float dForward = 0;
    [Tooltip("Right Vector2 of player movement")]
    private float dRight = 0;
        private PlayerInput m_PlayerInput;

    // Audio
    [Header("Audio")]
    public AudioSource AudioSourceBoots;
    public AudioClip AudioBootsOn;
    public AudioClip AudioBootsOff;
    public AudioSource AudioSourceGloves;
    public AudioClip AudioGlovesNeg;
    public AudioClip AudioGlovesPos;
    private float gloveTargetVolume = 0;
    private float gloveChangeTimer = 0;
    private float gloveChangeTimerMax = 0.2f;

    [Tooltip("The current cutscene the player is in (if this is set, the player will immediately enter it on load)")]
    [Header("Cutscenes")]
    public CutsceneData ActiveCutscene = null;
    [Tooltip("Whether the player is actively in a cutscene")]
    public bool InCutscene = false;
    private float cutsceneTimer = 0f;
    private float cutsceneUnlockPoint = 0f; // how much time should be left on the clock when the player gains input
    private bool cutsceneEndEffect = false; // whether the end effect of the current cutscene has started

    [Tooltip("Whether to show the fade out/in animation when pulling to a surface")]
    [Header("Accessibility Settings")]
    public bool UseBootFade = false;
    private float bootFade = 0f;
    private const float BOOTFADE_MAX = 0.1f;
    private const float FADE_ANGLE_THRESHOLD = 4f; // angle difference between current and target up at which to activate fade
    [Tooltip("Angle below which boot fade overlay becomes transparent")]
    private const float FADEOUT_ANGLE = 5f;

    void Awake() { }

    // Start is called before the first frame update
    void Start()
    {
        m_CameraTransform = GetComponentInChildren<Camera>().transform;
        RB = GetComponent<Rigidbody>();
        
        GM = GameManager.getGameManager();
        m_PlayerInput = GM.getPlayerInput();

        CrosshairInteract.enabled = false; // start with the interact crosshair hidden

        // automatically start assigned cutscene, if present
        if (ActiveCutscene != null)
        {
            StartCutscene(ActiveCutscene);
        }
    }

    /// <summary>
    /// Play a cutscene
    /// </summary>
    public void StartCutscene(CutsceneData newScene)
    {
        // load cutscene info
        ActiveCutscene = newScene;
        cutsceneTimer = newScene.FullDuration;
        cutsceneUnlockPoint = cutsceneTimer - newScene.LockDuration;
        cutsceneEndEffect = false;
        if (newScene.ShowBootup)
        {
            BC.StartBootupSequence(newScene.LockDuration);
        }

        // load dialogue data
        for (int i = 0; i < newScene.Dialogue.Length; i++)
        {
            SM.QueueSubtitle(newScene.Dialogue[i]);
        }

        // actually start cutscene
        InCutscene = true;
    }

    void Boots() 
    {
        // skip if paused, in intro, or pulling
        if (GM.isPaused || (InCutscene && cutsceneTimer > cutsceneUnlockPoint) || CurrentPlayerState == PlayerState.Pulling) 
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
        }
    }

    void Gloves(float value) 
    {
        if (GM.isPaused || (InCutscene && cutsceneTimer > cutsceneUnlockPoint) || CurrentPlayerState == PlayerState.Pulling) 
        {
            return;
        }

        bool polarityChanged = false;

        // left click --> -1 right click --> 1
        //DEBUG - Use Left Mouse and Right Mouse to change glove polarity
        // Negative glove
        if (value == -1)
        {
            // Negative (Cyan)
            if (!GM.glovesIsHold && GlovePolarity == Charge.Negative)
            {
                GlovePolarity = Charge.Neutral;
                AudioSourceGloves.Stop();
            }
            else if (!AudioSourceGloves.isPlaying || GlovePolarity == Charge.Positive)
            {
                GlovePolarity = Charge.Negative;
                AudioSourceGloves.clip = AudioGlovesNeg;
                gloveTargetVolume = 1;
                AudioSourceGloves.Play();
            }
            gloveChangeTimer = 0;
            polarityChanged = true;
        }
        // Positive glove
        if (value == 1)
        {
            // Positive (Red)
            if (!GM.glovesIsHold && GlovePolarity == Charge.Positive)
            {
                GlovePolarity = Charge.Neutral;
                AudioSourceGloves.Stop();
            }
            else if (!AudioSourceGloves.isPlaying || GlovePolarity == Charge.Negative)
            {
                GlovePolarity = Charge.Positive;
                AudioSourceGloves.clip = AudioGlovesPos;
                gloveTargetVolume = 1;
                AudioSourceGloves.Play();
            }
            gloveChangeTimer = 0;
            polarityChanged = true;
        }
        // No glove, head empty
        if (value == 0 && GM.glovesIsHold)
        {
            GlovePolarity = Charge.Neutral;
            AudioSourceGloves.Stop();
            gloveChangeTimer = 0;
            polarityChanged = true;
        }

        // update polarity glow colors
        if (polarityChanged)
        {
            ArmL.SetArmEmissive(EmissivePolarityColor);
            ArmR.SetArmEmissive(EmissivePolarityColor);
        }
    }

    void Interact() 
    {
        // DEBUG - Use F to flip a switch
        SwitchController sc = ReticleTarget.GetComponent<SwitchController>();
        if (sc != null  && targetDistance < INTERACT_RANGE)
        {
            sc.UseSwitch();
            PlayInteractAnim();
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
    }

    void Update()
    {
        var move = m_PlayerInput.actions["move"].ReadValue<Vector2>();
        var look = m_PlayerInput.actions["camera"].ReadValue<Vector2>();
        var gloves = m_PlayerInput.actions["gloves"].ReadValue<float>();

        if (m_PlayerInput.actions["boots"].triggered)
        {
            Boots();
        }

        if (GM.glovesIsHold) 
        {
            Gloves(gloves);
        }
        else if (m_PlayerInput.actions["gloves"].triggered)
        {
            Gloves(gloves);
        }


        if (m_PlayerInput.actions["interact"].triggered) 
        {
            Interact();
        }

        // play cutscene
        if (InCutscene) 
        {
            cutsceneTimer -= Time.deltaTime;
            // end cutscene at end of timer
            if (ActiveCutscene.ShowShutdown && !cutsceneEndEffect && cutsceneTimer <= ActiveCutscene.ShutdownDuration)
            {
                // shutdown effect
                BC.StartShutdownSequence(ActiveCutscene.ShutdownDuration);
                cutsceneEndEffect = true;
            }
            if (cutsceneTimer <= 0)
            {
                InCutscene = false;
                // restart game if desired
                if (ActiveCutscene.ExitAtEnd)
                {
                    GameManager.Instance.mainMenu();
                }
                // advance scene if desired
                else if (ActiveCutscene.AdvanceAtEnd)
                {
                    GameManager.Instance.nextScene();
                }
                // reload scene if desired
                else if (ActiveCutscene.ReloadAtEnd)
                {
                    GameManager.Instance.reloadScene();
                }
            }
            // disable other actions after this point if necessary
            if (cutsceneTimer > cutsceneUnlockPoint)
            {
                return;
            }
        }

        // boot fade overlay
        // reduces percieved motion when pulling to a surface or dropping from one
        if (UseBootFade && Vector3.Angle(transform.up, targetUpDirection) > FADE_ANGLE_THRESHOLD)
        {
            bootFade = Mathf.Min(BOOTFADE_MAX, bootFade+Time.deltaTime);
        }
        else if (bootFade > 0)
        {
            bootFade = Mathf.Max(0, bootFade-Time.deltaTime/2f);
        }
        float fadeOverlayAlpha = 0f;
        if (UseBootFade)
        {
            //fadeOverlayAlpha = (bootFade/bootFadeMax);
            fadeOverlayAlpha = Mathf.Min(2f,(Vector3.Angle(transform.up, targetUpDirection)/FADEOUT_ANGLE)) * (bootFade/BOOTFADE_MAX);
        }
        BootFadeOverlay.color = new Vector4(0,0,0, fadeOverlayAlpha);

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
                dLookRight = look.x * GM.lookSpeedX * 10;
                dLookUp = look.y * GM.lookSpeedY * 10;
            }
            else 
            {
                dLookRight = look.x * GM.lookSpeedX; 
                dLookUp = look.y * GM.lookSpeedY; 
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

            // lead with hands
            armLookX += dLookRight*armLookFactor;
            armLookY += dLookUp*armLookFactor;
        }
        else 
        {
            dForward = 0;
            dRight = 0;
        }

        //GlovePolarityReadout.sprite = GlovePolarityIcons[1 + (int) GlovePolarity];
        CrosshairTop.color = GlovePolarityColor;

        //BootPolarityReadout.sprite = BootPolarityIcons[1 + (int) BootPolarity];
        CrosshairBottom.color = BootPolarityColor;
        //BootPolarityGlow.color = BootPolarityColor;

        // Animate arm movement 
        // - lead camera with hands
        armLookX -= armLookX*armLookSpeed*Time.deltaTime;
        armLookY -= armLookY*armLookSpeed*Time.deltaTime;
        Vector3 lookOffset = new Vector3(armLookY, armLookX, 0);
        ArmL.SetArmAngleOffset(lookOffset);
        ArmR.SetArmAngleOffset(lookOffset);

        // - pull arms inwards if using magnets
        if (GlovePolarity != Charge.Neutral)
        {
            if (armGripPos < 1)
            {
                armGripPos = Mathf.Min(1,armGripPos+Time.deltaTime*armGripSpeed);
            }   
        }
        else if (armGripPos > 0)
        {
            armGripPos = Mathf.Max(0,armGripPos-Time.deltaTime*armGripSpeed);
        }
        float gripOffset = 0.2f*armGripPos;
        // - extend right arm if using object
        if (armInteractAnim)
        {
            armInteractPos += Mathf.Max(0.4f,1-armInteractPos)*armInteractSpeed*Time.deltaTime;
            if (armInteractPos >= 1)
            {
                armInteractPos = 0;
                armInteractAnim = false;
            }
        }
        // - settle arms back to rest if not moving OR if using magnet
        if (armCyclePos > 0 && (GlovePolarity != Charge.Neutral || (dForward == 0 && dRight == 0 && CurrentPlayerState != PlayerState.Pulling)))
        {
            float restOffset = 0f;
            if (armCyclePos >= 0.5)
            {
                restOffset = 0.5f;
            }
            if (armCyclePos < 0.25f || (armCyclePos > 0.5f && armCyclePos < 0.75f))
            {
                armCyclePos -= (armCyclePos-restOffset)*Time.deltaTime*armRestSpeed;
                if (armCyclePos < restOffset)
                {
                    armCyclePos = 0;
                }
            }
            else
            {
                armCyclePos += (restOffset+0.5f-armCyclePos)*Time.deltaTime*armRestSpeed;
                if (armCyclePos >= restOffset+0.5f)
                {
                    armCyclePos = 0;
                }
            }
        }
        float leftOffset = armSwing*Mathf.Sin(armCyclePos*2f*Mathf.PI);
        float rightOffset = armSwing*Mathf.Sin(armCyclePos*2f*Mathf.PI + Mathf.PI); // keep the arms at opposite phase
        float interactOffsetX = Mathf.Cos(armInteractPos*2f*Mathf.PI)-1f;
        float interactOffsetZ = Mathf.Max(-0.5f,Mathf.Sin(armInteractPos*Mathf.PI));
        ArmL.SetArmPositionOffset(new Vector3(gripOffset,leftOffset+0.5f*gripOffset,0.4f*leftOffset));
        ArmR.SetArmPositionOffset(new Vector3(-gripOffset,rightOffset+0.5f*gripOffset,0.4f*rightOffset) + new Vector3(0.3f*interactOffsetX,0,1.2f*interactOffsetZ));

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
                // GENERATE FULL PATH HERE
                // (I currently skip this part because I'm actually just using a simpler avoidance system for now.)
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
            // animate movement if not using gloves
            if (GlovePolarity == Charge.Neutral)
            {
                armCyclePos += RB.velocity.magnitude * armCycleSpeed;
            }
            if (armCyclePos > 1)
            {
                armCyclePos -= 1;
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
                // avoid obstacles via simple steering
                float obstacleDistance = 10f;
                for (int i = 0; i < 8; i++)
                {
                    float angle = Mathf.PI*i/4;
                    Vector3 direction = gravityDirection + 3*transform.up*Mathf.Cos(angle) + 3*transform.right*Mathf.Sin(angle);
                    RaycastHit obstacleHit;
                    Physics.SphereCast(transform.position, Collider.radius, direction, out obstacleHit, obstacleDistance, LayerMask.GetMask("Wall"));
                    if (obstacleHit.collider)
                    {
                        // apply force pushing player away from wall, which is stronger the closer they are to it
                        RB.AddForce(-1.7f * Vector3.ProjectOnPlane(direction,-gravityDirection) * (1-(obstacleHit.distance/obstacleDistance)));
                    }
                }
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
                /*
                AudioSourceBoots.clip = AudioBootsOff;
                AudioSourceBoots.Play();
                */
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
        // - if no target found, try again with a spherecast that only detects interactables
        if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Interactable"))
        {
            // cast up to max distance or the hit distance
            float sphereDistance = castDistance;
            if (hit.collider)
            {
                sphereDistance = hit.distance;
            }
            Physics.SphereCast(MainCamera.transform.position, ReticleRadius, MainCamera.transform.forward, out hit, sphereDistance, LayerMask.GetMask("Interactable"), QueryTriggerInteraction.Ignore);
        }
        // - process hit
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
                CrosshairInteract.enabled = false;

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
                    CrosshairInteract.enabled = false;
                }
                else
                {
                    CrosshairTopMarks.enabled = false;
                    CrosshairInteract.enabled = true;
                }
            }

            // Case 3: Target is not polarized
            else
            {
                CrosshairBottomMarks.enabled = false;
                CrosshairTopMarks.enabled = false;
                CrosshairInteract.enabled = false;
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
            CrosshairInteract.enabled = false;
        }


        Ray ray = new Ray(m_CameraTransform.position, m_CameraTransform.forward);

        if (Physics.SphereCast(ray, ReticleRadius, out m_RaycastFocus, PICKUP_RANGE, LayerMask.GetMask("Interactable")) && m_RaycastFocus.collider.transform.tag == "Interactable") {
            m_CanInteract = true;
            m_CanInteract_Far = false;
        }

        else if (Physics.SphereCast(ray, ReticleRadius, out m_RaycastFocus, PULL_RANGE, LayerMask.GetMask("Interactable")) && m_RaycastFocus.collider.transform.tag == "Interactable") {
            m_CanInteract_Far = true;
            m_CanInteract = false;
            
        }
        // No Interactable in range
        else {
            m_CanInteract = false;
            m_CanInteract_Far = false;   
        }

        // Has interact button been pressed whilst interactable object is in front of player?
        if (m_CanInteract) {
            IInteractable interactComponent = m_RaycastFocus.collider.transform.GetComponent<IInteractable>();
            if (interactComponent != null) {
                // Perform object's interaction
                interactComponent.Interact(this, m_RaycastFocus.distance);
            }
        }
        // Wasn't right in front of the player, but is it within pull range?
        else if ((GlovePolarity != Charge.Neutral) && m_CanInteract_Far) {
            IInteractable interactComponent = m_RaycastFocus.collider.transform.GetComponent<IInteractable>();
            if (interactComponent != null) {
                // Perform object's interaction
                interactComponent.InteractFar(this, m_RaycastFocus.distance);
            }

        }
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


    /// <summary>
    /// Play the arm interaction animation
    /// </summary>
    public void PlayInteractAnim()
    {
        armInteractPos = 0f;
        armInteractAnim = true;
    }
}
