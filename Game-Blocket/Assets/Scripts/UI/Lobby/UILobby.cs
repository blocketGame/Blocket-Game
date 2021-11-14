using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;

using MLAPI;
using MLAPI.Configuration;
using MLAPI.SceneManagement;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;

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
	public Text ipInput, portInput, ipPlaceHolder;

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
		ipPlaceHolder.text = GlobalVariables.ipAddress;
		serverBtn.onClick.AddListener(() => {
			CheckAndSetInputs();
			SetNetworkAddress();
			NetworkManager.Singleton.StartServer();
			StartSiteOpen = false;
		});
		hostBtn.onClick.AddListener(() => {
			CheckAndSetInputs();
			if(GlobalVariables.ipAddress != "127.0.0.1")
			GlobalVariables.ipAddress = GetLocalIPAddress();
			StartSiteOpen = false; 
			SetNetworkAddress();
			NetworkManager.Singleton.StartHost(null, null, false, playPrefab.PrefabHash);
		});
		clientBtn.onClick.AddListener(() => {
			CheckAndSetInputs();
			StartSiteOpen = false;
			startGame.gameObject.SetActive(false);
			SetNetworkAddress();
			NetworkManager.Singleton.StartClient();
		});

		startGame.onClick.AddListener(() => {
			NetworkSceneManager.SwitchScene("MainGame");
		});

		goBackBtn.onClick.AddListener(() => { 
			StartSiteOpen = !StartSiteOpen; startGame.gameObject.SetActive(true);
			NetworkManager.Singleton.Shutdown();
		});

		testBtn.onClick.AddListener(() => {
			Debug.Log("Pending: " + NetworkManager.Singleton.PendingClients.Keys.ToList<ulong>().Count);
			Debug.Log("Connected: " + NetworkManager.Singleton.ConnectedClientsList.Count);
		});
	}

	private void SetNetworkAddress()
    {
		GlobalVariables.GameManager.uNetTransport.ConnectAddress = GlobalVariables.ipAddress;
		GlobalVariables.GameManager.uNetTransport.ConnectPort = GlobalVariables.portAddress;
	}

    /// <summary>
    /// TODO: More check
    /// </summary>
    /// <returns></returns>
    private void CheckAndSetInputs(){
		if (ipInput.text.Length > 8 && ipInput.text.Trim() != string.Empty && ipInput.text.IndexOf(".") != ipInput.text.LastIndexOf("."))
			GlobalVariables.ipAddress = ipInput.text;
        else
			Debug.LogWarning($"IP-Input empty! Using: {GlobalVariables.ipAddress}");
			
		if (portInput.text.Trim() != "" && portInput.text.ToUpper() == portInput.text.ToLower())
			GlobalVariables.portAddress = int.Parse(portInput.text);
        else
			Debug.LogWarning($"Port-Input empty! Using: {GlobalVariables.portAddress}");
		
    }

	public static string GetLocalIPAddress(){
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
			socket.Connect("8.8.8.8", 65530);
        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
        return endPoint.Address.ToString();
    }

}