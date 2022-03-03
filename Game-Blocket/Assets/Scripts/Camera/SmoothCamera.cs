using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour
{

    public Transform Target;
    public Vector3 offset;
    [Range(1, 10)]
    public float smoothFactor;

    private void Awake()
    {
    }

    public void FixedUpdate()
    {
        if(Target==null)
        Target = GlobalVariables.LocalPlayer.transform;
        Follow();
    }

    void Follow()
    {
        
        Vector3 targetPosition = Target.position + offset;
        Vector3 smoothposition = Vector3.Lerp(transform.position, targetPosition, smoothFactor*Time.fixedDeltaTime);
        transform.position = smoothposition;
    }
}
