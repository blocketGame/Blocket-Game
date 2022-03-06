#if(UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(AssetIO))]
public class AssetIOEditor : Editor
{
	public AssetIO TargetScript { get => _trgetScript; set {
			_trgetScript = value;
			TargetGO = value.gameObject;
		} }
	private AssetIO _trgetScript;
	public GameObject TargetGO { get; private set; }

	public void OnEnable() {
		// Method 1
		TargetScript = (target as AssetIO);
	}

	public override void OnInspectorGUI() {
		if (GUILayout.Button("Save")) {
			TargetScript.CheckAssets();	
		}
		if (GUILayout.Button("Load")) {
			
		}

		// Draw default inspector after button...
		base.OnInspectorGUI();
	}
}
#endif