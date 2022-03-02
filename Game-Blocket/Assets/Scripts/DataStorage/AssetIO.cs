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
		foreach(Structure so in GlobalVariables.Structures.structures)
			Debug.Log(EditorJsonUtility.ToJson(so));
	}
}
