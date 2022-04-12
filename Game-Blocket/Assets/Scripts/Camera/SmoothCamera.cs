using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour{
    public static SmoothCamera Singleton { get; private set; }

    public Transform target;
    public Vector3 offset;
    [Range(1, 10)]
    public float smoothFactor;

    private void Awake() => Singleton = this;

    public void FixedUpdate()
    {
        if(target==null)
            target = GlobalVariables.LocalPlayer.transform;
        Follow();
    }

    void Follow()
    {
        Vector3 targetPosition = target.position + offset;
        Vector3 smoothposition = Vector3.Lerp(transform.position, targetPosition, smoothFactor * Time.fixedDeltaTime);
        transform.position = smoothposition;
    }
}
