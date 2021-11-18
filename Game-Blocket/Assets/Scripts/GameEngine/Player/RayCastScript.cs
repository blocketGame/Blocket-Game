using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO
/// </summary>
public class RayCastScript : MonoBehaviour
{
    // Start is called before the first frame update

    public Ray ray2D;
    public RaycastHit hit;

    public Camera mainCamera;
    void Start()
    {
        ray2D = mainCamera.ScreenPointToRay(Input.mousePosition);
        
    }

    // Update is called once per frame
    void Update()
    {
        ray2D = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray2D.origin, ray2D.direction * 10);
        Physics.Raycast(ray2D, out hit);
        Debug.Log(hit.transform);
    }
}
