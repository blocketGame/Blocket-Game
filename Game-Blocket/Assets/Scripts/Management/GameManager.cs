using MLAPI;
using MLAPI.Transports.UNET;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used for importend Gameengineparts<br></br>
/// Coroutines, Threads, Multiplayerstuff...
/// </summary>
public class GameManager : NetworkBehaviour
{
	public static GameState State { get => _state; 
		set { 
			_state = value;
			SateSwitched(value);
		} 
	}
	private static GameState _state;

	public GameObject playerPrefab, worldPrefab;
	/// <summary>Is true if the MainGame is online</summary>
	public static bool severRunning, gameRunning;
	/// <summary>Not used!</summary>
	public static bool isMultiplayer = true;
	public static List<NetworkObject> Players { get; } = new List<NetworkObject>();

	//TODO not static
	public static PlayerProfile playerProfileNow;
	public static WorldProfile worldProfileNow;
	public static SettingsProfile SPNow { get; private set; } = new SettingsProfile("local", null);

	public UNetTransport uNetTransport;
	//TODO: Coroutines, Ticks....

	public static void SateSwitched(GameState state) {
		if (DebugVariables.ShowGameStateEvent)
			Debug.Log($"GameState Switched to: {state}");
		switch (state) {
			case GameState.MENU: break;
			case GameState.LOBBY: break;
			case GameState.LOADING: 
				break;
			case GameState.INGAME:
				///TODO: Clean
				if (!GlobalVariables.LocalPlayer.TryGetComponent(out Rigidbody2D rig))
					throw new NullReferenceException("Rigidbody is Null!");
				rig.simulated = true;
				rig.gravityScale = 1;
			break;
			case GameState.PAUSED: break;
			case GameState.NEVER: throw new ArgumentException();
		}
		
	}

	/// <summary>Sets this class into the <see cref="GlobalVariables"/></summary>
	public void Awake() {
		GlobalVariables.GameManager = this;
		State = GameState.LOBBY;
	}

	public void FixedUpdate()
	{
		if (GlobalVariables.LocalPlayer == null && State == GameState.LOADING){
			InitPlayerComponents();
			FindAndSetPlayer();
			InitLocal();
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

	public void InitLocal(){
		GlobalVariables.UIInventory.Init();
		GlobalVariables.PlayerVariables.Init();
	}

	public void FindAndSetPlayer(){
		foreach (GameObject iGo in GameObject.FindGameObjectsWithTag("Player"))
		{
			if (iGo.TryGetComponent(out NetworkObject no) && no.IsLocalPlayer)
			{
				GlobalVariables.LocalPlayer = iGo;
				iGo.name += "(this)";
			} else {
				iGo.GetComponent<PlayerVariables>().playerLogic.SetActive(false);
				if (iGo.TryGetComponent(out Rigidbody2D rig))
					rig.gravityScale = 0;
				else
					Debug.LogWarning($"Rigidody Missing: {no.NetworkObjectId}");
			
			}
				
		}
	}

	/// <summary>
	/// Init the player Components<br></br>
	/// <see cref="TerrainGeneration"/>, <see cref="UIInventory"/>...
	/// </summary>
	public void InitPlayerComponents() {
		//Inventory
		GlobalVariables.LocalUI = Instantiate(GlobalVariables.PrefabAssets.prefabUI);

		//Worldgeneration
		if (NetworkManager.Singleton.IsHost)
		{
			GlobalVariables.World = Instantiate(GlobalVariables.PrefabAssets.world);
			GlobalVariables.WorldData.Grid = GlobalVariables.World.GetComponentInChildren<Grid>();
			GlobalVariables.World.GetComponent<NetworkObject>().Spawn();
		}

		//ProfileHandler.LoadComponentsFromPlayerProfile(playerProfileNow);
		//ProfileHandler.LoadComponentsFromWorldProfile(worldProfileNow);
	}

	/// <summary>
	/// After the Scene switches to the Main Game
	/// </summary>
	/// <param name="s1"></param>
	public void SceneSwitched(Scene s1, LoadSceneMode lsm) {
		if (s1.name != "MainGame")
			return;
		if (NetworkManager.Singleton.IsHost) {
			SpawnPlayers();
			State = GameState.LOADING;
		}
	}

	public void OnApplicationQuit() => SaveAll();

	public static void SaveAll() {
		if (State != GameState.INGAME && State != GameState.PAUSED)
			return;
		PlayerProfile.SavePlayerProfile();
		ProfileHandler.ExportProfile(playerProfileNow, true);
		WorldProfile.SaveComponentsInWorldProfile();
		WorldProfile.SaveWorld(worldProfileNow);
	}

}

/// <summary>
/// Defines in which State our Game is
/// </summary>
public enum GameState {
	/// <summary>
	/// Game is in MainMenu
	/// </summary>
	MENU,

	/// <summary>
	/// Game is in Lobby
	/// </summary>
	LOBBY, 

	/// <summary>
	/// Started but LOADING
	/// </summary>
	LOADING, 

	/// <summary>
	/// Loading finished and playable
	/// </summary>
	INGAME,

	/// <summary>
	/// Ingame but the Game is paused; For frontend and singleplayer
	/// </summary>
	PAUSED,

	/// <summary>
	/// Debug Only! Will never ocure
	/// </summary>
	NEVER
}