using System.Collections;
using System.Collections.Generic;
using UnityEditor;

using UnityEngine;

public class AssetIO : MonoBehaviour {

    public void SaveAssets() {

	}

	public void LoadAssets() {

	}

	public void CheckAssets() {
		if (!gameObject.TryGetComponent(out ItemAssets ia))
			Debug.LogError("A");
		foreach(Structure so in ia.Structures)
			Debug.Log(EditorJsonUtility.ToJson(so));
	}
}
