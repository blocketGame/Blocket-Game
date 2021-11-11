using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheckScript : MonoBehaviour
{
    private GameObject player;

    void Start()
    {
        player = gameObject.transform.parent.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        player.GetComponent<BetterMovementScript>().groundCheck = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        player.GetComponent<BetterMovementScript>().groundCheck = false;
    }
}
