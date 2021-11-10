using System.Linq;

using MLAPI;
using MLAPI.Configuration;
using MLAPI.SceneManagement;
using MLAPI.Spawning;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

/// <summary>
/// TODO: Cleanup
/// </summary>
public class UILobby : NetworkBehaviour {
	#region UIResources
	[Header("Static: General")]
	public NetworkObject playerNetPrefab;
	public GameObject startSite, lobbySite;

	[Header("Static: Start Site")]
	public Button serverBtn, hostBtn, clientBtn;
	public Text ipInput, portInput;

	[Header("Static: Lobby Site")]
	public Button startGame, goBackBtn, testBtn;
	#endregion

	private bool _startSiteOpen;
	public bool StartSiteOpen {
		get { return _startSiteOpen; }
		set {
			_startSiteOpen = value;
			startSite.SetActive(value);
			lobbySite.SetActive(!value);
		}
	}
	
	public void Awake() {
		//NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectCallback;
		SceneManager.sceneLoaded += SceneSwitched;
		StartSiteOpen = true;
		
		serverBtn.onClick.AddListener(() => {
			NetworkManager.Singleton.StartServer();
			StartSiteOpen = false;
		});
		hostBtn.onClick.AddListener(() => {
			StartSiteOpen = false;
			NetworkManager.Singleton.StartHost(null, null, false, playerNetPrefab.PrefabHash);
		});
		clientBtn.onClick.AddListener(() => {
			StartSiteOpen = false;
			startGame.gameObject.SetActive(false);
			NetworkManager.Singleton.StartClient();
		});

		startGame.onClick.AddListener(() => {
			/*
			SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
			SceneManager.MoveGameObjectToScene(GameObject.Find("NetworkManager"), SceneManager.GetSceneByName("MainGame"));
			*/
			NetworkSceneManager.SwitchScene("MainGame");
		});

		goBackBtn.onClick.AddListener(() => { StartSiteOpen = !StartSiteOpen; startGame.gameObject.SetActive(true); });

		testBtn.onClick.AddListener(() => {
			Debug.Log("Pending: " + NetworkManager.Singleton.PendingClients.Keys.ToList<ulong>().Count);
			Debug.Log("Connected: " + NetworkManager.Singleton.ConnectedClientsList.Count);
		});
	}

	public static GameObject GetLocalPlayer() {
		foreach(GameObject iGo in GameObject.FindGameObjectsWithTag("Player")) {
			if(iGo.GetComponent<NetworkObject>()?.IsLocalPlayer ?? false)
				return iGo;
		}
		return null;
	}

	public void SceneSwitched(Scene s1, LoadSceneMode s2){
		if(s1.name != "MainGame")
			return;
		//Only Host
		if(NetworkManager.Singleton.IsHost)
			foreach(ulong clientNow in NetworkManager.Singleton.ConnectedClients.Keys) {
				GameObject go = Instantiate(playerNetPrefab.gameObject, new Vector3Int(0, 100, 0), Quaternion.identity);
				go.name = $"Player: {clientNow}";
				go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientNow);
			}
		//Both
		foreach(GameObject iGo in GameObject.FindGameObjectsWithTag("Player")) {
			if(iGo.GetComponent<NetworkObject>()?.IsLocalPlayer ?? false) {
				GlobalVariables.localGameVariables.localPlayer = iGo;
				iGo.name += "(this)";
			} else
				iGo.GetComponent<PlayerVariables>().playerLogic.SetActive(false);
		}
		GlobalVariables.localGameVariables.globalAssets = GameObject.Find("Assets");
		//Inventory
		GlobalVariables.localGameVariables.localUI = Instantiate(GlobalVariables.localGameVariables.globalAssets.GetComponent<PrefabAssets>().prefabUI);

		//Worldgeneration
		if(NetworkManager.Singleton.IsHost) {
			GlobalVariables.localGameVariables.world = Instantiate(GlobalVariables.localGameVariables.globalAssets.GetComponent<PrefabAssets>().world);
			GlobalVariables.WorldData.Grid = GlobalVariables.localGameVariables.world.GetComponentInChildren<Grid>();
		}

		/*Debug.LogWarning($"Switched");
		//GameObject.FindGameObjectWithTag("Player")?.SetActive(false);
		GameObject thisPlayer = null;


		foreach(ulong clientNow in NetworkManager.Singleton.ConnectedClients.Keys) {
			GameObject go = Instantiate(playerNetPrefab.gameObject, new Vector3Int(0, 100, 0), Quaternion.identity);
			go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientNow);
			if(clientNow == NetworkManager.Singleton.LocalClientId)
				thisPlayer = go;

			if(clientNow == NetworkManager.Singleton.LocalClientId) {
				//if player is localplayer
				thisPlayer = go;

				GameObject.Find("UI").GetComponent<UIInventory>().inventory = thisPlayer.GetComponent<Inventory>();
				GameObject.Find("Block Editing").GetComponent<Block_Editing>().mainCamera = thisPlayer.GetComponentInChildren<Camera>();
				GameObject.Find("Block Editing").GetComponent<Block_Editing>().player = thisPlayer;
				GameObject.Find("World-Generation").GetComponent<World_Data>().player = thisPlayer;
				GameObject.Find("UI").GetComponent<UIInventory>().Load();
				GameObject.Find("World-Generation").GetComponent<Terrain_Generation>().PlayerPosition = thisPlayer.transform;

				thisPlayer.transform.position = new Vector3Int(0, 100, 0);
				GlobalVariables.gameStarted = true;
			} else {
				go.GetComponentInChildren<Camera>().enabled = false;
			}

		}
		GameObject.Find("World-Generation").GetComponent<TerrainGeneration>().PlayerPosition = thisPlayer.transform;
		GameObject.Find("UI").GetComponent<UIInventory>(). = thisPlayer.GetComponent<Inventory>();
		GameObject.Find("UI").GetComponent<UIInventory>().Load();*/
	}
}
	/*
	private void ClientConnectCallback(ulong clientId) {
		Debug.Log($"Client Connect: {clientId}");
	}
	*/

