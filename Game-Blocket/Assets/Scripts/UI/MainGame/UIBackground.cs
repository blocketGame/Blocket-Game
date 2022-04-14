using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBackground : MonoBehaviour
{
	public GameObject parallaxPrefab;
	public BackgroundParallax paralaxNow;

	public uint fullWithInWorld = 1920;
	public uint canvasWith = 1920;

	private List<UIBackgroundLayer> Layers { get; } = new List<UIBackgroundLayer>();

	public Vector2 PlayerVelocity => Movement.Singleton.playerRigidbody.velocity;

	public float OffsetX => -(GlobalVariables.LocalPlayerPos.x % fullWithInWorld);

	public void FixedUpdate() {
		//TODO: ...

		foreach(UIBackgroundLayer uIBackgroundLayer in Layers){

			float x = OffsetX * uIBackgroundLayer.speedIndicator;

			if(x >= canvasWith)
				InitBackgroundLayers(-1, uIBackgroundLayer.layer, uIBackgroundLayer);
			if(x <= -canvasWith)
				InitBackgroundLayers(1, uIBackgroundLayer.layer, uIBackgroundLayer);

			if(uIBackgroundLayer.layerLeft != null)
				uIBackgroundLayer.layerLeft.transform.localPosition = new Vector3(x - canvasWith, 0);

			if(uIBackgroundLayer.layerRight != null)
				uIBackgroundLayer.layerRight.transform.localPosition = new Vector3(x + canvasWith, 0);

			if(uIBackgroundLayer.layerCenter != null)
				uIBackgroundLayer.layerCenter.transform.localPosition = new Vector3(x, 0);
		}
	}

	private void Awake() {
		//Clean
		for(int i = 0; i < paralaxNow.paralaxLayers.Count; i++){
			ParalaxLayer paralaxLayer = paralaxNow.paralaxLayers[i];
			UIBackgroundLayer uIBackgroundLayer = new UIBackgroundLayer() {
				layer = paralaxLayer,
				speedIndicator = paralaxLayer.speed
			};
			InitBackgroundLayers(null, paralaxLayer, uIBackgroundLayer);
			Layers.Add(uIBackgroundLayer);
		}
	}

	public void InitBackgroundLayers(sbyte? layer, ParalaxLayer paralaxLayer, UIBackgroundLayer uIBackgroundLayer) {
		//Center
		if(layer == null || layer == 0){
			if(uIBackgroundLayer.layerCenter != null)
				Destroy(uIBackgroundLayer.layerCenter);
			uIBackgroundLayer.layerCenter = Instantiate(parallaxPrefab, transform);
			uIBackgroundLayer.layerCenter.name = paralaxLayer.name + " (Center)";
			uIBackgroundLayer.layerCenter.layer = 5;
			uIBackgroundLayer.layerCenter.transform.localPosition = new Vector3(0,0);
			uIBackgroundLayer.layerCenter.GetComponent<Image>().sprite = paralaxLayer.image;
		}
		   
		//Left
		if(layer == null || layer < 0){
			if(uIBackgroundLayer.layerLeft != null)
				Destroy(uIBackgroundLayer.layerLeft);
			uIBackgroundLayer.layerLeft = Instantiate(parallaxPrefab, transform);
			uIBackgroundLayer.layerLeft.name = paralaxLayer.name + " (Left)";
			uIBackgroundLayer.layerLeft.layer = 5;
			uIBackgroundLayer.layerLeft.transform.localPosition = new Vector3(-canvasWith, 0);
			uIBackgroundLayer.layerLeft.GetComponent<Image>().sprite = paralaxLayer.image;
		}

		//Right
		if(layer == null || layer > 0) {
			//Right
			if(uIBackgroundLayer.layerRight != null)
				Destroy(uIBackgroundLayer.layerRight);
			uIBackgroundLayer.layerRight = Instantiate(parallaxPrefab, transform);
			uIBackgroundLayer.layerRight.name = paralaxLayer.name + " (Right)";
			uIBackgroundLayer.layerRight.layer = 5;
			uIBackgroundLayer.layerRight.transform.localPosition = new Vector3(canvasWith, 0);
			uIBackgroundLayer.layerRight.GetComponent<Image>().sprite = paralaxLayer.image;
		}
	}

	private void Start() {
		//foreach(UIBackgroundLayer uIBackgroundLayer in Layers) {
		//	InitRectTransfrom(uIBackgroundLayer.layerLeft.transform, -1);
		//	InitRectTransfrom(uIBackgroundLayer.layerRight.transform, 1);
		//	InitRectTransfrom(uIBackgroundLayer.layerCenter.transform, 0);
		//}
	}

	private void InitRectTransfrom(Transform rT, sbyte x){
		if(x < 0) {
			rT.localPosition = new Vector2(-1920, 0);
			//rT.localScale = new Vector2(-1, 1);
		}
		if(x == 0) {
			rT.localPosition = new Vector2(0, 0);
			//rT.transform.localScale = new Vector2(0.95f, 0.95f);
		}
		if(x > 0) {
			rT.localPosition = new Vector2(1920, 0);
			//rT.localScale = new Vector2(-1, 1);
		}
	}
}

[Serializable]
public class UIBackgroundLayer{
	public ParalaxLayer layer;
	public float speedIndicator;
	public GameObject layerCenter, layerLeft, layerRight;
}