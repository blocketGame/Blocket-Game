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
	public static GameManager Singleton { get; set; }
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
				UILoadingscreen.Visible = false;
			break;
			case GameState.LOBBY:
			UILoadingscreen.Visible = false;
			break;
			case GameState.LOADING:
			UILoadingscreen.Visible = true;
				break;
			case GameState.INGAME:
				///TODO: Clean
				if (!GlobalVariables.LocalPlayer.TryGetComponent(out Rigidbody2D rig))
					throw new NullReferenceException("Rigidbody is Null!");
				rig.simulated = true;
				rig.gravityScale = 1;

				UILoadingscreen.Visible = false;
			break;
			case GameState.PAUSED: break;
			case GameState.NEVER: throw new ArgumentException();
		}
		
	}

    #region Unitymethods

    /// <summary>Sets this class into the <see cref="GlobalVariables"/></summary>
    public void Awake() {
		Singleton = this;
		State = GameState.LOBBY;
		NetworkManager.Singleton.OnClientConnectedCallback += (clientId) => {
			if(!NetworkManager.Singleton.IsServer)
				return;
			if(State == GameState.INGAME || State == GameState.LOADING)
				SpawnPlayer(clientId);
		};
		NetworkManager.Singleton.OnClientDisconnectCallback += (id) => {
			Players[id].Despawn(true);
			Players.Remove(id);
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
		if(NetworkManager.Singleton.IsClient && (State == GameState.LOADING || State == GameState.INGAME)){
			if(GlobalVariables.World != null && ClientTerrainHandler.Singleton == null)
				GlobalVariables.World.AddComponent<ClientTerrainHandler>();

			if (GlobalVariables.LocalPlayer == null)
				FindAndSetPlayer();
		}

		//Serverstuff
	}

	public void OnApplicationQuit(){
		if (State != GameState.INGAME && State != GameState.PAUSED)
			return;
		if (ClientTerrainHandler.Singleton != null)
		{
			PlayerProfile.SavePlayerProfile();
			ProfileHandler.ExportProfile(PlayerProfileNow, true);
		}
		if (ServerTerrainHandler.Singleton != null)
		{
			WorldProfile.SaveComponentsInWorldProfile();
			WorldProfile.SaveWorld(WorldProfileNow);
		}
	}

    #endregion

    #region Clientside

    //Client
    public void InitLocal(){
		UIInventory.Singleton.Init();
		PlayerVariables.Singleton.Init();
	}

	//Client
	public void FindAndSetPlayer(){
		foreach (GameObject iGo in GameObject.FindGameObjectsWithTag("Player")){
			if (iGo.TryGetComponent(out NetworkObject no) && no.IsLocalPlayer){
				//If Local player
				GlobalVariables.LocalPlayer = iGo;
				iGo.name += "(this)";
				Instantiate(PrefabAssets.Singleton.playerCamera).GetComponent<SmoothCamera>().target = iGo.transform;
				GlobalVariables.LocalUI = Instantiate(PrefabAssets.Singleton.mainGameUI);
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
		if (s1.name == "MainGame"){ 
			SceneManager.SetActiveScene(s1);
			State = GameState.LOADING;
			if (NetworkManager.Singleton.IsServer){
				SpawnPlayers();
				//For Server
				GlobalVariables.World = Instantiate(PrefabAssets.Singleton.world);
				//For Clients
				GlobalVariables.World.GetComponent<NetworkObject>().Spawn();
				GlobalVariables.World.AddComponent<ServerTerrainHandler>();
			}
		}

		if(s1.name == "Dungeon"){
			SceneManager.SetActiveScene(SceneManager.GetSceneByName("Dungeon"));
			//Dungeon only
			GameObject generator = GameObject.Find("Dungeongenerator");
			DungeonGenerator dg = generator.GetComponent<DungeonGenerator>();
			dg.GenerateDungeon();
			GlobalVariables.LocalPlayer.transform.position = dg.startposition;
		}
	}

	//Server
	/// <summary><b>Only Server</b><br></br>Spawns a player for one connected client + self if host</summary>
	public void SpawnPlayers(){
		foreach(ulong clientNow in NetworkManager.Singleton.ConnectedClients.Keys)
			SpawnPlayer(clientNow);
	}

	private void SpawnPlayer(ulong clientNow) {
		if(!NetworkManager.Singleton.IsServer || Players.ContainsKey(clientNow))
			return;
		GameObject go = Instantiate(PrefabAssets.Singleton.playerNetPrefab, new Vector3Int(new System.Random().Next(-20, 20), 25, 0), Quaternion.identity);//TODO: Serverrole
		go.name = $"Player: {clientNow}";
		NetworkObject playerNO = go.GetComponent<NetworkObject>();
		playerNO.SpawnAsPlayerObject(clientNow);
		Players.Add(clientNow, playerNO);
	}

    #endregion

	public static void SwitchDimension(Dimension dimensionTo){
		if(PlayerVariables.Dimension == dimensionTo)
			return;
		State = GameState.LOADING;
		PlayerVariables.Dimension = dimensionTo;
		switch(dimensionTo) {
			case Dimension.OVERWORLD:
				SceneManager.LoadScene("MainGame", LoadSceneMode.Additive);
				MoveImportantThings(SceneManager.GetSceneByName("MainGame"));
				UIInventory.Singleton.backgroundParent.SetActive(true);
				SceneManager.UnloadSceneAsync("Dungeon");
				
			break;
			case Dimension.DUNGEON:
				SceneManager.LoadScene("Dungeon", LoadSceneMode.Additive);
				MoveImportantThings(SceneManager.GetSceneByName("Dungeon"));
				//PlayerInteraction.Singleton.enabled = false;
				UIInventory.Singleton.backgroundParent.SetActive(false);
				SceneManager.UnloadSceneAsync("MainGame");
			break;
			case Dimension.OTHER:
				//Future
			break;
			case Dimension.NULL:
				throw new ArgumentException();
			default: 
				throw new ArgumentOutOfRangeException();
		}
		State = GameState.INGAME;
	}
	public static void MoveImportantThings(Scene scene){
		SceneManager.MoveGameObjectToScene(GlobalVariables.LocalPlayer, scene);
		SceneManager.MoveGameObjectToScene(SmoothCamera.Singleton.gameObject, scene);
		SceneManager.MoveGameObjectToScene(UIInventory.Singleton.gameObject, scene);
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