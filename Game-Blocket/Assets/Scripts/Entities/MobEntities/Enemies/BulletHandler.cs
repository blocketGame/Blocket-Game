using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    public int damage;
    public float timeToLive = 10;
    public float delay = 0.3f;
    public float count = 0;

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
            PlayerHealth.Singleton.CurrentHealth = PlayerHealth.Singleton.CurrentHealth - damage;
            ///KnockBack required
            GlobalVariables.LocalPlayer.GetComponent<Rigidbody2D>().AddRelativeForce(70f*GetComponent<Rigidbody2D>().velocity);
        }
    }
}
