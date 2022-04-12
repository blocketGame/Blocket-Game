using System;
using System.Collections.Generic;
using UnityEngine;

public class UIBackground : MonoBehaviour
{
    public BackgroundParallax paralaxNow;

    private List<UIBackgroundLayer> Layers { get; set; }

    public Vector2 PlayerVelocity => Movement.Singleton.playerRigidbody.velocity;

    public void FixedUpdate() {
        //TODO: ...
        foreach(UIBackgroundLayer uIBackgroundLayer in Layers){
            //...
        }
    }

    private void Awake() {
        foreach(ParalaxLayer paralaxLayer in paralaxNow.paralaxLayers){
            UIBackgroundLayer uIBackgroundLayer = new UIBackgroundLayer();

            //Center
            uIBackgroundLayer.layerCenter = new GameObject();
            uIBackgroundLayer.layerCenter.transform.SetParent(transform);
            uIBackgroundLayer.spriteRendererCenter = uIBackgroundLayer.layerCenter.AddComponent<SpriteRenderer>();
            uIBackgroundLayer.spriteRendererCenter.sprite = paralaxLayer.image;

            //Left
            uIBackgroundLayer.layerLeft = new GameObject();
            uIBackgroundLayer.layerLeft.transform.position = new Vector3(-paralaxLayer.image.rect.width, 0);
            uIBackgroundLayer.layerLeft.transform.SetParent(transform);
            uIBackgroundLayer.spriteRendererLeft = uIBackgroundLayer.layerLeft.AddComponent<SpriteRenderer>();
            uIBackgroundLayer.spriteRendererLeft.sprite = paralaxLayer.image;

            //Right
            uIBackgroundLayer.layerRight = new GameObject();
            uIBackgroundLayer.layerRight.transform.position = new Vector3(paralaxLayer.image.rect.width, 0);
            uIBackgroundLayer.layerRight.transform.SetParent(transform);
            uIBackgroundLayer.spriteRendererRight = uIBackgroundLayer.layerRight.AddComponent<SpriteRenderer>();
            uIBackgroundLayer.spriteRendererRight.sprite = paralaxLayer.image;

            Layers.Add(uIBackgroundLayer);
        }
    }
}

[Serializable]
public struct UIBackgroundLayer{
    public GameObject layerCenter, layerLeft, layerRight;
    public SpriteRenderer spriteRendererCenter, spriteRendererLeft, spriteRendererRight;
}