using MLAPI;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProfileSite : MonoBehaviour
{
	[Header("Static Resources")]
	public Text createInput;
	public GameObject worldSelectionSite, characterSelectionSite, listContentPrefab;
	public Button backBtn, selectBtn, characterSlectBtn, worldSelectBtn, createBtn;
	public ScrollRect playerScrollRect, worldScrollRect;

	private bool _characterSelectonOpen = false;
	public bool CharacterSelectonOpen { get { return _characterSelectonOpen; } set { 
			_characterSelectonOpen = value;
			characterSelectionSite.SetActive(value);
			worldSelectionSite.SetActive(!value);
		} 
	}

	private void InitButtons(){
		if(GlobalVariables.muliplayer)
			if(NetworkManager.Singleton.IsClient)
				worldSelectBtn.interactable = false;
		backBtn.onClick.AddListener(() => {
			GlobalVariables.UILobby.SiteIndexOpen = 0;
		});
		selectBtn.onClick.AddListener(() => {

		});
		characterSlectBtn.onClick.AddListener(() => {

		});
		worldSelectBtn.onClick.AddListener(() => {

		});
		createBtn.onClick.AddListener(() => { 
			
		});
    }

	public void Awake()
	{
		GlobalVariables.UIProfileSite = this;
		InitButtons();
	}
}
