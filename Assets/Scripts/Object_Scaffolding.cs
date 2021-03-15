using UnityEngine;
public class Object_Scaffolding : MonoBehaviour, IInteractable {
    private Rigidbody m_ThisRigidbody = null;
    public Charge Obj_Polarity = Charge.Neutral;
    private Vector3 speed;
    public float m_force = 4.0f;
    public Transform m_Bound1;
    public Transform m_Bound2;
    public float min_pull_range = 5.0f;

    Collider m_Collider;
    RaycastHit m_Hit;
    bool m_HitDetect;
    float m_MaxDistance;
 
    private void Start() {
        gameObject.tag = "Interactable";
        m_ThisRigidbody = GetComponent<Rigidbody>();
        speed = new Vector3(0,0,0);
        m_MaxDistance = 1.0f;
        m_Collider = GetComponent<Collider>();
    }
 
    // Perform physics
    private void FixedUpdate() {

        Vector3 newPos = m_ThisRigidbody.position + (m_force* speed * Time.fixedDeltaTime);
        float range = (m_Bound1.position - m_Bound2.position).magnitude;
        // If we will stay within the bounds, update our position
        if(((m_Bound1.position - newPos).magnitude <=range) && (m_Bound2.position - newPos).magnitude <= range){
            m_ThisRigidbody.MovePosition( newPos);
        }
        // Don't move next update unless we are still pulling
        speed = new Vector3(0,0,0);
    }
 
    // Pick up the object, or drop it if it is already being held
    public void Interact(PlayerController playerScript, float dist) {
        InteractFar(playerScript, dist);
    }

    // Pull object towards us or push away
    public void InteractFar(PlayerController playerScript, float dist) {
        if(playerScript.GlovePolarity == Charge.Neutral) {return;}
        Vector3 player_pull =  m_ThisRigidbody.position - playerScript.RB.position;
        // Pull
        if(playerScript.GlovePolarity != Obj_Polarity){
            //Boxcast to see if we ware going to collide with something
            m_HitDetect = Physics.BoxCast(m_Collider.bounds.center, transform.localScale, Vector3.Project(-player_pull, transform.forward), out m_Hit, transform.rotation, m_MaxDistance);
            if(m_HitDetect) {
                Debug.Log("Hit " + m_Hit.collider.gameObject.name);
                return;
            }
            AddForce(-player_pull, playerScript.MagnetGloveFalloff, playerScript.MagnetGloveIntensity);
        }
        // Push
        else if (playerScript.GlovePolarity == Obj_Polarity) {
            m_HitDetect = Physics.BoxCast(m_Collider.bounds.center, transform.localScale, Vector3.Project(player_pull, transform.forward), out m_Hit, transform.rotation, m_MaxDistance);
            if(m_HitDetect) {
                Debug.Log("Hit " + m_Hit.collider.gameObject.name);
                return;
            }
            AddForce(player_pull, playerScript.MagnetGloveFalloff, playerScript.MagnetGloveIntensity);
        }
    }
 
    private void AddForce(Vector3 player_pull, float falloff, float intensity) {
        float impulse_by_dist = falloff/player_pull.magnitude;
        Vector3 projection = Vector3.Project(player_pull, transform.forward);
        intensity = Mathf.Min(intensity, projection.magnitude);
        projection.Normalize();
        Vector3 force = projection *impulse_by_dist * intensity;
        speed = force;
    }
} 