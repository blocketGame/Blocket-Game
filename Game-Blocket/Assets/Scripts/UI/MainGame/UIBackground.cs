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
		if(!DebugVariables.BackgroundParalax)
			return;

		foreach(UIBackgroundLayer uIBackgroundLayer in Layers){
			float x = OffsetX * uIBackgroundLayer.speedIndicator;

			//if(x >= canvasWith)
			//	InitBackgroundLayers(1, uIBackgroundLayer);
			//if(x <= -canvasWith)
			//	InitBackgroundLayers(-1, uIBackgroundLayer);
			try{ 
				if(uIBackgroundLayer.speedIndicator == 3 && DebugVariables.BackgroundParalaxDebug)
					Debug.Log($"Speed: {uIBackgroundLayer?.speedIndicator}; Left:{uIBackgroundLayer?.layerLeft?.transform.position.x}; Center:{uIBackgroundLayer?.layerCenter?.transform.position.x}; Right:{uIBackgroundLayer?.layerRight?.transform.position.x}");
			}catch(Exception e){
				Debug.LogWarning(e);
            }

			float withToSwitch = canvasWith / uIBackgroundLayer.speedIndicator;
			if(Mathf.Abs(x) > withToSwitch){
				if(x > 0)
					CanvasSwitch(true, uIBackgroundLayer);
				else
					CanvasSwitch(false, uIBackgroundLayer);
            }

			if(uIBackgroundLayer.layerLeft != null)
				uIBackgroundLayer.layerLeft.transform.localPosition = new Vector3(x - canvasWith, 0);

			if(uIBackgroundLayer.layerRight != null)
				uIBackgroundLayer.layerRight.transform.localPosition = new Vector3(x + canvasWith, 0);

			if(uIBackgroundLayer.layerCenter != null)
				uIBackgroundLayer.layerCenter.transform.localPosition = new Vector3(x, 0);
		}
	}



	public bool LayersNotNull(UIBackgroundLayer uIBackgroundLayer) => uIBackgroundLayer.layerCenter != null && uIBackgroundLayer.layerRight != null && uIBackgroundLayer.layerLeft;

	private void CanvasSwitch(bool left, UIBackgroundLayer uIBackgroundLayer) {
		if(!LayersNotNull(uIBackgroundLayer)){
			Debug.LogWarning("BackgroundLayer null!");
			return;
        }

		if(left){
			uIBackgroundLayer.layerCenter = uIBackgroundLayer.layerLeft;
			Destroy(uIBackgroundLayer.layerRight);
			InitBackgroundLayers(-1, uIBackgroundLayer);
		} else{
			uIBackgroundLayer.layerCenter = uIBackgroundLayer.layerRight;
			Destroy(uIBackgroundLayer.layerLeft);
			InitBackgroundLayers(1, uIBackgroundLayer);
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
			InitBackgroundLayers(null, uIBackgroundLayer);
			Layers.Add(uIBackgroundLayer);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="allignment">1 = Right; -1 = Left; 0 = Center</param>
	/// <param name="uIBackgroundLayer"></param>
	public void InitBackgroundLayers(sbyte? allignment, UIBackgroundLayer uIBackgroundLayer) {
		ParalaxLayer paralaxLayer = uIBackgroundLayer.layer;
		//Center
		if(allignment == null || allignment == 0){
			if(uIBackgroundLayer.layerCenter != null)
				Destroy(uIBackgroundLayer.layerCenter);
			uIBackgroundLayer.layerCenter = Instantiate(parallaxPrefab, transform);
			uIBackgroundLayer.layerCenter.name = paralaxLayer.name + " (Center)";
			uIBackgroundLayer.layerCenter.layer = 5;
			uIBackgroundLayer.layerCenter.transform.localPosition = new Vector3(0,0);
			uIBackgroundLayer.layerCenter.GetComponent<Image>().sprite = paralaxLayer.image;
			if(DebugVariables.BackgroundParalaxDebug)
				Debug.Log("Instantiate Left");
		}
		   
		//Left
		if(allignment == null || allignment < 0){
			if(uIBackgroundLayer.layerLeft != null)
				Destroy(uIBackgroundLayer.layerLeft);
			uIBackgroundLayer.layerLeft = Instantiate(parallaxPrefab, transform);
			uIBackgroundLayer.layerLeft.name = paralaxLayer.name + " (Left)";
			uIBackgroundLayer.layerLeft.layer = 5;
			uIBackgroundLayer.layerLeft.transform.localPosition = new Vector3(-canvasWith, 0);
			uIBackgroundLayer.layerLeft.GetComponent<Image>().sprite = paralaxLayer.image;
			if(DebugVariables.BackgroundParalaxDebug)
				Debug.Log("Instantiate Center");
		}

		//Right
		if(allignment == null || allignment > 0) {
			//Right
			if(uIBackgroundLayer.layerRight != null)
				Destroy(uIBackgroundLayer.layerRight);
			uIBackgroundLayer.layerRight = Instantiate(parallaxPrefab, transform);
			uIBackgroundLayer.layerRight.name = paralaxLayer.name + " (Right)";
			uIBackgroundLayer.layerRight.layer = 5;
			uIBackgroundLayer.layerRight.transform.localPosition = new Vector3(canvasWith, 0);
			uIBackgroundLayer.layerRight.GetComponent<Image>().sprite = paralaxLayer.image;
			if(DebugVariables.BackgroundParalaxDebug)
				Debug.Log("Instantiate Right");
		}
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