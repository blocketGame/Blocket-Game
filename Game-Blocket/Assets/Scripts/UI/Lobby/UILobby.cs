using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;

using MLAPI;
using MLAPI.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// LobbyUI-Handling
/// </summary>
public class UILobby : NetworkBehaviour {
	#region UIResources
	[Header("Static: General")]
	public NetworkObject playPrefab;
	public GameObject startSite, lobbySite, uiprofileSitePrefab;

	[Header("Static: Start Site")]
	public Button serverBtn;
	public Button hostBtn, clientBtn;
	public Text ipInput, portInput, ipPlaceHolder;

	[Header("Static: Lobby Site")]
	public Button startGame;
	public Button goBackBtn, testBtn;

	public static readonly bool useProfiles = false;
	#endregion

	private byte _siteIndexOpen;
	public byte SiteIndexOpen { get => _siteIndexOpen; set{
			_siteIndexOpen = value;
			ManageSites(value);
		}
	}

	/// <summary>
	/// Manages the sides
	/// </summary>
	/// <param name="site"></param>
	public void ManageSites(byte site)
	{
		if(site == 0)
			startSite.SetActive(true);
		else
			startSite.SetActive(false);
		if (site == 1)
			lobbySite.SetActive(true);
		else
			lobbySite.SetActive(false);
		if (site == 2)
			uiprofileSitePrefab.SetActive(true);
		else
			uiprofileSitePrefab.SetActive(false);
	}

	/// <summary>
	/// Inits the Buttons from the LobbyUI
	/// </summary>
	private void InitButtons()
    {
		serverBtn.onClick.AddListener(() => {
			CheckAndSetInputs();
			SetNetworkAddress();
			NetworkManager.Singleton.StartServer();
			SiteIndexOpen = 1;
		});
		hostBtn.onClick.AddListener(() => {
			CheckAndSetInputs();
			if (GlobalVariables.ipAddress != "127.0.0.1")
				GlobalVariables.ipAddress = GetLocalIPAddress();
			SiteIndexOpen = useProfiles ? (byte)2 : (byte)1;
			SetNetworkAddress();
			NetworkManager.Singleton.StartHost(null, null, false, playPrefab.PrefabHash);
		});
		clientBtn.onClick.AddListener(() => {
			CheckAndSetInputs();
			SiteIndexOpen = useProfiles ? (byte)2 : (byte)1;
			startGame.gameObject.SetActive(false);
			SetNetworkAddress();
			NetworkManager.Singleton.StartClient();
		});

		startGame.onClick.AddListener(() => {
			NetworkSceneManager.SwitchScene("MainGame");
		});

		goBackBtn.onClick.AddListener(() => {

			SiteIndexOpen = 0;
			startGame.gameObject.SetActive(true);
			NetworkManager.Singleton.Shutdown();
		});

		testBtn.onClick.AddListener(() => {
			Debug.Log("Pending: " + NetworkManager.Singleton.PendingClients.Keys.ToList<ulong>().Count);
			Debug.Log("Connected: " + NetworkManager.Singleton.ConnectedClientsList.Count);
		});
	}

	public void Awake() {
		GlobalVariables.UILobby = this;
		uiprofileSitePrefab = Instantiate(uiprofileSitePrefab, gameObject.transform);
		//NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectCallback;
		SceneManager.sceneLoaded += GlobalVariables.GameManager.SceneSwitched;
		SiteIndexOpen = 0;
		ipPlaceHolder.text = GlobalVariables.ipAddress;
		InitButtons();
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

	/// <summary>
	/// Gets the IP address which goes to the Internet
	/// </summary>
	/// <returns>String of the IP</returns>
	public static string GetLocalIPAddress(){
		using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
			socket.Connect("8.8.8.8", 65530);
		IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
		return endPoint.Address.ToString();
	}

}