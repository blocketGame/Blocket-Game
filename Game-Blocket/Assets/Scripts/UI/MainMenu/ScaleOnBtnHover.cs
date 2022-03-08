using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScaleOnBtnHover : MonoBehaviour
{
    private Vector2 originalPosition { get; set; }
    private Vector2 admiredPosition;

    public Transform SettingsTransform;
    private Vector2 originalSettingsTransform;
    public Vector2 AdmiredSettingsTransform;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;  
        admiredPosition = originalPosition;
        if (SettingsTransform != null)
        {
            originalSettingsTransform = SettingsTransform.position;
            AdmiredSettingsTransform = SettingsTransform.position;
        }
    }


    private void Update()
    {
        this.transform.position = Vector2.Lerp(transform.position, admiredPosition, Time.deltaTime*3);
        if (SettingsTransform != null)
            SettingsTransform.position = Vector2.Lerp(SettingsTransform.position, AdmiredSettingsTransform, Time.deltaTime * 10);
    }


    public void Expand()
    {
        admiredPosition = new Vector2(transform.position.x + 100, transform.position.y);
    }
    public void Shrink()
    {
        admiredPosition = originalPosition;
    }

    private bool close=false;
    public void OpenSettings()
    {
        if (close)
            AdmiredSettingsTransform = originalSettingsTransform;
        else
            AdmiredSettingsTransform = new Vector2(originalSettingsTransform.x, originalSettingsTransform.y + 1000);
        close = !close;

    }

}
