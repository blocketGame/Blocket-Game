using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFollowing : MonoBehaviour
{
    public float maxDistanceDelta = 20;

    private void LateUpdate()
    {
        TurnItemToMouseAngle();
    }

    private void TurnItemToMouseAngle()
    {
        transform.rotation = Quaternion.FromToRotation(Vector2.up , NormalizeVector(Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono) - transform.position));
        transform.localPosition = Vector2.MoveTowards(transform.localPosition,NormalizeVector(Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono) - transform.position)*2,maxDistanceDelta);
        Vector2 v = Vector3.Normalize(transform.localPosition);
        if (v.x < 0)
        {
            //transform.rotation = Quaternion.FromToRotation(transform.localPosition, NormalizeVector(Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono) - transform.position));
        }
    }

    private Vector2 NormalizeVector(Vector3 vector)
    {
        return Vector3.Normalize(vector); 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
    }
}
