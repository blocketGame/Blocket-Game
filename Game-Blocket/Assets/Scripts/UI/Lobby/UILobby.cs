using System.Collections;
using System.Collections.Generic;

using MLAPI;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILobby : MonoBehaviour
{
	#region UIResources
	[Header("Static: General")]
	public GameObject startSite, lobbySite;

	[Header("Static: Start Site")]
	public Button serverBtn, hostBtn, clientBtn;
	public Text ipInput, portInput;

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
		StartSiteOpen = true;

		serverBtn.onClick.AddListener(() => {
			NetworkManager.Singleton.StartServer();
		});
		hostBtn.onClick.AddListener(() => {
			NetworkManager.Singleton.StartHost();
			SceneManager.LoadScene("MainGame", LoadSceneMode.Single);
			//TODO... Better
			SceneManager.MoveGameObjectToScene(GameObject.Find("NetworkManager"), SceneManager.GetSceneByName("MainGame"));
		});
		clientBtn.onClick.AddListener(() => {
			NetworkManager.Singleton.StartClient();
		});
	}
}
