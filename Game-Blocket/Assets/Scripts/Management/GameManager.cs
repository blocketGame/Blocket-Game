using System;
using System.Collections.Generic;

using Unity.Netcode;
using Unity.Netcode.Transports.UNET;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used for importend Gameengineparts<br></br>
/// Coroutines, Threads, Multiplayerstuff...
/// </summary>
public class GameManager : MonoBehaviour {
	//Both
	public static GameState State { get => _state; 
		set { 
			_state = value;
			SateSwitched(value);
		} 
	}
	private static GameState _state;
	
	//Client
	public static PlayerProfile PlayerProfileNow { get; set; }
	public static SettingsProfile SettingsProfile { get; private set; } = new SettingsProfile("local", null);

	public static GameObject LoadinScreenNow { get; set; }


	public UNetTransport uNetTransport;

	//Server
	public static Dictionary<ulong, NetworkObject> Players { get; } = new Dictionary<ulong, NetworkObject>();
	public static WorldProfile WorldProfileNow { get; set; }
	public static void SateSwitched(GameState state) {
		if (DebugVariables.ShowGameStateEvent)
			Debug.Log($"GameState Switched to: {state}");
		switch (state) {
			case GameState.MENU: 

				break;
			case GameState.LOBBY: 

				break;
			case GameState.LOADING:
				GlobalVariables.UIInventory?.loadingScreen?.SetActive(true);
				break;
			case GameState.INGAME:
				///TODO: Clean
				if (!GlobalVariables.LocalPlayer.TryGetComponent(out Rigidbody2D rig))
					throw new NullReferenceException("Rigidbody is Null!");
				rig.simulated = true;
				rig.gravityScale = 1;

				GlobalVariables.UIInventory.loadingScreen.SetActive(false);
			break;
			case GameState.DUNGEON: break;
			case GameState.PAUSED: break;
			case GameState.NEVER: throw new ArgumentException();
		}
		
	}

    #region Unitymethods

    /// <summary>Sets this class into the <see cref="GlobalVariables"/></summary>
    public void Awake() {
		GlobalVariables.GameManager = this;
		State = GameState.LOBBY;
		NetworkManager.Singleton.OnClientConnectedCallback += (clientId) => {
			if(!NetworkManager.Singleton.IsServer)
				return;
			if(State == GameState.INGAME || State == GameState.LOADING)
				SpawnPlayer(clientId);
		};
		NetworkManager.Singleton.OnClientDisconnectCallback += (id) => {
			Players[id].Despawn(true);
			if(NetworkManager.Singleton.ServerClientId == id && NetworkManager.Singleton.IsClient)
				QuitGame();
        };
	}

	public void QuitGame(){
		SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
		Camera.main.gameObject.SetActive(false);
		SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
	}

    public void FixedUpdate(){
		if(NetworkManager.Singleton.IsClient){
			//Find and set World
			if(GlobalVariables.World == null)
				GlobalVariables.World ??= GameObject.FindGameObjectWithTag("World");

			if(GlobalVariables.World != null)
				GlobalVariables.ClientTerrainHandler ??= GlobalVariables.World.AddComponent<ClientTerrainHandler>();

			if (GlobalVariables.LocalPlayer == null)
				FindAndSetPlayer();
		}

		//Serverstuff
	}

	public void OnApplicationQuit(){
		if (State != GameState.INGAME && State != GameState.PAUSED)
			return;
		if (GlobalVariables.ClientTerrainHandler != null)
		{
			PlayerProfile.SavePlayerProfile();
			ProfileHandler.ExportProfile(PlayerProfileNow, true);
		}
		if (GlobalVariables.ServerTerrainHandler != null)
		{
			WorldProfile.SaveComponentsInWorldProfile();
			WorldProfile.SaveWorld(WorldProfileNow);
		}
	}

    #endregion

    #region Clientside

    //Client
    public void InitLocal(){
		GlobalVariables.UIInventory.Init();
		GlobalVariables.PlayerVariables.Init();
	}

	//Client
	public void FindAndSetPlayer(){
		foreach (GameObject iGo in GameObject.FindGameObjectsWithTag("Player")){
			if (iGo.TryGetComponent(out NetworkObject no) && no.IsLocalPlayer){
				//If Local player
				GlobalVariables.LocalPlayer = iGo;
				iGo.name += "(this)";
				
				GlobalVariables.LocalUI = Instantiate(GlobalVariables.PrefabAssets.prefabUI);
				InitLocal();
			} else {
				//If not Local player
				iGo.GetComponent<PlayerVariables>().playerLogic.SetActive(false);
				if (iGo.TryGetComponent(out Rigidbody2D rig))
					rig.gravityScale = 0;
				else
					Debug.LogWarning($"Rigidody Missing: {no.NetworkObjectId}");
			}
				
		}
	}

	#endregion

    #region Serverside

    //Both (more server)
    /// <summary>After the Scene switches to the Main Game</summary>
    public void SceneSwitched(Scene s1, LoadSceneMode lsm) {
		if (s1.name != "MainGame")
			return;
		State = GameState.LOADING;
		if (NetworkManager.Singleton.IsServer){
			SpawnPlayers();
			//For Server
			GlobalVariables.World = Instantiate(GlobalVariables.PrefabAssets.world);
			//For Clients
			GlobalVariables.World.GetComponent<NetworkObject>().Spawn();
			GlobalVariables.ServerTerrainHandler = GlobalVariables.World.AddComponent<ServerTerrainHandler>();
		}
	}

	//Server
	/// <summary><b>Only Server</b><br></br>Spawns a player for one connected client + self if host</summary>
	public void SpawnPlayers(){
		foreach(ulong clientNow in NetworkManager.Singleton.ConnectedClients.Keys)
			SpawnPlayer(clientNow);
	}

	private void SpawnPlayer(ulong clientNow) {
		if(!NetworkManager.Singleton.IsServer)
			return;
		GameObject go = Instantiate(GlobalVariables.PrefabAssets.playerNetPrefab, new Vector3Int(new System.Random().Next(-20, 20), 25, 0), Quaternion.identity);//TODO: Serverrole
		go.name = $"Player: {clientNow}";
		NetworkObject playerNO = go.GetComponent<NetworkObject>();
		playerNO.SpawnAsPlayerObject(clientNow);
		Players.Add(clientNow, playerNO);
	}

    #endregion

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
	/// Player is currently in a dungeon
	/// </summary>
	DUNGEON,

	/// <summary>
	/// Ingame but the Game is paused; For frontend and singleplayer
	/// </summary>
	PAUSED,

	/// <summary>
	/// Debug Only! Will never ocure
	/// </summary>
	NEVER
}