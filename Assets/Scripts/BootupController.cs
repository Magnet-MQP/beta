using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animate the opening sequence
/// </summary>

public class BootupController : MonoBehaviour
{
    public Canvas UICanvas;
    public Image PanelTop;
    public Image PanelBottom;
    public Image PanelLeft;
    public Image PanelRight;
    private float timer = 0;
    private float timerMax = 15f;
    private float width;
    private float height;

    // Start is called before the first frame update
    void Start()
    {
        width = Screen.width;
        height = Screen.height;

        /*
        Vector3 xoffset = new Vector3(width/2, 0, 0);
        PanelLeft.rectTransform.position -= xoffset;
        PanelRight.rectTransform.position += xoffset;
        Vector3 yoffset = new Vector3(0, height/2, 0);
        PanelTop.rectTransform.position -= yoffset;
        PanelBottom.rectTransform.position += yoffset;
        */
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < timerMax)
        {
            timer += Time.deltaTime;
            float speed = 250f*Time.deltaTime;
            float wait = 10f;
            if (timer > wait && (timer < wait+0.4f || timer > wait+2f))
            {
                Vector3 yspeed = new Vector3(0, 2*speed, 0);
                PanelTop.rectTransform.position -= yspeed;
                PanelBottom.rectTransform.position += yspeed;
            }
            if (timer > wait && (timer < wait+0.4f || (timer > wait+1f && timer < wait+6f)))
            {
                Vector3 xspeed = new Vector3(4f*speed, 0, 0);
                PanelLeft.rectTransform.position -= xspeed;
                PanelRight.rectTransform.position += xspeed;
            }

            // hide at the end of the animation
            if (timer >= timerMax)
            {
                PanelTop.enabled = false;
                PanelBottom.enabled = false;
                PanelLeft.enabled = false;
                PanelRight.enabled = false;
            }
        }
    }
}
