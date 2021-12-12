using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MLAPI;
using UnityEngine.SceneManagement;
using MLAPI.Transports.UNET;
using MLAPI.NetworkVariable.Collections;
using MLAPI.NetworkVariable;

/// <summary>
/// Used for importend Gameengineparts<br></br>
/// Coroutines, Threads, Multiplayerstuff...
/// </summary>
public class GameManager : NetworkBehaviour
{

	public GameObject playerPrefab, worldPrefab;
	public ItemAssets ia;
	/// <summary>Is true if the MainGame is online</summary>
	public static bool severRunning, gameRunning;
	/// <summary>Not used!</summary>
	public static bool isMultiplayer = true;
	public static List<NetworkObject> Players { get; } = new List<NetworkObject>();

	public static PlayerProfile playerProfileNow;
	public static WorldProfile worldProfileNow;

	public UNetTransport uNetTransport;
	//TODO: Coroutines, Ticks....

	/// <summary>Sets this class into the <see cref="GlobalVariables"/></summary>
	public void Awake() {
		GlobalVariables.GameManager = this;
		//GlobalVariables.PrefabAssets = assets;
	}

	public void FixedUpdate()
	{
		if (GlobalVariables.LocalPlayer == null && severRunning){
			FindAndSetPlayer();
			InitPlayerComponents();
		}
	}

	/// <summary>
	/// Not used!<br></br>Use: 
	/// <seealso cref="GlobalVariables.LocalGameVariables.localPlayer"/>
	/// </summary>
	/// <returns>Localplayer-Gameobject</returns>
	public static GameObject GetLocalPlayer() {
		foreach(GameObject iGo in GameObject.FindGameObjectsWithTag("Player")) {
			if(iGo.GetComponent<NetworkObject>()?.IsLocalPlayer ?? false)
				return iGo;
		}
		return null;
	}

	/// <summary>
	/// <b>Only Server/Host</b><br></br>
	/// Spawns a player for one connected client + self if host
	/// </summary>
	public void SpawnPlayers() {
		foreach(ulong clientNow in NetworkManager.Singleton.ConnectedClients.Keys) {
			GameObject go = Instantiate(playerPrefab, new Vector3Int(new System.Random().Next(-20, 20), 25, 0), Quaternion.identity);
			go.name = $"Player: {clientNow}";
			NetworkObject playerNO = go.GetComponent<NetworkObject>();
			playerNO.SpawnAsPlayerObject(clientNow);
			Players.Add(playerNO);
		}
	}

	public void FindAndSetPlayer(){
		foreach (GameObject iGo in GameObject.FindGameObjectsWithTag("Player"))
		{
			if (iGo.GetComponent<NetworkObject>()?.IsLocalPlayer ?? false)
			{
				GlobalVariables.LocalPlayer = iGo;
				iGo.name += "(this)";
			} else {
				//Remove Static Player in MainGameScene
				if (iGo.GetComponent<NetworkObject>()?.IsOwner ?? false)
					Destroy(iGo);
				iGo.GetComponent<PlayerVariables>().playerLogic.SetActive(false);
				iGo.GetComponent<Rigidbody2D>().simulated = false;
			}
		}
	}

	/// <summary>
	/// Init the player Components<br></br>
	/// <see cref="TerrainGeneration"/>, <see cref="UIInventory"/>...
	/// </summary>
	public void InitPlayerComponents() {
		GlobalVariables.GlobalAssets = GameObject.Find("Assets");
		//Inventory
		GlobalVariables.localUI = Instantiate(GlobalVariables.PrefabAssets.prefabUI);

		//Worldgeneration
		if (NetworkManager.Singleton.IsHost)
		{
			GlobalVariables.World = Instantiate(GlobalVariables.PrefabAssets.world);
			GlobalVariables.WorldData.Grid = GlobalVariables.World.GetComponentInChildren<Grid>();
			GlobalVariables.World.GetComponent<NetworkObject>().Spawn();
		}

		FileHandler.LoadComponentsFromPlayerProfile(playerProfileNow);
		FileHandler.LoadComponentsFromWorldProfile(worldProfileNow);
	}

	/// <summary>
	/// After the Scene switches to the Main Game
	/// </summary>
	/// <param name="s1"></param>
	public void SceneSwitched(Scene s1, LoadSceneMode lsm) {
		if(s1.name != "MainGame")
			return;
		if(NetworkManager.Singleton.IsHost)
			SpawnPlayers();
		//Both
		severRunning = true;
	}

	public void OnApplicationQuit() {
		if (severRunning) {
			FileHandler.SavePlayerProfile();
			FileHandler.ExportProfile(playerProfileNow, true);
			FileHandler.SaveWorldProfile();
			FileHandler.SaveWorld(worldProfileNow);
		}
			
	}

}
