using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBackgroundWithPlayer : MonoBehaviour
{
    private Vector3 lastCameraPos;
    private Transform cameraTransf;
    [SerializeField] public Vector2 parallaxEffectMultiplier;
    private float textureUnitSizeX;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransf = Camera.main.transform;
        lastCameraPos = cameraTransf.position;
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        textureUnitSizeX = (texture.width*5 / sprite.pixelsPerUnit);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransf.position - lastCameraPos;
        Vector2.Lerp(transform.position, new Vector2(deltaMovement.x * parallaxEffectMultiplier.x, deltaMovement.y * parallaxEffectMultiplier.y), Time.deltaTime);
        lastCameraPos = cameraTransf.position;

        if(Math.Abs(cameraTransf.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetPosX = (cameraTransf.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cameraTransf.position.x + offsetPosX, transform.position.y);
        }
    }
}