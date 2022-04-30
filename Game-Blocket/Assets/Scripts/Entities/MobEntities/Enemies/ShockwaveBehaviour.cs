using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveBehaviour : MonoBehaviour
{
    public float damage = 20f;
    public float range = 4f;
    public float timeToLive = 1f;

    public float delay = 0.3f;
    public float count = 0;

    private float startingValue; //starting value and running value
    private float value;

    public void Start(){
        startingValue = 0.05f;
        value = (range / 50) / timeToLive;
    }

    void FixedUpdate()
    {
        timeToLive -= Time.fixedDeltaTime;
        count += Time.fixedDeltaTime;

            if (timeToLive < 0)
            {
                Destroy(gameObject);
            }

        transform.localScale += new Vector3(value, value, value);
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //deal damage
        }
    }
}
