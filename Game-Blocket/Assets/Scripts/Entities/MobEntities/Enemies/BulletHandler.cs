using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    float timeToLive = 10;
    float delay = 0.3f;
    float count = 0;

    private float rotate = 0;

    void FixedUpdate()
    {
        timeToLive -= Time.fixedDeltaTime;
        count += Time.fixedDeltaTime;

        if(count > delay) { 

        if (timeToLive < 0) {
            Destroy(gameObject);
        }
        }

        if (rotate > 360)
        {
            rotate = 0;
        }

        transform.rotation = Quaternion.Euler(0, 0, rotate);
        rotate+=6;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            //deal dmg to player
        }
    }
}
