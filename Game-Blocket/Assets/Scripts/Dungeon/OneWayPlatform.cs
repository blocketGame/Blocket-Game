using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    public PlatformEffector2D effector;
    private float disabletime = 0.1f;

    // Update is called once per frame
    void Update()
    {
        if (effector.colliderMask == 1024)
        {
            if (disabletime <= 0)
                effector.colliderMask = 1152;
            else
                disabletime -= Time.deltaTime;
            
        }

        if (Input.GetKey(KeyCode.S))
        {
            effector.colliderMask = 1024;
            disabletime = 0.25f;
        }
    }
}
