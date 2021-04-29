using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_PA : MonoBehaviour
{
    public AudioSource source;
    public float maxWait = 120.0f;
    public float minWait = 60.0f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlaySound());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator PlaySound() {
        while(true) {
            float timeToWait = Random.Range(minWait, maxWait);
            yield return new WaitForSeconds(timeToWait);
            source.Play(0);

        }
    }
}
