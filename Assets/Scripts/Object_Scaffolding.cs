using UnityEngine;
public class Object_Scaffolding : MonoBehaviour, IInteractable {
    private Rigidbody m_ThisRigidbody = null;
    public Charge Obj_Polarity = Charge.Neutral;
    private Vector3 speed;
    public float m_force = 4.0f;
    public Transform m_Bound1;
    public Transform m_Bound2;
    public float min_pull_range = 3.0f;
 
    private void Start() {
        gameObject.tag = "Interactable";
        m_ThisRigidbody = GetComponent<Rigidbody>();
        speed = new Vector3(0,0,0);
    }
 
    // Perform physics
    private void FixedUpdate() {
        Vector3 newPos = m_ThisRigidbody.position + (m_force* speed * Time.fixedDeltaTime);
        float range = (m_Bound1.position - m_Bound2.position).magnitude;
        // If we will stay within the bounds, update our position
        if((m_Bound1.position - newPos).magnitude <=range && (m_Bound2.position - newPos).magnitude <= range){
            m_ThisRigidbody.MovePosition( newPos);
        }
        // Don't move next update unless we are still pulling
        speed = new Vector3(0,0,0);
    }
 
    // Pick up the object, or drop it if it is already being held
    public void Interact(PlayerController playerScript, float dist) {
        if(dist >= min_pull_range) {
            InteractFar(playerScript, dist);
        }
    }

    // Pull object towards us or push away
    public void InteractFar(PlayerController playerScript, float dist) {
        if(playerScript.GlovePolarity == Charge.Neutral) {return;}
        Vector3 player_pull =  m_ThisRigidbody.position - playerScript.RB.position;
        if(playerScript.GlovePolarity != Obj_Polarity){
            AddForce(-player_pull, playerScript.MagnetGloveFalloff, playerScript.MagnetGloveIntensity);
        }
        else if (playerScript.GlovePolarity == Obj_Polarity) {
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