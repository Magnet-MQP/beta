using UnityEngine;
public interface IInteractable
{
    void Interact(PlayerController script, float dist);
    void InteractFar(PlayerController script, float dist);
}
public class Object_PhysicsObject : MonoBehaviour, IInteractable {
    public bool m_Held = false;
 
    private Rigidbody m_ThisRigidbody = null;
    private FixedJoint m_HoldJoint = null;
    private GameObject m_HoldObject = null;
    public Charge Obj_Polarity = Charge.Neutral;
    
    [Tooltip("Marked as true when the object has been 'used up', ex. placed in a power supply")]
    public bool Consumed = false; 
    private PlayerController player;
    private GameManager manager;
 
    private void Start() {
        gameObject.tag = "Interactable";
        m_ThisRigidbody = GetComponent<Rigidbody>();
        manager = GameManager.getGameManager();
        player = manager.GetPlayer().GetComponent<PlayerController>();
    }
 
    private void Update() {
        // If the holding joint has broken, drop the object
        if (m_Held)
        {
            if (m_HoldJoint == null || player.GlovePolarity == Charge.Neutral) {
                Drop();
            }
            else if (m_HoldObject)
            {
                // Lock to hands
                transform.position = Vector3.MoveTowards(transform.position, m_HoldObject.transform.position, 1f*Time.deltaTime);
                Debug.DrawLine(transform.position, m_HoldObject.transform.position, Color.white,0f);
            }
        }
    }
 
    // Pick up the object, or drop it if it is already being held
    public void Interact(PlayerController playerScript, float dist) {
        // Is the object currently being held?
        if(player == null) {
            player = playerScript;
        }
        bool repel = (Obj_Polarity == player.GlovePolarity);
        // have they decided to repel the held object?
        if (m_Held && repel) {
            Drop();
            Repel();
        }
        // Are they picking up the object?
        else if(!player.holding && player.GlovePolarity != Charge.Neutral && Obj_Polarity != player.GlovePolarity){
            PickUp();
        }
        // Are they repelling something close to them?
        else if (repel)
        {
            Repel();
        }
    }

    ///<summary>
    //  Pull object towards us or push away
    /// </summary>
    public void InteractFar(PlayerController playerScript, float dist) {
        if(player == null){
            player = playerScript;
        }
        if(player.GlovePolarity != Obj_Polarity){
            Attract();
        }
        else if (player.GlovePolarity == Obj_Polarity) {
            Repel();
        }
    }
 
    /// <summary>
    /// Attach object to hold joint on player
    /// </summary>
    private void PickUp() {
        m_Held = true;
        m_ThisRigidbody.useGravity = false;
        m_HoldJoint = player.m_HandTransform.gameObject.AddComponent<FixedJoint>();
        m_HoldJoint.breakForce = 5000f; // Play with this value
        m_HoldJoint.connectedBody = m_ThisRigidbody;
        m_HoldObject = player.m_HandTransform.gameObject;
        player.holding = true;
    }

 
    /// <summary>
    /// Drops the currently held object by deleting the joint and setting the appropriate variables
    /// </summary>
    private void Drop() {
        m_Held = false;
        m_ThisRigidbody.useGravity = true;
        Destroy(m_HoldJoint);
        m_HoldObject = null;
        player.holding = false;
    }

    /// <summary>
    /// Attract the object to the player
    /// </summary>
    private void Attract() {
        Vector3 forcedir = player.RB.position - m_ThisRigidbody.position;
        float impulse_by_dist = player.MagnetGloveFalloff/forcedir.magnitude;
        forcedir.Normalize();
        Vector3 force = forcedir *impulse_by_dist * player.MagnetGloveIntensity;
        m_ThisRigidbody.AddForce(force);
    }

    /// <summary>
    /// Repel the object from the player
    /// </summary>
    private void Repel() {
        Drop();
        Vector3 forcedir =  m_ThisRigidbody.position - player.RB.position;
        float impulse_by_dist = player.MagnetGloveFalloff/forcedir.magnitude;
        forcedir.Normalize();
        Vector3 force = forcedir * impulse_by_dist * player.MagnetGloveIntensity;
        m_ThisRigidbody.AddForce(force);
    }

    /// <summary>
    /// Safely destroy self, even if held
    /// </summary>
    public void DeleteSelf() {
        Consumed = true;
        if (player != null)
        {
            player.holding = false;
        }
        Destroy(gameObject);
    }
} 