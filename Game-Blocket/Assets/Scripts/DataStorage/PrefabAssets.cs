using Unity.Netcode;

using UnityEngine;

public class PrefabAssets : NetworkBehaviour{ 
	public GameObject prefabUI, world, playerNetPrefab, loadingScreenPrefab, consoleText, craftingUIListView;
    public GameObject mobEntity;


    public void Awake() => GlobalVariables.PrefabAssets = this;

    public void Start() => gameObject.SetActive(false);
}
