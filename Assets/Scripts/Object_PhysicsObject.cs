using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    private PlayerController controller = null;
 
 
    private void Start() {
        gameObject.tag = "Interactable";
        m_ThisRigidbody = GetComponent<Rigidbody>();
    }
 
    private void Update() {
        // If the holding joint has broken, drop the object
        if (m_Held)
        {
            if (m_HoldJoint == null) {
                m_Held = false;
                m_ThisRigidbody.useGravity = true;
                controller.holding = false;
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
        //Debug.Log("Interact");
        // Is the object currently being held?
        if(controller == null) {
            controller = playerScript;
        }
        bool repel = (Obj_Polarity == playerScript.GlovePolarity);
        if (m_Held && (repel || (playerScript.GlovePolarity == Charge.Neutral))) {
            // Debug.Log("DROP");
            Drop();
            Repel(playerScript);
            playerScript.holding = false;
            if (repel)
            {
                m_ThisRigidbody.AddForce(1000f*playerScript.MainCamera.transform.forward, ForceMode.Impulse);
            }
        }
        else if(!playerScript.holding && playerScript.GlovePolarity != Charge.Neutral && Obj_Polarity != playerScript.GlovePolarity){
            m_Held = true;
            m_ThisRigidbody.useGravity = false;
            //m_ThisRigidbody.position = playerScript.m_HandTransform.position;
 
            m_HoldJoint = playerScript.m_HandTransform.gameObject.AddComponent<FixedJoint>();
            m_HoldJoint.breakForce = 5000f; // Play with this value
            m_HoldJoint.connectedBody = m_ThisRigidbody;
            m_HoldObject = playerScript.m_HandTransform.gameObject;
            //Debug.Log("HOLDING");
            playerScript.holding = true;
        }
        else if (repel)
        {
            Repel(playerScript);
        }
    }

    // Pull object towards us or push away
    public void InteractFar(PlayerController playerScript, float dist) {
        //Debug.Log("Interact far");
        if(playerScript.GlovePolarity != Obj_Polarity){
            Attract(playerScript);
        }
        else if (playerScript.GlovePolarity == Obj_Polarity) {
            Repel(playerScript);
        }
    }
 
 
    // Drop the object
    private void Drop() {
        //Debug.Log("Drop");
        m_Held = false;
        m_ThisRigidbody.useGravity = true;
 
        Destroy(m_HoldJoint);
        m_HoldObject = null;
    }
    private void Attract(PlayerController playerScript) {
        // Debug.Log("Attract");
        Vector3 forcedir = playerScript.RB.position - m_ThisRigidbody.position;
        float impulse_by_dist = playerScript.MagnetGloveFalloff/forcedir.magnitude;
//            Debug.Log(forcedir);
        forcedir.Normalize();
        Vector3 force = forcedir *impulse_by_dist * playerScript.MagnetGloveIntensity;
        m_ThisRigidbody.AddForce(force);

    }

    private void Repel(PlayerController playerScript) {
        //Debug.Log("Repel");
    Vector3 forcedir =  m_ThisRigidbody.position - playerScript.RB.position;
    float impulse_by_dist = playerScript.MagnetGloveFalloff/forcedir.magnitude;
    forcedir.Normalize();
    Vector3 force = forcedir * impulse_by_dist * playerScript.MagnetGloveIntensity;
    m_ThisRigidbody.AddForce(force);

    }
} 