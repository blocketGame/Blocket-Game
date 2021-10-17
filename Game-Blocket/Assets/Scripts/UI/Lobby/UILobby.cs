using System.Linq;

using MLAPI;
using MLAPI.Configuration;
using MLAPI.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILobby : MonoBehaviour
{
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

	/// <summary>
	/// Temporary!!<br></br>
	/// 1 = Host; 2 = Client, 3 = Server;
	/// </summary>
	private byte _instaceRole;

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
		
		NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectCallback;
		StartSiteOpen = true;

		serverBtn.onClick.AddListener(() => {
			NetworkManager.Singleton.StartServer();
			StartSiteOpen = false;
			_instaceRole = 3;
		});
		hostBtn.onClick.AddListener(() => {
			StartSiteOpen = false;
			_instaceRole = 1;
			NetworkManager.Singleton.StartHost(null, null, false, playerNetPrefab.PrefabHash);
		});
		clientBtn.onClick.AddListener(() => {
			StartSiteOpen = false;
			startGame.gameObject.SetActive(false);
			_instaceRole = 3;
			NetworkManager.Singleton.StartClient();
		});

		startGame.onClick.AddListener(() => {
			/*
			SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
			SceneManager.MoveGameObjectToScene(GameObject.Find("NetworkManager"), SceneManager.GetSceneByName("MainGame"));
			*/
			NetworkSceneManager.SwitchScene("MainGame");
			GameObject.Find("Player").SetActive(false);
			if(_instaceRole == 1)
				foreach(ulong clientId in NetworkManager.Singleton.ConnectedClients.Keys) {
					Debug.Log(clientId);
					GameObject go = Instantiate(playerNetPrefab.gameObject);
					go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
				}
		});

		goBackBtn.onClick.AddListener(() => {StartSiteOpen = !StartSiteOpen; startGame.gameObject.SetActive(true); });

		testBtn.onClick.AddListener(() => {
			Debug.Log("Pending: " + NetworkManager.Singleton.PendingClients.Keys.ToList<ulong>().Count);
			Debug.Log("Connected: " + NetworkManager.Singleton.ConnectedClientsList.Count);
		});
	}

	private void ClientConnectCallback(ulong clientId) {
		Debug.Log($"Client Connect: {clientId}");
	}
}
