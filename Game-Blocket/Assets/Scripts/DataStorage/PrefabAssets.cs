
using MLAPI;

using UnityEngine;

public class PrefabAssets : NetworkBehaviour{ 
	public GameObject prefabUI, world, playerNetPrefab, loadingScreen , craftingUIListView;

    public void Awake() => GlobalVariables.PrefabAssets = this;

    public void Start() => gameObject.SetActive(false);
}
