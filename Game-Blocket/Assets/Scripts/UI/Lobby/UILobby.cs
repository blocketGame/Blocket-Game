using System;
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
	public NetworkObject playPrefab;
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
		SceneManager.sceneLoaded += GlobalVariables.GameManager.SceneSwitched;
		StartSiteOpen = true;
		
		serverBtn.onClick.AddListener(() => {
			NetworkManager.Singleton.StartServer();
			StartSiteOpen = false;
		});
		hostBtn.onClick.AddListener(() => {
			StartSiteOpen = false;
			NetworkManager.Singleton.StartHost(null, null, false, playPrefab.PrefabHash);
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
}
	/*
	private void ClientConnectCallback(ulong clientId) {
		Debug.Log($"Client Connect: {clientId}");
	}
	*/

