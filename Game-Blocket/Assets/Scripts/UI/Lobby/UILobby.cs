using System.Linq;
using System.Net.Sockets;
using System.Net;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// LobbyUI-Handling
/// </summary>
public class UILobby : MonoBehaviour {
	public static UILobby Singleton { get; protected set; }

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
	#endregion

	private byte _siteIndexOpen;
	public byte SiteIndexOpen { get => _siteIndexOpen; set{
			_siteIndexOpen = value;
			ManageSites(value);
		}
	}

	public static byte role = 0;

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
			if (NetworkVariables.ipAddress != "127.0.0.1")
				NetworkVariables.ipAddress = GetLocalIPAddress();
			SiteIndexOpen = 2;
			SetNetworkAddress();
			NetworkManager.Singleton.StartHost();
		});
		clientBtn.onClick.AddListener(() => {
			CheckAndSetInputs();
			SiteIndexOpen = 2;
			//startGame.gameObject.SetActive(false);
			SetNetworkAddress();
			role = 1;
		});

		startGame.onClick.AddListener(() => {
			if(role == 1) {
				NetworkManager.Singleton.StartClient();
			}
			SceneManager.UnloadSceneAsync("Lobby");
			if(NetworkManager.Singleton.IsServer)
				NetworkManager.Singleton.SceneManager.LoadScene("MainGame", LoadSceneMode.Additive);
			if(NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
				SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
		});

		goBackBtn.onClick.AddListener(() => {
			SiteIndexOpen = 0;
			startGame.gameObject.SetActive(true);
			NetworkManager.Singleton.Shutdown();
		});

		testBtn.onClick.AddListener(() => {
			Debug.Log("Pending: " + NetworkManager.Singleton.PendingClients.Keys.ToList().Count);
			Debug.Log("Connected: " + NetworkManager.Singleton.ConnectedClientsList.Count);
		});
	}

	public void Awake() {
		Singleton = this;
		uiprofileSitePrefab = Instantiate(uiprofileSitePrefab, gameObject.transform);
		//NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectCallback;
		SceneManager.sceneLoaded += GameManager.Singleton.SceneSwitched;
		SiteIndexOpen = 0;
		ipPlaceHolder.text = NetworkVariables.ipAddress;
		InitButtons();
		UIMainMenu.CheckForLoadingScene();
	}

	private void SetNetworkAddress()
	{
		GameManager.Singleton.uNetTransport.ConnectAddress = NetworkVariables.ipAddress;
		GameManager.Singleton.uNetTransport.ConnectPort = NetworkVariables.portAddress;
	}

	/// <summary>
	/// TODO: More check
	/// </summary>
	/// <returns></returns>
	private void CheckAndSetInputs(){
		if (ipInput.text.Length > 8 && ipInput.text.Trim() != string.Empty && ipInput.text.IndexOf(".") != ipInput.text.LastIndexOf("."))
			NetworkVariables.ipAddress = ipInput.text;
		else
			Debug.LogWarning($"IP-Input empty! Using: {NetworkVariables.ipAddress}");
			
		if (portInput.text.Trim() != "" && portInput.text.ToUpper() == portInput.text.ToLower())
			NetworkVariables.portAddress = int.Parse(portInput.text);
		else
			Debug.LogWarning($"Port-Input empty! Using: {NetworkVariables.portAddress}");
		
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