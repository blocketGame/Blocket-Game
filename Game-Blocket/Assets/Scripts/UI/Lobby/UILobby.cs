using System.Collections;
using System.Collections.Generic;

using UnityEngine;
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

		});
		hostBtn.onClick.AddListener(() => {

		});
		clientBtn.onClick.AddListener(() => {

		});
	}
}
